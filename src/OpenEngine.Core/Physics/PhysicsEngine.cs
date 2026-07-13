// Created By Levi Enama
using System;
using System.Collections.Generic;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Physics
{
    public class PhysicsEngine
    {
        private Dictionary<string, Rigidbody> _rigidbodies;
        private Dictionary<string, Collider> _colliders;
        private float _gravity;
        private int _solverIterations;
        private int _solverVelocityIterations;

        public float Gravity => _gravity;

        public PhysicsEngine()
        {
            _rigidbodies = new Dictionary<string, Rigidbody>();
            _colliders = new Dictionary<string, Collider>();
            _gravity = 9.81f;
            _solverIterations = 6;
            _solverVelocityIterations = 1;
        }

        public void AddRigidbody(Rigidbody rb)
        {
            if (rb != null && !_rigidbodies.ContainsKey(rb.EntityId))
            {
                _rigidbodies[rb.EntityId] = rb;
            }
        }

        public void AddCollider(Collider collider)
        {
            if (collider != null && !_colliders.ContainsKey(collider.EntityId))
            {
                _colliders[collider.EntityId] = collider;
            }
        }

        public Rigidbody? GetRigidbody(string entityId)
        {
            return _rigidbodies.ContainsKey(entityId) ? _rigidbodies[entityId] : null;
        }

        public Collider? GetCollider(string entityId)
        {
            return _colliders.ContainsKey(entityId) ? _colliders[entityId] : null;
        }

        public void RemoveRigidbody(string entityId)
        {
            _rigidbodies.Remove(entityId);
        }

        public void RemoveCollider(string entityId)
        {
            _colliders.Remove(entityId);
        }

        public void Simulate(float deltaTime)
        {
            Time.fixedDeltaTime = deltaTime;
            
            foreach (var rb in _rigidbodies.Values)
            {
                if (rb.Type == RigidbodyType.Dynamic && !rb.IsSleeping)
                {
                    rb.ApplyGravity(_gravity, deltaTime);
                    rb.ApplyDamping(deltaTime);
                    rb.UpdatePosition(deltaTime);
                }
            }

            DetectCollisions();
            ResolveCollisions();
        }

        private void DetectCollisions()
        {
            // Broad phase
            // Narrow phase
            // Trigger events
        }

        private void ResolveCollisions()
        {
            // Apply collision responses
            // Update velocities
            // Apply impulses
        }

        public bool Raycast(Ray ray, out RaycastHit hit, float maxDistance = float.MaxValue)
        {
            hit = default;
            foreach (var collider in _colliders.Values)
            {
                if (collider.Enabled && collider.Raycast(ray, out hit))
                {
                    if (hit.Distance <= maxDistance)
                        return true;
                }
            }
            return false;
        }

        public RaycastHit[] RaycastAll(Ray ray, float maxDistance = float.MaxValue)
        {
            var hits = new List<RaycastHit>();
            foreach (var collider in _colliders.Values)
            {
                if (collider.Enabled && collider.Raycast(ray, out var hit))
                {
                    if (hit.Distance <= maxDistance)
                        hits.Add(hit);
                }
            }
            return hits.ToArray();
        }

        public bool CheckSphere(Vector3 position, float radius)
        {
            foreach (var collider in _colliders.Values)
            {
                if (collider.Enabled && collider.OverlapSphere(position, radius))
                    return true;
            }
            return false;
        }

        public void SetGravity(float gravity)
        {
            _gravity = gravity;
        }

        public void SetSolverIterations(int velocity, int position)
        {
            _solverIterations = position;
            _solverVelocityIterations = velocity;
        }
    }
}
