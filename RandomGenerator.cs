using UnityEngine;

public class RandomGenerator : MonoBehaviour
{
    [SerializeField] GameObject speedPrefab;
    [SerializeField] GameObject invisibilityPrefab;
    [SerializeField] GameObject bulletFrequencyPrefab;
    [SerializeField] GameObject healthPrefab;
    [SerializeField] GameObject biggerBulletPrefab;

    void Start()
    {
        for (int i = 0; i < 20; i++)
        {
            Instantiate(speedPrefab, new Vector2(Random.Range(-100, 200), Random.Range(-100, 100)), Quaternion.identity);

        }

        for (int i = 0; i < 10; i++)
        {
            Instantiate(invisibilityPrefab, new Vector2(Random.Range(-100, 200), Random.Range(-100, 100)), Quaternion.identity);

        }

        for (int i = 0; i < 20; i++)
        {
            Instantiate(bulletFrequencyPrefab, new Vector2(Random.Range(-100, 200), Random.Range(-100, 100)), Quaternion.identity);

        }

        for (int i = 0; i < 20; i++)
        {
            Instantiate(healthPrefab, new Vector2(Random.Range(-100, 200), Random.Range(-100, 100)), Quaternion.identity);

        }

        for (int i = 0; i < 10; i++)
        {
            Instantiate(biggerBulletPrefab, new Vector2(Random.Range(-100, 200), Random.Range(-100, 100)), Quaternion.identity);

        }

    }

}
