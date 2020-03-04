using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WolcenEditor
{
    public static class InventoryManager
    {
        private static Dictionary<string, int> charMap = new Dictionary<string, int>
        {
            {"charHelm", 3 },
            {"charChest" , 1},
            {"charLPad", 6 },
            {"charRPad" , 5},
            {"charLHand", 10 },
            {"charRHand" , 9},
            {"charBelt", 19 },
            {"charPants", 11 },
            {"charNeck" , 14},
            {"charBoots", 17 },
            {"charLRing" , 22},
            {"charRRing", 21 },
            {"charLWeapon" , 15},
            {"charRWeapon" , 16},
        };

        public static void LoadCharacterInventory(object sender)
        {
            TabPage tabPage = (sender as TabPage);
            bool flip = false;

            foreach (Control control in tabPage.Controls)
            {
                try
                {
                    PictureBox pictureBox = (control as PictureBox);
                    if(charMap[pictureBox.Name] >= 0)
                    {
                        if (pictureBox.Name == "charLPad" || pictureBox.Name == "charLHand") flip = true;
                        pictureBox.Image = GetItemBitmap(charMap[pictureBox.Name], flip);
                        pictureBox.Click += LoadItemData;
                        //pictureBox.BackgroundImage = setRarityBackground(pictureBox);
                    }
                }
                catch (Exception) { }
            }
        }

        private static int posY = 0;

        private static void LoadItemData(object sender, EventArgs e)
        {
            Panel itemStatDisplay = ((sender as PictureBox).Parent.Controls["itemStatDisplay"] as Panel);
            UnloadItemData(itemStatDisplay);

            PictureBox pictureBox = (sender as PictureBox);
            int bodyPart = charMap[pictureBox.Name];
            string itemName = getItemName_Color(bodyPart);
            if (itemName == "Item not found!") return;
            Color nameColor = ColorTranslator.FromHtml("#" + itemName.Split('#')[1]);
            itemName = itemName.Split('#')[0];
            itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, WolcenStaticData.ItemLocalizedNames[itemName], itemStatDisplay, 13, nameColor));
            if (bodyPart != 15 && bodyPart != 16)
            {       // Armor
                string itemStat = getItemStat(bodyPart, "Health");
                if (itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Health: " + itemStat, itemStatDisplay, 9, Color.White));
                itemStat = getItemStat(bodyPart, "Armor");
                if (itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Force Shield: " + itemStat, itemStatDisplay, 9, Color.White));
                itemStat = getItemStat(bodyPart, "Resistance");
                if (itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "All Resistance: " + itemStat, itemStatDisplay, 9, Color.White));

                itemStatDisplay.Controls.Add(createLabelLineBreak(itemStatDisplay));

                List <Socket> Sockets = getSockets(bodyPart);
                if (Sockets != null)
                {
                    foreach (Socket socket in Sockets)
                    {
                        string s_Socket = WolcenStaticData.SocketType[socket.Effect];
                        if (socket.Gem == null) s_Socket += " [empty]";
                        else s_Socket += " " + WolcenStaticData.ItemLocalizedNames[socket.Gem.Name];
                        itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, s_Socket, itemStatDisplay, 9, ColorTranslator.FromHtml(WolcenStaticData.SocketColor[socket.Effect])));
                        if (socket.Gem != null) itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, getGemStats(socket.Gem.Name, socket.Effect), itemStatDisplay, 7, ColorTranslator.FromHtml(WolcenStaticData.SocketColor[socket.Effect])));
                    }
                }

                List<Effect> defaultEffects = getItemMagicEffect(bodyPart, "Default");
                foreach (Effect effect in defaultEffects)
                {
                    string s_Effect = WolcenStaticData.MagicLocalized[effect.EffectId].Replace("%1", effect.Parameters[0].value.ToString());
                    if (s_Effect.Contains("%2")) s_Effect = s_Effect.Replace("%2", effect.Parameters[1].value.ToString());
                    itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "+" + s_Effect, itemStatDisplay, 9, Color.White));
                }

                itemStatDisplay.Controls.Add(createLabelLineBreak(itemStatDisplay));

                List<Effect> magicEffects = getItemMagicEffect(bodyPart, "RolledAffixes");
                if (magicEffects != null)
                {
                    foreach (Effect effect in magicEffects)
                    {
                        string s_Effect = WolcenStaticData.MagicLocalized[effect.EffectId];
                        if (effect.EffectId.Contains("percent")) s_Effect = s_Effect.Replace("%1", "%1%");
                        s_Effect = s_Effect.Replace("%1", effect.Parameters[0].value.ToString());
                        if (s_Effect.Contains("%2")) s_Effect = s_Effect.Replace("%2", effect.Parameters[1].value.ToString());
                        itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "+" + s_Effect, itemStatDisplay, 9, Color.White));
                    }
                }
            }
            else   // Weapons
            {
                string itemStat = getItemStat(bodyPart, "DamageMin");
                string itemStat2 = getItemStat(bodyPart, "DamageMax");
                if (itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Material Damage: " + itemStat + "-" + itemStat2, itemStatDisplay, 9, Color.White));
                itemStat = getItemStat(bodyPart, "ResourceGeneration");
                if (itemStat != "0") itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "Resource Generation: " + itemStat, itemStatDisplay, 9, Color.White));

                itemStatDisplay.Controls.Add(createLabelLineBreak(itemStatDisplay));

                List<Socket> Sockets = getSockets(bodyPart);
                if (Sockets != null)
                {
                    foreach (Socket socket in Sockets)
                    {
                        string s_Socket = WolcenStaticData.SocketType[socket.Effect];
                        if (socket.Gem == null) s_Socket += " [empty]";
                        else s_Socket += " " + WolcenStaticData.ItemLocalizedNames[socket.Gem.Name];
                        itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, s_Socket, itemStatDisplay, 9, ColorTranslator.FromHtml(WolcenStaticData.SocketColor[socket.Effect])));
                        if(socket.Gem != null) itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, getGemStats(socket.Gem.Name, socket.Effect), itemStatDisplay, 7, ColorTranslator.FromHtml(WolcenStaticData.SocketColor[socket.Effect])));
                    }
                }

                itemStatDisplay.Controls.Add(createLabelLineBreak(itemStatDisplay));

                List<Effect> magicEffects = getItemMagicEffect(bodyPart, "RolledAffixes");
                if (magicEffects != null)
                {
                    foreach (Effect effect in magicEffects)
                    {
                        string s_Effect = WolcenStaticData.MagicLocalized[effect.EffectId].Replace("%1", effect.Parameters[0].value.ToString());
                        if (s_Effect.Contains("%2")) s_Effect = s_Effect.Replace("%2", effect.Parameters[1].value.ToString());
                        itemStatDisplay.Controls.Add(createLabel(pictureBox.Name, "+" + s_Effect, itemStatDisplay, 9, Color.White));
                    }
                }
            }
        }

        private static Image setRarityBackground(PictureBox pictureBox)
        {
            int bodyPart = charMap[pictureBox.Name];
            Image returnImage = null;
            if (pictureBox.Name == "charLRing" || pictureBox.Name == "charRRing") returnImage = WolcenEditor.Properties.Resources.charRing;
            else if (pictureBox.Name == "charLWeapon" || pictureBox.Name == "charRWeapon") returnImage = WolcenEditor.Properties.Resources.charWeapon;
            else returnImage = (Properties.Resources.ResourceManager.GetObject(pictureBox.Name) as Image);

            string dirPath = @".\UIResources\ItemBorders\";
            foreach (var equip in cData.Character.InventoryEquipped)
            {
                if (equip.BodyPart == bodyPart)
                {
                    return new Bitmap(dirPath + WolcenStaticData.itemBordersByRarity[equip.Rarity]);
                }
            }
            return returnImage;
        }

        private static void UnloadItemData(Panel itemStatDisplay)
        {
            posY = 0;
            itemStatDisplay.Controls.Clear();
        }

        private static string getGemStats(string gemName, int gemEffect)
        {
            foreach (var gem in WolcenStaticData.GemAffixesWithValues)
            {
                if (gem.Key == gemName)
                {
                    string returnStr = WolcenStaticData.MagicLocalized[gem.Value.ElementAt(gemEffect).Key];
                    if (gemName.Contains("physical_Gem") && gemEffect == 8) gemEffect = 6;
                    if (gem.Value.ElementAt(gemEffect).Key.Contains("percent")) returnStr = returnStr.Replace("%1", "%1%");
                    returnStr = "+" + returnStr.Replace("%1", gem.Value.ElementAt(gemEffect).Value);
                    if (returnStr.Contains("%2")) returnStr = returnStr.Replace("%2", gem.Value.ElementAt(gemEffect).Value);
                    return returnStr;
                }
            }
            return "[NOT FOUND]";
        }

        private static List<Socket> getSockets(int bodyPart)
        {
            foreach (var equip in cData.Character.InventoryEquipped)
            {
                if (equip.BodyPart == bodyPart)
                {
                    return (equip.GetType().GetProperty("Sockets").GetValue(equip, null) as List<Socket>);
                }
            }
            return null;
        }

        private static List<Effect> getItemMagicEffect(int bodyPart, string stat)
        {
            foreach (var equip in cData.Character.InventoryEquipped)
            {
                if (equip.BodyPart == bodyPart)
                {
                    return (equip.MagicEffects.GetType().GetProperty(stat).GetValue(equip.MagicEffects, null) as List<Effect>);
                }
            }
            return null;
        }

        private static string getItemStat(int bodyPart, string stat)
        {
            foreach (var equip in cData.Character.InventoryEquipped)
            {
                if (equip.BodyPart == bodyPart)
                {
                    string itemStat = "";
                    if (bodyPart == 16 || bodyPart == 15)
                        itemStat = equip.Weapon.GetType().GetProperty(stat).GetValue(equip.Weapon, null).ToString();
                    else if (bodyPart == 14 || bodyPart == 19 || bodyPart == 21 || bodyPart == 22)
                        itemStat = equip.Armor.GetType().GetProperty(stat).GetValue(equip.Armor, null).ToString();
                    else
                        itemStat = equip.Armor.GetType().GetProperty(stat).GetValue(equip.Armor, null).ToString();

                    return itemStat;
                }
            }
            return "Item stat not found!";
        }

        private static Label createLabelLineBreak(Panel panel)
        {
            Label lb = new Label();
            lb.Name = "s_lbl_LineBreak";
            lb.Text = "__________________________________________________";
            lb.Font = new Font(Form1.DefaultFont.FontFamily, 5, FontStyle.Regular);
            lb.ForeColor = Color.LightGray;
            lb.TextAlign = ContentAlignment.MiddleCenter;
            lb.Size = new Size(panel.Width - 20, 5 + 5);
            lb.Location = new Point(0, posY - 5);
            posY += 5;
            return lb;
        }

        private static Label createLabel(string name, string text, Panel panel, int fontSize, Color fontColor)
        {
            Label lb = new Label();
            lb.Name = "s_lbl" + name;
            lb.Text = text;
            lb.Font = new Font(Form1.DefaultFont.FontFamily, fontSize, FontStyle.Regular);
            lb.ForeColor = fontColor;
            lb.TextAlign = ContentAlignment.MiddleCenter;
            lb.Size = new Size(panel.Width - 20, fontSize + 10);
            lb.Location = new Point(0, posY);
            posY += fontSize + 10;
            return lb;
        }

        private static string getItemName_Color(int bodyPart)
        {
            foreach (var equip in cData.Character.InventoryEquipped)
            {
                if (equip.BodyPart == bodyPart)
                {
                    string itemName = "";
                    if (bodyPart == 16 || bodyPart == 15)
                        itemName = equip.Weapon.Name;
                    else if (bodyPart == 14 || bodyPart == 19 || bodyPart == 21 || bodyPart == 22)
                        itemName = equip.Armor.Name;
                    else
                        itemName = equip.Armor.Name;

                    itemName += WolcenStaticData.qualityColorBank[equip.Rarity];

                    return itemName;
                }
            }
            return "Item not found!";
        }

        private static Bitmap GetItemBitmap(int bodyPart, bool flip = false)
        {
            foreach (var i in cData.Character.InventoryEquipped)
            {
                string dirPath = @".\UIResources\Items\";
                if (i.BodyPart == bodyPart)
                {
                    string itemName = "";
                    int itemRarity = i.Rarity;
                    if (bodyPart == 16 || bodyPart == 15)
                        itemName = WolcenStaticData.ItemWeapon[i.Weapon.Name];
                    else if (bodyPart == 14 || bodyPart == 19 || bodyPart == 21 || bodyPart == 22)
                        itemName = WolcenStaticData.ItemAccessories[i.Armor.Name];
                    else
                        itemName = WolcenStaticData.ItemArmor[i.Armor.Name];

                    if (File.Exists(dirPath + itemName))
                    {
                        if (flip == true)
                        {
                            Bitmap bmp = ResizeAndCombine(dirPath, itemName, itemRarity);
                            bmp.RotateFlip(RotateFlipType.Rotate180FlipY);
                            return bmp;
                        }
                        else
                        {
                            return ResizeAndCombine(dirPath, itemName, itemRarity);
                        }
                    }
                    else
                    {
                        if (i.BodyPart == 15 || i.BodyPart == 16)
                        {
                            return new Bitmap(Image.FromFile(dirPath + "unknown_weapon.png"));
                        }
                        else
                        {
                            return new Bitmap(Image.FromFile(dirPath + "unknown_armor.png"));
                        }
                    }
                }
            }
            return null;
        }

        private static Bitmap ResizeAndCombine(string dirPath, string itemName, int quality)
        {
            List<Bitmap> images = new List<Bitmap>();
            var underImage = new Bitmap(Image.FromFile(@".\UIResources\ItemBorders\" + quality + ".png"));

            var finalImage = new Bitmap(underImage.Width, underImage.Height);

            var itemImage = new Bitmap(Image.FromFile(dirPath + itemName));

            Bitmap resized = new Bitmap(itemImage, new Size(underImage.Width, underImage.Height));

            images.Add(underImage);
            images.Add(resized);

            using (Graphics g = Graphics.FromImage(finalImage))
            {
                g.Clear(Color.Black);
                //go through each image and draw it on the final image (Notice the offset; since I want to overlay the images i won't have any offset between the images in the finalImage)
                int offset = 0;
                foreach (Bitmap image in images)
                {
                    g.DrawImage(image, new Rectangle(offset, 0, image.Width, image.Height));
                }
            }
            return finalImage;
        }
    }
}
