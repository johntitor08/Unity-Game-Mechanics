using UnityEngine;

public class RegionHighlighter : MonoBehaviour
{
    public Camera cam;
    public LayerMask mask;
    public GameObject highlight;

    void Update()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, mask);

        if (hit.collider != null)
        {
            highlight.SetActive(true);
            highlight.transform.position = hit.point;
        }
        else
        {
            highlight.SetActive(false);
        }
    }
}

