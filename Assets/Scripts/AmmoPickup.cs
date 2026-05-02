using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public int ammoAmount = 10;

    bool playerNearby = false;
    FPSController player;

    void OnEnable()
    {
        EventManager.OnInteractPressed += TryPickup;
    }

    void OnDisable()
    {
        EventManager.OnInteractPressed -= TryPickup;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Something entered trigger: " + other.name);

        player = other.GetComponentInParent<FPSController>();

        if (player != null)
        {
            playerNearby = true;
            Debug.Log("Player entered pickup range");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<FPSController>())
        {
            playerNearby = false;
            Debug.Log("Player left pickup range");
        }
    }

    void TryPickup()
    {
        if (!playerNearby) return;

        player.IncreaseAmmo(ammoAmount);

        Destroy(gameObject);
    }
}