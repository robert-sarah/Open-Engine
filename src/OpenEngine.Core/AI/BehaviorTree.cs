// Created By Levi Enama
using System;
using System.Collections.Generic;

namespace OpenEngine.Core.AI
{
    public enum NodeStatus
    {
        Success,
        Failure,
        Running
    }

    public abstract class BehaviorNode
    {
        public string Name { get; set; }
        public BehaviorNode Parent { get; set; }
        public List<BehaviorNode> Children { get; set; }

        protected BehaviorNode()
        {
            Children = new List<BehaviorNode>();
        }

        public virtual NodeStatus Execute()
        {
            return NodeStatus.Failure;
        }

        public virtual void Reset()
        {
            foreach (var child in Children)
            {
                child.Reset();
            }
        }

        public void AddChild(BehaviorNode child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        public void RemoveChild(BehaviorNode child)
        {
            child.Parent = null;
            Children.Remove(child);
        }
    }

    public class SelectorNode : BehaviorNode
    {
        private int _currentChildIndex;

        public SelectorNode()
        {
            Name = "Selector";
        }

        public override NodeStatus Execute()
        {
            for (int i = _currentChildIndex; i < Children.Count; i++)
            {
                var status = Children[i].Execute();

                if (status == NodeStatus.Running)
                {
                    _currentChildIndex = i;
                    return NodeStatus.Running;
                }

                if (status == NodeStatus.Success)
                {
                    _currentChildIndex = 0;
                    return NodeStatus.Success;
                }
            }

            _currentChildIndex = 0;
            return NodeStatus.Failure;
        }

        public override void Reset()
        {
            base.Reset();
            _currentChildIndex = 0;
        }
    }

    public class SequenceNode : BehaviorNode
    {
        private int _currentChildIndex;

        public SequenceNode()
        {
            Name = "Sequence";
        }

        public override NodeStatus Execute()
        {
            for (int i = _currentChildIndex; i < Children.Count; i++)
            {
                var status = Children[i].Execute();

                if (status == NodeStatus.Running)
                {
                    _currentChildIndex = i;
                    return NodeStatus.Running;
                }

                if (status == NodeStatus.Failure)
                {
                    _currentChildIndex = 0;
                    return NodeStatus.Failure;
                }
            }

            _currentChildIndex = 0;
            return NodeStatus.Success;
        }

        public override void Reset()
        {
            base.Reset();
            _currentChildIndex = 0;
        }
    }

    public class ParallelNode : BehaviorNode
    {
        public enum Policy
        {
            RequireOne,
            RequireAll
        }

        public Policy SuccessPolicy { get; set; }
        public Policy FailurePolicy { get; set; }

        public ParallelNode()
        {
            Name = "Parallel";
            SuccessPolicy = Policy.RequireAll;
            FailurePolicy = Policy.RequireOne;
        }

        public override NodeStatus Execute()
        {
            int successCount = 0;
            int failureCount = 0;
            int runningCount = 0;

            foreach (var child in Children)
            {
                var status = child.Execute();

                switch (status)
                {
                    case NodeStatus.Success:
                        successCount++;
                        break;
                    case NodeStatus.Failure:
                        failureCount++;
                        break;
                    case NodeStatus.Running:
                        runningCount++;
                        break;
                }
            }

            // Check failure condition
            if (FailurePolicy == Policy.RequireOne && failureCount > 0)
            {
                return NodeStatus.Failure;
            }

            if (FailurePolicy == Policy.RequireAll && failureCount == Children.Count)
            {
                return NodeStatus.Failure;
            }

            // Check success condition
            if (SuccessPolicy == Policy.RequireOne && successCount > 0)
            {
                return NodeStatus.Success;
            }

            if (SuccessPolicy == Policy.RequireAll && successCount == Children.Count)
            {
                return NodeStatus.Success;
            }

            // Still running
            if (runningCount > 0)
            {
                return NodeStatus.Running;
            }

            return NodeStatus.Failure;
        }
    }

    public class DecoratorNode : BehaviorNode
    {
        protected BehaviorNode Child;

        public DecoratorNode(BehaviorNode child)
        {
            Name = "Decorator";
            Child = child;
            if (child != null)
            {
                AddChild(child);
            }
        }
    }

    public class InverterNode : DecoratorNode
    {
        public InverterNode(BehaviorNode child) : base(child)
        {
            Name = "Inverter";
        }

        public override NodeStatus Execute()
        {
            var status = Child?.Execute() ?? NodeStatus.Failure;

            if (status == NodeStatus.Success)
            {
                return NodeStatus.Failure;
            }

            if (status == NodeStatus.Failure)
            {
                return NodeStatus.Success;
            }

            return NodeStatus.Running;
        }
    }

    public class RepeaterNode : DecoratorNode
    {
        public int RepeatCount { get; set; }
        private int _currentCount;

        public RepeaterNode(BehaviorNode child, int repeatCount = -1) : base(child)
        {
            Name = "Repeater";
            RepeatCount = repeatCount;
            _currentCount = 0;
        }

        public override NodeStatus Execute()
        {
            if (RepeatCount > 0 && _currentCount >= RepeatCount)
            {
                _currentCount = 0;
                return NodeStatus.Success;
            }

            var status = Child?.Execute() ?? NodeStatus.Failure;

            if (status == NodeStatus.Success)
            {
                _currentCount++;
                if (RepeatCount > 0 && _currentCount >= RepeatCount)
                {
                    _currentCount = 0;
                    return NodeStatus.Success;
                }
                return NodeStatus.Running;
            }

            return status;
        }

        public override void Reset()
        {
            base.Reset();
            _currentCount = 0;
        }
    }

    public class ConditionNode : BehaviorNode
    {
        public Func<bool> Condition { get; set; }

        public ConditionNode(Func<bool> condition)
        {
            Name = "Condition";
            Condition = condition;
        }

        public override NodeStatus Execute()
        {
            return Condition?.Invoke() == true ? NodeStatus.Success : NodeStatus.Failure;
        }
    }

    public class ActionNode : BehaviorNode
    {
        public Func<NodeStatus> Action { get; set; }

        public ActionNode(Func<NodeStatus> action)
        {
            Name = "Action";
            Action = action;
        }

        public override NodeStatus Execute()
        {
            return Action?.Invoke() ?? NodeStatus.Failure;
        }
    }

    public class WaitNode : BehaviorNode
    {
        public float Duration { get; set; }
        private float _elapsedTime;

        public WaitNode(float duration)
        {
            Name = "Wait";
            Duration = duration;
            _elapsedTime = 0f;
        }

        public override NodeStatus Execute()
        {
            _elapsedTime += 0.016f; // Assuming 60 FPS

            if (_elapsedTime >= Duration)
            {
                _elapsedTime = 0f;
                return NodeStatus.Success;
            }

            return NodeStatus.Running;
        }

        public override void Reset()
        {
            base.Reset();
            _elapsedTime = 0f;
        }
    }

    public class BehaviorTree
    {
        public BehaviorNode Root { get; set; }
        public Entities.SimEntity Entity { get; set; }
        public bool IsRunning { get; set; }

        public BehaviorTree(Entities.SimEntity entity)
        {
            Entity = entity;
            IsRunning = true;
        }

        public void Update()
        {
            if (!IsRunning || Root == null) return;

            var status = Root.Execute();

            if (status == NodeStatus.Success || status == NodeStatus.Failure)
            {
                Root.Reset();
            }
        }

        public void Reset()
        {
            Root?.Reset();
        }
    }

    public class BehaviorTreeBuilder
    {
        private BehaviorNode _currentNode;
        private Stack<BehaviorNode> _nodeStack;

        public BehaviorTreeBuilder()
        {
            _nodeStack = new Stack<BehaviorNode>();
        }

        public BehaviorTreeBuilder Selector(string name = "Selector")
        {
            var selector = new SelectorNode { Name = name };
            AddNode(selector);
            return this;
        }

        public BehaviorTreeBuilder Sequence(string name = "Sequence")
        {
            var sequence = new SequenceNode { Name = name };
            AddNode(sequence);
            return this;
        }

        public BehaviorTreeBuilder Parallel(ParallelNode.Policy successPolicy, ParallelNode.Policy failurePolicy)
        {
            var parallel = new ParallelNode
            {
                Name = "Parallel",
                SuccessPolicy = successPolicy,
                FailurePolicy = failurePolicy
            };
            AddNode(parallel);
            return this;
        }

        public BehaviorTreeBuilder Condition(Func<bool> condition, string name = "Condition")
        {
            var conditionNode = new ConditionNode(condition) { Name = name };
            AddLeafNode(conditionNode);
            return this;
        }

        public BehaviorTreeBuilder Action(Func<NodeStatus> action, string name = "Action")
        {
            var actionNode = new ActionNode(action) { Name = name };
            AddLeafNode(actionNode);
            return this;
        }

        public BehaviorTreeBuilder Wait(float duration)
        {
            var waitNode = new WaitNode(duration);
            AddLeafNode(waitNode);
            return this;
        }

        public BehaviorTreeBuilder Inverter()
        {
            var inverter = new InverterNode(_currentNode);
            ReplaceCurrentNode(inverter);
            return this;
        }

        public BehaviorTreeBuilder Repeater(int count = -1)
        {
            var repeater = new RepeaterNode(_currentNode, count);
            ReplaceCurrentNode(repeater);
            return this;
        }

        public BehaviorTreeBuilder End()
        {
            if (_nodeStack.Count > 0)
            {
                _currentNode = _nodeStack.Pop();
            }
            return this;
        }

        public BehaviorTree Build(Entities.SimEntity entity)
        {
            var tree = new BehaviorTree(entity);
            tree.Root = _currentNode;
            return tree;
        }

        private void AddNode(BehaviorNode node)
        {
            if (_currentNode != null)
            {
                _currentNode.AddChild(node);
                _nodeStack.Push(_currentNode);
            }
            _currentNode = node;
        }

        private void AddLeafNode(BehaviorNode node)
        {
            if (_currentNode != null)
            {
                _currentNode.AddChild(node);
            }
        }

        private void ReplaceCurrentNode(BehaviorNode newNode)
        {
            if (_nodeStack.Count > 0)
            {
                var parent = _nodeStack.Peek();
                parent.RemoveChild(_currentNode);
                parent.AddChild(newNode);
            }
            _currentNode = newNode;
        }
    }
}
