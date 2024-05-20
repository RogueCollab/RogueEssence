using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RogueEssence.Menu
{
    public interface ILabeled
    {
        public string Label { get; set; }
        public bool HasLabel()
        {
            return !string.IsNullOrEmpty(Label);
        }
    }
}
