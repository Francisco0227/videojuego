using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SatelliteWeapon : WeaponBase
{
    private float currentDamage = 8f;
    private float orbitRadius   = 2.5f;
    private float orbitSpeed    = 90f;    // grados/segundo
    private int   targetOrbCount = 1;

    private float          orbitAngle = 0f;
    private readonly List<GameObject> orbs = new List<GameObject>();
    private Sprite cachedSprite;

    protected override void Awake()
    {
        base.Awake();
        cachedSprite = CreateOrbSprite();
    }

    void Update()
    {
        if (!isActive) return;

        orbitAngle += orbitSpeed * Time.deltaTime;

        for (int i = 0; i < orbs.Count; i++)
        {
            if (orbs[i] == null) continue;
            float angle = (orbitAngle + 360f / orbs.Count * i) * Mathf.Deg2Rad;
            orbs[i].transform.position = (Vector2)transform.position
                + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * orbitRadius;
        }
    }

    protected override IEnumerator AttackLoop()
    {
        // Los orbes están siempre activos; no se necesita loop de disparo.
        yield return null;
    }

    protected override void OnLevelUp(int newLevel, LevelData levelData)
    {
        currentDamage  = levelData.damage;
        if (levelData.area  > 0) orbitRadius  = levelData.area;
        if (levelData.speed > 0) orbitSpeed   = levelData.speed;

        int newCount = levelData.projectileCount > 0 ? levelData.projectileCount : targetOrbCount;

        // Crear orbes nuevos si hacen falta
        while (orbs.Count < newCount)
            AddOrb();

        targetOrbCount = newCount;

        // Actualizar daño en orbes existentes
        float finalDmg = GetFinalDamage(currentDamage);
        foreach (var orb in orbs)
            orb?.GetComponent<SatelliteOrb>()?.SetDamage(finalDmg);
    }

    private void AddOrb()
    {
        var orbGO = new GameObject("SatelliteOrb");
        orbGO.layer = gameObject.layer;

        var sr = orbGO.AddComponent<SpriteRenderer>();
        sr.sprite       = cachedSprite;
        sr.color        = new Color(0.3f, 0.9f, 1f);
        sr.sortingOrder = 2;
        orbGO.transform.localScale = Vector3.one * 0.38f;

        // Rigidbody2D kinematic para que los triggers funcionen correctamente
        var rb = orbGO.AddComponent<Rigidbody2D>();
        rb.bodyType    = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = orbGO.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.5f;

        var comp = orbGO.AddComponent<SatelliteOrb>();
        comp.SetDamage(GetFinalDamage(currentDamage));

        orbs.Add(orbGO);
    }

    void OnDestroy()
    {
        foreach (var orb in orbs)
            if (orb != null) Destroy(orb);
    }

    private Sprite CreateOrbSprite()
    {
        const int res = 64;
        var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float center = res / 2f;
        float r      = res * 0.45f;
        var pixels   = new Color[res * res];
        for (int y = 0; y < res; y++)
            for (int x = 0; x < res; x++)
            {
                float dist = Mathf.Sqrt((x - center) * (x - center) + (y - center) * (y - center));
                float a    = Mathf.InverseLerp(r, r * 0.25f, dist);
                pixels[y * res + x] = new Color(1f, 1f, 1f, a);
            }
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), Vector2.one * 0.5f, res);
    }
}
