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
        private static List<IEditorConverter> converters;

        public static object clipboardObj;

        public static void Init()
        {
            clipboardObj = new object();
            converters = new List<IEditorConverter>();
            //AddConverter(new AutoTileBaseConverter());
            AddConverter(new BaseEmitterConverter());
            AddConverter(new BattleDataConverter());
            AddConverter(new BattleFXConverter());
            AddConverter(new CircleSquareEmitterConverter());
            AddConverter(new CombatActionConverter());
            AddConverter(new ExplosionDataConverter());
            //AddConverter(new ItemDataConverter());
            //AddConverter(new TileLayerConverter());
            AddConverter(new ShootingEmitterConverter());
            AddConverter(new SkillDataConverter());
            AddConverter(new ColumnAnimConverter());
            AddConverter(new StaticAnimConverter());
            AddConverter(new TypeDictConverter());
            //AddConverter(new SpawnListConverter());
            //AddConverter(new SpawnRangeListConverter());
            AddConverter(new PriorityListConverter());
            AddConverter(new PriorityConverter());
            AddConverter(new SegLocConverter());
            AddConverter(new LocConverter());
            AddConverter(new IntRangeConverter());
            AddConverter(new FlagTypeConverter());
            AddConverter(new ColorConverter());
            AddConverter(new TypeConverter());
            AddConverter(new DictionaryConverter());
            AddConverter(new ListConverter());
            AddConverter(new ArrayConverter());
            AddConverter(new EnumConverter());
            AddConverter(new StringConverter());
            AddConverter(new DoubleConverter());
            AddConverter(new SingleConverter());
            AddConverter(new BooleanConverter());
            AddConverter(new IntConverter());
            AddConverter(new ByteConverter());
            AddConverter(new EditorConverter<object>());
        }

        public static void AddConverter(IEditorConverter converter)
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

        private static void loadClassControls(StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            Type objType = member.GetType();
            Type[] interfaces = objType.GetInterfaces();
            foreach (IEditorConverter converter in converters)
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

        public static void LoadWindowControls(object obj, StackPanel control)
        {
            Type objType = obj.GetType();
            Type[] interfaces = objType.GetInterfaces();
            foreach (IEditorConverter converter in converters)
            {
                Type convertType = converter.GetConvertingType();
                if (convertType == objType || objType.IsSubclassOf(convertType) || interfaces.Contains(convertType))
                {
                    converter.LoadWindowControls(obj, control);
                    return;
                }
            }

            throw new ArgumentException("Unhandled type!");
        }

        public static void StaticLoadClassControls(object obj, StackPanel control)
        {
            //go through all members and add for them
            //control starts off clean; this is the control that will have all member controls on it
            try
            {
                Type type = obj.GetType();

                List<MemberInfo> myFields = type.GetEditableMembers();

                List<List<MemberInfo>> tieredFields = new List<List<MemberInfo>>();
                for (int ii = 0; ii < myFields.Count; ii++)
                {
                    if (myFields[ii].GetCustomAttributes(typeof(NonEditedAttribute), false).Length > 0)
                        continue;
                    if (myFields[ii].GetCustomAttributes(typeof(NonSerializedAttribute), false).Length > 0)
                        continue;

                    object member = myFields[ii].GetValue(obj);
                    if (member == null && myFields[ii].GetCustomAttributes(typeof(NonNullAttribute), false).Length > 0)
                        throw new Exception("Null class member found in " + type.ToString() + ": " + myFields[ii].Name);

                    if (myFields[ii].GetCustomAttributes(typeof(SharedRowAttribute), false).Length == 0)
                        tieredFields.Add(new List<MemberInfo>());
                    tieredFields[tieredFields.Count - 1].Add(myFields[ii]);
                }

                for (int ii = 0; ii < tieredFields.Count; ii++)
                {
                    if (tieredFields[ii].Count == 1)
                    {
                        MemberInfo myInfo = tieredFields[ii][0];
                        StackPanel stack = new StackPanel();
                        control.Children.Add(stack);
                        loadMemberControl(obj, stack, myInfo.Name, myInfo.GetMemberInfoType(), myInfo.GetCustomAttributes(false), myInfo.GetValue(obj), false);
                    }
                    else
                    {
                        Avalonia.Controls.Grid sharedRowPanel = getSharedRowPanel(tieredFields[ii].Count);
                        control.Children.Add(sharedRowPanel);
                        for (int jj = 0; jj < tieredFields[ii].Count; jj++)
                        {
                            MemberInfo myInfo = tieredFields[ii][jj];
                            StackPanel stack = new StackPanel();
                            sharedRowPanel.Children.Add(stack);
                            stack.SetValue(Avalonia.Controls.Grid.ColumnProperty, jj);
                            loadMemberControl(obj, stack, myInfo.Name, myInfo.GetMemberInfoType(), myInfo.GetCustomAttributes(false), myInfo.GetValue(obj), false);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(e);
            }
        }

        public static Avalonia.Controls.Grid getSharedRowPanel(int cols)
        {
            Avalonia.Controls.Grid sharedRowPanel = new Avalonia.Controls.Grid();
            for(int ii = 0; ii < cols; ii++)
                sharedRowPanel.ColumnDefinitions.Add(new ColumnDefinition());

            return sharedRowPanel;
        }




        private static void loadMemberControl(object obj, StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            Type objType = obj.GetType();
            Type[] interfaces = objType.GetInterfaces();
            foreach (IEditorConverter converter in converters)
            {
                Type convertType = converter.GetConvertingType();
                if (convertType == objType || objType.IsSubclassOf(convertType) || interfaces.Contains(convertType))
                {
                    converter.LoadMemberControl(obj, control, name, type, attributes, member, isWindow);
                    return;
                }
            }
            StaticLoadMemberControl(control, name, type, attributes, member, isWindow);
        }

        public static void StaticLoadMemberControl(StackPanel control, string name, Type type, object[] attributes, object member, bool isWindow)
        {
            try
            {
                loadClassControls(control, name, type, attributes, member, isWindow);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(e);
            }
        }

        public static void SaveDataControls(ref object obj, StackPanel control)
        {
            saveMemberControl(obj, control, obj.ToString(), obj.GetType(), null, ref obj, true);
        }

        private static void saveClassControls(StackPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow)
        {
            Type objType = member.GetType();
            Type[] interfaces = objType.GetInterfaces();
            foreach (IEditorConverter converter in converters)
            {
                Type convertType = converter.GetConvertingType();
                if (convertType == objType || objType.IsSubclassOf(convertType) || interfaces.Contains(convertType))
                {
                    converter.SaveClassControls(control, name, type, attributes, ref member, isWindow);
                    return;
                }
            }
            throw new ArgumentException("Unhandled type!");
            //StaticSaveClassControls(member, control);
        }


        public static void SaveWindowControls(object obj, StackPanel control)
        {
            Type objType = obj.GetType();
            Type[] interfaces = objType.GetInterfaces();
            foreach (IEditorConverter converter in converters)
            {
                Type convertType = converter.GetConvertingType();
                if (convertType == objType || objType.IsSubclassOf(convertType) || interfaces.Contains(convertType))
                {
                    converter.SaveWindowControls(obj, control);
                    return;
                }
            }
            throw new ArgumentException("Unhandled type!");
        }

        public static void StaticSaveClassControls(object obj, StackPanel control)
        {
            try
            {
                Type type = obj.GetType();

                List<MemberInfo> myFields = type.GetEditableMembers();


                List<List<MemberInfo>> tieredFields = new List<List<MemberInfo>>();
                for (int ii = 0; ii < myFields.Count; ii++)
                {
                    if (myFields[ii].GetCustomAttributes(typeof(NonEditedAttribute), false).Length > 0)
                        continue;
                    if (myFields[ii].GetCustomAttributes(typeof(NonSerializedAttribute), false).Length > 0)
                        continue;

                    object member = myFields[ii].GetValue(obj);
                    if (member == null && myFields[ii].GetCustomAttributes(typeof(NonNullAttribute), false).Length > 0)
                        throw new Exception("Null class member found in " + type.ToString() + ": " + myFields[ii].Name);

                    if (myFields[ii].GetCustomAttributes(typeof(SharedRowAttribute), false).Length == 0)
                        tieredFields.Add(new List<MemberInfo>());

                    tieredFields[tieredFields.Count - 1].Add(myFields[ii]);
                }

                int controlIndex = 0;
                for (int ii = 0; ii < tieredFields.Count; ii++)
                {
                    if (tieredFields[ii].Count == 1)
                    {
                        MemberInfo myInfo = tieredFields[ii][0];
                        object member = myInfo.GetValue(obj);

                        saveMemberControl(obj, (StackPanel)control.Children[controlIndex], myInfo.Name, myInfo.GetMemberInfoType(), myInfo.GetCustomAttributes(false), ref member, false);
                        myInfo.SetValue(obj, member);
                        controlIndex++;
                    }
                    else
                    {
                        StackPanel sharedRowControl = (StackPanel)control.Children[controlIndex];
                        int sharedRowControlIndex = 0;
                        for (int jj = 0; jj < tieredFields[ii].Count; jj++)
                        {
                            MemberInfo myInfo = tieredFields[ii][jj];
                            object member = myInfo.GetValue(obj);

                            saveMemberControl(obj, (StackPanel)sharedRowControl.Children[jj], myInfo.Name, myInfo.GetMemberInfoType(), myInfo.GetCustomAttributes(false), ref member, false);
                            myInfo.SetValue(obj, member);
                            sharedRowControlIndex++;
                        }
                        controlIndex++;
                    }
                }
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(e);
            }
        }



        private static void saveMemberControl(object obj, StackPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow)
        {
            Type objType = obj.GetType();
            Type[] interfaces = objType.GetInterfaces();
            foreach (IEditorConverter converter in converters)
            {
                Type convertType = converter.GetConvertingType();
                if (convertType == objType || objType.IsSubclassOf(convertType) || interfaces.Contains(convertType))
                {
                    converter.SaveMemberControl(obj, control, name, type, attributes, ref member, isWindow);
                    return;
                }
            }

            StaticSaveMemberControl(control, name, type, attributes, ref member, isWindow);
        }

        public static void StaticSaveMemberControl(StackPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow)
        {
            //do not set anything
            //on save, write value to object
            //use completely clean controls for iterating child controls
            //must invoke save and load for structs
            //must use the attribute tag
            //does not need members; can be static methods then
            try
            {
                saveClassControls(control, name, type, attributes, ref member, isWindow);
            }
            catch (Exception e)
            {
                DiagManager.Instance.LogError(e);
            }
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

