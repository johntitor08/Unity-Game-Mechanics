using UnityEngine;

public class CircleOutlineOnHit : MonoBehaviour
{
    public Camera cam;
    public LayerMask hitLayers;
    public LineRenderer circleRenderer;
    public float radius = 0.3f;
    public int segments = 60;

    private Vector3 lastHitCenter = Vector3.positiveInfinity;

    void Awake()
    {
        if (circleRenderer != null)
            circleRenderer.loop = true;
    }

    void Update()
    {
        if (cam == null)
            return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero, Mathf.Infinity, hitLayers);

        if (hit.collider != null)
        {
            circleRenderer.gameObject.SetActive(true);
            Vector3 center = hit.collider.bounds.center;
            center.z = 0f;

            if (center != lastHitCenter)
            {
                DrawCircle(center);
                lastHitCenter = center;
            }
        }
        else
        {
            circleRenderer.gameObject.SetActive(false);
            lastHitCenter = Vector3.positiveInfinity;
        }
    }

    void DrawCircle(Vector3 center)
    {
        circleRenderer.positionCount = segments;
        float angleStep = (2f * Mathf.PI) / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = angleStep * i;
            float x = center.x + Mathf.Cos(angle) * radius;
            float y = center.y + Mathf.Sin(angle) * radius;
            circleRenderer.SetPosition(i, new Vector3(x, y, 0f));
        }
    }
}
