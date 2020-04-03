using System.Windows.Forms;
using System;
using System.Drawing;
using System.Diagnostics;

namespace WolcenEditor
{
    public partial class NewTextDialog : Form
    {
        public NewTextDialog(int width, int height, string Title)
        {
            this.Text = Title;
            this.MaximumSize = new Size(width, height);
            this.MinimumSize = new Size(width, height);
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
        }

        public void DrawLabel(int y, string text, int fontSize, bool bold = false)
        {
            Font f = new Font(Form1.DefaultFont.FontFamily, fontSize, bold ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Pixel);

            Label lbl = new Label
            {
                Text = text,
                Location = new Point(-5, y),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = f,
                Parent = this
            };

            lbl.AutoSize = false;
            lbl.Width = this.Width;
            lbl.Height = fontSize + 5;

            this.Controls.Add(lbl);
        }

        public void DrawLinkLabel(int y, string text, Uri link, int fontSize, bool bold = false)
        {
            Font f = new Font(Form1.DefaultFont.FontFamily, fontSize, bold ? FontStyle.Bold : FontStyle.Regular, GraphicsUnit.Pixel);

            Label lbl = new Label
            {
                Name = "linkLabel",
                Text = text,
                Location = new Point(-5, y),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = f,
                Parent = this,
                ForeColor = Color.CadetBlue,
                Tag = link
            };

            lbl.AutoSize = false;
            lbl.Width = this.Width;
            lbl.Height = fontSize + 5;

            lbl.Click += Lbl_Click;
            lbl.MouseMove += Lbl_MouseMove;
            lbl.MouseLeave += Lbl_MouseLeave;

            this.Controls.Add(lbl);

        }

        private void Lbl_MouseLeave(object sender, EventArgs e)
        {
            Font f = new Font((sender as Label).Font.FontFamily, (sender as Label).Font.Size, FontStyle.Bold, GraphicsUnit.Pixel);
            (sender as Label).Font = f;
            (sender as Label).ForeColor = Color.CadetBlue;
        }

        private void Lbl_MouseMove(object sender, MouseEventArgs e)
        {
            Font f = new Font((sender as Label).Font.FontFamily, (sender as Label).Font.Size, FontStyle.Bold | FontStyle.Underline | FontStyle.Italic, GraphicsUnit.Pixel);
            (sender as Label).Font = f;
            (sender as Label).ForeColor = Color.DarkSlateBlue;
            Cursor.Current = Cursors.Hand;
        }

        private void Lbl_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Process.Start((sender as Label).Tag.ToString());
        }
    }
}
