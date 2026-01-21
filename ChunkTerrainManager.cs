using UnityEngine;
using System.Collections.Generic;

public class ChunkTerrainManager : MonoBehaviour
{
    [Header("Chunk Settings")]
    public int chunkSize = 64;
    public int viewDistance = 3; // Chunks to load around player
    public Transform player;
    public GameObject terrainChunkPrefab;
    
    [Header("Terrain Settings")]
    public int heightMapResolution = 65; // chunkSize + 1
    public float terrainHeight = 50f;
    
    [Header("Noise Settings")]
    public float noiseScale = 20f;
    public int octaves = 4;
    [Range(0f, 1f)]
    public float persistence = 0.5f;
    public float lacunarity = 2f;
    public float seed = 42f;
    
    private Dictionary<Vector2Int, GameObject> terrainChunks = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int currentPlayerChunk = new Vector2Int(int.MaxValue, int.MaxValue);
    
    void Start()
    {
        if (player == null)
        {
            player = Camera.main.transform;
        }
        
        UpdateChunks();
    }
    
    void Update()
    {
        Vector2Int playerChunk = GetChunkCoord(player.position);
        
        if (playerChunk != currentPlayerChunk)
        {
            currentPlayerChunk = playerChunk;
            UpdateChunks();
        }
    }
    
    void UpdateChunks()
    {
        HashSet<Vector2Int> chunksToKeep = new HashSet<Vector2Int>();
        
        // Generate chunks around player
        for (int x = -viewDistance; x <= viewDistance; x++)
        {
            for (int z = -viewDistance; z <= viewDistance; z++)
            {
                Vector2Int chunkCoord = currentPlayerChunk + new Vector2Int(x, z);
                chunksToKeep.Add(chunkCoord);
                
                if (!terrainChunks.ContainsKey(chunkCoord))
                {
                    GenerateChunk(chunkCoord);
                }
            }
        }
        
        // Remove chunks outside view distance
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var chunk in terrainChunks)
        {
            if (!chunksToKeep.Contains(chunk.Key))
            {
                chunksToRemove.Add(chunk.Key);
            }
        }
        
        foreach (var chunkCoord in chunksToRemove)
        {
            Destroy(terrainChunks[chunkCoord]);
            terrainChunks.Remove(chunkCoord);
        }
    }
    
    void GenerateChunk(Vector2Int chunkCoord)
    {
        GameObject chunkObj = new GameObject($"Chunk_{chunkCoord.x}_{chunkCoord.y}");
        chunkObj.transform.parent = transform;
        
        Terrain terrain = chunkObj.AddComponent<Terrain>();
        TerrainCollider collider = chunkObj.AddComponent<TerrainCollider>();
        
        TerrainData terrainData = new TerrainData();
        terrainData.heightmapResolution = heightMapResolution;
        terrainData.size = new Vector3(chunkSize, terrainHeight, chunkSize);
        
        // Generate heights
        float[,] heights = GenerateHeights(chunkCoord);
        terrainData.SetHeights(0, 0, heights);
        
        terrain.terrainData = terrainData;
        collider.terrainData = terrainData;
        
        // Position chunk
        Vector3 position = new Vector3(chunkCoord.x * chunkSize, 0, chunkCoord.y * chunkSize);
        chunkObj.transform.position = position;
        
        terrainChunks[chunkCoord] = chunkObj;
    }
    
    float[,] GenerateHeights(Vector2Int chunkCoord)
    {
        float[,] heights = new float[heightMapResolution, heightMapResolution];
        
        for (int z = 0; z < heightMapResolution; z++)
        {
            for (int x = 0; x < heightMapResolution; x++)
            {
                // World coordinates
                float worldX = chunkCoord.x * chunkSize + x;
                float worldZ = chunkCoord.y * chunkSize + z;
                
                float noiseValue = GeneratePerlinNoise(worldX, worldZ);
                heights[x, z] = (noiseValue + 1f) / 2f; // Normalize to 0-1
            }
        }
        
        return heights;
    }
    
    float GeneratePerlinNoise(float x, float z)
    {
        float amplitude = 1f;
        float frequency = 1f;
        float noiseHeight = 0f;
        
        for (int i = 0; i < octaves; i++)
        {
            float sampleX = x / noiseScale * frequency + seed;
            float sampleZ = z / noiseScale * frequency + seed;
            
            float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2f - 1f;
            
            noiseHeight += perlinValue * amplitude;
            
            amplitude *= persistence;
            frequency *= lacunarity;
        }
        
        return noiseHeight;
    }
    
    Vector2Int GetChunkCoord(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / chunkSize),
            Mathf.FloorToInt(position.z / chunkSize)
        );
    }
    
    [ContextMenu("Clear All Chunks")]
    public void ClearAllChunks()
    {
        foreach (var chunk in terrainChunks.Values)
        {
            Destroy(chunk);
        }
        terrainChunks.Clear();
        currentPlayerChunk = new Vector2Int(int.MaxValue, int.MaxValue);
    }
    
    [ContextMenu("Regenerate Chunks")]
    public void RegenerateChunks()
    {
        seed = Random.Range(0f, 10000f);
        ClearAllChunks();
        UpdateChunks();
    }
}