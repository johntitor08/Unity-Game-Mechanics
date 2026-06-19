using UnityEngine;
using UnityEngine.Tilemaps;

public class Digging : MonoBehaviour
{
    public Tilemap groundTilemap;
    public float digRange = 1.5f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos =
                Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Vector3Int cellPos =
                groundTilemap.WorldToCell(mouseWorldPos);
            float distance =
                Vector2.Distance(transform.position, mouseWorldPos);

            if (distance <= digRange)
            {
                groundTilemap.SetTile(cellPos, null);
            }
        }
    }
}
