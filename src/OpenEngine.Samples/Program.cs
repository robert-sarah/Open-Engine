// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenEngine.Core.Entities;
using OpenEngine.Core.Engine;
using OpenEngine.Core.Intent;
using OpenEngine.Core.Math;
using OpenEngine.LLM.Providers;
using OpenEngine.Editor.Panels;
using OpenEngine.Editor.Workspace;
using OpenEngine.Editor.UI;

namespace OpenEngine.Samples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=========================================");
            Console.WriteLine("         OPEN ENGINE SIMULATION          ");
            Console.WriteLine("=========================================");
            Console.WriteLine();
            
            Console.WriteLine("1) Run 3D GUI Editor");
            Console.WriteLine("2) Run Console Demo");
            Console.Write("Choose option [1]: ");
            var choice = Console.ReadLine()?.Trim();
            if (string.IsNullOrWhiteSpace(choice) || choice == "1")
            {
                Console.WriteLine("Launching GUI Editor...");
                var app = new EditorApp();
                app.Run();
                return;
            }
            Console.WriteLine();

            // Step 1: Initialize the Editor Workspace
            Console.WriteLine("[1/6] Initializing Editor Workspace...");
            var workspace = new EditorWorkspace("Space Colony Simulation");
            workspace.OnLogMessage += (sender, msg) => Console.WriteLine($"  {msg}");
            workspace.Initialize();

            var engine = workspace.Engine;

            // Step 2: Initialize the Universal Intent Parser
            Console.WriteLine();
            Console.WriteLine("[2/6] Setting up Intent Parser...");
            var intentParser = new UniversalIntentParser();

            // Step 3: Initialize LLM Provider (using HuggingFaceLocal with smart fallback)
            Console.WriteLine();
            Console.WriteLine("[3/6] Initializing LLM Provider...");
            var llmProvider = new HuggingFaceLocalProvider();
            await llmProvider.InitializeAsync();
            Console.WriteLine($"  Provider: {llmProvider.ProviderName}");
            Console.WriteLine($"  Available: {llmProvider.IsAvailable}");

            // Step 4: Create our Simulation Entities
            Console.WriteLine();
            Console.WriteLine("[4/6] Creating Simulation Entities...");

            // Create a Starship
            var starship = engine.CreateEntity("USS Explorer", "Starship");
            starship.Position3D = new Vector3(100.0f, 50.0f, 200.0f);
            starship.AddAttribute("HullIntegrity", 100.0f);
            starship.AddAttribute("ShieldPower", 85.0f);
            starship.AddAttribute("Fuel", 75.5f);
            starship.AddAttribute("CrewMorale", 92.0f);
            starship.AddCapability("WarpDrive");
            starship.AddCapability("Weapons");
            starship.AddCapability("Sensors");
            starship.AddTag("Player");
            Console.WriteLine($"  Created: {starship.Name} at {starship.Position3D}");

            // Create a Space Station
            var station = engine.CreateEntity("Starbase Alpha", "SpaceStation");
            station.Position3D = new Vector3(0.0f, 0.0f, 0.0f);
            station.AddAttribute("Population", 5000.0f);
            station.AddAttribute("EnergyProduction", 150.0f);
            station.AddAttribute("DefenseRating", 95.0f);
            station.AddCapability("Docking");
            station.AddCapability("Refinery");
            station.AddCapability("Research");
            station.AddTag("Ally");
            Console.WriteLine($"  Created: {station.Name} at {station.Position3D}");

            // Create some Asteroids
            var asteroid1 = engine.CreateEntity("Asteroid A-113", "Asteroid");
            asteroid1.Position3D = new Vector3(250.0f, 30.0f, 150.0f);
            asteroid1.AddAttribute("MineralContent", 80.0f);
            asteroid1.AddAttribute("Size", 45.0f);
            asteroid1.AddCapability("Mineable");
            asteroid1.AddTag("Resource");
            Console.WriteLine($"  Created: {asteroid1.Name} at {asteroid1.Position3D}");

            var asteroid2 = engine.CreateEntity("Asteroid B-42", "Asteroid");
            asteroid2.Position3D = new Vector3(-180.0f, -60.0f, 220.0f);
            asteroid2.AddAttribute("MineralContent", 65.0f);
            asteroid2.AddAttribute("Size", 32.0f);
            asteroid2.AddCapability("Mineable");
            asteroid2.AddTag("Resource");
            Console.WriteLine($"  Created: {asteroid2.Name} at {asteroid2.Position3D}");

            // Create an Enemy Ship
            var enemy = engine.CreateEntity("Raider X-7", "Warship");
            enemy.Position3D = new Vector3(-300.0f, 100.0f, -150.0f);
            enemy.AddAttribute("HullIntegrity", 90.0f);
            enemy.AddAttribute("ShieldPower", 70.0f);
            enemy.AddAttribute("Aggression", 85.0f);
            enemy.AddCapability("Weapons");
            enemy.AddCapability("Cloak");
            enemy.AddTag("Hostile");
            Console.WriteLine($"  Created: {enemy.Name} at {enemy.Position3D}");

            // Step 5: Create Connections between entities
            Console.WriteLine();
            Console.WriteLine("[5/6] Creating Entity Connections...");

            var conn1 = engine.CreateConnection(starship.Id, station.Id, ConnectionPredicate.DOCKED_AT);
            conn1.Strength = 0.9f;
            Console.WriteLine($"  Connection: {starship.Name} -> {conn1.Predicate} -> {station.Name}");

            var conn2 = engine.CreateConnection(starship.Id, enemy.Id, ConnectionPredicate.HOSTILE_TO);
            conn2.Strength = 1.0f;
            Console.WriteLine($"  Connection: {starship.Name} -> {conn2.Predicate} -> {enemy.Name}");

            var conn3 = engine.CreateConnection(starship.Id, asteroid1.Id, ConnectionPredicate.CONSUMES_ENERGY);
            conn3.Strength = 0.6f;
            Console.WriteLine($"  Connection: {starship.Name} -> {conn2.Predicate} -> {asteroid1.Name}");

            // Step 6: Run the multi-turn simulation demo
            Console.WriteLine();
            Console.WriteLine("[6/6] Starting Multi-Turn Simulation...");
            Console.WriteLine("-----------------------------------------");
            Console.WriteLine();

            await RunMultiTurnSimulation(engine, intentParser, llmProvider, starship);

            // Cleanup
            Console.WriteLine();
            Console.WriteLine("=========================================");
            Console.WriteLine("         SIMULATION COMPLETE             ");
            Console.WriteLine("=========================================");

            Console.WriteLine();
            Console.WriteLine("Shutting down...");
            await llmProvider.ShutdownAsync();
            workspace.Shutdown();

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async Task RunMultiTurnSimulation(
            OpenSimulationEngine engine,
            UniversalIntentParser intentParser,
            ILLMProvider llmProvider,
            SimEntity playerEntity)
        {
            // ===== TURN 1: Initial State =====
            Console.WriteLine("===== TURN 1: INITIAL STATE =====");
            Console.WriteLine();
            await ShowEntityContext(engine, playerEntity);
            Console.WriteLine();

            // ===== TURN 2: Inject Threat via God Panel =====
            Console.WriteLine("===== TURN 2: GOD PANEL ALERT =====");
            Console.WriteLine();
            string threatAlert = "⚠️ METEOR SWARM THREAT DETECTED! INCOMING PROXIMITY: 500 UNITS!";
            Console.WriteLine($"[GOD PANEL] {threatAlert}");
            engine.AddGodPanelAlert(threatAlert);
            Console.WriteLine();

            // ===== TURN 3: LLM Decision Making =====
            Console.WriteLine("===== TURN 3: AI AGENT THINKING =====");
            Console.WriteLine();
            Console.WriteLine("[LLM] Generating situational analysis...");
            Console.WriteLine();

            string contextMarkdown = engine.Serialize3DContextToMarkdown(playerEntity.Id);
            string prompt = $@"
You are the captain of the {playerEntity.Name}. Based on the following situation report, what action do you take?

SITUATION REPORT:
{contextMarkdown}

Respond in natural language explaining your decision, and include ONE clear action verb from this list: MOVE, ATTACK, DEFEND, HARVEST, IDLE, ESCAPE, EXPLORE, FOLLOW.
";

            string llmResponse = await llmProvider.GenerateResponseAsync(prompt);
            Console.WriteLine("[LLM RESPONSE]:");
            Console.WriteLine("---");
            Console.WriteLine(llmResponse);
            Console.WriteLine("---");
            Console.WriteLine();

            // ===== TURN 4: Intent Parsing =====
            Console.WriteLine("===== TURN 4: INTENT PARSING =====");
            Console.WriteLine();
            var actions = intentParser.ParseIntent(llmResponse, playerEntity.Id);
            Console.WriteLine($"[PARSER] Found {actions.Count} action(s):");
            foreach (var action in actions)
            {
                Console.WriteLine($"  - {action.Action}");
            }

            // Fallback if no intent detected
            if (actions.Count == 0)
            {
                Console.WriteLine("  (Using fallback parser...)");
                var fallbackAction = intentParser.FallbackRegexParse(llmResponse, playerEntity.Id);
                if (fallbackAction.HasValue)
                {
                    actions.Add(fallbackAction.Value);
                    Console.WriteLine($"  - [FALLBACK] {fallbackAction.Value.Action}");
                }
            }

            // Default to ESCAPE if no action found
            if (actions.Count == 0)
            {
                actions.Add(new EngineActionEvent(playerEntity.Id, ActionVerb.ESCAPE));
                Console.WriteLine($"  - [DEFAULT] {ActionVerb.ESCAPE}");
            }
            Console.WriteLine();

            // ===== TURN 5: Execute Action =====
            Console.WriteLine("===== TURN 5: EXECUTING ACTION =====");
            Console.WriteLine();
            foreach (var action in actions)
            {
                await ExecuteEngineAction(engine, playerEntity, action);
            }
            Console.WriteLine();

            // ===== TURN 6: Updated State =====
            Console.WriteLine("===== TURN 6: UPDATED STATE =====");
            Console.WriteLine();
            await ShowEntityContext(engine, playerEntity);
            Console.WriteLine();

            // ===== TURN 7: Another Situation =====
            Console.WriteLine("===== TURN 7: NEW SITUATION =====");
            Console.WriteLine();
            string economicAlert = "💰 ECONOMY CRASH ALERT! Resource values dropping rapidly!";
            Console.WriteLine($"[GOD PANEL] {economicAlert}");
            engine.AddGodPanelAlert(economicAlert);
            Console.WriteLine();

            Console.WriteLine("[LLM] Analyzing economic situation...");
            Console.WriteLine();
            contextMarkdown = engine.Serialize3DContextToMarkdown(playerEntity.Id);
            prompt = $@"
Economic crisis! Resource values are dropping! What should the {playerEntity.Name} do now?

SITUATION:
{contextMarkdown}

What is your next action? Choose from: MOVE, ATTACK, DEFEND, HARVEST, IDLE, ESCAPE, EXPLORE, TRADE.
";

            llmResponse = await llmProvider.GenerateResponseAsync(prompt);
            Console.WriteLine("[LLM RESPONSE]:");
            Console.WriteLine("---");
            Console.WriteLine(llmResponse);
            Console.WriteLine("---");
            Console.WriteLine();

            // Parse and execute this action
            actions = intentParser.ParseIntent(llmResponse, playerEntity.Id);
            if (actions.Count == 0)
            {
                var fallback = intentParser.FallbackRegexParse(llmResponse, playerEntity.Id);
                if (fallback.HasValue) actions.Add(fallback.Value);
            }
            if (actions.Count == 0) actions.Add(new EngineActionEvent(playerEntity.Id, ActionVerb.HARVEST));

            Console.WriteLine("===== TURN 8: EXECUTING ECONOMIC ACTION =====");
            Console.WriteLine();
            foreach (var action in actions)
            {
                await ExecuteEngineAction(engine, playerEntity, action);
            }
            Console.WriteLine();

            Console.WriteLine("===== TURN 9: FINAL SIMULATION STATE =====");
            Console.WriteLine();
            await ShowEntityContext(engine, playerEntity);
        }

        static async Task ShowEntityContext(OpenSimulationEngine engine, SimEntity entity)
        {
            string context = engine.Serialize3DContextToMarkdown(entity.Id);
            Console.WriteLine(context);
        }

        static async Task ExecuteEngineAction(OpenSimulationEngine engine, SimEntity actor, EngineActionEvent action)
        {
            Console.WriteLine($"[EXECUTE] {actor.Name} is performing: {action.Action}");

            switch (action.Action)
            {
                case ActionVerb.MOVE_TO:
                    // Move toward a safe location
                    Vector3 newPosition;
                    if (action.TargetCoordinates3D != Vector3.Zero)
                    {
                        newPosition = action.TargetCoordinates3D;
                    }
                    else
                    {
                        newPosition = new Vector3(
                            actor.Position3D.X + 100.0f,
                            actor.Position3D.Y + 50.0f,
                            actor.Position3D.Z + 100.0f
                        );
                    }
                    actor.Position3D = newPosition;
                    Console.WriteLine($"  -> Moved to {actor.Position3D}");
                    break;

                case ActionVerb.ATTACK:
                    actor.AddAttribute("ShieldPower", Math.Max(0, actor.Attributes.GetValueOrDefault("ShieldPower", 0) - 10));
                    Console.WriteLine($"  -> Weapons fired! Shield power now at {actor.Attributes.GetValueOrDefault("ShieldPower", 0):F1}%");
                    break;

                case ActionVerb.DEFEND:
                    actor.AddAttribute("ShieldPower", Math.Min(100, actor.Attributes.GetValueOrDefault("ShieldPower", 0) + 15));
                    Console.WriteLine($"  -> Shields raised! Shield power now at {actor.Attributes.GetValueOrDefault("ShieldPower", 0):F1}%");
                    break;

                case ActionVerb.HARVEST:
                case ActionVerb.GATHER:
                    actor.AddAttribute("Fuel", Math.Min(100, actor.Attributes.GetValueOrDefault("Fuel", 0) + 20));
                    Console.WriteLine($"  -> Resources collected! Fuel now at {actor.Attributes.GetValueOrDefault("Fuel", 0):F1}%");
                    break;

                case ActionVerb.ESCAPE:
                case ActionVerb.RETREAT:
                    actor.Position3D = new Vector3(
                        actor.Position3D.X - 200.0f,
                        actor.Position3D.Y,
                        actor.Position3D.Z - 200.0f
                    );
                    Console.WriteLine($"  -> Escaped to {actor.Position3D}");
                    break;

                case ActionVerb.TRADE:
                    actor.AddAttribute("CrewMorale", Math.Min(100, actor.Attributes.GetValueOrDefault("CrewMorale", 0) + 5));
                    Console.WriteLine($"  -> Trade successful! Crew morale at {actor.Attributes.GetValueOrDefault("CrewMorale", 0):F1}%");
                    break;

                case ActionVerb.EXPLORE:
                    actor.Position3D = new Vector3(
                        actor.Position3D.X + 50.0f,
                        actor.Position3D.Y - 25.0f,
                        actor.Position3D.Z + 75.0f
                    );
                    Console.WriteLine($"  -> Exploring new sector at {actor.Position3D}");
                    break;

                case ActionVerb.IDLE:
                default:
                    Console.WriteLine("  -> Holding position...");
                    break;
            }

            await Task.Delay(100);
        }
    }
}
