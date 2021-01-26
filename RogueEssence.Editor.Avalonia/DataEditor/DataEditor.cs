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
                else if (converter.GetConvertingType() == converters[ii].GetConvertingType() && converter.GetAttributeType() != null && converters[ii].GetAttributeType() == null)
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

        private static IEditor findEditor(Type objType, object[] attributes)
        {
            foreach (IEditor converter in converters)
            {
                Type convertType = converter.GetConvertingType();
                if (convertType.IsAssignableFrom(objType))
                {
                    Type attrType = converter.GetAttributeType();
                    if (attrType == null)
                        return converter;
                    else
                    {
                        foreach (object attr in attributes)
                        {
                            if (attr.GetType() == attrType)
                                return converter;
                        }
                    }
                }
            }
            throw new ArgumentException("Unhandled type!");
        }

        public static void LoadClassControls(StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            IEditor converter = findEditor(type, attributes);
            converter.LoadClassControls(control, name, type, attributes, member, isWindow);
        }

        public static void LoadWindowControls(StackPanel control, string name, Type type, object[] attributes, object obj)
        {
            IEditor converter = findEditor(type, attributes);
            converter.LoadWindowControls(control, name, type, attributes, obj);
        }

        public static void LoadMemberControl(object obj, StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            IEditor converter = findEditor(obj.GetType(), attributes);
            converter.LoadMemberControl(obj, control, name, type, attributes, member, isWindow);
        }

        public static void SaveDataControls(ref object obj, StackPanel control)
        {
            obj = SaveClassControls(control, obj.ToString(), obj.GetType(), new object[0], true);
        }

        public static object SaveClassControls(StackPanel control, string name, Type type, object[] attributes, bool isWindow)
        {
            IEditor converter = findEditor(type, attributes);
            return converter.SaveClassControls(control, name, type, attributes, isWindow);
        }


        public static object SaveWindowControls(StackPanel control, string name, Type type, object[] attributes)
        {
            IEditor converter = findEditor(type, attributes);
            return converter.SaveWindowControls(control, name, type, attributes);
        }


        public static object SaveMemberControl(object obj, StackPanel control, string name, Type type, object[] attributes, bool isWindow)
        {
            IEditor converter = findEditor(obj.GetType(), attributes);
            return converter.SaveMemberControl(obj, control, name, type, attributes, isWindow);
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

