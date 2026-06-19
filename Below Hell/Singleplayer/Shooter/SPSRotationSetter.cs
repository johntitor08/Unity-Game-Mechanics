using UnityEngine;

public class SPSRotationSetter : MonoBehaviour
{
    private Transform target;
    public float rotationSpeed = 5f;

    private void Start()
    {
        target = GameObject.FindWithTag("Player").GetComponent<Transform>();

    }

    private void Update()
    {
        if (target)
        {
            Vector3 directionToPlayer = target.position - transform.position;
            directionToPlayer.Normalize();
            Quaternion targetRotation = Quaternion.LookRotation(Vector3.forward, directionToPlayer);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        }

    }
}