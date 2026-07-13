// Created By Levi Enama
using System;
using System.Collections.Generic;
using OpenEngine.Core.Entities;
using OpenEngine.Core.Engine;
using OpenEngine.Core.Math;

namespace OpenEngine.Editor.Panels
{
    public class SceneView
    {
        public OpenSimulationEngine Engine { get; }
        public Camera SceneCamera { get; set; }
        public Vector3 CameraPosition { get; set; }
        public Quaternion CameraRotation { get; set; }
        public float OrthographicSize { get; set; }
        public bool IsOrthographic { get; set; }
        public List<string> SelectedEntityIds { get; set; }
        public GizmoMode GizmoMode { get; set; }
        public bool ShowGrid { get; set; }
        public bool ShowGizmos { get; set; }

        public event EventHandler<string> OnEntitySelected;
        public event EventHandler OnSceneChanged;

        public SceneView(OpenSimulationEngine engine)
        {
            Engine = engine;
            CameraPosition = new Vector3(10f, 10f, -10f);
            CameraRotation = Quaternion.LookRotation(-CameraPosition.Normalized(), Vector3.Up);
            OrthographicSize = 10f;
            IsOrthographic = false;
            SelectedEntityIds = new List<string>();
            GizmoMode = GizmoMode.Translate;
            ShowGrid = true;
            ShowGizmos = true;
        }

        public void SetCameraPosition(Vector3 position)
        {
            CameraPosition = position;
            OnSceneChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetCameraRotation(Quaternion rotation)
        {
            CameraRotation = rotation;
            OnSceneChanged?.Invoke(this, EventArgs.Empty);
        }

        public void FocusOnEntity(string entityId)
        {
            var entity = Engine.GetEntity(entityId);
            if (entity != null)
            {
                CameraPosition = entity.Position3D + new Vector3(5f, 5f, -5f);
                CameraRotation = Quaternion.LookRotation((entity.Position3D - CameraPosition).Normalized(), Vector3.Up);
            }
        }

        public void SelectEntity(string entityId)
        {
            if (!SelectedEntityIds.Contains(entityId))
            {
                SelectedEntityIds.Add(entityId);
                OnEntitySelected?.Invoke(this, entityId);
            }
        }

        public void DeselectEntity(string entityId)
        {
            SelectedEntityIds.Remove(entityId);
        }

        public void ClearSelection()
        {
            SelectedEntityIds.Clear();
        }

        public void SelectAll()
        {
            SelectedEntityIds.Clear();
            foreach (var entity in Engine.Entities.Values)
            {
                SelectedEntityIds.Add(entity.Id);
            }
        }

        public List<SimEntity> GetVisibleEntities()
        {
            return new List<SimEntity>(Engine.Entities.Values);
        }

        public SimEntity? GetEntityAtPosition(Vector3 screenPosition)
        {
            // Raycast from camera to find entity at screen position
            return null;
        }

        public void SetGizmoMode(GizmoMode mode)
        {
            GizmoMode = mode;
        }

        public void ToggleGrid()
        {
            ShowGrid = !ShowGrid;
        }

        public void ToggleGizmos()
        {
            ShowGizmos = !ShowGizmos;
        }

        public void ToggleCameraMode()
        {
            IsOrthographic = !IsOrthographic;
        }

        public void FrameSelected()
        {
            if (SelectedEntityIds.Count > 0)
            {
                var entity = Engine.GetEntity(SelectedEntityIds[0]);
                if (entity != null)
                {
                    FocusOnEntity(entity.Id);
                }
            }
        }

        public void FrameAll()
        {
            if (Engine.Entities.Count > 0)
            {
                var center = Vector3.Zero;
                foreach (var entity in Engine.Entities.Values)
                {
                    center += entity.Position3D;
                }
                center /= Engine.Entities.Count;
                CameraPosition = center + new Vector3(10f, 10f, -10f);
                CameraRotation = Quaternion.LookRotation((center - CameraPosition).Normalized(), Vector3.Up);
            }
        }
    }

    public enum GizmoMode { Translate, Rotate, Scale }
}
