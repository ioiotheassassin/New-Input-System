using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState
    {
        Wander,
        Pursue,
        Attack,
        Recovery
    }

    [Header("State")]
    public EnemyState currentState;

    [Header("Ranges")]
    public float wanderRange = 10f;
    public float playerSightRange = 15f;
    public float playerAttackRange = 3f;

    [Header("Timing")]
    public float recoveryTime = 1.5f;
    public float attackCooldown = 2f;

    [Header("Movement Smoothing")]
    public float rotationSpeed = 6f;

    [Header("Attack")]
    public float lungeForce = 5f;

    [Header("Jump Settings")]
    public float jumpHeight = 2.5f;
    public float jumpDuration = 0.5f;

    private float stateTimer;
    private float lastAttackTime;

    private Vector3 startPos;
    private Transform player;

    private NavMeshAgent agent;
    private Rigidbody rb;

    private bool hasLunged;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        player = GameObject.FindGameObjectWithTag("Player").transform;
        startPos = transform.position;

        agent.acceleration = 20f;
        agent.angularSpeed = 999f;   
        agent.updateRotation = false; 
        agent.autoBraking = true;
        agent.stoppingDistance = 1.5f;

        ChangeState(EnemyState.Wander);
    }

    void Update()
    {
        stateTimer += Time.deltaTime;

        if (agent.isOnOffMeshLink)
        {
            StartCoroutine(JumpAcrossLink());
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        SmoothRotate(agent.velocity);

        switch (currentState)
        {
            case EnemyState.Wander:
                Wander(distance);
                break;

            case EnemyState.Pursue:
                Pursue(distance);
                break;

            case EnemyState.Attack:
                Attack();
                break;

            case EnemyState.Recovery:
                Recovery();
                break;
        }
    }

    
    void Wander(float distance)
    {
        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            Vector3 randomPoint = startPos + Random.insideUnitSphere * wanderRange;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, wanderRange, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }

        if (distance <= playerSightRange)
            ChangeState(EnemyState.Pursue);
    }

    
    void Pursue(float distance)
    {
        agent.isStopped = false;

        
        if (Time.frameCount % 3 == 0)
        {
            agent.SetDestination(player.position);
        }

        if (distance <= playerAttackRange && Time.time > lastAttackTime + attackCooldown)
        {
            ChangeState(EnemyState.Attack);
        }
        else if (distance > playerSightRange)
        {
            ChangeState(EnemyState.Wander);
        }
    }

    
    void Attack()
    {
        if (!hasLunged)
        {
            hasLunged = true;

            agent.isStopped = true;
            rb.linearVelocity = Vector3.zero;

            Vector3 dir = (player.position - transform.position).normalized;
            rb.AddForce(dir * lungeForce, ForceMode.Impulse);

            lastAttackTime = Time.time;

            Debug.Log("Enemy lunged!");
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= 2f)
        {
            Debug.Log("Player was hit!");
            EventManager.OnPlayerDamaged?.Invoke(1);

            ChangeState(EnemyState.Recovery);
            return;
        }

        if (stateTimer > 0.6f)
        {
            ChangeState(EnemyState.Recovery);
        }
    }

    
    void Recovery()
    {
        if (stateTimer >= recoveryTime)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            ChangeState(distance > playerSightRange ? EnemyState.Wander : EnemyState.Pursue);
        }
    }

    
    void ChangeState(EnemyState newState)
    {
        currentState = newState;
        stateTimer = 0f;
        hasLunged = false;

        agent.isStopped = false;

        if (newState == EnemyState.Wander || newState == EnemyState.Pursue)
        {
            agent.ResetPath();
            rb.linearVelocity = Vector3.zero;
        }
    }

    
    void SmoothRotate(Vector3 velocity)
    {
        if (velocity.sqrMagnitude < 0.01f) return;

        Vector3 dir = velocity.normalized;
        dir.y = 0;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * rotationSpeed
        );
    }

    
    IEnumerator JumpAcrossLink()
    {
        agent.isStopped = true;

        OffMeshLinkData data = agent.currentOffMeshLinkData;

        Vector3 start = transform.position;
        Vector3 end = data.endPos;

        float time = 0f;

        while (time < jumpDuration)
        {
            float t = time / jumpDuration;

            float height = Mathf.Sin(Mathf.PI * t) * jumpHeight;

            transform.position = Vector3.Lerp(start, end, t) + Vector3.up * height;

            time += Time.deltaTime;
            yield return null;
        }

        transform.position = end;

        agent.CompleteOffMeshLink();
        agent.isStopped = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player was hit!");
        }
    }
}