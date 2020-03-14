using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace WolcenEditor
{
    partial class Form1
    {
        public void initCity()
        {
            var label = new Label();
            label.Text = "WWWWWWWWWWWWWW";
            label.Location = new Point(50, 50);
            label.ForeColor = Color.White;
            charCity.Controls.Add(label);
        }

    }
}
