using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace WolcenEditor
{
    public class SkillTree
    {
        public static bool isShiftDown { get; set; }
        public static bool isCtrlDown { get; set; }

        public static Dictionary<string, SkillImageBox> SkillTreeDict = new Dictionary<string, SkillImageBox>
        {
            { "player_aetherblade", new SkillImageBox("Infinity Blades") },
            { "player_aetherblast", new SkillImageBox("Eclipse") },
            { "player_arrowsrain", new SkillImageBox("Wailing Arrows") },
            { "player_bladeslinger", new SkillImageBox("Phantom Blades") },
            { "player_bomb", new SkillImageBox("Havoc Orb") },
            { "player_brutalstrike", new SkillImageBox("Anvil's Woe") },
            { "player_bulleye", new SkillImageBox("Gunslinger's Brand") },
            { "player_chainlightning", new SkillImageBox("Thunderstrike") },
            { "player_charge", new SkillImageBox("Warpath") },
            { "player_corpseexplosion", new SkillImageBox("Plagueburst") },
            { "player_deathmark", new SkillImageBox("Mark of Impurity") },
            { "player_dualstrike", new SkillImageBox("Slayer's Flurry") },
            { "player_fireball", new SkillImageBox("Consuming Embers") },
            { "player_frostnova", new SkillImageBox("Winter's Grasp") },
            { "player_frostcomet", new SkillImageBox("Tear of Etheliel") },
            { "player_frostlance", new SkillImageBox("Arctic Spear") },
            { "player_hammer", new SkillImageBox("Flight of Gaavanir") },
            { "player_hook", new SkillImageBox("Tracker's Reach") },
            { "player_ironguard", new SkillImageBox("Juggernaut") },
            { "player_laceration", new SkillImageBox("Bleeding Edge") },
            { "player_laser", new SkillImageBox("Annihilation") },
            { "player_leap", new SkillImageBox("Wings of Ishmir") },
            { "player_sacredground", new SkillImageBox("Bulwark of Dawn") },
            { "player_smokebomb", new SkillImageBox("Duskshroud") },
            { "player_sniper", new SkillImageBox("\"Deathgazer\" Railgun") },
            { "player_summon_melee", new SkillImageBox("Feeding Swarm") },
            { "player_summon_ranged", new SkillImageBox("Hunting Swarm") },
            { "player_teleport", new SkillImageBox("Aether Jump") },
            { "player_turret", new SkillImageBox("\"Avenger\" Autoturret") },
            { "player_vault", new SkillImageBox("Evasion") },
            { "player_vortex", new SkillImageBox("Anomaly") },
            { "player_warcry", new SkillImageBox("Sovereign Shout") },
            { "player_whirlwind", new SkillImageBox("Bladestorm") },
            { "player_ringofpain", new SkillImageBox("Blood for Blood") },
            { "player_reave", new SkillImageBox("Wrath of Baapheth") },
            { "player_solarfall", new SkillImageBox("Solarfall") }
        };

        public static void LoadTree(ref TabControl tabControl)
        {
            isShiftDown = false;
            int x = 15;
            int y = 25;
            int lineCount = 1;
            foreach (var skill in SkillTreeDict)
            {
                skill.Value.sImage = new Bitmap(Image.FromFile(@".\UIResources\Skills\" + skill.Key + ".png"), 50, 50);

                PictureBox pb = new PictureBox();
                pb.Name = "_" + skill.Key;
                pb.Size = skill.Value.sImage.Size;
                pb.MaximumSize = skill.Value.sImage.Size;
                pb.BackgroundImage = skill.Value.sImage;
                pb.BackgroundImageLayout = ImageLayout.Stretch;
                pb.Image = WolcenEditor.Properties.Resources.c_beltSlot;
                pb.MouseMove += Pb_MouseMove;
                pb.MouseLeave += Pb_MouseLeave;
                pb.MouseClick += Pb_MouseClick;
                pb.Visible = true;
                pb.Location = new Point(x, y);
                pb.BorderStyle = BorderStyle.Fixed3D;
                tabControl.TabPages["charSkills"].Controls.Add(pb);

                ToolTip tp = new ToolTip();
                tp.SetToolTip(pb, "Left click to activate or level up a skill.\n"
                                + "Right click to remove a level.\n"
                                + "Shift (right/left) click to (add/remove) levels.\n"
                                + "Ctrl right click to remove skill from player.");

                Label lb = new Label();
                lb.Text = skill.Value.Name;
                lb.MaximumSize = new Size(74, 30);
                lb.MinimumSize = lb.MaximumSize;
                lb.AutoSize = true;
                lb.TextAlign = ContentAlignment.TopCenter;
                lb.Font = new Font(Form1.DefaultFont.FontFamily, 7, FontStyle.Regular);
                lb.ForeColor = Color.White;
                lb.Location = new Point(x - 12, y + pb.Height + 2);
                //lb.BorderStyle = BorderStyle.FixedSingle;
                tabControl.TabPages["charSkills"].Controls.Add(lb);

                Label lb2 = new Label();
                lb2.Name = "_lbl" + skill.Key;
                lb2.MaximumSize = new Size(30, 20);
                lb2.MinimumSize = lb2.MaximumSize;
                lb2.AutoSize = true;
                lb2.TextAlign = ContentAlignment.MiddleCenter;
                lb2.Font = new Font(Form1.DefaultFont.FontFamily, 11, FontStyle.Regular);
                lb2.ForeColor = Color.White;
                lb2.Location = new Point(x + 20 - 10, y + pb.Height + 2 + lb.Height);
                lb2.Text = "0";
                lb2.BorderStyle = BorderStyle.FixedSingle;
                tabControl.TabPages["charSkills"].Controls.Add(lb2);

                x += skill.Value.sImage.Width + 24;
                lineCount++;
                if (lineCount >= 12)
                {
                    x = 15;
                    lineCount = 1;
                    y += skill.Value.sImage.Height + 75;
                }
            }
        }

        private static void Pb_MouseClick(object sender, MouseEventArgs e)
        {
            if (cData.Character == null) return;
            if (e.Button == MouseButtons.Left)
            {
                if ((sender as PictureBox).Image != null)
                {
                    UnlockedSkill uSkill = new UnlockedSkill();
                    uSkill.SkillName = (sender as PictureBox).Name.Substring(1, (sender as PictureBox).Name.Length - 1);
                    uSkill.Level = 0;
                    uSkill.CurrentXp = "0";
                    uSkill.Variants = "0000000000000000";
                    cData.Character.UnlockedSkills.Add(uSkill);
                    (sender as PictureBox).Image = null;
                }
                else
                {
                    int i = 0;
                    foreach (UnlockedSkill skill in cData.Character.UnlockedSkills)
                    {
                        if ((sender as PictureBox).Name == ("_" + skill.SkillName))
                        {
                            if (isShiftDown)
                            {
                                if (cData.Character.UnlockedSkills[i].Level + 10 >= 90)
                                {
                                    cData.Character.UnlockedSkills[i].Level = 90;
                                }
                                else
                                {
                                    cData.Character.UnlockedSkills[i].Level += 10;
                                }
                            }
                            else
                            {
                                if (cData.Character.UnlockedSkills[i].Level >= 90) return;
                                cData.Character.UnlockedSkills[i].Level++;
                            }
                            (sender as PictureBox).Parent.Controls["_lbl" + skill.SkillName].Text = cData.Character.UnlockedSkills[i].Level.ToString();
                            return;
                        }
                        i++;
                    }
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                int i = 0;
                if (isCtrlDown)
                {
                    if ((sender as PictureBox).Image == null)
                    {
                        foreach (var uSkill in cData.Character.UnlockedSkills)
                        {
                            if ((sender as PictureBox).Name.Substring(1, (sender as PictureBox).Name.Length - 1) == uSkill.SkillName)
                            {
                                cData.Character.UnlockedSkills.RemoveAt(i);
                                TabControl tabControl = (TabControl)(sender as PictureBox).Parent.Parent;
                                LoadSkillInformation(ref tabControl);
                                return;
                            }
                            i++;
                        }
                    }
                }

                i = 0;
                foreach (UnlockedSkill skill in cData.Character.UnlockedSkills)
                {
                    if ((sender as PictureBox).Name == ("_" + skill.SkillName))
                    {
                        if (isShiftDown)
                        {
                            if (cData.Character.UnlockedSkills[i].Level - 10 <= 0)
                            {
                                cData.Character.UnlockedSkills[i].Level = 0;
                            }
                            else
                            {
                                cData.Character.UnlockedSkills[i].Level -= 10;
                            }
                        }
                        else
                        {
                            if (cData.Character.UnlockedSkills[i].Level <= 0) return;
                            cData.Character.UnlockedSkills[i].Level--;
                        }
                        (sender as PictureBox).Parent.Controls["_lbl" + skill.SkillName].Text = cData.Character.UnlockedSkills[i].Level.ToString();
                        return;
                    }
                    i++;
                }
            }
        }

        public static void LoadSkillInformation(ref TabControl tabControl)
        {
            foreach (var skill in SkillTreeDict)
            {
                tabControl.TabPages["charSkills"].Controls["_lbl" + skill.Key].Text = "0";
                (tabControl.TabPages["charSkills"].Controls["_" + skill.Key] as PictureBox).Image = WolcenEditor.Properties.Resources.c_beltSlot;
                if (cData.Character != null)
                {
                    foreach (UnlockedSkill uSkill in cData.Character.UnlockedSkills)
                    {
                        if (uSkill.SkillName == skill.Key)
                        {
                            PictureBox pb = (tabControl.TabPages["charSkills"].Controls["_" + skill.Key] as PictureBox);
                            pb.Image = null;
                            Label lb2 = (tabControl.TabPages["charSkills"].Controls["_lbl" + uSkill.SkillName] as Label);
                            lb2.Text = uSkill.Level.ToString();
                        }
                    }
                }
            }
        }

        private static void Pb_MouseLeave(object sender, EventArgs e)
        {
            PictureBox pb = (PictureBox)sender;
            pb.BorderStyle = BorderStyle.Fixed3D;
        }

        private static void Pb_MouseMove(object sender, MouseEventArgs e)
        {
            PictureBox pb = (PictureBox)sender;
            pb.BorderStyle = BorderStyle.FixedSingle;
        }
    }

    public class SkillImageBox
    {
        public string Name { get; set; }
        public Image sImage { get; set; }
        public int Level { get; set; }

        public SkillImageBox(string _name)
        {
            Name = _name;
            sImage = null;
            Level = 0;
        }
    }
}
