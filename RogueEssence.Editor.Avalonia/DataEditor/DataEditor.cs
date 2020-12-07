using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RogueEssence.Content;
using RogueEssence.Dungeon;
using RogueElements;
using System.IO;
using Avalonia.Controls;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using RogueEssence.Dev.Views;
using Microsoft.Xna.Framework;
using Avalonia.Interactivity;
using Avalonia;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace RogueEssence.Dev
{
    public static class DataEditor
    {
        private static List<IEditor> converters;

        public static object clipboardObj;

        public static void Init()
        {
            clipboardObj = new object();
            converters = new List<IEditor>();
            //AddConverter(new AutoTileBaseConverter());
            AddConverter(new BaseEmitterEditor());
            AddConverter(new BattleDataEditor());
            AddConverter(new BattleFXEditor());
            AddConverter(new CircleSquareEmitterEditor());
            AddConverter(new CombatActionEditor());
            AddConverter(new ExplosionDataEditor());
            //AddConverter(new ItemDataConverter());
            //AddConverter(new TileLayerConverter());
            AddConverter(new ShootingEmitterEditor());
            AddConverter(new SkillDataEditor());
            AddConverter(new ColumnAnimEditor());
            AddConverter(new StaticAnimEditor());
            AddConverter(new TypeDictEditor());
            //AddConverter(new SpawnListConverter());
            //AddConverter(new SpawnRangeListConverter());
            AddConverter(new PriorityListEditor());
            AddConverter(new PriorityEditor());
            AddConverter(new SegLocEditor());
            AddConverter(new LocEditor());
            AddConverter(new IntRangeEditor());
            AddConverter(new FlagTypeEditor());
            AddConverter(new ColorEditor());
            AddConverter(new TypeEditor());
            AddConverter(new DictionaryEditor());
            AddConverter(new ListEditor());
            AddConverter(new ArrayEditor());
            AddConverter(new EnumEditor());
            AddConverter(new StringEditor());
            AddConverter(new DoubleEditor());
            AddConverter(new SingleEditor());
            AddConverter(new BooleanEditor());
            AddConverter(new IntEditor());
            AddConverter(new ByteEditor());
            AddConverter(new ObjectEditor());
        }

        public static void AddConverter(IEditor converter)
        {
            //maintain inheritance order
            for (int ii = 0; ii < converters.Count; ii++)
            {
                if (converter.GetConvertingType().IsSubclassOf(converters[ii].GetConvertingType()))
                {
                    converters.Insert(ii, converter);
                    return;
                }
            }
            converters.Add(converter);
        }


        public static void LoadDataControls(object obj, StackPanel control)
        {
            loadMemberControl(obj, control, obj.ToString(), obj.GetType(), null, obj, true);
        }

        public static void loadClassControls(StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            Type objType = member.GetType();
            Type[] interfaces = objType.GetInterfaces();
            foreach (IEditor converter in converters)
            {
                Type convertType = converter.GetConvertingType();
                if (convertType == objType || objType.IsSubclassOf(convertType) || interfaces.Contains(convertType))
                {
                    converter.LoadClassControls(control, name, type, attributes, member, isWindow);
                    return;
                }
            }

            throw new ArgumentException("Unhandled type!");
            //StaticLoadClassControls(member, control);
        }

        public static void LoadWindowControls(StackPanel control, string name, Type type, object[] attributes, object obj)
        {
            Type[] interfaces = type.GetInterfaces();
            foreach (IEditor converter in converters)
            {
                Type convertType = converter.GetConvertingType();
                if (convertType == type || type.IsSubclassOf(convertType) || interfaces.Contains(convertType))
                {
                    converter.LoadWindowControls(control, name, type, attributes, obj);
                    return;
                }
            }

            throw new ArgumentException("Unhandled type!");
        }

        public static Avalonia.Controls.Grid getSharedRowPanel(int cols)
        {
            Avalonia.Controls.Grid sharedRowPanel = new Avalonia.Controls.Grid();
            for(int ii = 0; ii < cols; ii++)
                sharedRowPanel.ColumnDefinitions.Add(new ColumnDefinition());

            return sharedRowPanel;
        }

        public static void loadMemberControl(object obj, StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            Type objType = obj.GetType();
            Type[] interfaces = objType.GetInterfaces();
            foreach (IEditor converter in converters)
            {
                Type convertType = converter.GetConvertingType();
                if (convertType == objType || objType.IsSubclassOf(convertType) || interfaces.Contains(convertType))
                {
                    converter.LoadMemberControl(obj, control, name, type, attributes, member, isWindow);
                    return;
                }
            }
            loadClassControls(control, name, type, attributes, member, isWindow);
        }

        public static void SaveDataControls(ref object obj, StackPanel control)
        {
            obj = saveMemberControl(obj, control, obj.ToString(), obj.GetType(), null, true);
        }

        //TODO: do a sweep of this and LoadClassControls; we may want to call Save/Load WindowControls instead
        public static object saveClassControls(StackPanel control, string name, Type type, object[] attributes, bool isWindow)
        {
            Type[] interfaces = type.GetInterfaces();
            foreach (IEditor converter in converters)
            {
                Type convertType = converter.GetConvertingType();
                if (convertType == type || type.IsSubclassOf(convertType) || interfaces.Contains(convertType))
                {
                    return converter.SaveClassControls(control, name, type, attributes, isWindow);
                }
            }
            throw new ArgumentException("Unhandled type!");
        }


        public static object SaveWindowControls(StackPanel control, string name, Type type, object[] attributes)
        {
            Type[] interfaces = type.GetInterfaces();
            foreach (IEditor converter in converters)
            {
                Type convertType = converter.GetConvertingType();
                if (convertType == type || type.IsSubclassOf(convertType) || interfaces.Contains(convertType))
                {
                    return converter.SaveWindowControls(control, name, type, attributes);
                }
            }
            throw new ArgumentException("Unhandled type!");
        }


        public static object saveMemberControl(object obj, StackPanel control, string name, Type type, object[] attributes, bool isWindow)
        {
            Type objType = obj.GetType();
            Type[] interfaces = objType.GetInterfaces();
            foreach (IEditor converter in converters)
            {
                Type convertType = converter.GetConvertingType();
                if (convertType == objType || objType.IsSubclassOf(convertType) || interfaces.Contains(convertType))
                {
                    return converter.SaveMemberControl(obj, control, name, type, attributes, isWindow);
                }
            }

            return saveClassControls(control, name, type, attributes, isWindow);
        }

        //TODO: WPF data binding would invalidate this

        public static ReflectionExt.TypeStringConv GetStringRep(Type type, object[] attributes)
        {
            if (type == typeof(Int32))
            {
                DataTypeAttribute dataAtt = ReflectionExt.FindAttribute<DataTypeAttribute>(attributes);
                FrameTypeAttribute frameAtt = ReflectionExt.FindAttribute<FrameTypeAttribute>(attributes);
                if (dataAtt != null)
                {
                    Data.EntryDataIndex nameIndex = Data.DataManager.Instance.DataIndices[dataAtt.DataType];
                    return (obj) => { return ((int)obj >= 0 & (int)obj < nameIndex.Count) ? nameIndex.Entries[(int)obj].GetLocalString(false) : "---"; };
                }
                else if (frameAtt != null)
                {
                    return (obj) => { return ((int)obj >= 0 & (int)obj < GraphicsManager.Actions.Count) ? GraphicsManager.Actions[(int)obj].Name : "---"; };
                }
            }
            return (obj) => { return obj == null ? "[NULL]" : obj.ToString(); };
        }

        public static void setClipboardObj(object obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);

                stream.Flush();
                stream.Position = 0;

                clipboardObj = formatter.Deserialize(stream);
            }
        }
    }
}

