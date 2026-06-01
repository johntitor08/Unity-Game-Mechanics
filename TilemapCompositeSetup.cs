using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(TilemapCollider2D))]
[RequireComponent(typeof(CompositeCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]

public class TilemapCompositeSetup : MonoBehaviour
{
    void Awake()
    {
        Tilemap tilemap = GetComponent<Tilemap>();
        TilemapCollider2D tilemapCollider = GetComponent<TilemapCollider2D>();
        CompositeCollider2D compositeCollider = GetComponent<CompositeCollider2D>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        tilemap.RefreshAllTiles();
        rb.bodyType = RigidbodyType2D.Static;
        compositeCollider.geometryType = CompositeCollider2D.GeometryType.Outlines;
        compositeCollider.generationType = CompositeCollider2D.GenerationType.Synchronous;
        tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
    }
}
