// Created By Levi Enama
using System;
using System.Collections.Generic;
using OpenEngine.Core.Math;
using OpenEngine.Core.AI;

namespace OpenEngine.Core.Environment
{
    public enum AnimalType { Mammoth, Rabbit, Wolf, Deer, Bear, Boar }

    public enum AnimalState { Idle, Grazing, Drinking, Fleeing, Hunting, Sleeping, Mating, Herding }

    public class Animal
    {
        public string Id { get; set; }
        public AnimalType Type { get; set; }
        public AnimalState CurrentState { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public float Health { get; set; }
        public float Hunger { get; set; }
        public float Thirst { get; set; }
        public float Energy { get; set; }
        public float Speed { get; set; }
        public float Size { get; set; }
        public float Aggression { get; set; }
        public bool IsMale { get; set; }
        public int Age { get; set; }
        public List<string> Herd { get; set; }
        public StateMachine AIStateMachine { get; set; }

        public Animal()
        {
            Herd = new List<string>();
            AIStateMachine = new StateMachine();
            InitializeAI();
        }

        private void InitializeAI()
        {
            // Create AI states based on animal type
            switch (Type)
            {
                case AnimalType.Wolf:
                    InitializeWolfAI();
                    break;
                case AnimalType.Rabbit:
                    InitializeRabbitAI();
                    break;
                case AnimalType.Mammoth:
                    InitializeMammothAI();
                    break;
            }
        }

        private void InitializeWolfAI()
        {
            AIStateMachine.AddState("Idle", new AIState
            {
                OnEnter = () => { CurrentState = AnimalState.Idle; },
                OnUpdate = () => {
                    if (Hunger > 0.7f) AIStateMachine.TransitionTo("Hunting");
                    if (Energy < 0.3f) AIStateMachine.TransitionTo("Sleeping");
                }
            });

            AIStateMachine.AddState("Hunting", new AIState
            {
                OnEnter = () => { CurrentState = AnimalState.Hunting; },
                OnUpdate = () => {
                    if (Hunger < 0.3f) AIStateMachine.TransitionTo("Idle");
                }
            });

            AIStateMachine.AddState("Sleeping", new AIState
            {
                OnEnter = () => { CurrentState = AnimalState.Sleeping; },
                OnUpdate = () => {
                    Energy += 0.01f;
                    if (Energy > 0.8f) AIStateMachine.TransitionTo("Idle");
                }
            });

            AIStateMachine.SetInitialState("Idle");
        }

        private void InitializeRabbitAI()
        {
            AIStateMachine.AddState("Idle", new AIState
            {
                OnEnter = () => { CurrentState = AnimalState.Idle; },
                OnUpdate = () => {
                    if (Hunger > 0.5f) AIStateMachine.TransitionTo("Grazing");
                    if (Thirst > 0.5f) AIStateMachine.TransitionTo("Drinking");
                }
            });

            AIStateMachine.AddState("Grazing", new AIState
            {
                OnEnter = () => { CurrentState = AnimalState.Grazing; },
                OnUpdate = () => {
                    Hunger -= 0.02f;
                    if (Hunger < 0.2f) AIStateMachine.TransitionTo("Idle");
                }
            });

            AIStateMachine.AddState("Drinking", new AIState
            {
                OnEnter = () => { CurrentState = AnimalState.Drinking; },
                OnUpdate = () => {
                    Thirst -= 0.03f;
                    if (Thirst < 0.2f) AIStateMachine.TransitionTo("Idle");
                }
            });

            AIStateMachine.AddState("Fleeing", new AIState
            {
                OnEnter = () => { CurrentState = AnimalState.Fleeing; },
                OnUpdate = () => {
                    // Run away from predator
                    Velocity = Vector3.Normalize(Position - new Vector3(0, 0, 0)) * Speed * 2;
                }
            });

            AIStateMachine.SetInitialState("Idle");
        }

        private void InitializeMammothAI()
        {
            AIStateMachine.AddState("Idle", new AIState
            {
                OnEnter = () => { CurrentState = AnimalState.Idle; },
                OnUpdate = () => {
                    if (Hunger > 0.6f) AIStateMachine.TransitionTo("Grazing");
                    if (Herd.Count > 0) AIStateMachine.TransitionTo("Herding");
                }
            });

            AIStateMachine.AddState("Grazing", new AIState
            {
                OnEnter = () => { CurrentState = AnimalState.Grazing; },
                OnUpdate = () => {
                    Hunger -= 0.01f;
                    if (Hunger < 0.2f) AIStateMachine.TransitionTo("Idle");
                }
            });

            AIStateMachine.AddState("Herding", new AIState
            {
                OnEnter = () => { CurrentState = AnimalState.Herding; },
                OnUpdate = () => {
                    // Move with herd
                    if (Hunger > 0.5f) AIStateMachine.TransitionTo("Grazing");
                }
            });

            AIStateMachine.SetInitialState("Idle");
        }

        public void Update(float deltaTime)
        {
            // Update stats
            Hunger += 0.001f * deltaTime;
            Thirst += 0.0015f * deltaTime;
            Energy -= 0.0005f * deltaTime;

            // Clamp values
            Hunger = Math.Clamp(Hunger, 0f, 1f);
            Thirst = Math.Clamp(Thirst, 0f, 1f);
            Energy = Math.Clamp(Energy, 0f, 1f);

            // Update AI
            AIStateMachine.Update();

            // Move
            Position += Velocity * deltaTime;
        }

        public void FleeFrom(Vector3 predatorPosition)
        {
            Vector3 direction = Vector3.Normalize(Position - predatorPosition);
            Velocity = direction * Speed * 1.5f;
            AIStateMachine.TransitionTo("Fleeing");
        }

        public void Attack(Animal target)
        {
            if (Aggression > 0.5f && target != null)
            {
                target.Health -= Aggression * 10;
            }
        }
    }

    public class AnimalSystem
    {
        private Dictionary<string, Animal> _animals;
        private List<Animal> _predators;
        private List<Animal> _prey;

        public Dictionary<string, Animal> Animals => _animals;

        public AnimalSystem()
        {
            _animals = new Dictionary<string, Animal>();
            _predators = new List<Animal>();
            _prey = new List<Animal>();
        }

        public Animal SpawnAnimal(AnimalType type, Vector3 position)
        {
            var animal = new Animal
            {
                Id = Guid.NewGuid().ToString(),
                Type = type,
                Position = position,
                Health = 100f,
                Hunger = 0.3f,
                Thirst = 0.3f,
                Energy = 1f,
                IsMale = new Random().NextDouble() > 0.5f,
                Age = new Random().Next(1, 10)
            };

            // Set stats based on type
            switch (type)
            {
                case AnimalType.Wolf:
                    animal.Speed = 8f;
                    animal.Size = 1.5f;
                    animal.Aggression = 0.8f;
                    _predators.Add(animal);
                    break;
                case AnimalType.Rabbit:
                    animal.Speed = 6f;
                    animal.Size = 0.3f;
                    animal.Aggression = 0.1f;
                    _prey.Add(animal);
                    break;
                case AnimalType.Mammoth:
                    animal.Speed = 3f;
                    animal.Size = 5f;
                    animal.Aggression = 0.3f;
                    _prey.Add(animal);
                    break;
                case AnimalType.Bear:
                    animal.Speed = 5f;
                    animal.Size = 3f;
                    animal.Aggression = 0.7f;
                    _predators.Add(animal);
                    break;
            }

            _animals[animal.Id] = animal;
            return animal;
        }

        public void Update(float deltaTime)
        {
            foreach (var animal in _animals.Values)
            {
                animal.Update(deltaTime);

                // Predator-prey interactions
                if (_predators.Contains(animal))
                {
                    CheckForPrey(animal);
                }
                else if (_prey.Contains(animal))
                {
                    CheckForPredators(animal);
                }
            }

            // Remove dead animals
            var deadAnimals = new List<string>();
            foreach (var kvp in _animals)
            {
                if (kvp.Value.Health <= 0)
                {
                    deadAnimals.Add(kvp.Key);
                }
            }

            foreach (var id in deadAnimals)
            {
                var animal = _animals[id];
                _predators.Remove(animal);
                _prey.Remove(animal);
                _animals.Remove(id);
            }
        }

        private void CheckForPrey(Animal predator)
        {
            foreach (var prey in _prey)
            {
                float distance = Vector3.Distance(predator.Position, prey.Position);
                if (distance < 10f && predator.Hunger > 0.5f)
                {
                    predator.AIStateMachine.TransitionTo("Hunting");
                    prey.FleeFrom(predator.Position);

                    if (distance < 2f)
                    {
                        predator.Attack(prey);
                    }
                }
            }
        }

        private void CheckForPredators(Animal prey)
        {
            foreach (var predator in _predators)
            {
                float distance = Vector3.Distance(prey.Position, predator.Position);
                if (distance < 15f)
                {
                    prey.FleeFrom(predator.Position);
                }
            }
        }

        public void FormHerd(string leaderId, List<string> memberIds)
        {
            if (_animals.TryGetValue(leaderId, out var leader))
            {
                leader.Herd.Clear();
                foreach (var memberId in memberIds)
                {
                    if (_animals.TryGetValue(memberId, out var member))
                    {
                        leader.Herd.Add(memberId);
                        member.Herd.Add(leaderId);
                    }
                }
            }
        }
    }
}
