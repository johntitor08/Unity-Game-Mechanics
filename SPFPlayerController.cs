using UnityEngine;
using TMPro;

#if !UNITY_WEBGL

using Firebase.Database;

#endif

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(HealthBar))]
[RequireComponent(typeof(Explodable))]
public class SPFPlayerController : MonoBehaviour
{
    private static readonly int WallSlideHash = Animator.StringToHash("wallSlide");
    private static readonly int Attack2Hash = Animator.StringToHash("attack2");
    private static readonly int AttackHash = Animator.StringToHash("attack");
    private static readonly int RunHash = Animator.StringToHash("run");
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");

#if !UNITY_WEBGL

    private DatabaseReference databaseReference;

#endif

    private UserManager userManager;
    private SPFGameManager gameManager;
    private SetKillScore setKillScore;
    [SerializeField] private TMP_Text usernameText;
    private Rigidbody2D rb;
    private float horizontal;
    [SerializeField] private float wallSlidingSpeed;
    [SerializeField] private float speed;
    [SerializeField] private float jumpingPower;
    private bool canJump;
    private bool isFacingRight = true;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    private Animator animator;
    [SerializeField] private int maxHealth = 10;
    private int currentHealth;
    [HideInInspector] public int enemyKill = 0;
    private Explodable explodable;
    private float attackTime;
    private int attackIndex;
    private bool canDamage;
    private HealthBar healthBar;

    void Start()
    {
#if !UNITY_WEBGL

        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

#endif
        userManager = GameObject.Find("UserData").GetComponent<UserManager>();
        gameManager = GameObject.Find("Manager").GetComponent<SPFGameManager>();
        setKillScore = GameObject.Find("Manager").GetComponent<SetKillScore>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        healthBar = GetComponent<HealthBar>();
        explodable = GetComponent<Explodable>();
        usernameText.text = userManager.user.username;
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");

        if (IsGrounded())
        {
            canJump = true;
            animator.SetFloat(RunHash, Mathf.Abs(horizontal));
        }
        else if (IsWalled())
        {
            canJump = (horizontal < 0 && transform.localScale.x > 0) || (horizontal > 0 && transform.localScale.x < 0);
        }

        if (Input.GetButtonDown("Jump") && canJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
            canJump = false;
        }

        if (Input.GetMouseButtonDown(0) && attackTime < 0)
        {
            canDamage = true;
            attackIndex = Random.Range(0, 2);
            animator.SetTrigger(attackIndex == 0 ? AttackHash : Attack2Hash);
            attackTime = 0.5f;
        }

        Flip();
        WallSlide();
        attackTime -= Time.deltaTime;
        gameManager.scoreText.text = "Score: " + enemyKill;
        animator.SetFloat("jumpOrFall", rb.linearVelocity.y);
        animator.SetBool(IsGroundedHash, IsGrounded());
        SetKillScore();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
    }

    private bool IsGrounded() => Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);

    private bool IsWalled() => Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);

    private void Flip()
    {
        if (isFacingRight && horizontal < 0 || !isFacingRight && horizontal > 0)
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale; ls.x *= -1; transform.localScale = ls;
            Vector3 us = usernameText.transform.localScale; us.x *= -1; usernameText.transform.localScale = us;
            healthBar.FlipHealthBar();
        }
    }

    private void WallSlide()
    {
        bool sliding = IsWalled() && horizontal != 0;

        if (sliding)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlidingSpeed, float.MaxValue));

        animator.SetBool(WallSlideHash, sliding);
    }

    public void TakeDamage(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        healthBar.SetHealth(currentHealth);
        if (currentHealth <= 0) Die();
    }

    public void Heal(int amount)
    {
        if (currentHealth >= maxHealth) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        healthBar.SetHealth(currentHealth);
    }

    private void Die()
    {
        explodable.Explode();
        gameManager.isJustDied = true;
        gameManager.scoreTextInPanelPlayerDied.text = "Your score is: " + enemyKill;
        Destroy(gameObject, 3);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Spawner")
            transform.position = new Vector3(-39.5f, -2.7f, 0);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && canDamage)
        {
            collision.gameObject.GetComponent<SPFEnemyController>().TakeDamage(1);
            canDamage = false;
        }
    }

    private void SetKillScore()
    {
        if (enemyKill > userManager.user.highscore)
        {
            userManager.user.highscore = enemyKill;

#if UNITY_WEBGL

            StartCoroutine(setKillScore.SaveKillScoreToDatabase(userManager.user, userManager.IdToken));

#else

            StartCoroutine(setKillScore.SaveKillScoreToDatabase(databaseReference, userManager.user));

#endif
        }
    }
}
