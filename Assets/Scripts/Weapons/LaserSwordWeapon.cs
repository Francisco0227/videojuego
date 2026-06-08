using UnityEngine;
using System.Collections;

public class LaserSwordWeapon : WeaponBase
{
    private float currentDamage      = 15f;
    private float swordLength        = 1f;
    private float activeDuration     = 0.35f;
    private float inactiveDuration   = 1.5f;
    private float damageCooldown     = 0.1f;

    private SpriteRenderer swordRenderer;
    private PlayerMovement playerMovement;
    private bool           swordOn    = false;
    private float          damageTimer = 0f;

    protected override void Awake()
    {
        base.Awake();

        playerMovement = GetComponentInParent<PlayerMovement>();

        // Visual en hijo separado
        var visualGO = new GameObject("SwordVisual");
        visualGO.transform.SetParent(transform, false);

        swordRenderer = visualGO.AddComponent<SpriteRenderer>();
        swordRenderer.sprite       = (itemData != null && itemData.icon != null)
            ? itemData.icon
            : CreateSwordSprite();
        swordRenderer.color        = new Color(1f, 1f, 1f, 0f);
        swordRenderer.sortingOrder = 2;

        UpdateSwordVisual();
    }

    void Update()
    {
        if (!isActive) return;

        // Rotar hacia la dirección del jugador
        if (playerMovement != null)
        {
            Vector2 dir   = playerMovement.LastDirection;
            float   angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        if (!swordOn) return;

        damageTimer += Time.deltaTime;
        if (damageTimer >= damageCooldown)
        {
            damageTimer = 0f;
            DamageEnemiesInRange();
        }
    }

    private void DamageEnemiesInRange()
    {
        // Posición y tamaño del área de golpe en espacio mundo
        float   lossyX     = Mathf.Abs(transform.lossyScale.x);
        float   lossyY     = Mathf.Abs(transform.lossyScale.y);
        Vector2 right      = transform.right;
        Vector2 boxCenter  = (Vector2)transform.position + right * swordLength * 0.5f * lossyX;
        Vector2 boxSize    = new Vector2(swordLength * lossyX, 0.32f * lossyY);
        float   boxAngle   = transform.eulerAngles.z;

        var hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, boxAngle);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            hit.GetComponent<EnemyBase>()?.TakeDamage(GetFinalDamage(currentDamage));
        }
    }

    protected override IEnumerator AttackLoop()
    {
        while (isActive)
        {
            // Activar
            swordOn = true;
            Color c = swordRenderer.color;

            float t = 0f;
            while (t < 0.08f)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(0f, 0.92f, t / 0.08f);
                swordRenderer.color = c;
                yield return null;
            }

            yield return new WaitForSeconds(activeDuration);

            // Desactivar
            swordOn = false;
            t = 0f;
            while (t < 0.08f)
            {
                t += Time.deltaTime;
                c.a = Mathf.Lerp(0.92f, 0f, t / 0.08f);
                swordRenderer.color = c;
                yield return null;
            }
            c.a = 0f;
            swordRenderer.color = c;

            yield return new WaitForSeconds(inactiveDuration);
        }
    }

    protected override void OnLevelUp(int newLevel, LevelData levelData)
    {
        currentDamage = levelData.damage;
        if (levelData.area     > 0) swordLength       = levelData.area;
        if (levelData.duration > 0) activeDuration    = levelData.duration;
        if (levelData.cooldown > 0) inactiveDuration  = levelData.cooldown;
        UpdateSwordVisual();
    }

    private void UpdateSwordVisual()
    {
        if (swordRenderer == null) return;
        // El sprite del ícono es pequeño (13x37 px a 100PPU = 0.13x0.37 u)
        // Se escala para que sea visible en la escena de juego
        float scale = swordLength * 5f;
        swordRenderer.transform.localScale    = new Vector3(scale * 0.35f, scale, 1f);
        swordRenderer.transform.localPosition = new Vector3(swordLength * 0.5f, 0f, 0f);
    }

    private Sprite CreateSwordSprite()
    {
        const int W = 256, H = 32;
        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        var pixels = new Color[W * H];
        for (int y = 0; y < H; y++)
        {
            float yFade = Mathf.Sin((float)y / H * Mathf.PI);
            for (int x = 0; x < W; x++)
            {
                float xFade = Mathf.Lerp(1f, 0f, (float)x / W);
                float a     = yFade * xFade;
                float bright = 0.6f + yFade * 0.4f;
                pixels[y * W + x] = new Color(bright, bright, 1f, a);
            }
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0f, 0.5f), W);
    }
}
