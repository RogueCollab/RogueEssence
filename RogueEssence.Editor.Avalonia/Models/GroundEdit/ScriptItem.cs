using System;
using System.Collections.Generic;
using System.Text;

namespace RogueEssence.Dev.Models
{
    public class ScriptItem
    {
        public ScriptItem() { }
        public ScriptItem(string desc, bool isChecked)
        {
            Description = desc;
            IsChecked = isChecked;
        }
        public string Description { get; set; }
        public bool IsChecked { get; set; }
    }
}
