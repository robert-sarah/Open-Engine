// Created By Levi Enama
using System;

namespace OpenEngine.Core.Platform
{
    public enum PlatformType
    {
        Windows,
        Linux,
        MacOS,
        Android,
        iOS,
        WebGL,
        PS5,
        XboxSeries,
        Switch
    }

    public class Platform
    {
        private static Platform _instance;
        public static Platform Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Platform();
                }
                return _instance;
            }
        }

        public PlatformType CurrentPlatform { get; private set; }
        public string DeviceModel { get; set; }
        public string DeviceName { get; set; }
        public string OperatingSystem { get; set; }
        public string OSVersion { get; set; }
        public int ProcessorCount { get; set; }
        public long SystemMemory { get; set; }
        public float ScreenDPI { get; set; }
        public System.Numerics.Vector2 ScreenResolution { get; set; }

        public Platform()
        {
            DetectPlatform();
        }

        private void DetectPlatform()
        {
            var os = Environment.OSVersion.Platform;
            var is64Bit = Environment.Is64BitOperatingSystem;

            if (os == PlatformID.Win32NT)
            {
                CurrentPlatform = PlatformType.Windows;
                OperatingSystem = "Windows";
                OSVersion = Environment.OSVersion.Version.ToString();
            }
            else if (os == PlatformID.Unix)
            {
                CurrentPlatform = PlatformType.Linux;
                OperatingSystem = "Linux";
                OSVersion = Environment.OSVersion.Version.ToString();
            }
            else if (os == PlatformID.MacOSX)
            {
                CurrentPlatform = PlatformType.MacOS;
                OperatingSystem = "macOS";
                OSVersion = Environment.OSVersion.Version.ToString();
            }
            else
            {
                CurrentPlatform = PlatformType.Windows; // Default
                OperatingSystem = "Unknown";
            }

            ProcessorCount = Environment.ProcessorCount;
            SystemMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        }

        public bool IsMobile()
        {
            return CurrentPlatform == PlatformType.Android || CurrentPlatform == PlatformType.iOS;
        }

        public bool IsConsole()
        {
            return CurrentPlatform == PlatformType.PS5 || CurrentPlatform == PlatformType.XboxSeries || CurrentPlatform == PlatformType.Switch;
        }

        public bool IsDesktop()
        {
            return CurrentPlatform == PlatformType.Windows || CurrentPlatform == PlatformType.Linux || CurrentPlatform == PlatformType.MacOS;
        }

        public bool IsWeb()
        {
            return CurrentPlatform == PlatformType.WebGL;
        }
    }

    public class AndroidPlatform : Platform
    {
        public string AndroidVersion { get; set; }
        public string DeviceManufacturer { get; set; }
        public bool HasCamera { get; set; }
        public bool HasGPS { get; set; }
        public bool HasAccelerometer { get; set; }
        public bool HasGyroscope { get; set; }
        public int BatteryLevel { get; set; }
        public bool IsCharging { get; set; }

        public AndroidPlatform()
        {
            CurrentPlatform = PlatformType.Android;
            InitializeAndroidSpecific();
        }

        private void InitializeAndroidSpecific()
        {
            // Android-specific initialization
            HasCamera = true;
            HasGPS = true;
            HasAccelerometer = true;
            HasGyroscope = true;
        }

        public void ShowToast(string message)
        {
            // Android toast notification
            Console.WriteLine($"[Android Toast] {message}");
        }

        public void Vibrate(int milliseconds)
        {
            // Android vibration
            Console.WriteLine($"[Android Vibrate] {milliseconds}ms");
        }

        public void RequestPermissions(string[] permissions)
        {
            // Request runtime permissions
            Console.WriteLine($"[Android] Requesting permissions: {string.Join(", ", permissions)}");
        }
    }

    public class IOSPlatform : Platform
    {
        public string IOSVersion { get; set; }
        public string DeviceModel { get; set; }
        public bool HasNotch { get; set; }
        public bool HasHomeIndicator { get; set; }
        public float SafeAreaTop { get; set; }
        public float SafeAreaBottom { get; set; }
        public float SafeAreaLeft { get; set; }
        public float SafeAreaRight { get; set; }

        public IOSPlatform()
        {
            CurrentPlatform = PlatformType.iOS;
            InitializeIOSSpecific();
        }

        private void InitializeIOSSpecific()
        {
            // iOS-specific initialization
            HasNotch = true;
            HasHomeIndicator = true;
            SafeAreaTop = 44f;
            SafeAreaBottom = 34f;
            SafeAreaLeft = 0f;
            SafeAreaRight = 0f;
        }

        public void ShowNotification(string title, string message)
        {
            // iOS notification
            Console.WriteLine($"[iOS Notification] {title}: {message}");
        }

        public void Vibrate()
        {
            // iOS haptic feedback
            Console.WriteLine("[iOS Haptic Feedback]");
        }

        public void RequestAuthorization(string authorizationType)
        {
            // Request authorization (camera, microphone, etc.)
            Console.WriteLine($"[iOS] Requesting authorization: {authorizationType}");
        }
    }

    public class BuildSettings
    {
        public PlatformType TargetPlatform { get; set; }
        public string BuildNumber { get; set; }
        public string Version { get; set; }
        public string BundleIdentifier { get; set; }
        public string CompanyName { get; set; }
        public string ProductName { get; set; }
        public bool EnableProguard { get; set; }
        public string AndroidArchitecture { get; set; }
        public string IOSArchitecture { get; set; }
        public int AndroidMinimumSDK { get; set; }
        public int IOSTargetSDK { get; set; }
        public bool EnableBitcode { get; set; }
        public bool EnableARKit { get; set; }
        public bool EnableMetal { get; set; }

        public BuildSettings()
        {
            TargetPlatform = PlatformType.Windows;
            BuildNumber = "1.0.0";
            Version = "1.0";
            BundleIdentifier = "com.company.product";
            CompanyName = "Company";
            ProductName = "Product";
            EnableProguard = false;
            AndroidArchitecture = "ARMv7";
            IOSArchitecture = "ARM64";
            AndroidMinimumSDK = 21;
            IOSTargetSDK = 12;
            EnableBitcode = false;
            EnableARKit = false;
            EnableMetal = true;
        }

        public string GetBuildCommand()
        {
            switch (TargetPlatform)
            {
                case PlatformType.Android:
                    return $"./gradlew assembleRelease -PbundleId={BundleIdentifier}";
                case PlatformType.iOS:
                    return $"xcodebuild -scheme {ProductName} -configuration Release";
                case PlatformType.Windows:
                    return $"dotnet publish -c Release -r win-x64";
                case PlatformType.Linux:
                    return $"dotnet publish -c Release -r linux-x64";
                case PlatformType.MacOS:
                    return $"dotnet publish -c Release -r osx-x64";
                default:
                    return "Unknown platform";
            }
        }
    }

    public class ObjectiveCppBridge
    {
        // Bridge for Objective-C++ interoperability
        // This provides hooks for native iOS/macOS code

        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void ObjectiveCpp_Initialize();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void ObjectiveCpp_Shutdown();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void ObjectiveCpp_Update(float deltaTime);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void ObjectiveCpp_Render();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern IntPtr ObjectiveCpp_GetMetalDevice();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void ObjectiveCpp_SetMetalDevice(IntPtr device);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void ObjectiveCpp_HandleTouch(int touchId, float x, float y, int phase);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void ObjectiveCpp_PlaySound(string soundName);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void ObjectiveCpp_Vibrate();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        public static extern void ObjectiveCpp_ShowNotification(string title, string message);

        public static void Initialize()
        {
            if (Platform.Instance.CurrentPlatform == PlatformType.iOS || 
                Platform.Instance.CurrentPlatform == PlatformType.MacOS)
            {
                try
                {
                    ObjectiveCpp_Initialize();
                }
                catch (System.DllNotFoundException)
                {
                    Console.WriteLine("Objective-C++ bridge not available in this build");
                }
            }
        }

        public static void Shutdown()
        {
            if (Platform.Instance.CurrentPlatform == PlatformType.iOS || 
                Platform.Instance.CurrentPlatform == PlatformType.MacOS)
            {
                try
                {
                    ObjectiveCpp_Shutdown();
                }
                catch (System.DllNotFoundException)
                {
                    // Ignore
                }
            }
        }
    }

    public class JavaBridge
    {
        // Bridge for Java/Android interoperability
        // This provides hooks for native Android code

        [System.Runtime.InteropServices.DllImport("android")]
        public static extern void Java_Initialize();

        [System.Runtime.InteropServices.DllImport("android")]
        public static extern void Java_Shutdown();

        [System.Runtime.InteropServices.DllImport("android")]
        public static extern void Java_Update(float deltaTime);

        [System.Runtime.InteropServices.DllImport("android")]
        public static extern void Java_Render();

        [System.Runtime.InteropServices.DllImport("android")]
        public static extern IntPtr Java_GetActivity();

        [System.Runtime.InteropServices.DllImport("android")]
        public static extern void Java_RequestPermissions(string[] permissions);

        [System.Runtime.InteropServices.DllImport("android")]
        public static extern void Java_ShowToast(string message);

        [System.Runtime.InteropServices.DllImport("android")]
        public static extern void Java_Vibrate(int milliseconds);

        [System.Runtime.InteropServices.DllImport("android")]
        public static extern void Java_PlaySound(string soundName);

        public static void Initialize()
        {
            if (Platform.Instance.CurrentPlatform == PlatformType.Android)
            {
                try
                {
                    Java_Initialize();
                }
                catch (System.DllNotFoundException)
                {
                    Console.WriteLine("Java bridge not available in this build");
                }
            }
        }

        public static void Shutdown()
        {
            if (Platform.Instance.CurrentPlatform == PlatformType.Android)
            {
                try
                {
                    Java_Shutdown();
                }
                catch (System.DllNotFoundException)
                {
                    // Ignore
                }
            }
        }
    }
}
