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

namespace RogueEssence.Dev
{
    public class MapItemEditor : Editor<MapItem>
    {
        public override string GetString(MapItem obj, Type type, object[] attributes)
        {
            if (obj.IsMoney)
                return String.Format("{0}P", obj.Value);
            else
            {
                ItemData entry = DataManager.Instance.GetItem(obj.Value);
                if (entry.MaxStack > 1)
                    return (obj.Cursed ? "[X]" : "") + entry.Name.ToLocal() + " (" + obj.HiddenValue + ")";
                else
                    return (obj.Cursed ? "[X]" : "") + entry.Name.ToLocal();
            }
        }
    }
}
