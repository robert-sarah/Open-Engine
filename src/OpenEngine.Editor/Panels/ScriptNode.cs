// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.Reflection;
using System.Text;

namespace OpenEngine.Editor.Panels
{
    public class ScriptNode : GraphNode
    {
        public string ScriptCode { get; set; }
        public string Namespace { get; set; }
        public string ClassName { get; set; }
        public List<string> UsingStatements { get; set; }
        public List<ScriptMethod> Methods { get; set; }
        public List<ScriptProperty> Properties { get; set; }
        public bool IsCompiled { get; set; }
        public Assembly CompiledAssembly { get; set; }
        public Type CompiledType { get; set; }
        public object CompiledInstance { get; set; }

        public ScriptNode()
        {
            NodeType = NodeType.Logic;
            Category = "Scripts";
            Color = "#8E44AD";
            Description = "Custom C# script node";
            ScriptCode = "";
            Namespace = "OpenEngine.Scripts";
            ClassName = "CustomScript";
            UsingStatements = new List<string>
            {
                "System",
                "System.Collections.Generic",
                "OpenEngine.Core.Entities",
                "OpenEngine.Core.Engine"
            };
            Methods = new List<ScriptMethod>();
            Properties = new List<ScriptProperty>();
            IsCompiled = false;
        }

        public string GenerateFullScript()
        {
            var sb = new StringBuilder();
            
            // Using statements
            foreach (var usingStmt in UsingStatements)
            {
                sb.AppendLine($"using {usingStmt};");
            }
            sb.AppendLine();

            // Namespace
            sb.AppendLine($"namespace {Namespace}");
            sb.AppendLine("{");
            
            // Class
            sb.AppendLine($"    public class {ClassName}");
            sb.AppendLine("    {");
            
            // Properties
            foreach (var prop in Properties)
            {
                sb.AppendLine($"        public {prop.Type} {prop.Name} {{ get; set; }}");
            }
            sb.AppendLine();

            // Methods
            foreach (var method in Methods)
            {
                sb.AppendLine($"        public {method.ReturnType} {method.Name}({method.Parameters})");
                sb.AppendLine("        {");
                sb.AppendLine($"            {method.Body}");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            // Main execute method from script code
            if (!string.IsNullOrEmpty(ScriptCode))
            {
                sb.AppendLine("        public void Execute()");
                sb.AppendLine("        {");
                sb.AppendLine(ScriptCode);
                sb.AppendLine("        }");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        public bool Compile()
        {
            try
            {
                var provider = new CSharpCodeProvider();
                var parameters = new CompilerParameters
                {
                    GenerateExecutable = false,
                    GenerateInMemory = true,
                    TreatWarningsAsErrors = false
                };

                // Add references
                parameters.ReferencedAssemblies.Add("System.dll");
                parameters.ReferencedAssemblies.Add("System.Core.dll");
                parameters.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);

                var script = GenerateFullScript();
                var results = provider.CompileAssemblyFromSource(parameters, script);

                if (results.Errors.Count > 0)
                {
                    foreach (CompilerError error in results.Errors)
                    {
                        Console.WriteLine($"Compilation Error: Line {error.Line} - {error.ErrorText}");
                    }
                    return false;
                }

                CompiledAssembly = results.CompiledAssembly;
                CompiledType = CompiledAssembly.GetType($"{Namespace}.{ClassName}");
                CompiledInstance = Activator.CreateInstance(CompiledType);
                IsCompiled = true;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Compilation failed: {ex.Message}");
                return false;
            }
        }

        public object ExecuteMethod(string methodName, params object[] parameters)
        {
            if (!IsCompiled || CompiledInstance == null)
            {
                if (!Compile()) return null;
            }

            var method = CompiledType.GetMethod(methodName);
            if (method == null)
            {
                Console.WriteLine($"Method {methodName} not found");
                return null;
            }

            try
            {
                return method.Invoke(CompiledInstance, parameters);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Method execution failed: {ex.Message}");
                return null;
            }
        }

        public void Execute()
        {
            ExecuteMethod("Execute");
        }
    }

    public class ScriptMethod
    {
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public string Parameters { get; set; }
        public string Body { get; set; }

        public ScriptMethod()
        {
            ReturnType = "void";
            Parameters = "";
            Body = "";
        }
    }

    public class ScriptProperty
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public object DefaultValue { get; set; }

        public ScriptProperty()
        {
            Type = "object";
            DefaultValue = null;
        }
    }

    public class GameActionNode : ScriptNode
    {
        public string ActionType { get; set; }
        public Dictionary<string, object> ActionParameters { get; set; }

        public GameActionNode()
        {
            NodeType = NodeType.Action;
            Category = "Game Actions";
            Color = "#E74C3C";
            ActionParameters = new Dictionary<string, object>();
        }

        public override string ToString()
        {
            return $"Action: {ActionType}";
        }
    }

    public static class GameActionTemplates
    {
        public static string MoveToTarget = @"
// Move entity to target position
var targetPosition = GetParameterValue<Vector3>(\"TargetPosition\");
var speed = GetParameterValue<float>(\"Speed\", 5.0f);

if (entity != null)
{
    var direction = (targetPosition - entity.Position3D).Normalized();
    entity.Position3D += direction * speed * deltaTime;
}";

        public static string AttackTarget = @"
// Attack target entity
var target = GetParameterValue<SimEntity>(\"Target\");
var damage = GetParameterValue<float>(\"Damage\", 10.0f);

if (target != null)
{
    // Apply damage to target
    Console.WriteLine($\"Attacking {target.Name} for {damage} damage\");
}";

        public static string InteractWithObject = @"
// Interact with object
var target = GetParameterValue<SimEntity>(\"Target\");
var interactionType = GetParameterValue<string>(\"InteractionType\", \"use\");

if (target != null)
{
    Console.WriteLine($\"Interacting with {target.Name}: {interactionType}\");
}";

        public static string SpawnEntity = @"
// Spawn new entity
var entityType = GetParameterValue<string>(\"EntityType\", \"DefaultEntity\");
var position = GetParameterValue<Vector3>(\"Position\", new Vector3(0, 0, 0));

var newEntity = engine.CreateEntity(entityType);
newEntity.Position3D = position;
engine.AddEntity(newEntity);";

        public static string DestroyEntity = @"
// Destroy entity
var target = GetParameterValue<SimEntity>(\"Target\");

if (target != null)
{
    engine.RemoveEntity(target.Id);
}";

        public static string ChangeEntityState = @"
// Change entity state
var target = GetParameterValue<SimEntity>(\"Target\");
var newState = GetParameterValue<string>(\"NewState\", \"Idle\");

if (target != null)
{
    target.Attributes[\"State\"] = newState;
}";

        public static string PlayAnimation = @"
// Play animation on entity
var target = GetParameterValue<SimEntity>(\"Target\");
var animationName = GetParameterValue<string>(\"AnimationName\", \"Idle\");
var loop = GetParameterValue<bool>(\"Loop\", true);

if (target != null)
{
    // Play animation
    Console.WriteLine($\"Playing {animationName} on {target.Name}\");
}";

        public static string PlaySound = @"
// Play sound effect
var soundName = GetParameterValue<string>(\"SoundName\", \"default\");
var volume = GetParameterValue<float>(\"Volume\", 1.0f);
var position = GetParameterValue<Vector3>(\"Position\", Vector3.Zero);

// Play sound at position
Console.WriteLine($\"Playing sound {soundName} at volume {volume}\");";

        public static string SetVariable = @"
// Set variable value
var variableName = GetParameterValue<string>(\"VariableName\");
var value = GetParameterValue<object>(\"Value\");

if (!string.IsNullOrEmpty(variableName))
{
    graph.SetVariableValue(variableName, value);
}";

        public static string GetVariable = @"
// Get variable value
var variableName = GetParameterValue<string>(\"VariableName\");

if (!string.IsNullOrEmpty(variableName))
{
    var value = graph.GetVariableValue(variableName);
    return value;
}
return null;";

        public static string WaitForSeconds = @"
// Wait for specified time
var seconds = GetParameterValue<float>(\"Seconds\", 1.0f);

// Implement wait logic
Console.WriteLine($\"Waiting for {seconds} seconds\");";

        public static string LogMessage = @"
// Log message to console
var message = GetParameterValue<string>(\"Message\", \"\");
var logLevel = GetParameterValue<string>(\"LogLevel\", \"Info\");

Console.WriteLine($\"[{logLevel}] {message}\");";
    }
}
