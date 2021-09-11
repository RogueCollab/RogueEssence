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

namespace RogueEssence.Dev
{
    public class MultiRandPickerEditor : Editor<IMultiRandPicker>
    {
        public override bool DefaultSubgroup => true;
    }
}
