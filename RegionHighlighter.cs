using UnityEngine;

public class RegionHighlighter : MonoBehaviour
{
    public Camera cam;
    public LayerMask mask;
    public GameObject highlight;
    private bool isHighlightActive = false;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;

        if (highlight != null)
            highlight.SetActive(false);
    }

    void Update()
    {
        if (cam == null || highlight == null)
            return;

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hitCollider = Physics2D.OverlapPoint(mousePos, mask);

        if (hitCollider != null)
        {
            if (!isHighlightActive)
            {
                highlight.SetActive(true);
                isHighlightActive = true;
            }

            highlight.transform.position = hitCollider.bounds.center;
        }
        else
        {
            if (isHighlightActive)
            {
                highlight.SetActive(false);
                isHighlightActive = false;
            }
        }
    }
}
