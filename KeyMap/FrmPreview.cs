using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KeyMap
{
    public partial class FrmPreview : Form
    {
        Control parent;
        Dictionary<Keys, int> keyCount = null;

        public FrmPreview(Dictionary<Keys, int> keyCount)
        {
            InitializeComponent();
            this.keyCount = keyCount;
            parent = this;

            SetKeysColor();
        }

        /// <summary>
        /// Set the keys colors
        /// </summary>
        public void SetKeysColor()
        {
            int max = GetMax();
            double limit = GetLimit(max);

            foreach (var keyValuePair in keyCount)
            {
                Control control = FindKey(parent, keyValuePair.Key.ToString());
                if (control == null)
                    continue;

                double percentage = GetPercentage(keyValuePair.Value, max);
                control.BackColor = GetColor(percentage, limit);
                string text = $"{(100*percentage).ToString("#0.0#")}% ({keyValuePair.Value})";
                toolTip1.SetToolTip(control, text);
            }
        }

        public double GetLimit(int max)
        {
            // Get percentage of median
            return GetPercentage(keyCount.ElementAt(keyCount.Count / 2).Value, max);
        }

        /// <summary>
        /// Return the percentage
        /// </summary>
        public double GetPercentage(int value, int max)
        {
            return (double)value / max;
        }

        /// <summary>
        /// Retourne la valeure milieu
        /// </summary>
        public int GetMax()
        {
            return keyCount.FirstOrDefault().Value;
        }

        /// <summary>
        /// Get color corresponding to percentage. 0 is green, 1 is red
        /// </summary>
        public Color GetColor(double percentage, double limit)
        {
            if (percentage < 0 || percentage > 1)
                return Color.Cyan;

            const int green0 = 255;
            const int green50 = 165;
            const int green100 = 0;

            const int red0 = 0;
            const int red50 = 255;
            const int red100 = 255;

            const int alpha = 165;

            // 0 to 0.5 : green to orange : green from 255 to 165, red from 0 to 255
            // 0.5 to 1 : orange to red : green from 165 to 0, red from 255 to 255
            int deltaGreen0_50 = green0 - green50;
            int deltaRed0_50 = red0 - red50;
            double mult0_50 = 1 / limit;

            int deltaGreen50_100 = green50 - green100;
            int deltaRed50_100 = red50 - red100;
            double mult50_100 = 1 / (1 - limit);

            int red;
            int green;
            if (percentage < limit)
            {
                red = red0 - (int)(deltaRed0_50 * percentage * mult0_50);
                green = green0 - (int)(deltaGreen0_50 * percentage * mult0_50);
            }
            else
            {
                red = red50 - (int)(deltaRed50_100 * (percentage - limit) * mult50_100);
                green = green50 - (int)(deltaGreen50_100 * (percentage - limit) * mult50_100);
            }

            return Color.FromArgb(alpha, red, green, 0);
        }

        /// <summary>
        /// Find the control corresponding to the key
        /// </summary>
        public Control FindKey(Control parent, string keyName)
        {
            foreach (Control item in parent.Controls)
            {
                if (string.Equals(item.Text, keyName, StringComparison.OrdinalIgnoreCase))
                    return item;
                if (item.HasChildren)
                {
                    Control foundControl = FindKey(item, keyName);
                    if (foundControl != null)
                        return foundControl;
                }
            }
            return null;
        }
    }
}
