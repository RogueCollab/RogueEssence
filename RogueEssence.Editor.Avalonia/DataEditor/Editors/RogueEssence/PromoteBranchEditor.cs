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
    public class PromoteBranchEditor : Editor<PromoteBranch>
    {
        public override string GetString(PromoteBranch obj, Type type, object[] attributes)
        {
            MonsterData entry = DataManager.Instance.GetMonster(obj.Result);
            return String.Format("#{0:D3} {1}: {2}", obj.Result, entry.Name.ToLocal(), obj.GetReqString());
        }
    }
}
