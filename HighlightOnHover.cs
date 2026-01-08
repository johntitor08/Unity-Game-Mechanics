using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HighlightOnHover : MonoBehaviour
{
    private SpriteRenderer sr;
    private Color originalColor;
    public Color highlightColor = new(1f, 1f, 1f, 0.5f);

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    void OnMouseEnter()
    {
        sr.color = highlightColor;
    }

    void OnMouseExit()
    {
        sr.color = originalColor;
    }
}
