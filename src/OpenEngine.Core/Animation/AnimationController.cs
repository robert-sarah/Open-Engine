// Created By Levi Enama
using System;
using System.Collections.Generic;

namespace OpenEngine.Core.Animation
{
    public class AnimatorControllerParameter
    {
        public string Name { get; set; }
        public AnimatorControllerParameterType Type { get; set; }
        public object DefaultValue { get; set; }
        public object Value { get; set; }

        public AnimatorControllerParameter(string name, AnimatorControllerParameterType type, object defaultValue)
        {
            Name = name;
            Type = type;
            DefaultValue = defaultValue;
            Value = defaultValue;
        }
    }

    public enum AnimatorControllerParameterType
    {
        Float,
        Int,
        Bool,
        Trigger
    }

    public class AnimationController
    {
        public List<AnimatorControllerParameter> Parameters { get; set; }
        public List<AnimatorState> States { get; set; }
        public List<AnimatorTransition> Transitions { get; set; }
        public List<AnimatorLayer> Layers { get; set; }

        private Dictionary<string, object> _parameterValues;

        public AnimationController()
        {
            Parameters = new List<AnimatorControllerParameter>();
            States = new List<AnimatorState>();
            Transitions = new List<AnimatorTransition>();
            Layers = new List<AnimatorLayer>();
            _parameterValues = new Dictionary<string, object>();
        }

        public void AddParameter(AnimatorControllerParameter parameter)
        {
            Parameters.Add(parameter);
            _parameterValues[parameter.Name] = parameter.DefaultValue;
        }

        public void SetParameter(string name, object value)
        {
            if (_parameterValues.ContainsKey(name))
            {
                _parameterValues[name] = value;
            }
        }

        public void SetTrigger(string name)
        {
            SetParameter(name, true);
        }

        public void ResetTrigger(string name)
        {
            SetParameter(name, false);
        }

        public bool GetBool(string name)
        {
            return _parameterValues.ContainsKey(name) && (bool)_parameterValues[name];
        }

        public float GetFloat(string name)
        {
            return _parameterValues.ContainsKey(name) ? (float)_parameterValues[name] : 0f;
        }

        public int GetInteger(string name)
        {
            return _parameterValues.ContainsKey(name) ? (int)_parameterValues[name] : 0;
        }

        public void Update(float deltaTime)
        {
            // Update current state animation time
            foreach (var state in States)
            {
                state.Update(deltaTime);
            }
            
            // Evaluate transitions and update current state
            EvaluateTransitions();
        }

        public AnimatorState GetCurrentState()
        {
            return States.Count > 0 ? States[0] : null;
        }

        public void SetState(string stateName)
        {
            var state = States.Find(s => s.Name == stateName);
            if (state != null)
            {
                state.Reset();
            }
        }

        private void EvaluateTransitions()
        {
            // Check conditions for state transitions
            foreach (var transition in Transitions)
            {
                if (transition.CanTransition(_parameterValues))
                {
                    // Execute transition
                    break;
                }
            }
        }
    }

    public class AnimatorState
    {
        public string Name { get; set; }
        public AnimationClip Motion { get; set; }
        public float Speed { get; set; }
        public bool Mirror { get; set; }
        public Dictionary<string, AvatarMask> AvatarMask { get; set; }

        public AnimatorState()
        {
            Speed = 1f;
            Mirror = false;
            AvatarMask = new Dictionary<string, AvatarMask>();
        }
    }

    public class AnimatorTransition
    {
        public string FromState { get; set; }
        public string ToState { get; set; }
        public List<TransitionCondition> Conditions { get; set; }
        public float ExitTime { get; set; }
        public float TransitionDuration { get; set; }
        public bool HasExitTime { get; set; }

        public AnimatorTransition()
        {
            Conditions = new List<TransitionCondition>();
            ExitTime = 0f;
            TransitionDuration = 0.25f;
            HasExitTime = false;
        }

        public bool CanTransition(Dictionary<string, object> parameters)
        {
            foreach (var condition in Conditions)
            {
                if (!condition.Evaluate(parameters))
                    return false;
            }
            return true;
        }
    }

    public class TransitionCondition
    {
        public string Parameter { get; set; }
        public AnimatorConditionMode Mode { get; set; }
        public float Threshold { get; set; }

        public bool Evaluate(Dictionary<string, object> parameters)
        {
            if (!parameters.ContainsKey(Parameter))
                return false;

            var value = parameters[Parameter];

            return Mode switch
            {
                AnimatorConditionMode.If => (bool)value,
                AnimatorConditionMode.IfNot => !(bool)value,
                AnimatorConditionMode.Greater => (float)value > Threshold,
                AnimatorConditionMode.Less => (float)value < Threshold,
                AnimatorConditionMode.Equals => (float)value == Threshold,
                AnimatorConditionMode.NotEqual => (float)value != Threshold,
                _ => false
            };
        }
    }

    public enum AnimatorConditionMode
    {
        If,
        IfNot,
        Greater,
        Less,
        Equals,
        NotEqual
    }

    public class AnimatorLayer
    {
        public string Name { get; set; }
        public AvatarMask AvatarMask { get; set; }
        public float Weight { get; set; }
        public int StateMachineIndex { get; set; }

        public AnimatorLayer()
        {
            Weight = 1f;
            StateMachineIndex = 0;
        }
    }

    public class AvatarMask
    {
        public string Name { get; set; }
        public Dictionary<string, bool> BoneMasks { get; set; }

        public AvatarMask()
        {
            BoneMasks = new Dictionary<string, bool>();
        }

        public void SetBoneActive(string boneName, bool active)
        {
            BoneMasks[boneName] = active;
        }

        public bool IsBoneActive(string boneName)
        {
            return BoneMasks.ContainsKey(boneName) ? BoneMasks[boneName] : true;
        }
    }
}
