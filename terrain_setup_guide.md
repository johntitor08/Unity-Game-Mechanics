# Unity 3D Terrain Generation - Complete Setup Guide

## What I've Improved

Your original code had some good basics! I've enhanced it with:

✓ **Proper normalization** - Heights now correctly map to 0-1 range
✓ **Island mode** - Optional falloff for creating islands
✓ **Height curves** - Control terrain shape with animation curves
✓ **Chunk system** - Load/unload terrain chunks for infinite worlds
✓ **Texture painting** - Automatic texture based on height/slope
✓ **Better performance** - Optimized calculations and memory usage

---

## Option 1: Single Large Terrain (Your Original Approach)

### Setup Steps

1. **Create Terrain GameObject**
   - Create new GameObject
   - Add `Terrain` component (Unity adds TerrainCollider automatically)
   - Add `TerrainGeneration.cs` script

2. **Configure Settings**
   ```
   World Size: 512
   Height Map Resolution: 513 (must be power of 2 + 1)
   Terrain Height: 50
   Noise Scale: 20
   Octaves: 4
   Persistence: 0.5
   Lacunarity: 2.0
   ```

3. **Optional: Add Height Curve**
   - Create new Animation Curve in inspector
   - Shape it like this for realistic terrain:
     - Start: (0, 0)
     - Middle: (0.5, 0.4)
     - End: (1, 1)
   - This creates flatter lowlands and steeper mountains

4. **Optional: Enable Island Mode**
   - Check `Use Island Mode`
   - Adjust `Falloff Strength` (3.0 is good start)

5. **Press Play** - Terrain generates automatically!

---

## Option 2: Infinite Chunk-Based Terrain (Better for Large Worlds)

### Setup Steps

1. **Create Terrain Manager**
   - Create empty GameObject named "TerrainManager"
   - Add `ChunkTerrainManager.cs` script

2. **Configure Settings**
   ```
   Chunk Size: 64
   View Distance: 3 (loads 7x7 grid of chunks)
   Height Map Resolution: 65 (chunkSize + 1)
   Player: Assign your player/camera
   ```

3. **Setup Player**
   - Assign your player transform (or Main Camera)
   - Chunks will generate around this object

4. **Press Play** - Chunks generate as you move!

---

## Adding Textures to Terrain

### Create Terrain Layers

1. **In Project Window:**
   - Right-click → Create → Terrain Layer
   - Create 4 layers: Sand, Grass, Rock, Snow

2. **Configure Each Layer:**
   - Assign diffuse texture
   - Assign normal map (optional)
   - Set tiling (16x16 is good start)

### Apply Texture Painter

1. **Add Script to Terrain**
   - Add `TerrainTexturePainter.cs` to terrain object

2. **Setup Layers:**
   - Right-click script → Create Default Layers
   - Assign your TerrainLayer assets to each slot

3. **Configure Height/Slope Ranges:**
   ```
   Sand:
   - Height: 0.0 - 0.2
   - Slope: 0 - 20°
   
   Grass:
   - Height: 0.15 - 0.6
   - Slope: 0 - 30°
   
   Rock:
   - Height: 0 - 1.0
   - Slope: 25 - 90°
   
   Snow:
   - Height: 0.5 - 1.0
   - Slope: 0 - 40°
   ```

4. **Paint Textures:**
   - Right-click script → Paint Textures
   - Or check "Paint On Start"

---

## Key Improvements Over Original Code

### 1. Fixed Normalization
```csharp
// OLD: Assumed Perlin is 0-1, but it's not normalized
heights[x, y] = noiseValue;

// NEW: Properly normalize to 0-1 range
float normalizedHeight = Mathf.InverseLerp(minHeight, maxHeight, heights[x, y]);
```

### 2. Added Lacunarity
```csharp
// Controls how much frequency increases each octave
frequency *= lacunarity; // Default 2.0
```

### 3. World-Space Coordinates for Chunks
```csharp
// Ensures seamless chunk boundaries
float worldX = chunkCoord.x * chunkSize + x;
```

### 4. Falloff for Islands
```csharp
// Creates natural island shapes
float falloff = Mathf.Pow(distance, falloffStrength);
height = Mathf.Clamp01(height - falloff);
```

---

## Performance Tips

### For Large Terrains:
- Use chunk system with view distance 2-4
- Set height map resolution to 65-129
- Reduce octaves to 3-4
- Use LOD (Terrain has built-in LOD)

### For Detailed Terrains:
- Single terrain with resolution 513-1025
- Increase octaves to 5-6
- Lower noise scale (10-15)
- Add detail maps

---

## Common Settings Explained

**Noise Scale**: Higher = smoother, larger features (20-40 for rolling hills)

**Octaves**: More = more detail layers (4-6 recommended)

**Persistence**: Lower = smoother terrain (0.4-0.6 typical)

**Lacunarity**: Higher = more variation between octaves (2.0 standard)

**Height Multiplier**: Amplifies final height (0.5-1.5 range)

---

## Troubleshooting

**Terrain is too flat:**
- Increase octaves (4-6)
- Increase height multiplier
- Use height curve with steeper ends

**Terrain is too spiky:**
- Reduce octaves (2-3)
- Increase noise scale
- Lower persistence (0.3-0.4)

**Chunks have visible seams:**
- Ensure heightMapResolution = chunkSize + 1
- Use world-space coordinates for noise sampling

**Textures look wrong:**
- Check height/slope ranges don't overlap too much
- Increase blend strength for smoother transitions
- Verify terrain layers are assigned

**Performance issues:**
- Reduce view distance (2-3 chunks)
- Lower heightmap resolution (65-129)
- Use fewer octaves (3-4)

---

## Next Steps

1. **Add Trees/Vegetation**: Use Unity's terrain detail system
2. **Add Objects**: Procedurally place rocks, plants based on height/slope
3. **Add Water**: Place water plane at sea level (height 0-0.2)
4. **Add Caves**: Use 3D Perlin noise to carve caves
5. **Add Biomes**: Different noise settings per region
6. **Add Weather**: Fog, rain based on height
7. **Add Minimap**: Use noise texture as minimap

Enjoy your procedural terrain generation!