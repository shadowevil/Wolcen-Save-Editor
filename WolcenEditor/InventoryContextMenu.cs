using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WolcenEditor
{
    public static class InventoryContextMenu
    {
        private static PictureBox accessablePictureBox;
        private static InventoryGrid iGridItem;
        private static InventoryGrid iGridCopiedItem;
        private struct coords
        {
            public static int x;
            public static int y;
            public static int panelID = -1;
        };

        public static ContextMenu LoadContextMenu(PictureBox pb)
        {
            ContextMenu _return = new ContextMenu();
            if (pb.Image == null)
            {
                MenuItem createItem = new MenuItem();
                createItem.Text = "Create item";
                createItem.Name = "CreateItem";
                createItem.Click += CreateItem_Click;

                MenuItem pasteItem = new MenuItem()
                {
                    Text = "Paste",
                    Name = "PasteItem"
                };
                pasteItem.Click += PasteItem_Click;

                _return.MenuItems.Add(createItem);
                _return.MenuItems.Add(pasteItem);
            }
            else
            {
                MenuItem editItem = new MenuItem()
                {
                    Text = "Edit item",
                    Name = "EditItem"
                };

                MenuItem copyItem = new MenuItem()
                {
                    Text = "Copy",
                    Name = "CopyItem"
                };

                MenuItem deleteItem = new MenuItem()
                {
                    Text = "Delete item",
                    Name = "DeleteItem"
                };

                deleteItem.Click += DeleteItem_Click;
                copyItem.Click += CopyItem_Click;
                editItem.Click += EditItem_Click;
                _return.MenuItems.Add(editItem);
                _return.MenuItems.Add(copyItem);
                _return.MenuItems.Add(deleteItem);
            }

            return _return;
        }

        public static void ShowContextMenu(PictureBox pb, Point location, object type, string select)
        {
            accessablePictureBox = pb;
            coords.x = Convert.ToInt32(pb.Name.Split('|')[0]);
            coords.y = Convert.ToInt32(pb.Name.Split('|')[1]);
            if (pb.Name.Split('|').Length == 3)
            {
                coords.panelID = Convert.ToInt32(pb.Name.Split('|')[2]);
                type = (type as IList)[coords.panelID];
            }
            setGridItem(type, select);
            LoadContextMenu(pb).Show(pb, location);
        }

        private static void setGridItem(object type, string select)
        {
            foreach (var iGrid in (GetPropertyValue(type, select) as IList<InventoryGrid>))
            {
                if (iGrid.InventoryX == coords.x && iGrid.InventoryY == coords.y)
                {
                    iGridItem = iGrid.copy;
                    return;
                }
            }
            iGridItem = null;
        }

        public static object GetPropertyValue(object obj, string propertyName)
        {
            if (obj.GetType() == null) return null;
            var objType = obj.GetType();
            var prop = objType.GetProperty(propertyName);

            return prop.GetValue(obj, null);
        }

        private static void EditItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void CopyItem_Click(object sender, EventArgs e)
        {
            iGridCopiedItem = iGridItem;
        }

        private static void DeleteItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void PasteItem_Click(object sender, EventArgs e)
        {
            if (iGridItem == null)
            {
                InventoryGrid item = iGridCopiedItem;
                item.InventoryX = coords.x;
                item.InventoryY = coords.y;

                if (coords.panelID == -1)
                {
                    cData.Character.InventoryGrid.Add(item);
                    InventoryManager.ReloadInventoryBitmap((accessablePictureBox.Parent as Panel), coords.x, coords.y);
                }
                else
                {
                    cData.PlayerChest.Panels[coords.panelID].InventoryGrid.Add(item);
                    StashManager.ReloadGridBitmap((accessablePictureBox.Parent as Panel), coords.x, coords.y, coords.panelID);
                }
            }
        }

        private static void CreateItem_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
