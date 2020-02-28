using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WolcenEditor
{
    public sealed class ImageComboBox : ComboBox
    {
        public ImageComboBox()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            DropDownStyle = ComboBoxStyle.DropDownList;
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();

            if (e.Index >= 0 && e.Index < Items.Count)
            {
                DropDownItem item = (DropDownItem)Items[e.Index];
                e.Graphics.DrawImage(item.Image, e.Bounds.Left, e.Bounds.Top + 2);
                e.Graphics.DrawString(item.Value, e.Font, new SolidBrush(e.ForeColor), e.Bounds.Left + item.Image.Width, e.Bounds.Top + 2);
            }

            base.OnDrawItem(e);
        }
    }

    public class DropDownItem
    {

        public string Value { get; set; }
        public Image Image { get; set; }

        public DropDownItem() : this("", Color.White, 50, 50)
        { }

        public DropDownItem(string val, Bitmap img)
        {
            Value = val;
            Image = img;
        }

        public DropDownItem(string val, Color color, int width, int height)
        {
            Value = val;
            Image = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(Image))
            {
                using (Brush b = new SolidBrush(color))
                {
                    g.DrawRectangle(Pens.White, 0, 0, Image.Width, Image.Height);
                    g.FillRectangle(b, 1, 1, Image.Width - 1, Image.Height - 1);
                }
            }
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
