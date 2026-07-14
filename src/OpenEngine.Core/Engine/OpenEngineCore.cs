// Created By Levi Enama
// Main Engine Core - Connects all systems
using System;
using System.Collections.Generic;
using OpenEngine.Core.Graphics;
using OpenEngine.Core.Physics;
using OpenEngine.Core.Audio;
using OpenEngine.Core.AI;
using OpenEngine.Core.Animation;
using OpenEngine.Core.Camera;
using OpenEngine.Core.Entities;
using OpenEngine.Core.Environment;
using OpenEngine.Core.GodMode;
using OpenEngine.Core.Social;
using OpenEngine.Core.Crafting;
using OpenEngine.Core.GameManager;
using OpenEngine.Core.Agents;
using OpenEngine.Core.Navigation;
using OpenEngine.Core.Particles;
using OpenEngine.Core.Rendering;
using OpenEngine.Core.UI;
using OpenEngine.Core.Input;
using OpenEngine.Core.Networking;
using OpenEngine.Core.Scripting;
using OpenEngine.Core.Serialization;
using OpenEngine.Core.Timeline;
using OpenEngine.Core.Localization;
using OpenEngine.Core.Analytics;
using OpenEngine.Core.Persistence;
using OpenEngine.Core.Platform;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Engine
{
    public class OpenEngineCore : IDisposable
    {
        private static OpenEngineCore _instance;
        private bool _disposed;
        private bool _isRunning;

        private SilkOpenGLRenderer _renderer;
        private RenderPipeline _renderPipeline;
        private LightingSystem _lightingSystem;
        private BepuPhysicsWrapper _physics;
        private SoLoudAudioWrapper _audio;
        private Pathfinding _pathfinding;
        private StateMachine _stateMachine;
        private BehaviorTree _behaviorTree;
        private AnimationController _animationController;
        private BlendTree _blendTree;
        private CameraController _cameraController;
        private CinemachineBrain _cinemachine;
        private Dictionary<string, SimEntity> _entities;
        private WeatherSystem _weatherSystem;
        private DayNightCycle _dayNightCycle;
        private ResourceManager _resourceManager;
        private AnimalSystem _animalSystem;
        private GodModeController _godModeController;
        private RelationshipManager _relationshipManager;
        private EmotionalSystem _emotionalSystem;
        private TribalSystem _tribalSystem;
        private CraftingSystem _craftingSystem;
        private InventorySystem _inventorySystem;
        private GameManager _gameManager;
        private WorldState _worldState;
        private RulesEngine _rulesEngine;
        private AgentMemory _agentMemory;
        private AgentSoul _agentSoul;
        private AgentBrain _agentBrain;
        private ParticleSystem _particleSystem;
        private Canvas _canvas;
        private InputSystem _inputSystem;
        private NetworkManager _networkManager;
        private ScriptEngine _scriptEngine;
        private SaveSystem _saveSystem;
        private Timeline _timeline;
        private LocalizationSystem _localizationSystem;
        private AnalyticsSystem _analyticsSystem;
        private SimulationPersistence _simulationPersistence;
        private Platform _platform;

        public static OpenEngineCore Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new OpenEngineCore();
                }
                return _instance;
            }
        }

        public bool IsRunning => _isRunning;
        public SilkOpenGLRenderer Renderer => _renderer;
        public BepuPhysicsWrapper Physics => _physics;
        public SoLoudAudioWrapper Audio => _audio;
        public Dictionary<string, SimEntity> Entities => _entities;
        public WeatherSystem Weather => _weatherSystem;
        public GodModeController GodMode => _godModeController;
        public CameraController Camera => _cameraController;
        public InputSystem Input => _inputSystem;
        public SaveSystem SaveSystem => _saveSystem;
        public Platform Platform => _platform;
        public OpenGLBindings NativeOpenGL => _nativeOpenGL;
        public VulkanBindings NativeVulkan => _nativeVulkan;
        public MetalBindings NativeMetal => _nativeMetal;
        public AssetManager Assets => _assetManager;

        private OpenEngineCore()
        {
            _entities = new Dictionary<string, SimEntity>();
        }

        public void Initialize()
        {
            Console.WriteLine("Initializing Open Engine Core...");
            _platform = Platform.Instance;
            
            // Initialize native bindings
            _nativeOpenGL = new OpenGLBindings();
            _nativeVulkan = new VulkanBindings();
            _nativeMetal = new MetalBindings();
            
            // Initialize threading
            _threadManager = ThreadManager.Instance;
            Initialize asset manager
            _assetManager = AssetManager.Instance;
            
            // 
            // Start background threads
            _threadManager.StartPhysicsThread(1f / 60f, PhysicsUpdateThread);
            _threadManager.StartAudioThread(AudioUpdateThread);
            _threadManager.StartAIThread(1f / 30f, AIUpdateThread);
            _threadManager.StartNetworkThread(NetworkUpdateThread);
            
            EngineEventBus.Instance.Log("Open Engine Core initialized successfully!");
            
            _renderer = new SilkOpenGLRenderer(null);
            _renderPipeline = new ForwardRenderPipeline();
            _lightingSystem = new LightingSystem();
            _physics = new BepuPhysicsWrapper();
            _physics.Initialize();
            _audio = new SoLoudAudioWrapper();
            _audio.Initialize();
            _pathfinding = new Pathfinding();
            _stateMachine = new StateMachine();
            _behaviorTree = new BehaviorTree(null);
            _animationController = new AnimationController();
            _blendTree = new BlendTree();
            _cameraController = new CameraController();
            _cinemachine = new CinemachineBrain();
            _weatherSystem = new WeatherSystem();
            _dayNightCycle = new DayNightCycle();
            _resourceManager = new ResourceManager();
            _animalSystem = new AnimalSystem();
            _godModeController = new GodModeController();
            _relationshipManager = new RelationshipManager();
            _emotionalSystem = new EmotionalSystem();
            _tribalSystem = new TribalSystem();
            _craftingSystem = new CraftingSystem();
            _inventorySystem = new InventorySystem();
            _gameManager = new GameManager();
            _worldState = new WorldState();
            _rulesEngine = new RulesEngine();
            _agentMemory = new AgentMemory();
            _agentSoul = new AgentSoul();
            _agentBrain = new AgentBrain();
            _particleSystem = new ParticleSystem();
            _canvas = new Canvas();
            _inputSystem = new InputSystem();
            _networkManager = new NetworkManager();
            _scriptEngine = new ScriptEngine();
            _saveSystem = new SaveSystem();
            _timeline = new Timeline();
            _localizationSystem = new LocalizationSystem();
            _analyticsSystem = new AnalyticsSystem();
            _simulationPersistence = new SimulationPersistence();
            Console.WriteLine("Open Engine Core initialized successfully!");
        }

        public void Start()
        {
            _isRunning = true;
            Console.WriteLine("Open Engine started!");
        }

        public void Update(float deltaTime)
        {
            if (!_isRunning) return;
            Time.Update(deltaTime);
            _physics?.Update(deltaTime);
            _audio?.Update();
            _inputSystem?.Update();
            _cameraController?.Update(deltaTime);
            _cinemachine?.Update(deltaTime, Math.Vector3.Zero, Quaternion.Identity);
            _weatherSystem?.Update(deltaTime);
            _dayNightCycle?.Update(deltaTime);
            _resourceManager?.Update(deltaTime);
            _animalSystem?.Update(deltaTime);
            foreach (var entity in _entities.Values)
            {
                entity?.Update(deltaTime);
            }
            _stateMachine?.Update();
            if (_behaviorTree != null && _behaviorTree.Entity != null)
            {
                _behaviorTree.Update();
            }
            _animationController?.Update(deltaTime);
            _particleSystem?.Update(deltaTime);
            _canvas?.Update(deltaTime);
            _networkManager?.Update(deltaTime);
            _scriptEngine?.Update(deltaTime);
            _godModeController?.Update(deltaTime);
            _emotionalSystem?.Update(deltaTime);
            _tribalSystem?.Update(deltaTime);
            _gameManager?.Update(deltaTime);
        }

        public void Render()
        {
            if (!_isRunning) return;
            _renderer?.Clear();
            _renderPipeline?.Render(_renderer);
            _canvas?.Render();
        }

        public void AddEntity(SimEntity entity)
        {
            if (entity != null && !_entities.ContainsKey(entity.Id))
            {
                _entities[entity.Id] = entity;
            }
        }

        public void RemoveEntity(string entityId)
        {
            _entities.Remove(entityId);
        }

        public SimEntity GetEntity(string entityId)
        {
            _entities.TryGetValue(entityId, out var entity);
            return entity;
        }

        public void Shutdown()
        {
            _isRunning = false;
            Console.WriteLine("Open Engine shutting down...");
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Shutdown();
                _renderer?.Dispose();
                _physics?.Dispose();
                _audio?.Dispose();
                _networkManager?.Dispose();
                _scriptEngine?.Dispose();
                _disposed = true;
            }
        }

        // Background thread update methods
        private void PhysicsUpdateThread(float deltaTime)
        {
            _physics?.Update(deltaTime);
        }

        private void AudioUpdateThread()
        {
            _audio?.Update();
        }

        private void AIUpdateThread(float deltaTime)
        {
            _stateMachine?.Update();
            if (_behaviorTree != null && _behaviorTree.Entity != null)
            {
                _behaviorTree.Update();
            }
        }

        private void NetworkUpdateThread()
        {
            _networkManager?.Update(0.016f);
        }
    }
}
