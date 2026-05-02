using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] Image healthBar;
    [SerializeField] TMP_Text currentAmmoText;
    [SerializeField] TMP_Text maxAmmoText;

    // red  flash image
    [SerializeField] Image damageFlash;

    FPSController player;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<FPSController>();
    }

    void OnEnable()
    {
        EventManager.OnAmmoChanged += UpdateAmmo;
        EventManager.OnPlayerDamaged += PlayerDamaged;
    }

    void OnDisable()
    {
        EventManager.OnAmmoChanged -= UpdateAmmo;
        EventManager.OnPlayerDamaged -= PlayerDamaged;
    }

    void UpdateAmmo(int ammo)
    {
        currentAmmoText.text = ammo.ToString();
    }

    void PlayerDamaged(int damage)
    {
        healthBar.fillAmount -= 0.1f;

        // stops any previous flash so it doesn't stack
        StopAllCoroutines();

        // starts red flash effect
        StartCoroutine(DamageFlashRoutine());
    }

    IEnumerator DamageFlashRoutine()
    {
        float duration = 0.2f;
        float elapsed = 0;

        Color c = damageFlash.color;

        // starts visible (semi-transparent red)
        c.a = 0.5f;
        damageFlash.color = c;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // fade out over time
            float alpha = Mathf.Lerp(0.5f, 0f, elapsed / duration);

            c.a = alpha;
            damageFlash.color = c;

            yield return null;
        }

        // make sure it's fully invisible at the end
        c.a = 0;
        damageFlash.color = c;
    }
}