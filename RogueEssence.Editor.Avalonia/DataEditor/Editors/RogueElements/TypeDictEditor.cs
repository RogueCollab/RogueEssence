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
using RogueEssence.Dev.ViewModels;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Layout;
using RogueEssence.Dev.Utility;

namespace RogueEssence.Dev
{
    public class TypeDictEditor : Editor<ITypeDict>
    {

        public TypeDictEditor(EditorContext context) : base(context)
        {
            
        }
        public override bool DefaultSubgroup => true;

        public override bool DefaultDecoration => false;
        public override bool DefaultLabel => false;

        public override void LoadWindowControls(StackPanel control, string parent, Type parentType, string name, Type type, object[] attributes, ITypeDict member, Type[] subGroupStack)
        {
            Type elementType = ReflectionExt.GetBaseTypeArg(typeof(ITypeDict<>), member.GetType(), 0);

            CollectionBox lbxValue = new CollectionBox();

            EditorHeightAttribute heightAtt = ReflectionExt.FindAttribute<EditorHeightAttribute>(attributes);
            if (heightAtt != null)
                lbxValue.MaxHeight = heightAtt.Height;
            else
                lbxValue.MaxHeight = 180;

            CollectionBoxViewModel vm = new CollectionBoxViewModel(_context.DialogService, new StringConv(elementType, ReflectionExt.GetPassableAttributes(1, attributes)));

            CollectionAttribute confirmAtt = ReflectionExt.FindAttribute<CollectionAttribute>(attributes);
            if (confirmAtt != null)
                vm.ConfirmDelete = confirmAtt.ConfirmDelete;

            lbxValue.DataContext = vm;
            lbxValue.MinHeight = lbxValue.MaxHeight;//TODO: Uptake Avalonia fix for improperly updating Grid control dimensions

            //add lambda expression for editing a single element
            vm.OnEditItem += (int index, object element, bool advancedEdit, CollectionBoxViewModel.EditElementOp op) =>
            {
                EditorPageViewModel pageViewModel = control.FindAncestorViewModel<EditorPageViewModel>();
                string elementName = name + "[" + index + "]";
                // string title = DataEditor.GetWindowTitle(parent, elementName, element, elementType, ReflectionExt.GetPassableAttributes(1, attributes));

                NodeBase node =
                    _context.NodeFactory.CreateReflectedDataNode<ReflectedDataPageViewModel>(elementName,
                        pageViewModel.Node.Icon);
                pageViewModel.Node.AddNodeIfNotExists(node);

                NodeHelper.ExpandParents(node, true);
                ReflectedDataPageViewModel
                    newEditor = _context.PageFactory.CreatePage<ReflectedDataPageViewModel>(node);
                newEditor.SetPageTitle(elementName, pageViewModel.Node.Icon);

                newEditor.OnLoadAction = (StackPanel stack) =>
                {
                    DataEditor.LoadClassControls(stack, parent, null, elementName, elementType,
                        ReflectionExt.GetPassableAttributes(1, attributes), element, true, new Type[0],
                        advancedEdit);
                };

                newEditor.OnOKAction = async (StackPanel stack) =>
                {
                    object newElement = DataEditor.SaveClassControls(stack, elementName, elementType,
                        ReflectionExt.GetPassableAttributes(1, attributes), true, new Type[0], advancedEdit);

                    bool itemExists = false;
                    List<object> states = (List<object>)vm.GetList(typeof(List<object>));
                    for (int ii = 0; ii < states.Count; ii++)
                    {
                        //ignore the current index being edited
                        //if the element is null, then we are editing a new object, so skip
                        if (ii != index || element == null)
                        {
                            if (states[ii].GetType() == newElement.GetType())
                                itemExists = true;
                        }
                    }

                    if (itemExists)
                    {
                        await MessageBoxWindowView.Show(_context.DialogService, "Cannot add duplicate states.",
                            "Entry already exists.", MessageBoxWindowView.MessageBoxButtons.Ok);
                        return false;
                    }
                    else
                    {
                        op(index, newElement);
                        return true;
                    }
                };

                _context.TabEvents.AddChildPage(pageViewModel, newEditor);
            };

            List<object> states = new List<object>();
            foreach (object state in member)
                states.Add(state);
            vm.LoadFromList(states);



            ListCollapseAttribute collapseAtt = ReflectionExt.FindAttribute<ListCollapseAttribute>(attributes);

            string desc = DevDataManager.GetMemberDoc(parentType, name);
            if (collapseAtt != null)
            {

                Expander expander = new Expander();
                expander.Header = Text.GetMemberTitle(name) + ":";
                if (desc != null)
                    ToolTip.SetTip(expander, desc);
                expander.IsExpanded = member.Count > 0;
                expander.HorizontalAlignment = HorizontalAlignment.Stretch;
                expander.Content = lbxValue;
                expander.Padding = new Thickness(0);
                control.Children.Add(expander);
            }
            else
            {
                LoadLabelControl(control, name, desc);
                control.Children.Add(lbxValue);
            }
        }

        public override ITypeDict SaveWindowControls(StackPanel control, string name, Type type, object[] attributes, Type[] subGroupStack)
        {
            int controlIndex = 0;

            ListCollapseAttribute collapseAtt = ReflectionExt.FindAttribute<ListCollapseAttribute>(attributes);

            CollectionBox lbxValue;
            if (collapseAtt != null)
                lbxValue = (CollectionBox)((Expander)control.Children[controlIndex]).Content;
            else
            {
                controlIndex++;
                lbxValue = (CollectionBox)control.Children[controlIndex];
            }

            ITypeDict member = (ITypeDict)Activator.CreateInstance(type);
            CollectionBoxViewModel mv = (CollectionBoxViewModel)lbxValue.DataContext;
            List<object> states = (List<object>)mv.GetList(typeof(List<object>));
            for (int ii = 0; ii < states.Count; ii++)
                member.Set(states[ii]);
            return member;
        }
    }
}
