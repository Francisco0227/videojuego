using UnityEngine;

public class SatelliteOrb : MonoBehaviour
{
    private float damage = 8f;

    public void SetDamage(float d) { damage = d; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        other.GetComponent<EnemyBase>()?.TakeDamage(damage);
    }
}
