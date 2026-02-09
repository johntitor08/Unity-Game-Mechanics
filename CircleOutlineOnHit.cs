using UnityEngine;

public class CircleOutlineOnHit : MonoBehaviour
{
    public Camera cam;
    public LayerMask hitLayers;
    public LineRenderer circleRenderer;
    public float radius = 0.3f;
    public int segments = 60;

    void Update()
    {
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0;
        RaycastHit2D hit = Physics2D.Raycast(mouseWorld, Vector2.zero, Mathf.Infinity, hitLayers);

        if (hit.collider != null)
        {
            circleRenderer.gameObject.SetActive(true);
            DrawCircle(hit.point);
        }
        else
        {
            circleRenderer.gameObject.SetActive(false);
        }
    }

    void DrawCircle(Vector3 center)
    {
        circleRenderer.positionCount = segments;
        float angleStep = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = Mathf.Deg2Rad * angleStep * i;
            float x = center.x + Mathf.Cos(angle) * radius;
            float y = center.y + Mathf.Sin(angle) * radius;
            circleRenderer.SetPosition(i, new Vector3(x, y, 0));
        }
    }
}


