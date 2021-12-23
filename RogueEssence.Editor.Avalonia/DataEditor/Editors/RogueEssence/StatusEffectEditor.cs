using System;
using System.Collections.Generic;
using System.Text;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueEssence.Data;
using System.Drawing;
using RogueElements;
using Avalonia.Controls;
using RogueEssence.Dev.Views;
using System.Collections;
using Avalonia;
using System.Reactive.Subjects;
using RogueEssence.LevelGen;
using System.Reflection;

namespace RogueEssence.Dev
{
    public class StatusEffectEditor : Editor<StatusEffect>
    {
        //TODO: add a "load from default" button to load all status states from the original definition
        public override string GetString(StatusEffect obj, Type type, object[] attributes)
        {
            StatusData statusData = DataManager.Instance.GetStatus(obj.ID);
            string statusStr = String.Format("{0}: {1}", obj.ID.ToString("D3"), statusData.Name.ToLocal());
            StackState stack = obj.StatusStates.GetWithDefault<StackState>();
            if (stack != null)
            {
                if (stack.Stack < 0)
                    statusStr += " " + stack.Stack;
                else if (stack.Stack > 0)
                    statusStr += " +" + stack.Stack;
            }

            CountDownState countDown = obj.StatusStates.GetWithDefault<CountDownState>();
            if (countDown != null && countDown.Counter > 0)
                statusStr += "  [" + countDown.Counter + "]";
            return statusStr;
        }
    }
    public class MapStatusEditor : Editor<MapStatus>
    {
        //TODO: add a "load from default" button to load all status states from the original definition
        public override string GetString(MapStatus obj, Type type, object[] attributes)
        {
            MapStatusData statusData = DataManager.Instance.GetMapStatus(obj.ID);
            string statusStr = String.Format("{0}: {1}", obj.ID.ToString("D3"), statusData.Name.ToLocal());

            MapCountDownState countDown = obj.StatusStates.GetWithDefault<MapCountDownState>();
            if (countDown != null && countDown.Counter > 0)
                statusStr += "  [" + countDown.Counter + "]";
            return statusStr;
        }
    }
}
