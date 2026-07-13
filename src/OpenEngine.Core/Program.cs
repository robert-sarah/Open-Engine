// Created By Levi Enama
// Main Entry Point for Open Engine
using System;
using OpenEngine.Core.Engine;
using OpenEngine.Core.Math;
using OpenEngine.Core.Entities;
using OpenEngine.Core.Events;

namespace OpenEngine.Core
{
    public class Program
    {
        private static OpenEngineCore _engine;

        public static void Main(string[] args)
        {
            Console.WriteLine("=== Open Engine ===");
            Console.WriteLine("Initializing...");

            try
            {
                _engine = OpenEngineCore.Instance;
                _engine.Initialize();

                var testEntity = new SimEntity
                {
                    Id = "test_entity_1",
                    Name = "Test Entity",
                    Position = new Vector3(0, 0, 0),
                    Rotation = Quaternion.Identity,
                    Scale = Vector3.One,
                    Health = 100,
                    IsActive = true
                };

                _engine.AddEntity(testEntity);
                _engine.Start();

                float deltaTime = 0.016f;
                bool running = true;

                while (running)
                {
                    _engine.Update(deltaTime);
                    _engine.Render();

                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Escape)
                        {
                            running = false;
                        }
                    }

                    System.Threading.Thread.Sleep(16);
                }

                _engine.Shutdown();
                _engine.Dispose();

                Console.WriteLine("Open Engine stopped successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void OnEngineLog(object sender, EngineEventArgs e)
        {
            Console.WriteLine($"[LOG] {e.Message}");
        }

        private static void OnEngineError(object sender, EngineEventArgs e)
        {
            Console.WriteLine($"[ERROR] {e.Message}");
        }

        private static void OnEngineWarning(object sender, EngineEventArgs e)
        {
            Console.WriteLine($"[WARNING] {e.Message}");
        }
    }
}
