using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public EdgeCollider2D edgeCollider;
    public Rigidbody2D rb;
    [HideInInspector] public List<Vector2> points = new();
    [HideInInspector] public int pointsCount = 0;
    private float pointsMinDistance = 0.1f;

    public Vector2 GetLastPoint()
    {
        return (Vector2)lineRenderer.GetPosition(pointsCount - 1);

    }

    public void UsePhysics(bool usePhysic)
    {
        rb.bodyType = usePhysic ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
    }

    public void SetLineColor(Gradient lineColor)
    {
        lineRenderer.colorGradient = lineColor;

    }

    public void SetLineWidth(float width)
    {
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        edgeCollider.edgeRadius = width / 2;

    }

    public void AddPoint(Vector2 newPoint)
    {
        if (pointsCount >= 1 && Vector2.Distance(newPoint, GetLastPoint()) < pointsMinDistance)
        {
            return;

        }

        points.Add(newPoint);
        pointsCount++;
        lineRenderer.positionCount = pointsCount;
        lineRenderer.SetPosition(pointsCount - 1, newPoint);

        if (pointsCount > 1)
        {
            edgeCollider.points = points.ToArray();

        }

    }

    public void SetPointMinDistance(float distance)
    {
        pointsMinDistance = distance;

    }

}