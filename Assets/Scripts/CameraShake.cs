using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    void OnEnable()
    {
        EventManager.OnGunFired += Shake;
    }

    void OnDisable()
    {
        EventManager.OnGunFired -= Shake;
    }

    void Shake()
    {
        StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        float duration = 0.15f;
        float magnitude = 0.15f;

        float elapsed = 0;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPosition;
    }
}