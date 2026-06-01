using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class CameraFollow : NetworkBehaviour
{
    private readonly float smoothTime = 0;
    private CinemachineVirtualCameraBase virtualCamera;
    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        if (IsOwner)
        {
            virtualCamera = GameObject.FindGameObjectWithTag("MainCVC").GetComponent<CinemachineVirtualCameraBase>();

        }

    }

    private void Update()
    {
        if (IsOwner)
        {
            Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, virtualCamera.transform.position.z);
            virtualCamera.transform.position = Vector3.SmoothDamp(virtualCamera.transform.position, targetPosition, ref velocity, smoothTime);

        }

    }

}