using UnityEngine;

public class MoveableObject : MonoBehaviour
{
    private Vector2 startPosition;
    private float pauseTime;
    public float movementSpeed;
    public float distance;
    private float timer = 0;
    private bool isWaiting = false;
    public bool isMovingRight;
    private bool isMotionReversed = false;

    private void Start()
    {
        startPosition = transform.position;
        pauseTime = Random.Range(1f, 2f);
    }

    private void Update()
    {
        if (!isWaiting)
        {
            if (isMovingRight)
            {
                if (isMotionReversed)
                {
                    transform.Translate(movementSpeed * Time.deltaTime * Vector3.left);

                    if (transform.position.x < startPosition.x)
                    {
                        isMotionReversed = false;
                        isWaiting = true;
                    }
                }
                else
                {
                    transform.Translate(movementSpeed * Time.deltaTime * Vector3.right);

                    if (transform.position.x > startPosition.x + distance)
                    {
                        isMotionReversed = true;
                    }
                }
            }
            else
            {
                if (isMotionReversed)
                {
                    transform.Translate(movementSpeed * Time.deltaTime * Vector3.right);

                    if (transform.position.x > startPosition.x)
                    {
                        isMotionReversed = false;
                        isWaiting = true;
                    }
                }
                else
                {
                    transform.Translate(movementSpeed * Time.deltaTime * Vector3.left);

                    if (transform.position.x < startPosition.x - distance)
                    {
                        isMotionReversed = true;
                    }
                }
            }
        }
        else
        {
            if (gameObject.layer == 8)
            {
                timer += Time.deltaTime;

                if (timer >= pauseTime)
                {
                    timer = 0;
                    pauseTime = Random.Range(1f, 2f);
                    isWaiting = false;
                }
            }
            else
            {
                isWaiting = false;
            }
        }
    }
}
