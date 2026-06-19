using UnityEngine;

public class FallingObjects : MonoBehaviour
{
    public GameObject objectPrefab;
    private bool isTriggered = false;
    private float spawnTime = 0;
    private MPPGameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<MPPGameManager>();
    }

    private void Update()
    {
        spawnTime += Time.deltaTime;

        if (spawnTime > 0.1f && isTriggered && gameManager.maps[1].activeInHierarchy)
        {
            GameObject singleObject = Instantiate(objectPrefab, new Vector3(Random.Range(40f, 100f), 50), Quaternion.identity);
            Destroy(singleObject, 3);
            spawnTime = 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isTriggered = true;
        }
    }
}
