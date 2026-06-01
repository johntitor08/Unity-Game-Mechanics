using TMPro;
using UnityEngine;

#if !UNITY_WEBGL

using Firebase.Database;

#endif

public class SPSPlayerComponents : MonoBehaviour
{
#if !UNITY_WEBGL

    private DatabaseReference databaseReference;

#endif

    private UserManager userManager;
    [SerializeField] private SPSShootingSystem shootingSystem;
    private SPSGameManager gameManager;
    private SetHighscore setHighscore;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer spriteRendererIcon;
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private int maxHealth = 30;
    private int currentHealth;
    public int enemyKill = 0;
    private float timeInvisibility = 0;
    private float elapsedTime;
    private readonly float transitionTime = 1;
    private float timeBulletFrequency = 0;
    public bool isInvisible = false;
    private bool isBulletFrequent = false;
    private float timeBiggerBullet = 0;

    private void Start()
    {
#if !UNITY_WEBGL

        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

#endif

        userManager = GameObject.Find("UserData").GetComponent<UserManager>();
        usernameText.text = userManager.user.username;
        gameManager = GameObject.Find("Manager").GetComponent<SPSGameManager>();
        setHighscore = GameObject.Find("Manager").GetComponent<SetHighscore>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRendererIcon = transform.Find("MinimapIcon").GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        usernameText.color = new Color32(255, 221, 0, 255);
        elapsedTime = transitionTime * 2;
    }

    private void Update()
    {
        if (timeInvisibility > 0)
        {
            BeInvisible();
            isInvisible = true;
        }
        else
        {
            BeVisible();
            isInvisible = false;
        }

        if (timeBulletFrequency > 0)
        {
            shootingSystem.fireFrequency = 0.2f;
            isBulletFrequent = true;
        }
        else
        {
            shootingSystem.fireFrequency = 0.5f;
            isBulletFrequent = false;
        }

        if (timeBiggerBullet > 0)
        {
            shootingSystem.isBiggerBullet = true;
        }
        else
        {
            shootingSystem.isBiggerBullet = false;
        }

        timeInvisibility -= Time.deltaTime;
        timeBulletFrequency -= Time.deltaTime;
        timeBiggerBullet -= Time.deltaTime;
        gameManager.scoreText.text = "Score: " + enemyKill;
        SetHighscore();
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        spriteRenderer.color -= new Color32(0, 5, 5, 0);
        spriteRendererIcon.color -= new Color32(0, 5, 5, 0);
    }

    private void Die()
    {
        gameManager.isJustDied = true;
        gameManager.scoreTextInPanelPlayerDied.text = "Your score is: " + enemyKill;
        Destroy(gameObject);
    }

    private void BeInvisible()
    {
        if (elapsedTime <= transitionTime)
        {
            elapsedTime += Time.deltaTime;
        }

        float time = elapsedTime / transitionTime;
        float alpha = Mathf.Lerp(1, 0, time);
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
        spriteRendererIcon.color = new Color(spriteRendererIcon.color.r, spriteRendererIcon.color.g, spriteRendererIcon.color.b, alpha);
    }

    private void BeVisible()
    {
        if (elapsedTime - transitionTime <= transitionTime)
        {
            elapsedTime += Time.deltaTime;
        }

        float time = (elapsedTime - transitionTime) / transitionTime;
        float alpha = Mathf.Lerp(0, 1, time);
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
        spriteRendererIcon.color = new Color(spriteRendererIcon.color.r, spriteRendererIcon.color.g, spriteRendererIcon.color.b, alpha);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Invisibility"))
        {
            timeInvisibility = 10;

            if (isInvisible)
            {
                timeInvisibility += 10;
            }
            else
            {
                elapsedTime = 0;
            }

            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("BulletFrequency"))
        {
            timeBulletFrequency = 10;

            if (isBulletFrequent)
            {
                timeBulletFrequency += 10;
            }

            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("Health"))
        {
            currentHealth = Mathf.Min(currentHealth + 1, maxHealth);
            spriteRenderer.color += new Color32(0, 5, 5, 0);
            spriteRendererIcon.color += new Color32(0, 5, 5, 0);
            Destroy(collision.gameObject);
        }

        if (collision.gameObject.CompareTag("BiggerBullet"))
        {
            timeBiggerBullet = 10;

            if (shootingSystem.isBiggerBullet)
            {
                timeBiggerBullet += 10;
            }

            Destroy(collision.gameObject);
        }
    }

    private void SetHighscore()
    {
        if (enemyKill > userManager.user.highscore)
        {
            userManager.user.highscore = enemyKill;

#if UNITY_WEBGL

            StartCoroutine(setHighscore.SaveHighscoreToDatabase(userManager.user));

#else

            StartCoroutine(setHighscore.SaveHighscoreToDatabase(databaseReference, userManager.user));

#endif
        }
    }
}
