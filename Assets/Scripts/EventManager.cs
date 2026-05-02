using System;
using UnityEngine;

public static class EventManager
{
    public static Action<int> OnAmmoChanged;

    public static Action OnInteractPressed;

    public static Action OnGunFired;

    public static Action<int> OnPlayerDamaged;
}