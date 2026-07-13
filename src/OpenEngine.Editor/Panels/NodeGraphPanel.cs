// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Linq;
using OpenEngine.Core.Entities;
using OpenEngine.Core.Engine;
using OpenEngine.Core.Math;

namespace OpenEngine.Editor.Panels
{
    public class GraphNode
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public Vector2 Position { get; set; }
        public NodeType NodeType { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public List<NodePort> InputPorts { get; set; }
        public List<NodePort> OutputPorts { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public bool IsSelected { get; set; }
        public bool IsEnabled { get; set; }
        public string Color { get; set; }
        public string GroupId { get; set; }
        public List<string> Tags { get; set; }

        public GraphNode()
        {
            Data = new Dictionary<string, object>();
            InputPorts = new List<NodePort>();
            OutputPorts = new List<NodePort>();
            IsEnabled = true;
            Color = "#4A90E2";
            Tags = new List<string>();
        }

        public NodePort GetPort(string portId)
        {
            return InputPorts.FirstOrDefault(p => p.Id == portId) ?? OutputPorts.FirstOrDefault(p => p.Id == portId);
        }

        public List<NodePort> GetAllPorts()
        {
            return InputPorts.Concat(OutputPorts).ToList();
        }
    }

    public class NodePort
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public PortType Type { get; set; }
        public PortDirection Direction { get; set; }
        public bool IsOptional { get; set; }
        public object DefaultValue { get; set; }
        public string Tooltip { get; set; }
        public bool IsConnected { get; set; }

        public NodePort()
        {
            IsOptional = false;
            Tooltip = "";
            IsConnected = false;
        }
    }

    public enum NodeType
    {
        Event,
        Action,
        Condition,
        Variable,
        Entity,
        Math,
        Logic,
        Flow,
        Comment,
        Group
    }

    public enum PortType
    {
        Flow,
        Data,
        Entity,
        Number,
        Boolean,
        String,
        Vector3,
        Color,
        GameObject,
        Any
    }

    public enum PortDirection
    {
        Input,
        Output
    }

    public class GraphConnection
    {
        public string Id { get; set; }
        public string SourceNodeId { get; set; }
        public string SourcePortId { get; set; }
        public string TargetNodeId { get; set; }
        public string TargetPortId { get; set; }
        public ConnectionType Type { get; set; }
        public string Color { get; set; }
        public bool IsSelected { get; set; }
        public List<Vector2> BezierPoints { get; set; }

        public GraphConnection()
        {
            Type = ConnectionType.Standard;
            Color = "#FFFFFF";
            BezierPoints = new List<Vector2>();
        }
    }

    public enum ConnectionType
    {
        Standard,
        Flow,
        Data,
        Event
    }

    public class GraphVariable
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public VariableType Type { get; set; }
        public bool IsGlobal { get; set; }
        public string Description { get; set; }

        public GraphVariable()
        {
            IsGlobal = false;
            Description = "";
        }
    }

    public enum VariableType
    {
        Number,
        Boolean,
        String,
        Vector3,
        GameObject,
        Entity
    }

    public class NodeGroup
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public string Color { get; set; }
        public bool IsCollapsed { get; set; }
        public List<string> NodeIds { get; set; }

        public NodeGroup()
        {
            Color = "#2C3E50";
            IsCollapsed = false;
            NodeIds = new List<string>();
        }
    }

    public class GraphComment
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public string Color { get; set; }
        public int FontSize { get; set; }

        public GraphComment()
        {
            Color = "#F39C12";
            FontSize = 14;
        }
    }

    public struct Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vector2 Zero => new Vector2(0, 0);
        public static Vector2 One => new Vector2(1, 1);
    }

    public class NodeGraphPanel
    {
        public OpenSimulationEngine Engine { get; }
        public List<GraphNode> Nodes { get; }
        public List<GraphConnection> Connections { get; }
        public List<GraphVariable> Variables { get; }
        public List<NodeGroup> Groups { get; }
        public List<GraphComment> Comments { get; }
        public string SelectedNodeId { get; set; }
        public List<string> SelectedNodeIds { get; set; }
        public string SelectedConnectionId { get; set; }
        public string GraphName { get; set; }
        public string GraphDescription { get; set; }
        public Vector2 PanOffset { get; set; }
        public float ZoomLevel { get; set; }
        public bool IsDirty { get; set; }

        private Stack<GraphAction> _undoStack;
        private Stack<GraphAction> _redoStack;
        private Dictionary<string, int> _nodeExecutionCount;

        public event EventHandler<string> OnNodeSelected;
        public event EventHandler<GraphNode> OnNodeAdded;
        public event EventHandler<GraphNode> OnNodeRemoved;
        public event EventHandler<GraphConnection> OnConnectionAdded;
        public event EventHandler<GraphConnection> OnConnectionRemoved;
        public event EventHandler OnGraphChanged;
        public event EventHandler<string> OnGraphExecuted;

        public NodeGraphPanel(OpenSimulationEngine engine)
        {
            Engine = engine;
            Nodes = new List<GraphNode>();
            Connections = new List<GraphConnection>();
            Variables = new List<GraphVariable>();
            Groups = new List<NodeGroup>();
            Comments = new List<GraphComment>();
            SelectedNodeIds = new List<string>();
            GraphName = "New Graph";
            GraphDescription = "";
            PanOffset = Vector2.Zero;
            ZoomLevel = 1f;
            _undoStack = new Stack<GraphAction>();
            _redoStack = new Stack<GraphAction>();
            _nodeExecutionCount = new Dictionary<string, int>();
        }

        public GraphNode CreateEventNode(string eventType, Vector2 position)
        {
            var node = new GraphNode
            {
                Id = Guid.NewGuid().ToString(),
                Title = $"On {eventType}",
                Position = position,
                NodeType = NodeType.Event,
                Category = "Events",
                Color = "#E74C3C",
                Description = $"Triggered when {eventType} occurs"
            };
            node.Data["EventType"] = eventType;
            node.OutputPorts.Add(new NodePort { Id = "exec", Name = "Exec", Type = PortType.Flow, Direction = PortDirection.Output });
            
            Nodes.Add(node);
            RecordAction(new GraphAction { Type = ActionType.AddNode, NodeId = node.Id });
            OnNodeAdded?.Invoke(this, node);
            MarkDirty();
            return node;
        }

        public GraphNode CreateActionNode(string actionType, Vector2 position)
        {
            var node = new GraphNode
            {
                Id = Guid.NewGuid().ToString(),
                Title = actionType,
                Position = position,
                NodeType = NodeType.Action,
                Category = "Actions",
                Color = "#27AE60",
                Description = $"Perform {actionType} action"
            };
            node.Data["ActionType"] = actionType;
            node.InputPorts.Add(new NodePort { Id = "exec", Name = "Exec", Type = PortType.Flow, Direction = PortDirection.Input });
            node.OutputPorts.Add(new NodePort { Id = "exec", Name = "Exec", Type = PortType.Flow, Direction = PortDirection.Output });
            
            Nodes.Add(node);
            RecordAction(new GraphAction { Type = ActionType.AddNode, NodeId = node.Id });
            OnNodeAdded?.Invoke(this, node);
            MarkDirty();
            return node;
        }

        public GraphNode CreateConditionNode(string conditionType, Vector2 position)
        {
            var node = new GraphNode
            {
                Id = Guid.NewGuid().ToString(),
                Title = conditionType,
                Position = position,
                NodeType = NodeType.Condition,
                Category = "Conditions",
                Color = "#F39C12",
                Description = $"Check {conditionType} condition"
            };
            node.Data["ConditionType"] = conditionType;
            node.InputPorts.Add(new NodePort { Id = "exec", Name = "Exec", Type = PortType.Flow, Direction = PortDirection.Input });
            node.InputPorts.Add(new NodePort { Id = "value", Name = "Value", Type = PortType.Any, Direction = PortDirection.Input });
            node.OutputPorts.Add(new NodePort { Id = "true", Name = "True", Type = PortType.Flow, Direction = PortDirection.Output });
            node.OutputPorts.Add(new NodePort { Id = "false", Name = "False", Type = PortType.Flow, Direction = PortDirection.Output });
            
            Nodes.Add(node);
            RecordAction(new GraphAction { Type = ActionType.AddNode, NodeId = node.Id });
            OnNodeAdded?.Invoke(this, node);
            MarkDirty();
            return node;
        }

        public GraphNode CreateVariableNode(string variableName, Vector2 position)
        {
            var node = new GraphNode
            {
                Id = Guid.NewGuid().ToString(),
                Title = variableName,
                Position = position,
                NodeType = NodeType.Variable,
                Category = "Variables",
                Color = "#9B59B6",
                Description = $"Access variable {variableName}"
            };
            node.Data["VariableName"] = variableName;
            node.OutputPorts.Add(new NodePort { Id = "get", Name = "Get", Type = PortType.Any, Direction = PortDirection.Output });
            node.InputPorts.Add(new NodePort { Id = "set", Name = "Set", Type = PortType.Any, Direction = PortDirection.Input });
            
            Nodes.Add(node);
            RecordAction(new GraphAction { Type = ActionType.AddNode, NodeId = node.Id });
            OnNodeAdded?.Invoke(this, node);
            MarkDirty();
            return node;
        }

        public GraphNode CreateMathNode(string operation, Vector2 position)
        {
            var node = new GraphNode
            {
                Id = Guid.NewGuid().ToString(),
                Title = operation,
                Position = position,
                NodeType = NodeType.Math,
                Category = "Math",
                Color = "#3498DB",
                Description = $"Perform {operation} operation"
            };
            node.Data["Operation"] = operation;
            node.InputPorts.Add(new NodePort { Id = "a", Name = "A", Type = PortType.Number, Direction = PortDirection.Input });
            node.InputPorts.Add(new NodePort { Id = "b", Name = "B", Type = PortType.Number, Direction = PortDirection.Input });
            node.OutputPorts.Add(new NodePort { Id = "result", Name = "Result", Type = PortType.Number, Direction = PortDirection.Output });
            
            Nodes.Add(node);
            RecordAction(new GraphAction { Type = ActionType.AddNode, NodeId = node.Id });
            OnNodeAdded?.Invoke(this, node);
            MarkDirty();
            return node;
        }

        public GraphNode CreateEntityNode(SimEntity entity, Vector2 position)
        {
            var node = new GraphNode
            {
                Id = Guid.NewGuid().ToString(),
                Title = entity.Name,
                Position = position,
                NodeType = NodeType.Entity,
                Category = "Entities",
                Color = "#1ABC9C",
                Description = "Entity reference"
            };
            node.Data["EntityId"] = entity.Id;
            node.OutputPorts.Add(new NodePort { Id = "entity", Name = "Entity", Type = PortType.Entity, Direction = PortDirection.Output });
            node.OutputPorts.Add(new NodePort { Id = "position", Name = "Position", Type = PortType.Vector3, Direction = PortDirection.Output });
            node.OutputPorts.Add(new NodePort { Id = "rotation", Name = "Rotation", Type = PortType.Vector3, Direction = PortDirection.Output });

            Nodes.Add(node);
            RecordAction(new GraphAction { Type = ActionType.AddNode, NodeId = node.Id });
            OnNodeAdded?.Invoke(this, node);
            MarkDirty();
            return node;
        }

        public GraphNode CreateLogicNode(string title, Vector2 position)
        {
            var node = new GraphNode
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Position = position,
                NodeType = NodeType.Logic,
                Category = "Logic",
                Color = "#95A5A6",
                Description = "Logic operation"
            };
            node.InputPorts.Add(new NodePort { Id = "in", Name = "In", Type = PortType.Flow, Direction = PortDirection.Input });
            node.OutputPorts.Add(new NodePort { Id = "out", Name = "Out", Type = PortType.Flow, Direction = PortDirection.Output });

            Nodes.Add(node);
            RecordAction(new GraphAction { Type = ActionType.AddNode, NodeId = node.Id });
            OnNodeAdded?.Invoke(this, node);
            MarkDirty();
            return node;
        }

        public GraphNode CreateFlowNode(string flowType, Vector2 position)
        {
            var node = new GraphNode
            {
                Id = Guid.NewGuid().ToString(),
                Title = flowType,
                Position = position,
                NodeType = NodeType.Flow,
                Category = "Flow",
                Color = "#E67E22",
                Description = $"Flow control: {flowType}"
            };
            node.Data["FlowType"] = flowType;
            
            if (flowType == "Branch")
            {
                node.InputPorts.Add(new NodePort { Id = "exec", Name = "Exec", Type = PortType.Flow, Direction = PortDirection.Input });
                node.InputPorts.Add(new NodePort { Id = "condition", Name = "Condition", Type = PortType.Boolean, Direction = PortDirection.Input });
                node.OutputPorts.Add(new NodePort { Id = "true", Name = "True", Type = PortType.Flow, Direction = PortDirection.Output });
                node.OutputPorts.Add(new NodePort { Id = "false", Name = "False", Type = PortType.Flow, Direction = PortDirection.Output });
            }
            else if (flowType == "Loop")
            {
                node.InputPorts.Add(new NodePort { Id = "exec", Name = "Exec", Type = PortType.Flow, Direction = PortDirection.Input });
                node.InputPorts.Add(new NodePort { Id = "condition", Name = "Condition", Type = PortType.Boolean, Direction = PortDirection.Input });
                node.InputPorts.Add(new NodePort { Id = "body", Name = "Body", Type = PortType.Flow, Direction = PortDirection.Input });
                node.OutputPorts.Add(new NodePort { Id = "completed", Name = "Completed", Type = PortType.Flow, Direction = PortDirection.Output });
            }
            else
            {
                node.InputPorts.Add(new NodePort { Id = "exec", Name = "Exec", Type = PortType.Flow, Direction = PortDirection.Input });
                node.OutputPorts.Add(new NodePort { Id = "exec", Name = "Exec", Type = PortType.Flow, Direction = PortDirection.Output });
            }

            Nodes.Add(node);
            RecordAction(new GraphAction { Type = ActionType.AddNode, NodeId = node.Id });
            OnNodeAdded?.Invoke(this, node);
            MarkDirty();
            return node;
        }

        public GraphComment CreateCommentNode(string text, Vector2 position)
        {
            var comment = new GraphComment
            {
                Id = Guid.NewGuid().ToString(),
                Text = text,
                Position = position,
                Size = new Vector2(200, 100)
            };
            Comments.Add(comment);
            MarkDirty();
            return comment;
        }

        public ScriptNode CreateScriptNode(string scriptName, Vector2 position)
        {
            var node = new ScriptNode
            {
                Id = Guid.NewGuid().ToString(),
                Title = scriptName,
                Position = position,
                ClassName = scriptName.Replace(" ", "")
            };
            
            node.InputPorts.Add(new NodePort { Id = "exec", Name = "Exec", Type = PortType.Flow, Direction = PortDirection.Input });
            node.OutputPorts.Add(new NodePort { Id = "exec", Name = "Exec", Type = PortType.Flow, Direction = PortDirection.Output });
            node.InputPorts.Add(new NodePort { Id = "entity", Name = "Entity", Type = PortType.Entity, Direction = PortDirection.Input, IsOptional = true });
            
            Nodes.Add(node);
            RecordAction(new GraphAction { Type = ActionType.AddNode, NodeId = node.Id });
            OnNodeAdded?.Invoke(this, node);
            MarkDirty();
            return node;
        }

        public GameActionNode CreateGameActionNode(string actionType, Vector2 position)
        {
            var node = new GameActionNode
            {
                Id = Guid.NewGuid().ToString(),
                Title = actionType,
                Position = position,
                ActionType = actionType,
                ClassName = $"{actionType}Action"
            };

            // Set script template based on action type
            switch (actionType.ToLower())
            {
                case "moveto":
                    node.ScriptCode = GameActionTemplates.MoveToTarget;
                    break;
                case "attack":
                    node.ScriptCode = GameActionTemplates.AttackTarget;
                    break;
                case "interact":
                    node.ScriptCode = GameActionTemplates.InteractWithObject;
                    break;
                case "spawn":
                    node.ScriptCode = GameActionTemplates.SpawnEntity;
                    break;
                case "destroy":
                    node.ScriptCode = GameActionTemplates.DestroyEntity;
                    break;
                case "setstate":
                    node.ScriptCode = GameActionTemplates.ChangeEntityState;
                    break;
                case "playanimation":
                    node.ScriptCode = GameActionTemplates.PlayAnimation;
                    break;
                case "playsound":
                    node.ScriptCode = GameActionTemplates.PlaySound;
                    break;
                case "setvariable":
                    node.ScriptCode = GameActionTemplates.SetVariable;
                    break;
                case "getvariable":
                    node.ScriptCode = GameActionTemplates.GetVariable;
                    break;
                case "wait":
                    node.ScriptCode = GameActionTemplates.WaitForSeconds;
                    break;
                case "log":
                    node.ScriptCode = GameActionTemplates.LogMessage;
                    break;
                default:
                    node.ScriptCode = "// Custom action\nConsole.WriteLine(\"Executing custom action\");";
                    break;
            }

            node.InputPorts.Add(new NodePort { Id = "exec", Name = "Exec", Type = PortType.Flow, Direction = PortDirection.Input });
            node.OutputPorts.Add(new NodePort { Id = "exec", Name = "Exec", Type = PortType.Flow, Direction = PortDirection.Output });
            node.InputPorts.Add(new NodePort { Id = "entity", Name = "Entity", Type = PortType.Entity, Direction = PortDirection.Input, IsOptional = true });

            Nodes.Add(node);
            RecordAction(new GraphAction { Type = ActionType.AddNode, NodeId = node.Id });
            OnNodeAdded?.Invoke(this, node);
            MarkDirty();
            return node;
        }

        public bool CompileScriptNode(string nodeId)
        {
            var node = Nodes.FirstOrDefault(n => n.Id == nodeId) as ScriptNode;
            if (node == null) return false;

            return node.Compile();
        }

        public void ExecuteScriptNode(string nodeId)
        {
            var node = Nodes.FirstOrDefault(n => n.Id == nodeId) as ScriptNode;
            if (node == null) return;

            if (!node.IsCompiled)
            {
                node.Compile();
            }

            node.Execute();
        }

        public void CompileAllScripts()
        {
            foreach (var node in Nodes.OfType<ScriptNode>())
            {
                node.Compile();
            }
        }

        public bool CanConnect(string sourceNodeId, string sourcePortId, string targetNodeId, string targetPortId)
        {
            if (sourceNodeId == targetNodeId) return false;
            
            var sourceNode = Nodes.FirstOrDefault(n => n.Id == sourceNodeId);
            var targetNode = Nodes.FirstOrDefault(n => n.Id == targetNodeId);
            
            if (sourceNode == null || targetNode == null) return false;
            
            var sourcePort = sourceNode.GetPort(sourcePortId);
            var targetPort = targetNode.GetPort(targetPortId);
            
            if (sourcePort == null || targetPort == null) return false;
            if (sourcePort.Direction != PortDirection.Output) return false;
            if (targetPort.Direction != PortDirection.Input) return false;
            
            if (!AreTypesCompatible(sourcePort.Type, targetPort.Type)) return false;
            
            if (Connections.Any(c => c.SourceNodeId == sourceNodeId && c.SourcePortId == sourcePortId && c.TargetNodeId == targetNodeId && c.TargetPortId == targetPortId))
                return false;
            
            if (targetPort.Direction == PortDirection.Input && !targetPort.IsOptional)
            {
                if (Connections.Any(c => c.TargetNodeId == targetNodeId && c.TargetPortId == targetPortId))
                    return false;
            }
            
            return true;
        }

        private bool AreTypesCompatible(PortType source, PortType target)
        {
            if (source == PortType.Any || target == PortType.Any) return true;
            if (source == target) return true;
            
            if (source == PortType.Number && (target == PortType.Number || target == PortType.Vector3)) return true;
            if (target == PortType.Number && (source == PortType.Number || source == PortType.Vector3)) return true;
            
            return false;
        }

        public GraphConnection ConnectNodes(string sourceNodeId, string sourcePortId, string targetNodeId, string targetPortId)
        {
            if (!CanConnect(sourceNodeId, sourcePortId, targetNodeId, targetPortId))
            {
                Console.WriteLine("Cannot connect these ports");
                return null;
            }

            var connection = new GraphConnection
            {
                Id = Guid.NewGuid().ToString(),
                SourceNodeId = sourceNodeId,
                SourcePortId = sourcePortId,
                TargetNodeId = targetNodeId,
                TargetPortId = targetPortId
            };

            var sourceNode = Nodes.FirstOrDefault(n => n.Id == sourceNodeId);
            var targetNode = Nodes.FirstOrDefault(n => n.Id == targetNodeId);
            
            if (sourceNode != null)
            {
                var sourcePort = sourceNode.GetPort(sourcePortId);
                if (sourcePort != null) sourcePort.IsConnected = true;
            }
            
            if (targetNode != null)
            {
                var targetPort = targetNode.GetPort(targetPortId);
                if (targetPort != null) targetPort.IsConnected = true;
            }

            Connections.Add(connection);
            RecordAction(new GraphAction { Type = ActionType.AddConnection, ConnectionId = connection.Id });
            OnConnectionAdded?.Invoke(this, connection);
            MarkDirty();
            return connection;
        }

        public void DisconnectNodes(string connectionId)
        {
            var connection = Connections.FirstOrDefault(c => c.Id == connectionId);
            if (connection == null) return;

            var sourceNode = Nodes.FirstOrDefault(n => n.Id == connection.SourceNodeId);
            var targetNode = Nodes.FirstOrDefault(n => n.Id == connection.TargetNodeId);
            
            if (sourceNode != null)
            {
                var sourcePort = sourceNode.GetPort(connection.SourcePortId);
                if (sourcePort != null) sourcePort.IsConnected = false;
            }
            
            if (targetNode != null)
            {
                var targetPort = targetNode.GetPort(connection.TargetPortId);
                if (targetPort != null) targetPort.IsConnected = false;
            }

            Connections.Remove(connection);
            RecordAction(new GraphAction { Type = ActionType.RemoveConnection, ConnectionId = connectionId, Connection = connection });
            OnConnectionRemoved?.Invoke(this, connection);
            MarkDirty();
        }

        public void RemoveNode(string nodeId)
        {
            var node = Nodes.FirstOrDefault(n => n.Id == nodeId);
            if (node == null) return;

            var connectionsToRemove = Connections.Where(c => c.SourceNodeId == nodeId || c.TargetNodeId == nodeId).ToList();
            foreach (var conn in connectionsToRemove)
            {
                DisconnectNodes(conn.Id);
            }

            Nodes.Remove(node);
            RecordAction(new GraphAction { Type = ActionType.RemoveNode, NodeId = nodeId, Node = node });
            OnNodeRemoved?.Invoke(this, node);
            MarkDirty();
        }

        public void SelectNode(string nodeId)
        {
            SelectedNodeId = nodeId;
            SelectedNodeIds.Clear();
            if (!string.IsNullOrEmpty(nodeId))
            {
                SelectedNodeIds.Add(nodeId);
                var node = Nodes.FirstOrDefault(n => n.Id == nodeId);
                if (node != null)
                {
                    node.IsSelected = true;
                }
            }
            OnNodeSelected?.Invoke(this, nodeId);
        }

        public void SelectMultipleNodes(List<string> nodeIds)
        {
            DeselectAll();
            SelectedNodeIds = nodeIds;
            foreach (var nodeId in nodeIds)
            {
                var node = Nodes.FirstOrDefault(n => n.Id == nodeId);
                if (node != null) node.IsSelected = true;
            }
        }

        public void DeselectAll()
        {
            foreach (var node in Nodes) node.IsSelected = false;
            foreach (var conn in Connections) conn.IsSelected = false;
            SelectedNodeIds.Clear();
            SelectedNodeId = null;
            SelectedConnectionId = null;
        }

        public void DuplicateNodes(List<string> nodeIds)
        {
            var offset = new Vector2(50, 50);
            var oldToNewIds = new Dictionary<string, string>();

            foreach (var nodeId in nodeIds)
            {
                var oldNode = Nodes.FirstOrDefault(n => n.Id == nodeId);
                if (oldNode == null) continue;

                var newNode = new GraphNode
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = oldNode.Title + " (Copy)",
                    Position = new Vector2(oldNode.Position.X + offset.X, oldNode.Position.Y + offset.Y),
                    NodeType = oldNode.NodeType,
                    Category = oldNode.Category,
                    Color = oldNode.Color,
                    Description = oldNode.Description
                };

                foreach (var kvp in oldNode.Data)
                {
                    newNode.Data[kvp.Key] = kvp.Value;
                }

                foreach (var port in oldNode.InputPorts)
                {
                    newNode.InputPorts.Add(new NodePort
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = port.Name,
                        Type = port.Type,
                        Direction = port.Direction,
                        IsOptional = port.IsOptional,
                        DefaultValue = port.DefaultValue,
                        Tooltip = port.Tooltip
                    });
                }

                foreach (var port in oldNode.OutputPorts)
                {
                    newNode.OutputPorts.Add(new NodePort
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = port.Name,
                        Type = port.Type,
                        Direction = port.Direction,
                        IsOptional = port.IsOptional,
                        DefaultValue = port.DefaultValue,
                        Tooltip = port.Tooltip
                    });
                }

                oldToNewIds[nodeId] = newNode.Id;
                Nodes.Add(newNode);
                OnNodeAdded?.Invoke(this, newNode);
            }

            foreach (var conn in Connections)
            {
                if (oldToNewIds.ContainsKey(conn.SourceNodeId) && oldToNewIds.ContainsKey(conn.TargetNodeId))
                {
                    var sourceNode = Nodes.FirstOrDefault(n => n.Id == oldToNewIds[conn.SourceNodeId]);
                    var targetNode = Nodes.FirstOrDefault(n => n.Id == oldToNewIds[conn.TargetNodeId]);
                    
                    if (sourceNode != null && targetNode != null)
                    {
                        var sourcePort = sourceNode.OutputPorts.FirstOrDefault(p => p.Name == Nodes.FirstOrDefault(n => n.Id == conn.SourceNodeId)?.GetPort(conn.SourcePortId)?.Name);
                        var targetPort = targetNode.InputPorts.FirstOrDefault(p => p.Name == Nodes.FirstOrDefault(n => n.Id == conn.TargetNodeId)?.GetPort(conn.TargetPortId)?.Name);
                        
                        if (sourcePort != null && targetPort != null)
                        {
                            ConnectNodes(oldToNewIds[conn.SourceNodeId], sourcePort.Id, oldToNewIds[conn.TargetNodeId], targetPort.Id);
                        }
                    }
                }
            }

            MarkDirty();
        }

        public void DeleteSelected()
        {
            foreach (var nodeId in new List<string>(SelectedNodeIds))
            {
                RemoveNode(nodeId);
            }
            DeselectAll();
        }

        public void SyncWithEngine()
        {
            var engineNodes = Nodes.Where(n => n.NodeType == NodeType.Entity).ToList();

            foreach (var node in engineNodes)
            {
                if (node.Data.TryGetValue("EntityId", out object entityIdObj)
                    && entityIdObj is string entityId
                    && Engine.Entities.TryGetValue(entityId, out SimEntity entity))
                {
                    node.Title = entity.Name;
                }
            }
        }

        public void ExecuteGraph()
        {
            _nodeExecutionCount.Clear();
            var eventNodes = Nodes.Where(n => n.NodeType == NodeType.Event).ToList();
            
            foreach (var eventNode in eventNodes)
            {
                ExecuteNode(eventNode);
            }
            
            OnGraphExecuted?.Invoke(this, "Graph executed successfully");
        }

        private void ExecuteNode(GraphNode node)
        {
            if (!node.IsEnabled) return;
            
            _nodeExecutionCount.TryGetValue(node.Id, out var count);
            if (count > 1000)
            {
                Console.WriteLine($"Node {node.Title} exceeded execution limit");
                return;
            }
            _nodeExecutionCount[node.Id] = count + 1;

            var outputConnections = Connections.Where(c => c.SourceNodeId == node.Id).ToList();
            
            foreach (var conn in outputConnections)
            {
                var targetNode = Nodes.FirstOrDefault(n => n.Id == conn.TargetNodeId);
                if (targetNode != null)
                {
                    ExecuteNode(targetNode);
                }
            }
        }

        public void AddVariable(string name, object value, VariableType type)
        {
            Variables.Add(new GraphVariable
            {
                Name = name,
                Value = value,
                Type = type
            });
            MarkDirty();
        }

        public object GetVariableValue(string name)
        {
            var variable = Variables.FirstOrDefault(v => v.Name == name);
            return variable?.Value;
        }

        public void SetVariableValue(string name, object value)
        {
            var variable = Variables.FirstOrDefault(v => v.Name == name);
            if (variable != null)
            {
                variable.Value = value;
                MarkDirty();
            }
        }

        public void Undo()
        {
            if (_undoStack.Count == 0) return;
            
            var action = _undoStack.Pop();
            _redoStack.Push(action);
            
            switch (action.Type)
            {
                case ActionType.AddNode:
                    RemoveNode(action.NodeId);
                    break;
                case ActionType.RemoveNode:
                    Nodes.Add(action.Node);
                    OnNodeAdded?.Invoke(this, action.Node);
                    break;
                case ActionType.AddConnection:
                    DisconnectNodes(action.ConnectionId);
                    break;
                case ActionType.RemoveConnection:
                    Connections.Add(action.Connection);
                    OnConnectionAdded?.Invoke(this, action.Connection);
                    break;
            }
            
            MarkDirty();
        }

        public void Redo()
        {
            if (_redoStack.Count == 0) return;
            
            var action = _redoStack.Pop();
            _undoStack.Push(action);
            
            switch (action.Type)
            {
                case ActionType.AddNode:
                    Nodes.Add(action.Node);
                    OnNodeAdded?.Invoke(this, action.Node);
                    break;
                case ActionType.RemoveNode:
                    RemoveNode(action.NodeId);
                    break;
                case ActionType.AddConnection:
                    Connections.Add(action.Connection);
                    OnConnectionAdded?.Invoke(this, action.Connection);
                    break;
                case ActionType.RemoveConnection:
                    DisconnectNodes(action.ConnectionId);
                    break;
            }
            
            MarkDirty();
        }

        private void RecordAction(GraphAction action)
        {
            _undoStack.Push(action);
            _redoStack.Clear();
        }

        public void ClearGraph()
        {
            Nodes.Clear();
            Connections.Clear();
            Variables.Clear();
            Groups.Clear();
            Comments.Clear();
            _undoStack.Clear();
            _redoStack.Clear();
            MarkDirty();
        }

        public void MarkDirty()
        {
            IsDirty = true;
            OnGraphChanged?.Invoke(this, EventArgs.Empty);
        }

        public void MarkClean()
        {
            IsDirty = false;
        }
    }

    public enum ActionType
    {
        AddNode,
        RemoveNode,
        AddConnection,
        RemoveConnection
    }

    public class GraphAction
    {
        public ActionType Type { get; set; }
        public string NodeId { get; set; }
        public GraphNode Node { get; set; }
        public string ConnectionId { get; set; }
        public GraphConnection Connection { get; set; }
    }
}
