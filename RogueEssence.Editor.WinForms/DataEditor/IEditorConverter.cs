using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace RogueEssence.Dev
{
    public interface IEditorConverter
    {
        Type GetConvertingType();

        void LoadClassControls(object obj, TableLayoutPanel control);

        void LoadMemberControl(object obj, TableLayoutPanel control, string name, Type type, object[] attributes, object member, bool isWindow);

        void SaveClassControls(object obj, TableLayoutPanel control);

        void SaveMemberControl(object obj, TableLayoutPanel control, string name, Type type, object[] attributes, ref object member, bool isWindow);
    }
}
