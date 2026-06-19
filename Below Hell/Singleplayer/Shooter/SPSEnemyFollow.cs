using UnityEngine;

public class SPSEnemyFollow : MonoBehaviour
{
    private Transform target;
    public float moveSpeed = 5f;

    private void Start()
    {
        target = GameObject.FindWithTag("Player").GetComponent<Transform>();

    }

    private void Update()
    {
        if (target != null && !target.GetComponent<SPSPlayerComponents>().isInvisible)
        {
            Vector3 direction = target.position - transform.position;
            direction.Normalize();
            transform.Translate(moveSpeed * Time.deltaTime * direction);

        }

    }

}
