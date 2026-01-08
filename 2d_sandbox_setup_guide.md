# Terraria-Style 2D Sandbox Game - Unity Setup Guide

## Quick Setup Instructions

### 1. Create Block Prefabs
1. Create 4 empty GameObjects and name them: `DirtBlock`, `StoneBlock`, `GrassBlock`, `OreBlock`
2. For each block:
   - Add a **SpriteRenderer** component
   - Add a **BoxCollider2D** component
   - Add the **Block.cs** script
   - Set appropriate sprite and color:
     - Dirt: Brown (#8B4513)
     - Stone: Gray (#808080)
     - Grass: Green (#228B22)
     - Ore: Gold (#FFD700)
   - Set BlockType in the Block component
   - Save as prefab

### 2. Create World Generator
1. Create empty GameObject named "WorldGenerator"
2. Add **WorldGenerator.cs** script
3. Assign the 4 block prefabs to the script fields
4. Adjust world settings (default: 200x100)

### 3. Create Player
1. Create GameObject named "Player"
2. Add **SpriteRenderer** (any color/sprite)
3. Add **Rigidbody2D**:
   - Gravity Scale: 3
   - Freeze Rotation Z: ✓
4. Add **BoxCollider2D**
5. Add **PlayerController.cs** script
6. Add **Inventory.cs** script
7. Assign block prefabs to Inventory component
8. Assign Main Camera reference

### 4. Camera Setup
1. Select Main Camera
2. Set Projection to **Orthographic**
3. Set Size to 10-15
4. Background color: Sky blue (#87CEEB)

### 5. Project Settings
1. **Physics2D** (Edit → Project Settings → Physics 2D):
   - Gravity Y: -20

## Controls

- **WASD/Arrow Keys**: Move left/right
- **Space**: Jump
- **Left Mouse**: Mine blocks (hold)
- **Right Mouse**: Place blocks
- **1-5 Keys**: Select inventory slot
- **Mouse Wheel**: Cycle through inventory

## Features

✓ Procedural terrain generation with Perlin noise
✓ Cave generation
✓ Mining with durability system
✓ Block placing
✓ Inventory system (10 slots)
✓ Player movement and jumping
✓ Camera following

## Customization Tips

### Adjust Terrain
- `terrainFrequency`: Lower = smoother hills
- `terrainAmplitude`: Higher = more dramatic height variation
- `caveFrequency`: Higher = smaller caves
- `caveThreshold`: Higher = fewer caves

### Improve Visuals
- Import sprite sheets for blocks
- Add particle effects when mining
- Add background parallax layers
- Create animated character sprite

### Add Features
- Health system
- Enemies and combat
- Crafting system
- Multiple biomes
- Day/night cycle
- Lighting system
- NPCs

## Troubleshooting

**Player falls through world**: Check that block prefabs have BoxCollider2D components

**Can't mine blocks**: Ensure blocks have the Block.cs script and Layer is set to Default

**Camera doesn't follow**: Assign Main Camera to PlayerController in inspector

**No blocks appearing**: Check that prefabs are assigned to WorldGenerator

Enjoy building your Terraria-style game!
