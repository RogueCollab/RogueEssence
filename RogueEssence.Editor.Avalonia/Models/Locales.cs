using System.Collections.Generic;

namespace RogueEssence.Dev.Models
{
    public class Locale
    {
        public string Name { get; set; }
        public string Key { get; set; }

        public static readonly List<Locale> Supported = new List<Locale>()
        {
            new Locale("English", "en_US"),
        };

        public Locale(string name, string key)
        {
            Name = name;
            Key = key;
        }
    }
}