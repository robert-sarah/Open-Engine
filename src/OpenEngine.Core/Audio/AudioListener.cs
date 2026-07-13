// Created By Levi Enama
using System;
using OpenEngine.Core.Math;

namespace OpenEngine.Core.Audio
{
    public class AudioListener
    {
        public string EntityId { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public float Volume { get; set; }

        public AudioListener(string entityId)
        {
            EntityId = entityId ?? throw new ArgumentNullException(nameof(entityId));
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Volume = 1f;
        }

        public void UpdatePosition(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public void SetVolume(float volume)
        {
            Volume = Math.Clamp(volume, 0f, 1f);
        }
    }
}
