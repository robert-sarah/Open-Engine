// Created By Levi Enama
using System;
using System.Collections.Generic;

namespace OpenEngine.Core.AI
{
    public class State
    {
        public string Name { get; set; }
        public Action OnEnter;
        public Action OnUpdate;
        public Action OnExit;
        public Dictionary<string, State> Transitions { get; set; }

        public State()
        {
            Transitions = new Dictionary<string, State>();
        }

        public virtual void Enter()
        {
            OnEnter?.Invoke();
        }

        public virtual void Update()
        {
            OnUpdate?.Invoke();
        }

        public virtual void Exit()
        {
            OnExit?.Invoke();
        }

        public void AddTransition(string trigger, State state)
        {
            Transitions[trigger] = state;
        }
    }

    public class StateMachine
    {
        public State CurrentState { get; private set; }
        public Dictionary<string, State> States { get; set; }
        public object Owner { get; set; }

        public StateMachine()
        {
            States = new Dictionary<string, State>();
        }

        public void AddState(State state)
        {
            States[state.Name] = state;
        }

        public void SetState(string stateName)
        {
            if (!States.ContainsKey(stateName))
            {
                Console.WriteLine($"State {stateName} not found");
                return;
            }

            if (CurrentState != null)
            {
                CurrentState.Exit();
            }

            CurrentState = States[stateName];
            CurrentState.Enter();
        }

        public void TriggerTransition(string trigger)
        {
            if (CurrentState == null) return;

            if (CurrentState.Transitions.TryGetValue(trigger, out var nextState))
            {
                SetState(nextState.Name);
            }
        }

        public void Update()
        {
            CurrentState?.Update();
        }
    }

    public class AIState : State
    {
        public Entities.SimEntity Entity { get; set; }
        public float StateTimer { get; set; }

        public AIState(Entities.SimEntity entity)
        {
            Entity = entity;
        }
    }

    public class IdleState : AIState
    {
        public float IdleDuration { get; set; }

        public IdleState(Entities.SimEntity entity, float idleDuration = 2.0f) : base(entity)
        {
            Name = "Idle";
            IdleDuration = idleDuration;
        }

        public override void Enter()
        {
            base.Enter();
            StateTimer = 0f;
            Console.WriteLine($"{Entity.Name} entered Idle state");
        }

        public override void Update()
        {
            base.Update();
            StateTimer += 0.016f; // Assuming 60 FPS

            if (StateTimer >= IdleDuration)
            {
                TriggerTransition("IdleComplete");
            }
        }
    }

    public class PatrolState : AIState
    {
        public List<System.Numerics.Vector3> Waypoints { get; set; }
        public int CurrentWaypointIndex { get; set; }
        public float PatrolSpeed { get; set; }

        public PatrolState(Entities.SimEntity entity, List<System.Numerics.Vector3> waypoints, float speed = 3.5f) : base(entity)
        {
            Name = "Patrol";
            Waypoints = waypoints;
            CurrentWaypointIndex = 0;
            PatrolSpeed = speed;
        }

        public override void Enter()
        {
            base.Enter();
            Console.WriteLine($"{Entity.Name} started patrolling");
        }

        public override void Update()
        {
            base.Update();

            if (Waypoints.Count == 0) return;

            var target = Waypoints[CurrentWaypointIndex];
            var direction = System.Numerics.Vector3.Normalize(target - Entity.Position3D);
            var distance = System.Numerics.Vector3.Distance(Entity.Position3D, target);

            if (distance < 0.5f)
            {
                CurrentWaypointIndex = (CurrentWaypointIndex + 1) % Waypoints.Count;
            }
            else
            {
                Entity.Position3D += direction * PatrolSpeed * 0.016f;
            }
        }
    }

    public class ChaseState : AIState
    {
        public Entities.SimEntity Target { get; set; }
        public float ChaseSpeed { get; set; }
        public float DetectionRange { get; set; }

        public ChaseState(Entities.SimEntity entity, Entities.SimEntity target, float speed = 5.0f, float range = 10.0f) : base(entity)
        {
            Name = "Chase";
            Target = target;
            ChaseSpeed = speed;
            DetectionRange = range;
        }

        public override void Enter()
        {
            base.Enter();
            Console.WriteLine($"{Entity.Name} is chasing {Target.Name}");
        }

        public override void Update()
        {
            base.Update();

            var distance = System.Numerics.Vector3.Distance(Entity.Position3D, Target.Position3D);

            if (distance > DetectionRange)
            {
                TriggerTransition("TargetLost");
                return;
            }

            if (distance < 1.0f)
            {
                TriggerTransition("TargetReached");
                return;
            }

            var direction = System.Numerics.Vector3.Normalize(Target.Position3D - Entity.Position3D);
            Entity.Position3D += direction * ChaseSpeed * 0.016f;
        }
    }

    public class AttackState : AIState
    {
        public Entities.SimEntity Target { get; set; }
        public float AttackRange { get; set; }
        public float AttackCooldown { get; set; }
        public float AttackTimer { get; set; }
        public float Damage { get; set; }

        public AttackState(Entities.SimEntity entity, Entities.SimEntity target, float range = 2.0f, float cooldown = 1.0f, float damage = 10.0f) : base(entity)
        {
            Name = "Attack";
            Target = target;
            AttackRange = range;
            AttackCooldown = cooldown;
            Damage = damage;
        }

        public override void Enter()
        {
            base.Enter();
            AttackTimer = 0f;
            Console.WriteLine($"{Entity.Name} is attacking {Target.Name}");
        }

        public override void Update()
        {
            base.Update();
            AttackTimer += 0.016f;

            var distance = System.Numerics.Vector3.Distance(Entity.Position3D, Target.Position3D);

            if (distance > AttackRange)
            {
                TriggerTransition("TargetOutOfRange");
                return;
            }

            if (AttackTimer >= AttackCooldown)
            {
                PerformAttack();
                AttackTimer = 0f;
            }
        }

        private void PerformAttack()
        {
            Console.WriteLine($"{Entity.Name} attacks {Target.Name} for {Damage} damage");
            // Apply damage logic here
        }
    }

    public class FleeState : AIState
    {
        public Entities.SimEntity Threat { get; set; }
        public float FleeSpeed { get; set; }
        public float FleeDistance { get; set; }

        public FleeState(Entities.SimEntity entity, Entities.SimEntity threat, float speed = 5.0f, float distance = 15.0f) : base(entity)
        {
            Name = "Flee";
            Threat = threat;
            FleeSpeed = speed;
            FleeDistance = distance;
        }

        public override void Enter()
        {
            base.Enter();
            Console.WriteLine($"{Entity.Name} is fleeing from {Threat.Name}");
        }

        public override void Update()
        {
            base.Update();

            var distance = System.Numerics.Vector3.Distance(Entity.Position3D, Threat.Position3D);

            if (distance > FleeDistance)
            {
                TriggerTransition("Safe");
                return;
            }

            var direction = System.Numerics.Vector3.Normalize(Entity.Position3D - Threat.Position3D);
            Entity.Position3D += direction * FleeSpeed * 0.016f;
        }
    }

    public class DeadState : AIState
    {
        public float DeathTimer { get; set; }
        public float RespawnTime { get; set; }

        public DeadState(Entities.SimEntity entity, float respawnTime = 5.0f) : base(entity)
        {
            Name = "Dead";
            RespawnTime = respawnTime;
        }

        public override void Enter()
        {
            base.Enter();
            DeathTimer = 0f;
            Console.WriteLine($"{Entity.Name} has died");
        }

        public override void Update()
        {
            base.Update();
            DeathTimer += 0.016f;

            if (DeathTimer >= RespawnTime)
            {
                TriggerTransition("Respawn");
            }
        }
    }
}
