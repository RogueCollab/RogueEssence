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
using Avalonia.Data.Converters;

namespace RogueEssence.Dev
{
    public static class DataEditor
    {
        private static List<IEditor> editors;

        public static object clipboardObj;

        public static void Init()
        {
            clipboardObj = new object();
            editors = new List<IEditor>();
        }

        public static void AddEditor(IEditor editor)
        {
            //maintain inheritance order
            for (int ii = 0; ii < editors.Count; ii++)
            {
                if (editor.GetConvertingType().IsSubclassOf(editors[ii].GetConvertingType()))
                {
                    editors.Insert(ii, editor);
                    return;
                }
                else if (editor.GetConvertingType() == editors[ii].GetConvertingType() && editor.GetAttributeType() != null && editors[ii].GetAttributeType() == null)
                {
                    editors.Insert(ii, editor);
                    return;
                }
            }
            editors.Add(editor);
        }

        public static void LoadDataControls(object obj, StackPanel control)
        {
            LoadClassControls(control, obj.ToString(), obj.GetType(), new object[0], obj, true);
        }

        private static IEditor findEditor(Type objType, object[] attributes)
        {
            foreach (IEditor editor in editors)
            {
                Type editType = editor.GetConvertingType();
                if (editType.IsAssignableFrom(objType))
                {
                    Type attrType = editor.GetAttributeType();
                    if (attrType == null)
                        return editor;
                    else
                    {
                        foreach (object attr in attributes)
                        {
                            if (attr.GetType() == attrType)
                                return editor;
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

        public static StringConv GetStringConv(Type type, object[] attributes)
        {
            IEditor editor = findEditor(type, attributes);
            return new StringConv(type, editor, attributes);
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

