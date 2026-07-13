// Created By Levi Enama
using System;
using System.Collections.Generic;
using OpenEngine.Core.Entities;
using OpenEngine.Core.Math;

namespace OpenEngine.Editor.Scene
{
    public class SceneView
    {
        public string SceneName { get; set; }
        public List<SimEntity> Entities { get; set; }
        public Vector3 CameraPosition { get; set; }
        public Quaternion CameraRotation { get; set; }
        public bool IsPlaying { get; set; }
        public float TimeScale { get; set; }
        public string SelectedEntityId { get; set; }

        public SceneView()
        {
            Entities = new List<SimEntity>();
            CameraPosition = new Vector3(0, 5, -10);
            CameraRotation = Quaternion.Euler(30, 0, 0);
            IsPlaying = false;
            TimeScale = 1f;
        }

        public void AddEntity(SimEntity entity)
        {
            if (entity != null && !Entities.Contains(entity))
            {
                Entities.Add(entity);
            }
        }

        public void RemoveEntity(SimEntity entity)
        {
            Entities.Remove(entity);
        }

        public SimEntity GetEntity(string id)
        {
            return Entities.Find(e => e.Id == id);
        }

        public void SelectEntity(string id)
        {
            SelectedEntityId = id;
        }

        public void DeselectAll()
        {
            SelectedEntityId = null;
        }

        public void Play()
        {
            IsPlaying = true;
        }

        public void Pause()
        {
            IsPlaying = false;
        }

        public void Stop()
        {
            IsPlaying = false;
            TimeScale = 1f;
        }
    }
}
