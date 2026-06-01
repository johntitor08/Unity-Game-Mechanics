using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SPSEnemyDamage : MonoBehaviour
{
    private SPSPlayerComponents playerComponents;
    public int maxHealth = 10;
    private int currentHealth;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer spriteRendererIcon;

    private void Start()
    {
        playerComponents = GameObject.FindWithTag("Player").GetComponent<SPSPlayerComponents>();
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRendererIcon = transform.Find("MinimapIcon").GetComponent<SpriteRenderer>();

    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            Die();

        }

        spriteRenderer.color += new Color32(10, 25, 25, 0);
        spriteRendererIcon.color += new Color32(10, 25, 25, 0);

    }

    private void Die()
    {
        playerComponents.enemyKill++;
        Destroy(gameObject);

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            TakeDamage(1);

        }

    }

}