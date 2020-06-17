/*
TemplateManager.cs
2017/07/01
psycommando@gmail.com
Description:This is the root of the template system for handling object and character templates used throughout the game.
*/
using System;
using System.Collections.Generic;
using RogueEssence.Content;
using RogueEssence.Ground;
using RogueEssence.Dungeon;
using RogueElements;

namespace RogueEssence.Data
{





    public enum ETemplateType
    {
        Invalid = -1,
        Character = 0,          //GroundChar
        Object = 1             //GroundObject
    }

    /// <summary>
    /// Base class for templates
    /// </summary>
    abstract class BaseTemplate
    {
        public virtual string        Name { get; set; }
        public virtual ETemplateType Type { get; set; }

        public abstract object create(string instancename);
    }

    /// <summary>
    /// Object templates contains data to build basically a perfect clone of an object. Its literally a template.
    /// Its meant to allow mappers and scripters to quickly instantiate recurring ground objects.
    /// </summary>
    [Serializable]
    class ObjectTemplate : BaseTemplate
    {
        public Rect     Rect        { get; set; }
        public ObjAnimData Anim        { get; set; }
        public bool     Contact     { get; set; }
        public override ETemplateType Type { get {return ETemplateType.Object; } }

        public override object create(string instancename)
        {
            return new GroundObject(Anim, Rect, Contact, instancename);
        }
    }



    /// <summary>
    /// Character templates contains data to build basically a perfect clone of a charater. Its literally a template.
    /// Its meant to allow mappers and scripters to quickly instantiate recurring ground characters.
    /// </summary>
    [Serializable]
    class CharacterTemplate : BaseTemplate //!#FIXME: Serialising this won't work, because they're stored as their base class.. Will need to look into it!
    {
        public Character Chara { get; set; }
        public override ETemplateType Type { get { return ETemplateType.Character; } }

        public override object create(string instancename)
        {
            return new GroundChar(Chara, new Loc(0, 0), Dir8.Down, instancename);
        }
    }

    /// <summary>
    /// This handles all templates for the game, and handle serialization for them too!
    /// </summary>
    [Serializable]
    class TemplateManager
    {
        public static readonly List<string> TemplateTypeNames = new List<string>
        {
            "Characters",
            "Objects",
        };


        //Instantiate singleton on use
        private static readonly Lazy<TemplateManager> lazy = new Lazy<TemplateManager>(() => new TemplateManager());
        public static TemplateManager Instance { get { return lazy.Value; } } ///<summary> Returns the current instance of the TemplateManager. Its instantiated when this property is first accessed.</summary>

        private Dictionary<string, ObjectTemplate>      m_objTemplates = new Dictionary<string, ObjectTemplate>();
        private Dictionary<string, CharacterTemplate>   m_charTemplates = new Dictionary<string, CharacterTemplate>();

        /// <summary>
        /// Assign or insert a template into the manager.
        /// </summary>
        /// <param name="templatename"></param>
        /// <param name="templ"></param>
        public void SetTemplate(string templatename, BaseTemplate templ)
        {
            if (templ.Type == ETemplateType.Character)
            {
                if (m_charTemplates.ContainsKey(templatename))
                    m_charTemplates[templatename] = (CharacterTemplate)templ;
                else
                    m_charTemplates.Add(templatename, (CharacterTemplate)templ);
            }
            else if (templ.Type == ETemplateType.Object)
            {
                if (m_objTemplates.ContainsKey(templatename))
                    m_objTemplates[templatename] = (ObjectTemplate)templ;
                else
                    m_objTemplates.Add(templatename, (ObjectTemplate)templ);
            }
            else
                throw new NotImplementedException();
        }

        /// <summary>
        /// Find a named template in the list of global templates and returns the basetemplate.
        /// </summary>
        /// <param name="name">Name of the template to find.</param>
        /// <returns>BaseTemplate found, or null if not found.</returns>
        public BaseTemplate FindTemplate(string name)
        {
            BaseTemplate found = null;

            if (m_objTemplates.ContainsKey(name))
                found = m_objTemplates[name];
            else if (m_charTemplates.ContainsKey(name))
                found = m_charTemplates[name];
            else
                DiagManager.Instance.LogInfo(String.Format("TemplateManager.FindTemplate({0}): Couldn't find template {0}!", name));

            return found;
        }


        public IEnumerable<BaseTemplate> IterateSpecified(ETemplateType ty)
        {
            switch(ty)
            {
                case ETemplateType.Character:
                    {
                        return IterateCharacters();
                    }
                case ETemplateType.Object:
                    {
                        return IterateObjects();
                    }
                default:
                    {
                        throw new Exception();
                    }
            }
        }

        public IEnumerable<CharacterTemplate> IterateCharacters()
        {
            return m_charTemplates.Values;
        }

        public IEnumerable<ObjectTemplate> IterateObjects()
        {
            return m_objTemplates.Values;
        }
    }


    ///// <summary>
    ///// This handles templates unique to a map. Store this in a map, and pass it to the template manager to access its content!
    ///// </summary>
    //[Serializable]
    //class LocalTemplates
    //{
    //    private Dictionary<string, BaseTemplate> m_templates = new Dictionary<string, BaseTemplate>();


    //    /// <summary>
    //    /// Assign or insert a template into the manager.
    //    /// </summary>
    //    /// <param name="templatename"></param>
    //    /// <param name="templ"></param>
    //    public void SetTemplate(string templatename, BaseTemplate templ)
    //    {
    //        m_templates[templatename] = templ;
    //    }

    //    /// <summary>
    //    /// Find a named template in the list of local templates and returns the basetemplate.
    //    /// </summary>
    //    /// <param name="name">Name of the template to find.</param>
    //    /// <returns>BaseTemplate found, or null if not found.</returns>
    //    public BaseTemplate FindTemplate(string name)
    //    {
    //        BaseTemplate found = null;
    //        if (!m_templates.TryGetValue(name, out found))
    //            DiagManager.Instance.LogInfo(String.Format("LocalTemplates.FindTemplate({0}): Couldn't find template {0}!", name));
    //        return found;
    //    }
    //}
}

