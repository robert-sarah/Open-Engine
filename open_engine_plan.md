# Open Engine - Agent-Based Simulation Game Engine

## Project Overview
A complete game engine for creating agent-based simulations with AI, inspired by:
- Unity-inspired Editor UI
- Visual relational/logic node graph
- Autonomous AI agents with offline unified LLM architecture
- Game Manager / Dungeon Master system for intent-to-action translation
- Memory & Soul system for agent personality and relationships
- Dynamic environment (weather, day/night cycle, resources)
- God Mode for divine interventions
- Multi-model LLM support (GPT, Nemotron, Ollama, etc.)
- Written in C# (.NET 8.0)

## Core Concepts from Video

### 1. Agent-Based Simulation
- Agents express intentions in natural language
- Game Manager translates intentions to deterministic actions
- Agents have Memory (experiences) and Soul (personality, relationships)
- Emergent behaviors: social bonds, conflicts, cooperation, gift-giving

### 2. Environment System
- Weather: rain, clear skies, storms
- Day/night cycle with time acceleration
- Resources: berries, meat, wood, flint, animal skins
- Animals: mammoths, rabbits, wolves
- Crafting: shelters, tools, weapons

### 3. Social Dynamics
- Family structures and tribal organization
- Emotional states: fear, joy, serenity, shame, anger
- Social behaviors: sharing, defending, romantic relationships
- Cultural emergence: different tribe behaviors

### 4. God Mode
- Divine interventions and commands
- Rewards and punishments
- Influence on agent behavior
- Benevolent vs hostile god approaches

---

## Phase 1: Project Structure
```
src/OpenEngine.Core/
├── Agents/              # Memory, Soul, AgentBrain, AI behaviors
├── Environment/         # Weather, DayNight, Resources, Ecosystems
├── GameManager/         # Intent translation, WorldState, Game rules
├── GodMode/             # Divine interventions, Debug tools
├── Social/              # Relationships, emotions, tribes, Economics
├── Crafting/            # Resources, inventory, recipes, Production chains
├── Physics/             # Rigidbody, Collisions, Forces, Joints
├── Animation/           # Skeletal animation, Blend trees, Anim controllers
├── Audio/               # 3D spatial audio, Music, SFX, Audio mixers
├── Rendering/           # Shaders, Lighting, PBR, Post-processing
├── Particles/           # Particle systems, VFX
├── Navigation/          # Pathfinding, Navigation mesh, AI steering
├── UI/                  # Canvas, UI elements, Event system
├── Networking/          # Multiplayer, Sync, RPC
├── Input/               # Keyboard, Mouse, Gamepad, Touch
├── Camera/              # Camera controllers, Cinemachine
├── Timeline/            # Cinematics, Sequences
├── Entities/            # SimEntity, SystemicConnection, Components
├── Engine/              # OpenSimulationEngine, Game loop
├── Intent/              # Intent parser, Natural language processing
└── Scripting/           # C# runtime compilation, Script components
```

---

## Phase 2: Agent Brain System

### 2.1 AgentMemory
- Stores experiences, important events, learned skills
- Methods: AddMemory, RecallMemories, ForgetOldMemories

### 2.2 AgentSoul  
- Personality traits, relationships, emotional states
- Emotions: Fear, Joy, Serenity, Shame, Anger (0-1)
- Methods: UpdateRelationship, GetCurrentEmotion

### 2.3 AgentBrain
- Combines memory and soul for intent generation
- Processes observations, generates natural language intentions

---

## Phase 3: Game Manager

### 3.1 GameManager
- Translates agent intents to deterministic actions
- Uses LLM for reasoning, applies heuristics for execution
- Provides natural language feedback to agents

### 3.2 WorldState
- Complete snapshot: entities, weather, time, resources
- Methods: GetVisibleContext, FindNearestResource

### 3.3 RulesEngine
- Pure deterministic: hunger, combat, crafting calculations

---

## Phase 4: Environment

### 4.1 WeatherSystem
- Types: Clear, Rain, Storm, Snow
- Affects fires, movement, visibility

### 4.2 DayNightCycle
- Time acceleration (configurable)
- Light levels, day/night events

### 4.3 ResourceManager
- Resources: Berries, Wood, Flint, Water
- Spawn rates, regeneration, depletion

### 4.4 AnimalSystem
- Animals: Mammoth, Rabbit, Wolf
- AI behaviors, hunting, attacks

---

## Phase 5: Crafting

### 5.1 CraftingSystem
- Recipes: Shelter, Axe, Basket, Bed
- Material combinations

### 5.2 InventorySystem
- Item management, capacity, weight

---

## Phase 6: Social Dynamics

### 6.1 RelationshipManager
- Family bonds, romantic relationships, friendships
- Relationship strength tracking

### 6.2 EmotionalSystem
- Dynamic emotional state updates
- Social behavior influence

### 6.3 TribalSystem
- Tribe organization, cultural emergence
- Leadership, social roles

---

## Phase 7: God Mode

### 7.1 GodModeController
- Divine commands, interventions
- Rewards and punishments
- Benevolent vs hostile approaches

### 7.2 DivineCommand
- Command types, execution, agent responses

---

## Phase 8: Multi-Model LLM Support

### 8.1 ModelManager
- Switch between GPT, Nemotron, Ollama, etc.
- Per-agent model assignment
- Performance comparison

---

## Implementation Order
1. Agent Brain (Memory, Soul, Brain)
2. Game Manager (translation system)
3. Environment (weather, day/night, resources)
4. Crafting system
5. Social dynamics
6. God Mode
7. Multi-model support
8. Physics Engine (rigidbody, collisions)
9. Animation System (skeletal, blend trees)
10. Audio System (3D spatial, music, SFX)
11. Rendering Engine (shaders, lighting, PBR)
12. Particle System (VFX)
13. Navigation System (pathfinding, navmesh)
14. UI System (canvas, events)
15. Input System (keyboard, mouse, gamepad)
16. Camera System (controllers, cinemachine)
17. Scripting System (C# runtime)
18. Networking (multiplayer)
19. Timeline/Cinematics
20. Save/Load System
21. Prefab System
22. Editor Enhancement (Scene view, Game view, Asset browser)
