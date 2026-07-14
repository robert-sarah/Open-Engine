// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using OpenEngine.Core.Scripting;
using OpenEngine.Core.Entities;
using OpenEngine.Core.Engine;

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
            
            foreach (var usingStmt in UsingStatements)
            {
                sb.AppendLine($"using {usingStmt};");
            }
            sb.AppendLine();

            sb.AppendLine($"namespace {Namespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    public class {ClassName}");
            sb.AppendLine("    {");
            
            // Infrastructure pour les templates
            sb.AppendLine("        public OpenEngine.Core.Engine.Engine engine;");
            sb.AppendLine("        public float deltaTime;");
            sb.AppendLine("        public Dictionary<string, object> parameters = new Dictionary<string, object>();");
            sb.AppendLine();
            sb.AppendLine("        public T GetParameterValue<T>(string name, T defaultValue = default)");
            sb.AppendLine("        {");
            sb.AppendLine("            if (parameters != null && parameters.ContainsKey(name)) return (T)parameters[name];");
            sb.AppendLine("            return defaultValue;");
            sb.AppendLine("        }");
            sb.AppendLine();
            
            foreach (var prop in Properties)
            {
                sb.AppendLine($"        public {prop.Type} {prop.Name} {{ get; set; }}");
            }
            sb.AppendLine();

            foreach (var method in Methods)
            {
                sb.AppendLine($"        public {method.ReturnType} {method.Name}({method.Parameters})");
                sb.AppendLine("        {");
                sb.AppendLine($"            {method.Body}");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            // Signature mise à jour pour inclure le contexte
            sb.AppendLine("        public void Execute(SimEntity entity, OpenEngine.Core.Engine.Engine engine, float deltaTime)");
            sb.AppendLine("        {");
            sb.AppendLine("            this.engine = engine;");
            sb.AppendLine("            this.deltaTime = deltaTime;");
            sb.AppendLine(ScriptCode);
            sb.AppendLine("        }");

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        public bool Compile()
        {
            try
            {
                var compiler = new ScriptCompiler();
                compiler.AddReference(Assembly.GetExecutingAssembly().Location);
                
                var script = GenerateFullScript();
                var result = compiler.CreateType(script, ClassName, Namespace);

                if (result == null) return false;

                CompiledType = result;
                CompiledInstance = Activator.CreateInstance(CompiledType);
                IsCompiled = true;
                return true;
            }
            catch { return false; }
        }

        // Méthode générique pour injecter les paramètres avant exécution
        public void SetParameters(Dictionary<string, object> parameters)
        {
            if (CompiledInstance != null)
            {
                var field = CompiledType.GetField("parameters");
                field?.SetValue(CompiledInstance, parameters);
            }
        }

        public void Execute(SimEntity entity, OpenEngine.Core.Engine.Engine engine, float deltaTime)
        {
            if (!IsCompiled || CompiledInstance == null) if (!Compile()) return;

            var method = CompiledType.GetMethod("Execute");
            method?.Invoke(CompiledInstance, new object[] { entity, engine, deltaTime });
        }
    }

    // ... (Reste des classes : ScriptMethod, ScriptProperty, GameActionNode)
    
    // Note : Les templates dans GameActionTemplates restent inchangés car ils utilisent
    // désormais les champs injectés (engine, deltaTime, GetParameterValue) grâce à la nouvelle génération.
}