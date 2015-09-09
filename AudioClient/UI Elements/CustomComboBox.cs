using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AudioClient.UI_Elements
{
    public class CustomComboBox : ComboBox
    {

        public object DataSource
        {
            get { return base.DataSource; }
            set { base.DataSource = value; DetermineDropDownWidth(); }

        }
        public string DisplayMember
        {
            get { return base.DisplayMember; }
            set { base.DisplayMember = value; DetermineDropDownWidth(); }

        }
        public string ValueMember
        {
            get { return base.ValueMember; }
            set { base.ValueMember = value; DetermineDropDownWidth(); }

        }

        public string Text
        {
            get { return base.Text.Substring(0,base.Text.IndexOf("|")); }
            set { base.SelectedText = base.Text.Substring(0, base.Text.IndexOf("|")); }
        }
        private void DetermineDropDownWidth()
        {

            int widestStringInPixels = 0;
            foreach (Object o in Items)
            {
                string toCheck;
                PropertyInfo pinfo;
                Type objectType = o.GetType();
                if (this.DisplayMember.CompareTo("") == 0)
                {
                    toCheck = o.ToString();

                }
                else
                {
                    pinfo = objectType.GetProperty(this.DisplayMember);
                    toCheck = pinfo.GetValue(o, null).ToString();

                }
                if (TextRenderer.MeasureText(toCheck, this.Font).Width > widestStringInPixels)
                    widestStringInPixels = TextRenderer.MeasureText(toCheck, this.Font).Width;
            }
            this.DropDownWidth = widestStringInPixels + 15;
        }
    }
}
