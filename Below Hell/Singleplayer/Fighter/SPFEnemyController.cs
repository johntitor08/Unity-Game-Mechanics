using TMPro;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Explodable))]
[RequireComponent(typeof(HealthBar))]
public class SPFEnemyController : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;
    private Explodable explodable;
    private Transform target;
    [SerializeField] private float moveSpeed = 5;
    private Animator animator;
    private float attackTime;
    private bool canDamage;
    private HealthBar healthBar;
    [SerializeField] private TMP_Text usernameText;

    private void Start()
    {
        target = GameObject.FindWithTag("Player").GetComponent<Transform>();
        animator = GetComponent<Animator>();
        healthBar = GetComponent<HealthBar>();
        explodable = GetComponent<Explodable>();
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    private void Update()
    {
        if (target != null)
        {
            if (Vector2.Distance(target.position, transform.position) > 3)
            {
                Vector3 direction = target.position - transform.position;
                direction.Normalize();
                transform.Translate(moveSpeed * Time.deltaTime * direction);
            }
            else if (attackTime < 0)
            {
                canDamage = true;
                animator.SetTrigger("attack");
                attackTime = 0.5f;
            }

            Flip();
        }

        attackTime -= Time.deltaTime;
    }

    private void Flip()
    {
        if (target.position.x - transform.position.x < 0)
        {
            transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
            usernameText.transform.localScale = new Vector2(Mathf.Abs(usernameText.transform.localScale.x), usernameText.transform.localScale.y);
            healthBar.FlipHealthBar(1);
        }
        else
        {
            transform.localScale = new Vector2(-Mathf.Abs(transform.localScale.x), transform.localScale.y);
            usernameText.transform.localScale = new Vector2(-Mathf.Abs(usernameText.transform.localScale.x), usernameText.transform.localScale.y);
            healthBar.FlipHealthBar(-1);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0)
        {
            GameObject.FindWithTag("Player").GetComponent<SPFPlayerController>().enemyKill++;
            Die();
        }
    }

    public void Heal(int healingAmount)
    {
        if (currentHealth >= maxHealth)
        {
            return;
        }

        currentHealth += healingAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        healthBar.SetHealth(currentHealth);
    }

    private void Die()
    {
        explodable.Explode();
        ExplosionForce explosionForce = FindAnyObjectByType<ExplosionForce>();

        if (explosionForce != null)
        {
            explosionForce.DoExplosion(transform.position);
        }
        else
        {
            Debug.LogWarning("No ExplosionForce found in scene to apply explosion.", this);
        }

        Destroy(gameObject, 3);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && canDamage)
        {
            target.GetComponent<SPFPlayerController>().TakeDamage(1);
            canDamage = false;
        }
    }
}
