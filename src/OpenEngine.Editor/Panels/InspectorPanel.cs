// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Reflection;
using OpenEngine.Core.Entities;
using OpenEngine.Core.Math;

namespace OpenEngine.Editor.Panels
{
    public class InspectorPanel
    {
        public SimEntity SelectedEntity { get; private set; }

        public event EventHandler OnEntityModified;

        public InspectorPanel()
        {
        }

        public void SetSelectedEntity(SimEntity entity)
        {
            SelectedEntity = entity;
        }

        public void UpdatePosition(Vector3 newPosition)
        {
            if (SelectedEntity != null)
            {
                SelectedEntity.Position3D = newPosition;
                OnEntityModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public void UpdateRotation(Vector3 newRotation)
        {
            if (SelectedEntity != null)
            {
                SelectedEntity.Rotation3D = newRotation;
                OnEntityModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public void UpdateScale(Vector3 newScale)
        {
            if (SelectedEntity != null)
            {
                SelectedEntity.Scale3D = newScale;
                OnEntityModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public void UpdateAttribute(string key, float value)
        {
            if (SelectedEntity != null)
            {
                SelectedEntity.AddAttribute(key, value);
                OnEntityModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public void UpdateName(string newName)
        {
            if (SelectedEntity != null)
            {
                SelectedEntity.Name = newName;
                OnEntityModified?.Invoke(this, EventArgs.Empty);
            }
        }

        public Dictionary<string, object> GetEditableProperties()
        {
            var props = new Dictionary<string, object>();
            if (SelectedEntity == null)
                return props;

            props["Name"] = SelectedEntity.Name;
            props["Type"] = SelectedEntity.Type;
            props["Position"] = SelectedEntity.Position3D;
            props["Rotation"] = SelectedEntity.Rotation3D;
            props["Scale"] = SelectedEntity.Scale3D;
            props["Attributes"] = new Dictionary<string, float>(SelectedEntity.Attributes);
            props["Capabilities"] = new List<string>(SelectedEntity.Capabilities);
            props["Tags"] = new List<string>(SelectedEntity.Tags);

            return props;
        }
    }
}
