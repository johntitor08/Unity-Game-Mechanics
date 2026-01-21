using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TerrainTexturePainter : MonoBehaviour
{
    [System.Serializable]
    public class TerrainLayer
    {
        public string name;
        public TerrainLayer layer;
        [Range(0f, 1f)]
        public float minHeight = 0f;
        [Range(0f, 1f)]
        public float maxHeight = 1f;
        [Range(0f, 90f)]
        public float minSlope = 0f;
        [Range(0f, 90f)]
        public float maxSlope = 90f;
        public float blendStrength = 0.1f;
    }
    
    [Header("Texture Layers (Bottom to Top)")]
    public TerrainLayer[] terrainLayers;
    
    [Header("Settings")]
    public bool paintOnStart = true;
    
    private Terrain terrain;
    private TerrainData terrainData;
    
    void Start()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        
        if (paintOnStart)
        {
            PaintTextures();
        }
    }
    
    [ContextMenu("Paint Textures")]
    public void PaintTextures()
    {
        if (terrainLayers == null || terrainLayers.Length == 0)
        {
            Debug.LogWarning("No terrain layers assigned!");
            return;
        }
        
        // Set up terrain layers
        UnityEngine.TerrainLayer[] layers = new UnityEngine.TerrainLayer[terrainLayers.Length];
        for (int i = 0; i < terrainLayers.Length; i++)
        {
            layers[i] = terrainLayers[i].layer;
        }
        terrainData.terrainLayers = layers;
        
        // Get terrain dimensions
        int alphaMapWidth = terrainData.alphamapWidth;
        int alphaMapHeight = terrainData.alphamapHeight;
        int numLayers = terrainLayers.Length;
        
        // Create alphamap array
        float[,,] alphaMap = new float[alphaMapWidth, alphaMapHeight, numLayers];
        
        for (int y = 0; y < alphaMapHeight; y++)
        {
            for (int x = 0; x < alphaMapWidth; x++)
            {
                // Get normalized position
                float xNorm = x / (float)alphaMapWidth;
                float yNorm = y / (float)alphaMapHeight;
                
                // Get height at this position
                float height = terrainData.GetHeight(
                    Mathf.RoundToInt(xNorm * terrainData.heightmapResolution),
                    Mathf.RoundToInt(yNorm * terrainData.heightmapResolution)
                );
                float normalizedHeight = height / terrainData.size.y;
                
                // Get steepness (slope)
                float steepness = terrainData.GetSteepness(xNorm, yNorm);
                
                // Calculate weights for each layer
                float[] weights = new float[numLayers];
                
                for (int i = 0; i < numLayers; i++)
                {
                    weights[i] = CalculateLayerWeight(
                        normalizedHeight,
                        steepness,
                        terrainLayers[i]
                    );
                }
                
                // Normalize weights
                float totalWeight = 0f;
                for (int i = 0; i < numLayers; i++)
                {
                    totalWeight += weights[i];
                }
                
                if (totalWeight > 0f)
                {
                    for (int i = 0; i < numLayers; i++)
                    {
                        alphaMap[x, y, i] = weights[i] / totalWeight;
                    }
                }
                else
                {
                    // Default to first layer if no weights
                    alphaMap[x, y, 0] = 1f;
                }
            }
        }
        
        terrainData.SetAlphamaps(0, 0, alphaMap);
        Debug.Log("Terrain textures painted successfully!");
    }
    
    float CalculateLayerWeight(float height, float slope, TerrainLayer layer)
    {
        // Check height range
        float heightWeight = 0f;
        if (height >= layer.minHeight && height <= layer.maxHeight)
        {
            // Calculate blend based on distance from edges
            float heightCenter = (layer.minHeight + layer.maxHeight) / 2f;
            float heightRange = (layer.maxHeight - layer.minHeight) / 2f;
            
            if (heightRange > 0f)
            {
                float heightDistance = Mathf.Abs(height - heightCenter) / heightRange;
                heightWeight = 1f - Mathf.Clamp01(heightDistance / layer.blendStrength);
            }
            else
            {
                heightWeight = 1f;
            }
        }
        
        // Check slope range
        float slopeWeight = 0f;
        if (slope >= layer.minSlope && slope <= layer.maxSlope)
        {
            float slopeCenter = (layer.minSlope + layer.maxSlope) / 2f;
            float slopeRange = (layer.maxSlope - layer.minSlope) / 2f;
            
            if (slopeRange > 0f)
            {
                float slopeDistance = Mathf.Abs(slope - slopeCenter) / slopeRange;
                slopeWeight = 1f - Mathf.Clamp01(slopeDistance / layer.blendStrength);
            }
            else
            {
                slopeWeight = 1f;
            }
        }
        
        // Combine weights (both conditions must be met)
        return heightWeight * slopeWeight;
    }
    
    [ContextMenu("Create Default Layers")]
    void CreateDefaultLayers()
    {
        terrainLayers = new TerrainLayer[4];
        
        // Water/Beach
        terrainLayers[0] = new TerrainLayer
        {
            name = "Sand/Beach",
            minHeight = 0f,
            maxHeight = 0.2f,
            minSlope = 0f,
            maxSlope = 20f,
            blendStrength = 0.1f
        };
        
        // Grass
        terrainLayers[1] = new TerrainLayer
        {
            name = "Grass",
            minHeight = 0.15f,
            maxHeight = 0.6f,
            minSlope = 0f,
            maxSlope = 30f,
            blendStrength = 0.15f
        };
        
        // Rock/Cliff
        terrainLayers[2] = new TerrainLayer
        {
            name = "Rock/Cliff",
            minHeight = 0f,
            maxHeight = 1f,
            minSlope = 25f,
            maxSlope = 90f,
            blendStrength = 0.1f
        };
        
        // Snow/Mountain
        terrainLayers[3] = new TerrainLayer
        {
            name = "Snow/Mountain",
            minHeight = 0.5f,
            maxHeight = 1f,
            minSlope = 0f,
            maxSlope = 40f,
            blendStrength = 0.15f
        };
        
        Debug.Log("Default terrain layers created. Assign TerrainLayer assets in inspector.");
    }
}