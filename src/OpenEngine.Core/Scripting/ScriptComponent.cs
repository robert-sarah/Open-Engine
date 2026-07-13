// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OpenEngine.Core.Scripting
{
    public abstract class ScriptComponent
    {
        public string EntityId { get; set; }
        public bool Enabled { get; set; }
        public int ExecutionOrder { get; set; }

        protected ScriptComponent()
        {
            Enabled = true;
            ExecutionOrder = 0;
        }

        public virtual void Awake() { }
        public virtual void Start() { }
        public virtual void Update(float deltaTime) { }
        public virtual void FixedUpdate(float fixedDeltaTime) { }
        public virtual void LateUpdate(float deltaTime) { }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public virtual void OnDestroy() { }
        public virtual void OnCollisionEnter(Collision collision) { }
        public virtual void OnCollisionStay(Collision collision) { }
        public virtual void OnCollisionExit(Collision collision) { }
        public virtual void OnTriggerEnter(Collider other) { }
        public virtual void OnTriggerStay(Collider other) { }
        public virtual void OnTriggerExit(Collider other) { }
    }

    public class Collision
    {
        public string OtherEntityId { get; set; }
        public Vector3 ContactPoint { get; set; }
        public Vector3 ContactNormal { get; set; }
        public float RelativeVelocity { get; set; }
    }

    public class Collider
    {
        public string EntityId { get; set; }
    }

    public struct Vector3
    {
        public float X, Y, Z;
        public Vector3(float x, float y, float z) { X = x; Y = y; Z = z; }
        public static Vector3 Zero => new Vector3(0, 0, 0);
        public static Vector3 One => new Vector3(1, 1, 1);
    }

    public class ScriptManager
    {
        private Dictionary<string, ScriptComponent> _scripts;
        private List<ScriptComponent> _updateScripts;
        private List<ScriptComponent> _fixedUpdateScripts;
        private List<ScriptComponent> _lateUpdateScripts;

        public ScriptManager()
        {
            _scripts = new Dictionary<string, ScriptComponent>();
            _updateScripts = new List<ScriptComponent>();
            _fixedUpdateScripts = new List<ScriptComponent>();
            _lateUpdateScripts = new List<ScriptComponent>();
        }

        public void RegisterScript(ScriptComponent script)
        {
            if (script != null && !_scripts.ContainsKey(script.EntityId))
            {
                _scripts[script.EntityId] = script;
                script.Awake();
            }
        }

        public void UnregisterScript(string entityId)
        {
            if (_scripts.ContainsKey(entityId))
            {
                _scripts[entityId].OnDestroy();
                _scripts.Remove(entityId);
            }
        }

        public void StartAll()
        {
            foreach (var script in _scripts.Values)
            {
                if (script.Enabled)
                {
                    script.Start();
                }
            }
        }

        public void Update(float deltaTime)
        {
            foreach (var script in _scripts.Values)
            {
                if (script.Enabled)
                {
                    script.Update(deltaTime);
                }
            }
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
            foreach (var script in _scripts.Values)
            {
                if (script.Enabled)
                {
                    script.FixedUpdate(fixedDeltaTime);
                }
            }
        }

        public void LateUpdate(float deltaTime)
        {
            foreach (var script in _scripts.Values)
            {
                if (script.Enabled)
                {
                    script.LateUpdate(deltaTime);
                }
            }
        }

        public T GetScript<T>(string entityId) where T : ScriptComponent
        {
            return _scripts.ContainsKey(entityId) ? _scripts[entityId] as T : null;
        }

        public List<ScriptComponent> GetAllScripts()
        {
            return new List<ScriptComponent>(_scripts.Values);
        }
    }
}
