// Created By Levi Enama
// BepuPhysics Wrapper for Open Engine
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using System.Numerics;

namespace OpenEngine.Core.Physics
{
    public class BepuPhysicsWrapper : IDisposable
    {
        private Simulation _simulation;
        private bool _disposed;

        public Simulation Simulation => _simulation;

        public BepuPhysicsWrapper()
        {
            var bufferPool = new BufferPool();
            _simulation = Simulation.Create(bufferPool, new NarrowPhaseCallbacks(), new ContactCallbacks());
        }

        public void Initialize()
        {
            // BepuPhysics initialized
        }

        public void Update(float deltaTime)
        {
            _simulation.Timestep(deltaTime);
        }

        public BodyHandle CreateBody(Vector3 position, Quaternion orientation, float mass, CollidableDescription collidable)
        {
            var bodyDescription = BodyDescription.CreateDynamic(
                position,
                orientation,
                collidable,
                mass
            );
            return _simulation.Bodies.Add(bodyDescription);
        }

        public BodyHandle CreateStaticBody(Vector3 position, Quaternion orientation, CollidableDescription collidable)
        {
            var bodyDescription = BodyDescription.CreateStatic(
                position,
                orientation,
                collidable
            );
            return _simulation.Bodies.Add(bodyDescription);
        }

        public CollidableDescription CreateBoxCollidable(float width, float height, float depth, float mass)
        {
            var box = new Box(width, height, depth);
            var shapeIndex = _simulation.Shapes.Add(box);
            return new CollidableDescription(shapeIndex, mass);
        }

        public CollidableDescription CreateSphereCollidable(float radius, float mass)
        {
            var sphere = new Sphere(radius);
            var shapeIndex = _simulation.Shapes.Add(sphere);
            return new CollidableDescription(shapeIndex, mass);
        }

        public CollidableDescription CreateCapsuleCollidable(float radius, float height, float mass)
        {
            var capsule = new Capsule(radius, height);
            var shapeIndex = _simulation.Shapes.Add(capsule);
            return new CollidableDescription(shapeIndex, mass);
        }

        public void ApplyForce(BodyHandle bodyHandle, Vector3 force)
        {
            ref var body = ref _simulation.Bodies[bodyHandle];
            body.Velocity += force / body.Mass;
        }

        public void ApplyImpulse(BodyHandle bodyHandle, Vector3 impulse)
        {
            ref var body = ref _simulation.Bodies[bodyHandle];
            body.Velocity += impulse / body.Mass;
        }

        public void SetPosition(BodyHandle bodyHandle, Vector3 position)
        {
            ref var body = ref _simulation.Bodies[bodyHandle];
            body.Pose.Position = position;
        }

        public void SetRotation(BodyHandle bodyHandle, Quaternion rotation)
        {
            ref var body = ref _simulation.Bodies[bodyHandle];
            body.Pose.Orientation = rotation;
        }

        public Vector3 GetPosition(BodyHandle bodyHandle)
        {
            return _simulation.Bodies[bodyHandle].Pose.Position;
        }

        public Quaternion GetRotation(BodyHandle bodyHandle)
        {
            return _simulation.Bodies[bodyHandle].Pose.Orientation;
        }

        public void RemoveBody(BodyHandle bodyHandle)
        {
            _simulation.Bodies.Remove(bodyHandle);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _simulation?.Dispose();
                _disposed = true;
            }
        }
    }

    public class NarrowPhaseCallbacks : INarrowPhaseCallbacks
    {
        public void Initialize(Simulation simulation)
        {
        }

        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, float speculativeMargin)
        {
            return true;
        }

        public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB, ref TManifold manifold, out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
        {
            pairMaterial = new PairMaterialProperties { FrictionCoefficient = 0.5f, MaximumRecoveryVelocity = 2f, MinimumRecoveryVelocity = 0.1f, SpringSettings = new SpringSettings(30, 1) };
            return true;
        }

        public void Dispose()
        {
        }
    }

    public class ContactCallbacks : IContactEventHandler
    {
        public void Initialize(Simulation simulation)
        {
        }

        public void OnContactAdded(int workerIndex, ref CollidablePair pair, int contactIndex, ref ContactProperties contact)
        {
        }

        public void OnContactRemoved(int workerIndex, ref CollidablePair pair, int contactIndex)
        {
        }
    }
}
