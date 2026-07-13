// Created By Levi Enama
using System;
using System.Collections.Generic;
using OpenEngine.Core.Engine;

namespace OpenEngine.Editor.Panels
{
    public class GodPanelConsole
    {
        public OpenSimulationEngine Engine { get; }
        public List<string> CommandHistory { get; }

        public event EventHandler<string> OnCommandExecuted;
        public event EventHandler<string> OnAlertAdded;

        public GodPanelConsole(OpenSimulationEngine engine)
        {
            Engine = engine;
            CommandHistory = new List<string>();
        }

        public void ExecuteCommand(string command)
        {
            CommandHistory.Add(command);
            Engine.AddGodPanelAlert(command);
            OnCommandExecuted?.Invoke(this, command);
            OnAlertAdded?.Invoke(this, command);
        }

        public List<string> GetRecentAlerts(int count = 10)
        {
            if (Engine.GodPanelAlerts.Count <= count)
                return new List<string>(Engine.GodPanelAlerts);

            return Engine.GodPanelAlerts.GetRange(Engine.GodPanelAlerts.Count - count, count);
        }

        public void ClearAlerts()
        {
            Engine.ClearGodPanelAlerts();
        }

        public List<string> GetAvailableCommands()
        {
            return new List<string>
            {
                "spawn [entity]",
                "delete [entity]",
                "move [entity] [x,y,z]",
                "alert [message]",
                "weather [type]",
                "time [set|add] [value]",
                "economy [crash|boom|stable]",
                "meteor [position]",
                "toxicity [level]",
                "pause",
                "resume",
                "speed [factor]"
            };
        }
    }
}
