﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BPCoreLib.Interfaces;
using BrokeProtocol.API;
using BrokeProtocol.Collections;
using BrokeProtocol.Entities;
using BrokeProtocol.Required;

namespace BPCoreLib.OptionMenu
{
    public class ActionLabel : IActionLabel
    {
        public string Name { get; }
        public Action<ShPlayer, string> Action { get; }

        public ActionLabel(string name, Action<ShPlayer, string> action)
        {
            this.Name = name;
            this.Action = action;
        }
    }

    internal class OptionMenu
    {
        private readonly string _title;
        public Dictionary<string, ActionLabel> Actions { get; private set; }
        private readonly LabelID[] _options;
        private readonly string menuId;

        public OptionMenu(string title, LabelID[] labels, ActionLabel[] actions)
        {
            this._title = title;
            this._options = labels;
            this.menuId = Guid.NewGuid().ToString();
            Menus.Add(menuId, this);
            Actions = new Dictionary<string, ActionLabel>();
            foreach (var action in actions)
            {
                Actions.Add(Guid.NewGuid().ToString(), action);
            }
        }

        public static Dictionary<string, OptionMenu> Menus = new Dictionary<string, OptionMenu>();

        internal void SendToPlayer(ShPlayer player)
        {
            var actions = new LabelID[this.Actions.Count];
            int i = 0;
            foreach (var action in this.Actions)
            {
                actions[i++] = new LabelID(action.Value.Name, action.Key);
            }
            player.svPlayer.SendOptionMenu(_title, player.ID, menuId, _options, actions);
        }
    }

    public static class Extension
    {
        public static void SendOptionMenu(this ShPlayer player, string title, LabelID[] labels, ActionLabel[] actions)
        {
            (new OptionMenu(title, labels, actions)).SendToPlayer(player);
        }
    }

    public class OnAction : IScript
    {
        [Target(GameSourceEvent.PlayerOptionAction, ExecutionMode.Event)]
        public void OnOptionAction(ShPlayer player, int targetID, string menuID, string optionID, string actionID)
        {
            if (!OptionMenu.Menus.ContainsKey(menuID))
            {
                return;
            }

            var menu = OptionMenu.Menus[menuID];

            menu.Actions[actionID].Action.Invoke(player, optionID);
        }
    }
}
