using UnityEngine;

public class SPSEnemyGenerator : MonoBehaviour
{
    [SerializeField] private SPSPlayerComponents playerComponents;
    [SerializeField] private GameObject enemyPrefab;
    private int stepLevel = 1;

    private void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            Instantiate(enemyPrefab, new Vector2(Random.Range(-100, 200), Random.Range(-100, 100)), Quaternion.identity);

        }

    }

    private void Update()
    {
        for (int i = 1; i <= 10; i++)
        {
            if (playerComponents.enemyKill == 10 * i && stepLevel == i)
            {
                for (int j = 0; j < 10; j++)
                {
                    Instantiate(enemyPrefab, new Vector2(Random.Range(-100, 200), Random.Range(-100, 100)), Quaternion.identity);

                }
                
                stepLevel++;

            }

        }

    }

}