// Created By Levi Enama
using System;

namespace OpenEngine.Core.Scripting
{
    public abstract class MonoBehaviour : ScriptComponent
    {
        public GameObject GameObject { get; set; }
        public Transform Transform { get; set; }

        public MonoBehaviour()
        {
            GameObject = new GameObject();
            Transform = new Transform();
        }
    }

    public class GameObject
    {
        public string Name { get; set; }
        public bool ActiveSelf { get; set; }

        public GameObject()
        {
            Name = "GameObject";
            ActiveSelf = true;
        }
    }

    public class Transform
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }

        public Transform()
        {
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Scale = Vector3.One;
        }
    }
}
