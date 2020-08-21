using System;
using System.Windows.Forms;

namespace RogueEssence.Dev
{
    public class IntNumericUpDown : NumericUpDown
    {
        protected override void OnTextBoxKeyPress(object source, KeyPressEventArgs e)
        {
            if (e.KeyChar == 44 || e.KeyChar == 46)
                e.Handled = true;

            base.OnTextBoxKeyPress(source, e);
        }



        public int GetIntFromNumeric()
        {
            int value;
            if (!Int32.TryParse(Text, out value))
                value = 0;
            if (value >= Minimum && value <= Maximum)
                return value;
            else
                return (int)Value;
        }
    }
}
