using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class TileDestroyer : MonoBehaviour
{
    [SerializeField] private Tile Grass;
    private List<TileType> localTileTypes;
    private Tilemap tilemap;
    private Vector3Int tilePos;
    [SerializeField] private GridLayout grid;
    private Vector3Int nTilePos;
    private Vector3Int wTilePos;
    private Vector3Int sTilePos;
    private Vector3Int eTilePos;
    private Tile nTile;
    private Tile sTile;
    private Tile wTile;
    private Tile eTile;

    private enum TileType
    {
        Unknow,
        Grass

    }

    private enum TileLocation
    {
        N, E, S, W

    }

    private void Start()
    {
        tilemap = GetComponent<Tilemap>();

    }

    private void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        tilePos = grid.WorldToCell(mousePos);

        if (Input.GetMouseButtonUp(0))
        {
            Tile clickedTile = tilemap.GetTile<Tile>(tilePos);

            if (clickedTile == Grass)
            {
                GetLocalTiles(tilePos);

                if (nTile && wTile && sTile && eTile)
                {
                    return;

                }

                tilemap.SetTile(tilePos, null);
                SetLocalTiles();

            }

        }

    }

    private TileType GetTileType(Tile tile)
    {
        if (tile == Grass) { return TileType.Grass; }
        return TileType.Unknow;

    }

    private void GetLocalTiles(Vector3Int tilePos)
    {
        nTilePos = new Vector3Int(tilePos.x, tilePos.y + 1, tilePos.z);
        sTilePos = new Vector3Int(tilePos.x, tilePos.y - 1, tilePos.z);
        wTilePos = new Vector3Int(tilePos.x - 1, tilePos.y, tilePos.z);
        eTilePos = new Vector3Int(tilePos.x + 1, tilePos.y, tilePos.z);
        nTile = tilemap.GetTile<Tile>(nTilePos);
        wTile = tilemap.GetTile<Tile>(wTilePos);
        sTile = tilemap.GetTile<Tile>(sTilePos);
        eTile = tilemap.GetTile<Tile>(eTilePos);
        localTileTypes = new List<TileType> { GetTileType(nTile), GetTileType(wTile), GetTileType(sTile), GetTileType(eTile) };

    }

    private void UpdateTile(TileLocation tileLocation)
    {
        switch (tileLocation)
        {
            case TileLocation.N:

                switch (localTileTypes[0])
                {
                    case TileType.Grass:

                        tilemap.SetTile(nTilePos, Grass);
                        break;

                }

                break;

            case TileLocation.W:

                switch (localTileTypes[1])
                {
                    case TileType.Grass:

                        tilemap.SetTile(wTilePos, Grass);
                        break;

                }

                break;

            case TileLocation.S:

                switch (localTileTypes[2])
                {
                    case TileType.Grass:

                        tilemap.SetTile(sTilePos, Grass);
                        break;

                }

                break;

            case TileLocation.E:

                switch (localTileTypes[3])
                {
                    case TileType.Grass:

                        tilemap.SetTile(eTilePos, Grass);
                        break;

                }

                break;

        }

    }

    private void SetLocalTiles()
    {
        foreach (TileLocation tileLocation in (TileLocation[])Enum.GetValues(typeof(TileLocation)))
        {
            UpdateTile(tileLocation);

        }

    }

}