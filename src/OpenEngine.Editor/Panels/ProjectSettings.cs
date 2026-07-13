// Created By Levi Enama
using System;
using System.Collections.Generic;

namespace OpenEngine.Editor.Panels
{
    public class ProjectSettings
    {
        public string ProjectName { get; set; }
        public string ProjectPath { get; set; }
        public string CompanyName { get; set; }
        public string ProductName { get; set; }
        public string Version { get; set; }
        public BuildSettings BuildSettings { get; set; }
        public QualitySettings QualitySettings { get; set; }
        public PhysicsSettings PhysicsSettings { get; set; }
        public AudioSettings AudioSettings { get; set; }
        public GraphicsSettings GraphicsSettings { get; set; }
        public InputSettings InputSettings { get; set; }
        public TagManager TagManager { get; set; }
        public LayerManager LayerManager { get; set; }

        public ProjectSettings()
        {
            ProjectName = "New Project";
            ProjectPath = "";
            CompanyName = "DefaultCompany";
            ProductName = "MyGame";
            Version = "1.0.0";
            BuildSettings = new BuildSettings();
            QualitySettings = new QualitySettings();
            PhysicsSettings = new PhysicsSettings();
            AudioSettings = new AudioSettings();
            GraphicsSettings = new GraphicsSettings();
            InputSettings = new InputSettings();
            TagManager = new TagManager();
            LayerManager = new LayerManager();
        }
    }

    public class BuildSettings
    {
        public string BuildTarget { get; set; }
        public string OutputPath { get; set; }
        public bool DevelopmentBuild { get; set; }
        public bool AutoconnectProfiler { get; set; }
        public bool DeepProfilingSupport { get; set; }
        public bool CompressionEnabled { get; set; }
        public List<string> Scenes { get; set; }

        public BuildSettings()
        {
            BuildTarget = "Windows";
            OutputPath = "Build";
            DevelopmentBuild = false;
            AutoconnectProfiler = false;
            DeepProfilingSupport = false;
            CompressionEnabled = true;
            Scenes = new List<string>();
        }
    }

    public class QualitySettings
    {
        public int CurrentQuality { get; set; }
        public List<QualityLevel> QualityLevels { get; set; }

        public QualitySettings()
        {
            CurrentQuality = 2;
            QualityLevels = new List<QualityLevel>
            {
                new QualityLevel("Low", 0),
                new QualityLevel("Medium", 1),
                new QualityLevel("High", 2),
                new QualityLevel("Ultra", 3)
            };
        }
    }

    public class QualityLevel
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public int PixelLightCount { get; set; }
        public float ShadowDistance { get; set; }
        public int ShadowResolution { get; set; }
        public int ShadowCascades { get; set; }
        public bool SoftVegetation { get; set; }
        public float SoftParticles { get; set; }
        public bool RealtimeReflectionProbes { get; set; }
        public int AntiAliasing { get; set; }

        public QualityLevel(string name, int index)
        {
            Name = name;
            Index = index;
            PixelLightCount = 2;
            ShadowDistance = 50f;
            ShadowResolution = 1;
            ShadowCascades = 1;
            SoftVegetation = false;
            SoftParticles = 0f;
            RealtimeReflectionProbes = false;
            AntiAliasing = 0;
        }
    }

    public class PhysicsSettings
    {
        public float Gravity { get; set; }
        public int DefaultSolverIterations { get; set; }
        public int DefaultSolverVelocityIterations { get; set; }
        public int FixedTimestep { get; set; }
        public int MaximumTimestep { get; set; }
        public bool AutoSyncTransforms { get; set; }

        public PhysicsSettings()
        {
            Gravity = 9.81f;
            DefaultSolverIterations = 6;
            DefaultSolverVelocityIterations = 1;
            FixedTimestep = 20;
            MaximumTimestep = 33333;
            AutoSyncTransforms = true;
        }
    }

    public class AudioSettings
    {
        public float GlobalVolume { get; set; }
        public int SampleRate { get; set; }
        public int DSPBufferSize { get; set; }
        public int VirtualVoiceCount { get; set; }
        public int RealVoiceCount { get; set; }
        public bool SpatializerPlugin { get; set; }

        public AudioSettings()
        {
            GlobalVolume = 1f;
            SampleRate = 48000;
            DSPBufferSize = 1024;
            VirtualVoiceCount = 50;
            RealVoiceCount = 32;
            SpatializerPlugin = false;
        }
    }

    public class GraphicsSettings
    {
        public int TierSettings { get; set; }
        public bool UseSRPBatcher { get; set; }
        public bool ScriptableRenderPipelineSettings { get; set; }
        public string RenderPipelineAsset { get; set; }

        public GraphicsSettings()
        {
            TierSettings = 2;
            UseSRPBatcher = false;
            ScriptableRenderPipelineSettings = false;
            RenderPipelineAsset = "";
        }
    }

    public class InputSettings
    {
        public List<InputAxis> Axes { get; set; }

        public InputSettings()
        {
            Axes = new List<InputAxis>
            {
                new InputAxis("Horizontal"),
                new InputAxis("Vertical"),
                new InputAxis("Fire1"),
                new InputAxis("Jump")
            };
        }
    }

    public class InputAxis
    {
        public string Name { get; set; }
        public string PositiveButton { get; set; }
        public string NegativeButton { get; set; }
        public string AltPositiveButton { get; set; }
        public string AltNegativeButton { get; set; }
        public float Gravity { get; set; }
        public float Sensitivity { get; set; }
        public float Dead { get; set; }
        public bool Snap { get; set; }
        public float Type { get; set; }

        public InputAxis(string name)
        {
            Name = name;
            Gravity = 3f;
            Sensitivity = 3f;
            Dead = 0.001f;
            Snap = false;
            Type = 0f;
        }
    }

    public class TagManager
    {
        public List<string> Tags { get; set; }
        public List<string> Layers { get; set; }
        public List<string> SortingLayers { get; set; }

        public TagManager()
        {
            Tags = new List<string> { "Untagged", "Respawn", "Finish", "EditorOnly", "MainCamera", "Player", "GameController" };
            Layers = new List<string> { "Default", "TransparentFX", "Ignore Raycast", "Reserved3", "Water", "UI", "3D", "2D" };
            SortingLayers = new List<string> { "Default", "Background", "Foreground" };
        }
    }

    public class LayerManager
    {
        public Dictionary<int, string> Layers { get; set; }

        public LayerManager()
        {
            Layers = new Dictionary<int, string>
            {
                { 0, "Default" },
                { 1, "TransparentFX" },
                { 2, "Ignore Raycast" },
                { 3, "Reserved3" },
                { 4, "Water" },
                { 5, "UI" },
                { 6, "3D" },
                { 7, "2D" }
            };
        }

        public string GetLayerName(int layer)
        {
            return Layers.ContainsKey(layer) ? Layers[layer] : "Default";
        }

        public int GetLayerIndex(string name)
        {
            foreach (var layer in Layers)
            {
                if (layer.Value == name)
                    return layer.Key;
            }
            return 0;
        }
    }
}
