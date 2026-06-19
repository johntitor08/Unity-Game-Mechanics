using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TerrainGeneration : MonoBehaviour
{
    [Header("World Settings")]
    public int worldSize = 512;
    public int heightMapResolution = 513; // Must be power of 2 + 1
    public float terrainHeight = 50f;
    
    [Header("Noise Settings")]
    public float noiseScale = 20f;
    public int octaves = 4;
    [Range(0f, 1f)]
    public float persistence = 0.5f;
    public float lacunarity = 2f;
    public float seed = 42f;
    
    [Header("Height Adjustment")]
    public AnimationCurve heightCurve;
    public float heightMultiplier = 1f;
    
    [Header("Island Settings")]
    public bool useIslandMode = true;
    public float falloffStrength = 3f;
    
    [Header("Debug")]
    public bool generateNoiseTexture = false;
    public Texture2D noiseTexture;
    
    private Terrain terrain;
    private TerrainData terrainData;
    
    void Start()
    {
        GenerateTerrain();
    }
    
    void GenerateTerrain()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
        
        // Set terrain data properties
        terrainData.heightmapResolution = heightMapResolution;
        terrainData.size = new Vector3(worldSize, terrainHeight, worldSize);
        
        // Generate heightmap
        float[,] heights = GenerateHeights();
        terrainData.SetHeights(0, 0, heights);
        
        // Optional: Generate debug texture
        if (generateNoiseTexture)
        {
            GenerateNoiseTexture(heights);
        }
        
        Debug.Log("Terrain generated successfully!");
    }
    
    float[,] GenerateHeights()
    {
        int resolution = terrainData.heightmapResolution;
        float[,] heights = new float[resolution, resolution];
        
        // Generate falloff map if island mode is enabled
        float[,] falloffMap = useIslandMode ? GenerateFalloffMap(resolution) : null;
        
        // Find min/max for normalization
        float minHeight = float.MaxValue;
        float maxHeight = float.MinValue;
        
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float noiseValue = GeneratePerlinNoise(x, y, resolution);
                
                if (noiseValue < minHeight) minHeight = noiseValue;
                if (noiseValue > maxHeight) maxHeight = noiseValue;
                
                heights[x, y] = noiseValue;
            }
        }
        
        // Normalize and apply falloff
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // Normalize to 0-1
                float normalizedHeight = Mathf.InverseLerp(minHeight, maxHeight, heights[x, y]);
                
                // Apply height curve if set
                if (heightCurve != null && heightCurve.keys.Length > 0)
                {
                    normalizedHeight = heightCurve.Evaluate(normalizedHeight);
                }
                
                // Apply falloff for island mode
                if (falloffMap != null)
                {
                    normalizedHeight = Mathf.Clamp01(normalizedHeight - falloffMap[x, y]);
                }
                
                // Apply height multiplier
                heights[x, y] = normalizedHeight * heightMultiplier;
            }
        }
        
        return heights;
    }
    
    float GeneratePerlinNoise(int x, int y, int resolution)
    {
        float amplitude = 1f;
        float frequency = 1f;
        float noiseHeight = 0f;
        
        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (x / (float)resolution) * noiseScale * frequency + seed;
            float sampleY = (y / (float)resolution) * noiseScale * frequency + seed;
            
            // Perlin noise returns 0-1, convert to -1 to 1
            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f;
            
            noiseHeight += perlinValue * amplitude;
            
            amplitude *= persistence;
            frequency *= lacunarity;
        }
        
        return noiseHeight;
    }
    
    float[,] GenerateFalloffMap(int size)
    {
        float[,] falloffMap = new float[size, size];
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Normalize coordinates to -1 to 1
                float xv = x / (float)size * 2f - 1f;
                float yv = y / (float)size * 2f - 1f;
                
                // Calculate distance from center
                float value = Mathf.Max(Mathf.Abs(xv), Mathf.Abs(yv));
                
                // Apply falloff curve
                falloffMap[x, y] = Evaluate(value);
            }
        }
        
        return falloffMap;
    }
    
    float Evaluate(float value)
    {
        float a = falloffStrength;
        float b = 2.2f;
        
        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
    
    void GenerateNoiseTexture(float[,] heights)
    {
        int resolution = terrainData.heightmapResolution;
        
        if (noiseTexture == null || noiseTexture.width != resolution)
        {
            noiseTexture = new Texture2D(resolution, resolution);
        }
        
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float value = heights[x, y];
                noiseTexture.SetPixel(x, y, new Color(value, value, value));
            }
        }
        
        noiseTexture.Apply();
        
        // Save texture as asset (optional)
        #if UNITY_EDITOR
        byte[] bytes = noiseTexture.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "/NoiseTexture.png", bytes);
        Debug.Log("Noise texture saved to Assets/NoiseTexture.png");
        #endif
    }
    
    // Call this from inspector or button
    [ContextMenu("Regenerate Terrain")]
    public void RegenerateTerrain()
    {
        seed = Random.Range(0f, 10000f);
        GenerateTerrain();
    }
}