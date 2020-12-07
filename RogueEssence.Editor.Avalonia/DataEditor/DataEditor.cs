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
            LoadClassControls(control, obj.ToString(), obj.GetType(), new object[0], obj, true);
        }

        public static void LoadClassControls(StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
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

        public static void LoadMemberControl(object obj, StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
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
            throw new ArgumentException("Unhandled type!");
        }

        public static void SaveDataControls(ref object obj, StackPanel control)
        {
            obj = SaveClassControls(control, obj.ToString(), obj.GetType(), new object[0], true);
        }

        public static object SaveClassControls(StackPanel control, string name, Type type, object[] attributes, bool isWindow)
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


        public static object SaveMemberControl(object obj, StackPanel control, string name, Type type, object[] attributes, bool isWindow)
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
            throw new ArgumentException("Unhandled type!");
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

        public static void SetClipboardObj(object obj)
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

