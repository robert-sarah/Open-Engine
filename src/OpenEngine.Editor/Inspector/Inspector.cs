// Created By Levi Enama
using System;
using System.Collections.Generic;
using OpenEngine.Core.Entities;

namespace OpenEngine.Editor.Inspector
{
    public class Inspector
    {
        public SimEntity SelectedEntity { get; set; }
        public List<InspectorProperty> Properties { get; set; }

        public Inspector()
        {
            Properties = new List<InspectorProperty>();
        }

        public void Inspect(SimEntity entity)
        {
            SelectedEntity = entity;
            Properties.Clear();

            if (entity == null) return;

            // Add basic properties
            Properties.Add(new InspectorProperty("Id", entity.Id, PropertyType.String, true));
            Properties.Add(new InspectorProperty("Name", entity.Name, PropertyType.String, false));
            Properties.Add(new InspectorProperty("Enabled", entity.Enabled, PropertyType.Boolean, false));
            Properties.Add(new InspectorProperty("Position", entity.Position, PropertyType.Vector3, false));
            Properties.Add(new InspectorProperty("Rotation", entity.Rotation, PropertyType.Quaternion, false));
            Properties.Add(new InspectorProperty("Scale", entity.Scale, PropertyType.Vector3, false));
        }

        public void UpdateProperty(string name, object value)
        {
            if (SelectedEntity == null) return;

            switch (name)
            {
                case "Name":
                    SelectedEntity.Name = (string)value;
                    break;
                case "Enabled":
                    SelectedEntity.Enabled = (bool)value;
                    break;
                case "Position":
                    SelectedEntity.Position = (Math.Vector3)value;
                    break;
                case "Rotation":
                    SelectedEntity.Rotation = (Math.Quaternion)value;
                    break;
                case "Scale":
                    SelectedEntity.Scale = (Math.Vector3)value;
                    break;
            }
        }
    }

    public class InspectorProperty
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public PropertyType Type { get; set; }
        public bool ReadOnly { get; set; }

        public InspectorProperty(string name, object value, PropertyType type, bool readOnly)
        {
            Name = name;
            Value = value;
            Type = type;
            ReadOnly = readOnly;
        }
    }

    public enum PropertyType { String, Integer, Float, Boolean, Vector3, Quaternion, Color, Enum, Object }
}
