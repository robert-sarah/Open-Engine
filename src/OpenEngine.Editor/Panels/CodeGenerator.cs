// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenEngine.Editor.Panels
{
    public class CodeGenerator
    {
        private NodeGraphPanel _graph;
        private StringBuilder _codeBuilder;
        private int _indentLevel;

        public CodeGenerator(NodeGraphPanel graph)
        {
            _graph = graph;
            _codeBuilder = new StringBuilder();
            _indentLevel = 0;
        }

        public string GenerateCSharpClass(string className, string namespaceName = "OpenEngine.Generated")
        {
            _codeBuilder.Clear();
            _indentLevel = 0;

            // Header
            AppendLine("using System;");
            AppendLine("using System.Collections.Generic;");
            AppendLine("using OpenEngine.Core.Entities;");
            AppendLine("using OpenEngine.Core.Engine;");
            AppendLine("using OpenEngine.Core.Math;");
            AppendLine();

            // Namespace
            AppendLine($"namespace {namespaceName}");
            AppendLine("{");
            _indentLevel++;

            // Class
            AppendLine($"public class {className}");
            AppendLine("{");
            _indentLevel++;

            // Variables
            GenerateVariables();

            // Methods
            GenerateEventMethods();
            GenerateActionMethods();

            _indentLevel--;
            AppendLine("}");

            _indentLevel--;
            AppendLine("}");

            return _codeBuilder.ToString();
        }

        private void GenerateVariables()
        {
            AppendLine("// Variables");
            foreach (var variable in _graph.Variables)
            {
                string csharpType = VariableTypeToCSharpType(variable.Type);
                AppendLine($"private {csharpType} _{SanitizeName(variable.Name)} = {GetDefaultValue(variable.Type)};");
                AppendLine($"public {csharpType} {variable.Name} {{ get => _{SanitizeName(variable.Name)}; set => _{SanitizeName(variable.Name)} = value; }}");
            }
            AppendLine();
        }

        private void GenerateEventMethods()
        {
            var eventNodes = _graph.Nodes.Where(n => n.NodeType == NodeType.Event).ToList();
            
            foreach (var eventNode in eventNodes)
            {
                string eventType = eventNode.Data.ContainsKey("EventType") ? eventNode.Data["EventType"].ToString() : "Unknown";
                string methodName = $"On{SanitizeName(eventType)}";
                
                AppendLine($"// Event: {eventType}");
                AppendLine($"public void {methodName}(SimEntity entity = null)");
                AppendLine("{");
                _indentLevel++;

                GenerateNodeLogic(eventNode);

                _indentLevel--;
                AppendLine("}");
                AppendLine();
            }
        }

        private void GenerateActionMethods()
        {
            var actionNodes = _graph.Nodes.Where(n => n.NodeType == NodeType.Action).ToList();
            
            foreach (var actionNode in actionNodes)
            {
                string actionType = actionNode.Data.ContainsKey("ActionType") ? actionNode.Data["ActionType"].ToString() : "Unknown";
                string methodName = $"{SanitizeName(actionType)}Action";
                
                AppendLine($"// Action: {actionType}");
                AppendLine($"public void {methodName}(SimEntity entity = null)");
                AppendLine("{");
                _indentLevel++;

                GenerateNodeLogic(actionNode);

                _indentLevel--;
                AppendLine("}");
                AppendLine();
            }
        }

        private void GenerateNodeLogic(GraphNode node)
        {
            var outputConnections = _graph.Connections
                .Where(c => c.SourceNodeId == node.Id)
                .OrderBy(c => c.SourcePortId)
                .ToList();

            // Generate node-specific code
            GenerateNodeCode(node);

            // Follow connections
            foreach (var connection in outputConnections)
            {
                var targetNode = _graph.Nodes.FirstOrDefault(n => n.Id == connection.TargetNodeId);
                if (targetNode != null)
                {
                    GenerateNodeLogic(targetNode);
                }
            }
        }

        private void GenerateNodeCode(GraphNode node)
        {
            switch (node.NodeType)
            {
                case NodeType.Event:
                    GenerateEventNodeCode(node);
                    break;
                case NodeType.Action:
                    GenerateActionNodeCode(node);
                    break;
                case NodeType.Condition:
                    GenerateConditionNodeCode(node);
                    break;
                case NodeType.Variable:
                    GenerateVariableNodeCode(node);
                    break;
                case NodeType.Math:
                    GenerateMathNodeCode(node);
                    break;
                case NodeType.Flow:
                    GenerateFlowNodeCode(node);
                    break;
                case NodeType.Logic:
                    if (node is ScriptNode scriptNode)
                    {
                        GenerateScriptNodeCode(scriptNode);
                    }
                    break;
                default:
                    AppendLine($"// Unsupported node type: {node.NodeType}");
                    break;
            }
        }

        private void GenerateEventNodeCode(GraphNode node)
        {
            string eventType = node.Data.ContainsKey("EventType") ? node.Data["EventType"].ToString() : "Unknown";
            AppendLine($"// Event: {eventType}");
        }

        private void GenerateActionNodeCode(GraphNode node)
        {
            if (node is GameActionNode gameActionNode)
            {
                AppendLine($"// Action: {gameActionNode.ActionType}");
                AppendLine(gameActionNode.ScriptCode);
            }
            else
            {
                string actionType = node.Data.ContainsKey("ActionType") ? node.Data["ActionType"].ToString() : "Unknown";
                AppendLine($"// Action: {actionType}");
                AppendLine($"Console.WriteLine(\"Executing {actionType}\");");
            }
        }

        private void GenerateConditionNodeCode(GraphNode node)
        {
            string conditionType = node.Data.ContainsKey("ConditionType") ? node.Data["ConditionType"].ToString() : "Unknown";
            
            AppendLine($"// Condition: {conditionType}");
            AppendLine($"if ({conditionType})");
            AppendLine("{");
            _indentLevel++;

            // True branch
            var trueConnection = _graph.Connections.FirstOrDefault(c => c.SourceNodeId == node.Id && c.SourcePortId == "true");
            if (trueConnection != null)
            {
                var trueNode = _graph.Nodes.FirstOrDefault(n => n.Id == trueConnection.TargetNodeId);
                if (trueNode != null)
                {
                    GenerateNodeLogic(trueNode);
                }
            }

            _indentLevel--;
            AppendLine("}");
            AppendLine("else");
            AppendLine("{");
            _indentLevel++;

            // False branch
            var falseConnection = _graph.Connections.FirstOrDefault(c => c.SourceNodeId == node.Id && c.SourcePortId == "false");
            if (falseConnection != null)
            {
                var falseNode = _graph.Nodes.FirstOrDefault(n => n.Id == falseConnection.TargetNodeId);
                if (falseNode != null)
                {
                    GenerateNodeLogic(falseNode);
                }
            }

            _indentLevel--;
            AppendLine("}");
        }

        private void GenerateVariableNodeCode(GraphNode node)
        {
            string variableName = node.Data.ContainsKey("VariableName") ? node.Data["VariableName"].ToString() : "Unknown";
            AppendLine($"// Variable: {variableName}");
        }

        private void GenerateMathNodeCode(GraphNode node)
        {
            string operation = node.Data.ContainsKey("Operation") ? node.Data["Operation"].ToString() : "Unknown";
            AppendLine($"// Math: {operation}");
            
            switch (operation.ToLower())
            {
                case "add":
                    AppendLine("var result = a + b;");
                    break;
                case "subtract":
                    AppendLine("var result = a - b;");
                    break;
                case "multiply":
                    AppendLine("var result = a * b;");
                    break;
                case "divide":
                    AppendLine("var result = a / b;");
                    break;
                default:
                    AppendLine($"var result = {operation}(a, b);");
                    break;
            }
        }

        private void GenerateFlowNodeCode(GraphNode node)
        {
            string flowType = node.Data.ContainsKey("FlowType") ? node.Data["FlowType"].ToString() : "Unknown";
            
            switch (flowType.ToLower())
            {
                case "branch":
                    AppendLine("// Branch");
                    AppendLine("if (condition)");
                    AppendLine("{");
                    _indentLevel++;
                    
                    var trueConn = _graph.Connections.FirstOrDefault(c => c.SourceNodeId == node.Id && c.SourcePortId == "true");
                    if (trueConn != null)
                    {
                        var trueNode = _graph.Nodes.FirstOrDefault(n => n.Id == trueConn.TargetNodeId);
                        if (trueNode != null) GenerateNodeLogic(trueNode);
                    }
                    
                    _indentLevel--;
                    AppendLine("}");
                    break;
                    
                case "loop":
                    AppendLine("// Loop");
                    AppendLine("while (condition)");
                    AppendLine("{");
                    _indentLevel++;
                    
                    var bodyConn = _graph.Connections.FirstOrDefault(c => c.SourceNodeId == node.Id && c.SourcePortId == "body");
                    if (bodyConn != null)
                    {
                        var bodyNode = _graph.Nodes.FirstOrDefault(n => n.Id == bodyConn.TargetNodeId);
                        if (bodyNode != null) GenerateNodeLogic(bodyNode);
                    }
                    
                    _indentLevel--;
                    AppendLine("}");
                    break;
                    
                default:
                    AppendLine($"// Flow: {flowType}");
                    break;
            }
        }

        private void GenerateScriptNodeCode(ScriptNode node)
        {
            AppendLine($"// Script: {node.ClassName}");
            if (!string.IsNullOrEmpty(node.ScriptCode))
            {
                AppendLine(node.ScriptCode);
            }
            else
            {
                AppendLine(node.GenerateFullScript());
            }
        }

        private string VariableTypeToCSharpType(VariableType type)
        {
            return type switch
            {
                VariableType.Number => "float",
                VariableType.Boolean => "bool",
                VariableType.String => "string",
                VariableType.Vector3 => "Vector3",
                VariableType.GameObject => "GameObject",
                VariableType.Entity => "SimEntity",
                _ => "object"
            };
        }

        private string GetDefaultValue(VariableType type)
        {
            return type switch
            {
                VariableType.Number => "0f",
                VariableType.Boolean => "false",
                VariableType.String => "\"\"",
                VariableType.Vector3 => "Vector3.Zero",
                VariableType.GameObject => "null",
                VariableType.Entity => "null",
                _ => "null"
            };
        }

        private string SanitizeName(string name)
        {
            // Remove invalid characters and ensure it starts with a letter
            var sanitized = Regex.Replace(name, @"[^a-zA-Z0-9_]", "_");
            if (char.IsDigit(sanitized[0]))
            {
                sanitized = "_" + sanitized;
            }
            return sanitized;
        }

        private void AppendLine(string line = "")
        {
            string indent = new string(' ', _indentLevel * 4);
            _codeBuilder.AppendLine(indent + line);
        }

        public string GenerateGraphSummary()
        {
            var summary = new StringBuilder();
            summary.AppendLine($"Graph: {_graph.GraphName}");
            summary.AppendLine($"Nodes: {_graph.Nodes.Count}");
            summary.AppendLine($"Connections: {_graph.Connections.Count}");
            summary.AppendLine($"Variables: {_graph.Variables.Count}");
            summary.AppendLine();
            
            foreach (var node in _graph.Nodes)
            {
                summary.AppendLine($"- {node.NodeType}: {node.Title}");
            }
            
            return summary.ToString();
        }
    }
}
