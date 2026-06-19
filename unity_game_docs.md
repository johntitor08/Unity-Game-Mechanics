# Unity Game System Documentation

## System Overview

This is a Unity-based narrative RPG game featuring time phases, inventory management, character statistics, dialogue system, and save/load functionality. The architecture follows a singleton pattern for manager classes with event-driven communication between systems.

## Core Systems

### Time Phase System

The game operates on a four-phase daily cycle that affects game world states.

**TimePhase.cs** - Defines four distinct phases:
- Morning
- Noon
- Evening
- Night

**TimePhaseManager.cs** - Singleton manager controlling phase transitions:
- Broadcasts `OnPhaseChanged` event when phase updates
- Provides `NextPhase()` for cycling through phases sequentially
- Provides `SetPhase()` for direct phase assignment
- Automatically triggers save on phase change
- Persists across scene loads via `DontDestroyOnLoad`

**PhaseLockedObject.cs** - Component for phase-dependent visibility:
- Activates/deactivates GameObjects based on current phase
- Automatically subscribes to phase change events
- Example use: Objects that only appear at specific times of day

**MarketController.cs** - Example implementation showing conditional UI:
- Displays different UI based on whether market is open
- Market opens during Morning and Noon phases
- Demonstrates event-driven UI response to phase changes

### Inventory System

A flexible item management system supporting various item types with metadata.

**ItemData.cs** - ScriptableObject defining item properties:
- Unique identifier (`itemID`)
- Display information (name, icon, description)
- Usability flag and stat modification values
- UnityEvent for custom use effects

**ItemDatabase.cs** - Central registry for all items:
- Singleton pattern for global access
- Dictionary-based lookups by ID
- Validates unique item IDs on initialization
- Provides error logging for missing items

**InventoryManager.cs** - Handles item storage and quantity tracking:
- Dictionary-based storage (ID → quantity)
- `OnChanged` event for UI updates
- Add/Remove operations with automatic event triggering
- Maintains reference to ItemData for each stored item
- Singleton pattern with scene persistence

**WorldItem.cs** - Pickable items in the game world:
- Implements `IPointerClickHandler` for mouse interaction
- Requires Collider2D component
- Automatically adds to inventory and destroys on pickup
- Configurable quantity per pickup

**InventoryUI.cs** - Dynamic inventory interface:
- Object pooling pattern for item slots
- Responds to inventory changes via events
- Toggle visibility with "I" key
- Automatically creates/reuses UI slots as needed

**ItemSlot.cs** - Individual inventory slot representation:
- Displays item icon, name, and quantity
- Clickable to show detailed information
- Bridges between InventoryManager and ItemDetailPanel

**ItemDetailPanel.cs** - Modal showing item details:
- Displays full item information
- Conditional "Use" button based on item type
- Handles item consumption and updates
- Automatically closes when item depleted

### Player Statistics System

A generic stat system for tracking player attributes.

**StatType.cs** - Enumeration of available stats:
- Health
- Energy
- Charisma
- Strength
- Intelligence

**PlayerStats.cs** - Manages player attributes:
- Serializable Stat class with base and current values
- Dictionary for fast stat lookups
- `Modify()` method with automatic save triggering
- `Get()` method for reading current values
- Singleton pattern with persistence

### Dialogue System

A node-based dialogue system supporting branching conversations with conditions.

**DialogueNode.cs** - ScriptableObject containing:
- Array of text lines for sequential display
- Array of DialogueChoice objects for branching
- `onEnter` UnityEvent for custom logic on node entry

**DialogueChoice.cs** - Defines conversation branches:
- Display text for the choice
- Reference to next DialogueNode
- Conditional requirements (flags, items)
- Effects (consume items, set flags)

**DialogueManager.cs** - Orchestrates dialogue flow:
- Singleton managing dialogue state
- Typewriter effect integration for text display
- Choice filtering based on requirements
- Input handling (Space/Click to advance)
- Automatic choice display after line completion
- Clears and rebuilds choice buttons dynamically

**Typewriter.cs** - Text animation component:
- Coroutine-based character-by-character display
- Configurable speed
- Skip functionality to complete immediately
- Status checking via `IsTyping` property

### Save/Load System

A JSON-based persistence system coordinating all game state.

**SaveData.cs** - Serializable data container:
- Inventory (parallel arrays for IDs and counts)
- Current scene name
- Story flags collection
- Current time phase
- Player stats (parallel arrays for types and values)

**SaveSystem.cs** - Static class handling serialization:
- `SaveGame()` collects data from all managers and writes JSON
- `LoadGame()` reads JSON and distributes to managers
- Uses Unity's JsonUtility for serialization
- Stores at `Application.persistentDataPath + "/save.json"`
- Scene loading integration
- Handles missing save files gracefully

**StoryFlags.cs** - Persistent boolean flags:
- Static HashSet for flag storage
- Simple Add/Has interface
- Used for tracking story progress and unlocking content

## System Integration

### Event Flow

1. **Phase Change**: TimePhaseManager broadcasts → PhaseLockedObjects/MarketController update → SaveSystem persists
2. **Inventory Change**: InventoryManager modifies → InventoryUI refreshes → SaveSystem persists
3. **Stat Modification**: PlayerStats updates → SaveSystem persists
4. **Dialogue Choice**: DialogueManager processes → Flags/Inventory affected → Cascading updates

### Singleton Dependencies

Most systems follow this initialization pattern:
- Awake: Establish singleton instance, configure DontDestroyOnLoad
- Start: Subscribe to events, perform initial state checks

Critical initialization order:
1. ItemDatabase (provides item lookups)
2. InventoryManager (depends on ItemDatabase)
3. TimePhaseManager (independent)
4. PlayerStats (independent)
5. UI Systems (depend on corresponding managers)

### Save/Load Flow

**Saving**: Triggered by TimePhaseManager phase changes and PlayerStats modifications → Collects all state → Writes JSON

**Loading**: Called explicitly → Reads JSON → Updates all managers → Loads saved scene

Note: LoadGame should be called after all singleton managers are initialized to prevent null references.

## Architecture Patterns

- **Singleton Pattern**: Used extensively for manager classes to ensure single instance and global access
- **Observer Pattern**: Event system (`OnPhaseChanged`, `OnChanged`) for decoupled communication
- **ScriptableObject Pattern**: Used for data definitions (ItemData, DialogueNode) enabling designer-friendly workflows
- **Object Pooling**: InventoryUI reuses slot GameObjects for performance

## Profile System

Manages player progression, experience, and currency.

**PlayerProfile.cs** - Data container for player information:
- Player name, level, experience tracking
- Experience curve (1.5x multiplier per level)
- Currency (gold) management
- Profile icon ID for customization

**ProfileManager.cs** - Singleton managing player progression:
- `AddExperience()` with automatic level-up handling
- `SpendCurrency()` / `AddCurrency()` with validation
- Auto-increases stats on level up (+10 Health, +5 Energy, +2 Str/Int)
- Events: `OnProfileChanged`, `OnLevelUp`, `OnCurrencyChanged`
- Integrates with SaveSystem

**ProfileUI.cs** - Displays player information:
- Shows name, level, currency, experience bar
- Displays all current stats from PlayerStats
- Toggle with "P" key
- Real-time updates via event subscription

## Shop System

A flexible shop system supporting stock management, requirements, and pricing.

**ShopItemData.cs** - ScriptableObject defining shop listings:
- References ItemData and sets price
- Stock management (unlimited or limited quantity)
- Level requirements
- Story flag requirements for unlocking

**ShopManager.cs** - Handles shop transactions:
- `CanBuy()` validates all purchase requirements
- `BuyItem()` processes transaction with currency and stock
- Maintains stock dictionary for limited items
- Integrates with ProfileManager for currency
- Singleton pattern with persistence

**ShopUI.cs** - Shop interface:
- Dynamic slot creation for shop items
- Displays item info, price, and stock
- Real-time currency updates
- Opens/closes shop panel
- `OpenShop()` / `CloseShop()` methods

**ShopSlot.cs** - Individual shop item display:
- Shows item icon, name, price, and stock
- Buy button with requirement validation
- Grayed out when requirements not met
- Refreshes on purchase

## Turn-Based Combat System

A strategic turn-based combat system with actions, defense mechanics, and rewards.

**CombatAction.cs** - Defines player abilities:
- Action name, damage, energy cost
- Stat scaling (uses Strength/Intelligence for damage calculation)
- Defensive actions with defense bonus
- Configured in CombatManager inspector

**EnemyData.cs** - ScriptableObject for enemy configuration:
- Enemy stats (name, health, attack, defense)
- Sprite for visual representation
- Experience and currency rewards
- Loot tables with drop chances (parallel arrays)

**CombatManager.cs** - Orchestrates combat flow:
- `StartCombat()` initializes combat state
- `PlayerAction()` handles player turn with energy costs
- `EnemyTurn()` automated enemy AI (attacks after 1 second delay)
- Defense bonus system (reduces incoming damage, decays by 2 per turn)
- Victory: awards XP, currency, rolls for loot
- Defeat: currency penalty (25% loss, max 50), health restoration
- Combat log system via `OnCombatLog` event
- Stat-scaled damage calculation (base damage + stat/5)

**CombatUI.cs** - Combat interface:
- Displays enemy sprite, name, and health bar
- Shows player health and energy bars
- Dynamic action button generation
- Combat log with 10-line history
- Disables action buttons when insufficient energy
- Auto-hides when combat ends

**CombatTrigger.cs** - World integration:
- Attach to GameObjects with Collider2D
- Triggers combat on player collision
- Deactivates after triggering (one-time encounter)
- Tag-based detection (requires "Player" tag)

### Combat Flow

1. Player enters trigger → Combat starts
2. Player selects action → Energy consumed, effect applied
3. Enemy turn executes after 1 second delay
4. Repeat until victory or defeat
5. Rewards distributed / penalties applied
6. Combat ends, energy restored (+50)

## Extension Points

- **New Stats**: Add entries to StatType enum and configure in PlayerStats inspector
- **New Items**: Create ItemData ScriptableObjects and add to ItemDatabase list
- **New Dialogue**: Create DialogueNode ScriptableObjects and connect via choices array
- **Phase-Dependent Behavior**: Attach PhaseLockedObject or subscribe to TimePhaseManager.OnPhaseChanged
- **Custom Item Effects**: Configure onUse UnityEvent in ItemData inspector
- **New Shop Items**: Create ShopItemData ScriptableObjects with pricing and requirements
- **New Enemies**: Create EnemyData ScriptableObjects with stats and loot tables
- **New Combat Actions**: Add CombatAction entries to CombatManager inspector

## Technical Notes

- Turkish language strings present in UI (e.g., "Sahip olunan", "Ayný itemID")
- No multiplayer or networking components
- 2D game architecture (Collider2D usage)
- Requires TextMeshPro package
- Save file is plain JSON (not encrypted)
- Combat uses Unity's Invoke for turn delays (1 second between turns)
- Experience curve: baseXP × 1.5^level