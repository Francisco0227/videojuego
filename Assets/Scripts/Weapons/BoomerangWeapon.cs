using UnityEngine;
using System.Collections;

public class BoomerangWeapon : WeaponBase
{
    private float currentDamage      = 20f;
    private float currentSpeed       = 7f;
    private int   currentProjectiles = 1;
    private float currentCooldown    = 3.5f;

    private Sprite cachedSprite;

    protected override void Awake()
    {
        base.Awake();
        cachedSprite = (itemData != null && itemData.icon != null)
            ? itemData.icon
            : CreateBoomerangSprite();
    }

    protected override IEnumerator AttackLoop()
    {
        while (isActive)
        {
            Fire();
            yield return new WaitForSeconds(GetFinalCooldown(currentCooldown));
        }
    }

    private void Fire()
    {
        Transform playerTr = transform.parent;

        for (int i = 0; i < currentProjectiles; i++)
        {
            // Dirección aleatoria, proyectiles distribuidos equitativamente si son varios
            float baseAngle  = Random.Range(0f, 360f);
            float angleStep  = currentProjectiles > 1 ? 360f / currentProjectiles : 0f;
            float angle      = (baseAngle + angleStep * i) * Mathf.Deg2Rad;
            Vector2 dir      = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            var boomGO = new GameObject("Boomerang");
            boomGO.transform.position = transform.position;

            var sr = boomGO.AddComponent<SpriteRenderer>();
            sr.sprite       = cachedSprite;
            sr.color        = new Color(1f, 0.5f, 0.1f);
            sr.sortingOrder = 2;
            boomGO.transform.localScale = Vector3.one * 0.4f;

            var rb = boomGO.AddComponent<Rigidbody2D>();
            rb.gravityScale  = 0f;
            rb.freezeRotation = false;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = boomGO.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius    = 0.5f;

            var comp = boomGO.AddComponent<BoomerangProjectile>();
            comp.Initialize(GetFinalDamage(currentDamage), currentSpeed, dir, playerTr);
        }
    }

    protected override void OnLevelUp(int newLevel, LevelData levelData)
    {
        currentDamage = levelData.damage;
        if (levelData.speed          > 0) currentSpeed       = levelData.speed;
        if (levelData.projectileCount > 0) currentProjectiles = levelData.projectileCount;
        if (levelData.cooldown       > 0) currentCooldown    = levelData.cooldown;
    }

    private Sprite CreateBoomerangSprite()
    {
        const int res = 64;
        var tex    = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float center = res / 2f;
        var pixels = new Color[res * res];

        for (int y = 0; y < res; y++)
            for (int x = 0; x < res; x++)
            {
                // Triángulo/flecha apuntando a la derecha
                float nx = (x - center) / center;  // -1 a 1
                float ny = (y - center) / center;
                // Flecha: x > |y| * 0.6 - 0.3  (triángulo)
                float a = 0f;
                if (nx > Mathf.Abs(ny) * 0.8f - 0.2f && nx < 0.9f)
                {
                    float edge = 1f - Mathf.Abs(nx - 0.35f) / 0.55f;
                    a = Mathf.Clamp01(edge * 1.5f);
                }
                pixels[y * res + x] = new Color(1f, 1f, 1f, a);
            }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), Vector2.one * 0.5f, res);
    }
}
