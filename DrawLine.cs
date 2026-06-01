using UnityEngine;

public class DrawLine : MonoBehaviour
{
    public GameObject linePrefab;
    public LayerMask notDrawnableLayer;
    private int notDrawnableLayerIndex;
    public float linePointsMinDistance;
    public float lineWidth;
    public Gradient lineColor;
    private Line currentLine;

    private void Start()
    {
        notDrawnableLayerIndex = LayerMask.NameToLayer("NotDrawnable");

    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            BeginDraw();

        }

        if (currentLine != null)
        {
            Draw();

        }

        if (Input.GetMouseButtonUp(0))
        {
            EndDraw();

        }

    }

    private void BeginDraw()
    {
        currentLine = Instantiate(linePrefab, transform).GetComponent<Line>();
        currentLine.SetLineColor(lineColor);
        currentLine.SetLineWidth(lineWidth);
        currentLine.SetPointMinDistance(linePointsMinDistance);
        currentLine.UsePhysics(false);

    }

    private void Draw()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.CircleCast(mousePosition, lineWidth / 3, Vector2.zero, 1, notDrawnableLayer);

        if (hit)
        {
            EndDraw();

        }
        else
        {
            currentLine.AddPoint(mousePosition);

        }

    }

    private void EndDraw()
    {
        if (currentLine != null)
        {
            if (currentLine.pointsCount < 2)
            {
                Destroy(currentLine);

            }
            else
            {
                currentLine.gameObject.layer = notDrawnableLayerIndex;
                currentLine.UsePhysics(true);
                currentLine = null;

            }

        }

    }

}
