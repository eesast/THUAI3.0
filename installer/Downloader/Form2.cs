using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Downloader
{
    public partial class Form2 : Form
    {
        static List<string> SelectedNewFile = new List<string>();
        static List<string> SelectedUpdateFile = new List<string>();
        static bool warning = false;
        public Form2()
        {
            InitializeComponent();
        }
        private void cycleChild(TreeNode n, bool check)
        {
            if (n.Nodes.Count != 0)
                foreach (TreeNode child in n.Nodes)
                {
                    child.Checked = check;
                    child.ForeColor = Color.Black;
                    if (child.Nodes.Count != 0)
                        cycleChild(child, check);
                }
            
        }
        private bool nextCheck(TreeNode n, bool check)
        {
            foreach (TreeNode node in n.Parent.Nodes)
                if (node.Checked != check || (!check && node.ForeColor == Color.Blue))
                    return false;
            return true;
        }

        private void SelectAll(TreeNodeCollection Nodes)
        {
            foreach (TreeNode n in Nodes)
            {
                if (n.Text != "player.cpp")
                    n.Checked = true;
                if (n.Nodes != null)
                    SelectAll(n.Nodes);
            }
        }

        private void cycleParent(TreeNode n, bool check)
        {
            if (n.Parent != null)
            {
                if (nextCheck(n, check))
                {
                    n.Parent.Checked = check;
                    n.Parent.ForeColor = Color.Black;
                }
                else
                {
                    n.Parent.Checked = false;
                    n.Parent.ForeColor = Color.Blue;
                }
                cycleParent(n.Parent, check);
            }
        }

        private void TreeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            var treeview = sender as TreeView;
            var brush = Brushes.Black;
            if (e.Node.ForeColor == Color.Blue)
            {
                var location = e.Node.Bounds.Location;
                location.Offset(-11, 2);
                var size = new Size(9, 9);
                brush = Brushes.Black;
                e.Graphics.FillRectangle(brush, new Rectangle(location, size));
            }
            e.Graphics.DrawString(e.Node.Text, treeview.Font, brush, e.Bounds.Left, e.Bounds.Top);
        }

            private void getFileName(TreeNode n, string current, bool isNewFile) //添加半选状态的判断
        {
            current += n.Text;
            if (n.Nodes.Count == 0 && n.Checked)
            {
                if (isNewFile) SelectedNewFile.Add(current);
                else SelectedUpdateFile.Add(current);
            }
            else
            {
                current += '/';
                foreach (TreeNode node in n.Nodes)
                    getFileName(node, current, isNewFile);
            }
        }

        public Form2(List<string> newFileName, List<string> updateFileName)
        {
            InitializeComponent();
            ImageList myImageList = new ImageList();
            myImageList.Images.Add(Image.FromFile(".\\folder.jpg"));
            myImageList.Images.Add(Image.FromFile(".\\file.jpg"));
            this.treeView1.ImageList = myImageList;
            this.treeView2.ImageList = myImageList;
            //this.treeView1.DrawMode = TreeViewDrawMode.OwnerDrawText;
            //this.treeView2.DrawMode = TreeViewDrawMode.OwnerDrawText;
            //this.treeView1.DrawMode+= new DrawTreeNodeEventHandler(TreeView_DrawNode);
            List<string[]> newFilePath = new List<string[]>();
            List<string[]> updateFilePath = new List<string[]>();
            if (newFileName.Count() > 0)
            {
                foreach (string name in newFileName)
                    newFilePath.Add(name.Split('/'));
                TreeNodeCollection Nodes = this.treeView1.Nodes;
                foreach (string[] path in newFilePath)
                {
                    Nodes = this.treeView1.Nodes;
                    foreach (string layer in path)
                    {
                        if (Nodes.Count == 0 || Nodes.Find(layer, false).Length == 0)
                            if(layer==path[path.Length - 1])
                                Nodes = Nodes.Add(layer, layer, 1).Nodes;
                            else Nodes = Nodes.Add(layer, layer, 0).Nodes;
                        else
                            Nodes = Nodes.Find(layer, false)[0].Nodes;
                    }
                }
                SelectAll(treeView2.Nodes);

            }
            if (updateFileName.Count() > 0)
            {
                foreach (string name in updateFileName)
                    updateFilePath.Add(name.Split('/'));
                TreeNodeCollection Nodes = this.treeView2.Nodes;
                foreach (string[] path in updateFilePath)
                {
                    Nodes = this.treeView2.Nodes;
                    foreach (string layer in path)
                    {
                        if (Nodes.Count == 0 || Nodes.Find(layer, false).Length == 0)
                            if (layer == path[path.Length - 1])
                                Nodes = Nodes.Add(layer, layer, 1).Nodes;
                            else Nodes = Nodes.Add(layer, layer, 0).Nodes;
                        else
                            Nodes = Nodes.Find(layer, false)[0].Nodes;
                    }
                }
                SelectAll(treeView2.Nodes);
            }


        }
        private void AfterCheck(object sender, TreeViewEventArgs e) //添加半选状态的判定
        {
            if (e.Node.Text == "player.cpp" && e.Node.Checked && this.treeView2.Nodes.Find("player.cpp", true).Length != 0)
            {
                this.label3.Text = "警告：player.cpp将被更新";
                warning = true;
            }
            else if (e.Node.Text == "player.cpp" && !e.Node.Checked)
            {
                this.label3.Text = "";
                warning = false;
            }
            if (e.Action != TreeViewAction.Unknown)
            {
                e.Node.ForeColor = Color.Black;
                cycleChild(e.Node, e.Node.Checked);
                cycleParent(e.Node, e.Node.Checked);
            }
        }
        private void treeView1_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if ((e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.Red), e.Bounds);
                e.Graphics.DrawString(e.Node.Text, treeView1.Font, new SolidBrush(Color.White), e.Bounds.Location);
            }
            else
            {
                e.DrawDefault = true;
            }
        }

            private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
           

        }

        private void treeView2_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string current = "";
            if (warning)
            {
                MessageBoxButtons mes = MessageBoxButtons.OKCancel;
                DialogResult dialogResult = MessageBox.Show("确认更新player.cpp?", "警告", mes);
                if (dialogResult != DialogResult.OK) return;
            }
            foreach (TreeNode node in this.treeView1.Nodes)
                getFileName(node, current, true);
            foreach (TreeNode node in this.treeView2.Nodes)
                getFileName(node, current, false);
            List<string>[] FileNames = { SelectedNewFile, SelectedUpdateFile };
            this.Tag = FileNames;
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            warning = false;
            this.DialogResult = DialogResult.No;
            this.Close();
        }

       
    }
}
