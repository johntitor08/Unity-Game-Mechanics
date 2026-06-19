using UnityEngine;
using UnityEngine.SceneManagement;

public class Bullet : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, 3f);

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (SceneManager.GetActiveScene().name == "SPSGame")
            {
                collision.gameObject.GetComponent<SPSPlayerComponents>().TakeDamage(1);

            }
            else if (SceneManager.GetActiveScene().name == "MPSGame")
            {
                collision.gameObject.GetComponent<MPSPlayerComponents>().TakeDamageServerRpc(1);

            }

        }

    }

}