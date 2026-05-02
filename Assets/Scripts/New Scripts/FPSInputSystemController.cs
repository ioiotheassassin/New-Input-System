using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class FPSInputSystemController : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public Gun gun;

    private PlayerControls input;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private bool isShootingHeld;
    private bool isBursting;

    [Header("Look")]
    public float mouseSensitivity = 2f;
    private float xRotation;

    [Header("Movement")]
    public CharacterController controller;
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    private float verticalVelocity;

    void Awake()
    {
        input = new PlayerControls();
    }

    void OnEnable()
    {
        input.Enable();

        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        input.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        input.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        input.Player.Jump.performed += ctx => Jump();

        input.Player.Shoot.performed += ctx => isShootingHeld = true;
        input.Player.Shoot.canceled += ctx => isShootingHeld = false;

        input.Player.AltFire.performed += ctx => TryBurst();
    }

    void OnDisable()
    {
        input.Disable();
    }

    void Update()
    {
        Move();
        Look();
        HandleShooting();
    }

    //  Movement
    void Move()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        verticalVelocity += Physics.gravity.y * Time.deltaTime;

        Vector3 velocity = move * moveSpeed + Vector3.up * verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    void Jump()
    {
        if (controller.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpForce * -2f * Physics.gravity.y);
        }
    }

    //  Look
    void Look()
    {
        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    //  Auto Fire
    void HandleShooting()
    {
        if (isBursting) return;

        if (isShootingHeld)
        {
            gun.AttemptFire(); 
        }
    }

    //  3 Round Burst system
    void TryBurst()
    {
        if (!isBursting)
        {
            StartCoroutine(BurstRoutine());
        }
    }

    IEnumerator BurstRoutine()
    {
        isBursting = true;

        int burstCount = 3;

        for (int i = 0; i < burstCount; i++)
        {
            bool fired = gun.AttemptFire();

            if (!fired)
                break;

            yield return new WaitForSeconds(0.08f);
        }

        isBursting = false;
    }
}