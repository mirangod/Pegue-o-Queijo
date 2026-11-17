using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField, Min(1)] private int health = 1;

    private bool dead;

    public void TakeDamage(int damage)
    {
        if (dead || damage <= 0)
            return;

        health = Mathf.Max(0, health - damage);
        if (health > 0)
            return;

        dead = true;
        foreach (Collider2D enemyCollider in GetComponentsInChildren<Collider2D>())
            enemyCollider.enabled = false;

        Destroy(gameObject);
    }
}
