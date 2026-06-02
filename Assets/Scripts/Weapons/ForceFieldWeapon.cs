using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ForceFieldWeapon : WeaponBase
{
    // ─────────────────────────────────────────────
    // ESTADÍSTICAS ACTUALES
    // ─────────────────────────────────────────────

    private float currentDamage        = 5f;
    private float currentRadius        = 2f;
    private float currentActiveDuration   = 1.5f;  // Segundos que el campo permanece activo
    private float currentInactiveDuration = 2.5f;  // Segundos entre pulsos
    private float damageInterval       = 0.5f;     // Daño cada N segundos mientras está activo
    private bool  hasSlowEffect        = false;
    private float slowMultiplier       = 0.4f;

    // ─────────────────────────────────────────────
    // COMPONENTES
    // ─────────────────────────────────────────────

    private CircleCollider2D fieldCollider;
    private SpriteRenderer   fieldRenderer;
    private bool             fieldOn    = false;
    private float            damageTimer = 0f;
    private List<EnemyBase>  enemiesInField = new List<EnemyBase>();

    // ─────────────────────────────────────────────
    // INICIALIZACIÓN
    // ─────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();

        // Collider en este GameObject (no cambia escala)
        fieldCollider = GetComponent<CircleCollider2D>();
        if (fieldCollider == null)
            fieldCollider = gameObject.AddComponent<CircleCollider2D>();
        fieldCollider.isTrigger = true;
        fieldCollider.radius    = currentRadius;
        fieldCollider.enabled   = false;

        // Visual en un hijo separado para controlar su escala sin afectar el collider
        var visualGO = new GameObject("Visual");
        visualGO.transform.SetParent(transform, false);
        fieldRenderer = visualGO.AddComponent<SpriteRenderer>();
        fieldRenderer.sprite       = CreateRingSprite();
        fieldRenderer.color        = new Color(0.2f, 0.85f, 1f, 0f);
        fieldRenderer.sortingOrder = -1;

        UpdateVisualSize();
    }

    // ─────────────────────────────────────────────
    // UPDATE: DAÑO CONTINUO
    // ─────────────────────────────────────────────

    void Update()
    {
        if (!isActive || !fieldOn) return;
        if (enemiesInField.Count == 0) return;

        damageTimer += Time.deltaTime;
        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;
            DamageEnemiesInField();
        }
    }

    // ─────────────────────────────────────────────
    // LOOP DE ATAQUE: PULSO INTERMITENTE
    // ─────────────────────────────────────────────

    protected override IEnumerator AttackLoop()
    {
        while (isActive)
        {
            // — Activar campo —
            fieldOn = true;
            fieldCollider.enabled = true;

            // Fade in rápido
            float t = 0f;
            Color c = fieldRenderer.color;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(0f, 0.55f, t / 0.2f);
                fieldRenderer.color = c;
                yield return null;
            }

            // Pulso mientras está activo
            float elapsed = 0f;
            while (elapsed < currentActiveDuration)
            {
                elapsed += Time.deltaTime;
                c.a = 0.45f + Mathf.Sin(elapsed * 5f) * 0.1f;
                fieldRenderer.color = c;
                yield return null;
            }

            // — Desactivar campo —
            fieldOn = false;
            fieldCollider.enabled = false;

            if (hasSlowEffect)
                foreach (EnemyBase e in enemiesInField)
                    if (e != null && e.IsAlive()) e.RemoveSlow();
            enemiesInField.Clear();

            // Fade out rápido
            float startAlpha = fieldRenderer.color.a;
            t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(startAlpha, 0f, t / 0.2f);
                fieldRenderer.color = c;
                yield return null;
            }
            c.a = 0f;
            fieldRenderer.color = c;

            // Esperar antes del próximo pulso
            yield return new WaitForSeconds(currentInactiveDuration);
        }
    }

    // ─────────────────────────────────────────────
    // DAÑO
    // ─────────────────────────────────────────────

    private void DamageEnemiesInField()
    {
        for (int i = enemiesInField.Count - 1; i >= 0; i--)
        {
            EnemyBase enemy = enemiesInField[i];
            if (enemy == null || !enemy.IsAlive())
            {
                enemiesInField.RemoveAt(i);
                continue;
            }
            enemy.TakeDamage(GetFinalDamage(currentDamage));
        }
    }

    // ─────────────────────────────────────────────
    // DETECCIÓN DE ENEMIGOS
    // ─────────────────────────────────────────────

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive || !fieldOn) return;
        if (!other.CompareTag("Enemy")) return;

        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy == null || enemiesInField.Contains(enemy)) return;

        enemiesInField.Add(enemy);
        if (hasSlowEffect) enemy.ApplySlow(slowMultiplier);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        EnemyBase enemy = other.GetComponent<EnemyBase>();
        if (enemy == null) return;

        enemiesInField.Remove(enemy);
        if (hasSlowEffect) enemy.RemoveSlow();
    }

    // ─────────────────────────────────────────────
    // SUBIDA DE NIVEL
    // ─────────────────────────────────────────────

    protected override void OnLevelUp(int newLevel, LevelData levelData)
    {
        currentDamage = levelData.damage;
        currentRadius = levelData.area;

        // duration  = tiempo que el campo permanece encendido
        // cooldown  = tiempo que permanece apagado entre pulsos
        if (levelData.duration > 0)  currentActiveDuration   = levelData.duration;
        if (levelData.cooldown > 0)  currentInactiveDuration = levelData.cooldown;

        if (fieldCollider != null) fieldCollider.radius = currentRadius;
        UpdateVisualSize();

        if (levelData.isEvolution && levelData.specialAbility == "Ralentizacion")
        {
            hasSlowEffect = true;
            foreach (EnemyBase enemy in enemiesInField)
                if (enemy != null && enemy.IsAlive())
                    enemy.ApplySlow(slowMultiplier);
            Debug.Log("Campo de Fuerza evolucionado: ralentización activa");
        }

        Debug.Log($"Campo de Fuerza nivel {newLevel}: " +
                  $"radio={currentRadius}, daño={currentDamage}, " +
                  $"activo={currentActiveDuration}s, pausa={currentInactiveDuration}s");
    }

    // ─────────────────────────────────────────────
    // VISUAL
    // ─────────────────────────────────────────────

    private void UpdateVisualSize()
    {
        if (fieldRenderer == null) return;
        // El sprite es 1x1 (radio 0.5). Para que coincida con el collider:
        // escala = currentRadius * 2
        // El objeto hijo hereda la escala del padre (Player = 0.8),
        // igual que el collider, así ambos se compensan igual.
        float s = currentRadius * 2f;
        fieldRenderer.transform.localScale = new Vector3(s, s, 1f);
    }

    // Genera un sprite de anillo en tiempo de ejecución (sin assets externos)
    private Sprite CreateRingSprite()
    {
        const int res = 256;
        var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        float center     = res / 2f;
        float outerR     = res * 0.47f;
        float innerR     = res * 0.34f;
        float softness   = res * 0.05f;

        var pixels = new Color[res * res];
        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dx   = x - center;
                float dy   = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float a    = 0f;

                if (dist >= innerR && dist <= outerR)
                {
                    float outerFade = Mathf.InverseLerp(outerR, outerR - softness, dist);
                    float innerFade = Mathf.InverseLerp(innerR, innerR + softness, dist);
                    a = Mathf.Min(outerFade, innerFade);
                }
                else if (dist < innerR)
                {
                    // Relleno muy tenue dentro del anillo
                    a = Mathf.InverseLerp(innerR, innerR - softness * 2f, dist) * 0.08f;
                }

                pixels[y * res + x] = new Color(1f, 1f, 1f, a);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), Vector2.one * 0.5f, res);
    }
}
