// Created By Levi Enama
using System;
using System.Collections.Generic;
using System.Linq;
using OpenEngine.Core.Entities;
using OpenEngine.Core.Engine;

namespace OpenEngine.Editor.Panels
{
    public class HierarchyPanel
    {
        public OpenSimulationEngine Engine { get; }
        public string SelectedEntityId { get; set; }

        public event EventHandler<string> OnEntitySelected;
        public event EventHandler<string> OnEntityCreated;
        public event EventHandler<string> OnEntityDeleted;

        public HierarchyPanel(OpenSimulationEngine engine)
        {
            Engine = engine;
        }

        public List<SimEntity> GetAllEntities()
        {
            return Engine.Entities.Values.OrderBy(e => e.Name).ToList();
        }

        public List<SimEntity> GetRootEntities()
        {
            return Engine.Entities.Values
                .Where(e => string.IsNullOrEmpty(e.ParentId))
                .OrderBy(e => e.Name)
                .ToList();
        }

        public List<SimEntity> GetChildEntities(string parentId)
        {
            return Engine.Entities.Values
                .Where(e => e.ParentId == parentId)
                .OrderBy(e => e.Name)
                .ToList();
        }

        public SimEntity CreateEntity(string name, string type)
        {
            var entity = Engine.CreateEntity(name, type);
            OnEntityCreated?.Invoke(this, entity.Id);
            return entity;
        }

        public bool DeleteEntity(string entityId)
        {
            bool result = Engine.RemoveEntity(entityId);
            if (result)
                OnEntityDeleted?.Invoke(this, entityId);
            return result;
        }

        public void SelectEntity(string entityId)
        {
            SelectedEntityId = entityId;
            OnEntitySelected?.Invoke(this, entityId);
        }

        public void SetParent(string entityId, string parentId)
        {
            if (Engine.Entities.TryGetValue(entityId, out SimEntity entity))
            {
                entity.ParentId = parentId;
            }
        }
    }
}
