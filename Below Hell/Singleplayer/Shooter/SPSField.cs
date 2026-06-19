using UnityEngine;

public class SPSField : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;

    private void Update()
    {
        Quaternion currentRotation = transform.rotation;

        if (Input.GetKey(KeyCode.F))
        {
            Quaternion newRotation = Quaternion.Euler(0f, 0f, rotationSpeed * Time.deltaTime * 50) * currentRotation;
            transform.rotation = newRotation;

        }

        if (Input.GetKey(KeyCode.R))
        {
            Quaternion newRotation = Quaternion.Euler(0f, 0f, -rotationSpeed * Time.deltaTime * 50) * currentRotation;
            transform.rotation = newRotation;

        }

    }

}