using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Downloader
{
    public partial class 文件列表 : Form
    {
        static List<string> SelectedNewFile = new List<string>();
        static List<string> SelectedUpdateFile = new List<string>();
        static bool warning0 = false;
        static bool warning1 = false;
        static bool warning2 = false;
        public 文件列表()
        {
            InitializeComponent();
        }
        public class NodeSorter : IComparer
        {
            public int Compare(object x, object y)
            {
                TreeNode tx = x as TreeNode;
                TreeNode ty = y as TreeNode;

                // Compare the length of the strings, returning the difference.
                if (tx.Nodes.Count != ty.Nodes.Count)
                {
                    if (tx.Nodes.Count == 0) return 1;
                    else if (ty.Nodes.Count == 0) return -1;
                }
                // If they are the same length, call Compare.
                return string.Compare(tx.Text, ty.Text);
            }
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

        private void SelectAll(TreeNodeCollection Nodes, bool flag)
        {
            foreach (TreeNode n in Nodes)
            {
                if (n.Text != "player.cpp" && n.Text != "server.playback" && n.Text != "ClientConfig.json")
                    n.Checked = true;
                if (n.Text == "player.cpp" || n.Text == "server.playback" || n.Text == "ClientConfig.json")
                    if (!flag)
                    {
                        TreeNode node = n;
                        while (node.Parent != null)
                        {
                            node.Parent.Checked = false;
                            node.Parent.ForeColor = Color.Blue;
                            node = node.Parent;
                        }
                    }
                    else n.Checked = true;
                if (n.Nodes.Count != 0)
                    SelectAll(n.Nodes, flag);
            }
        }

        private void DeSelectAll(TreeNodeCollection Nodes)
        {
            foreach (TreeNode n in Nodes)
            {
                n.Checked = false;
                n.ForeColor = Color.Black;
                if (n.Nodes.Count != 0)
                    DeSelectAll(n.Nodes);
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

        public 文件列表(List<string> newFileName, List<string> updateFileName)
        {
            InitializeComponent();
            ImageList myImageList = new ImageList();
            myImageList.Images.Add(Properties.Resources.folder);
            myImageList.Images.Add(Properties.Resources.file);
            this.treeView1.ImageList = myImageList;
            this.treeView2.ImageList = myImageList;
            this.treeView1.TreeViewNodeSorter = new NodeSorter();
            this.treeView2.TreeViewNodeSorter = new NodeSorter();
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
                            if (layer == path[path.Length - 1])
                                Nodes = Nodes.Add(layer, layer, 1, 1).Nodes;
                            else Nodes = Nodes.Add(layer, layer, 0, 0).Nodes;
                        else
                            Nodes = Nodes.Find(layer, false)[0].Nodes;
                    }
                }
                SelectAll(treeView1.Nodes, true);

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
                SelectAll(treeView2.Nodes, false);
            }


        }
        private void AfterCheck(object sender, TreeViewEventArgs e) //添加半选状态的判定
        {
            if (e.Node.Text == "player.cpp" && e.Node.Checked && this.treeView2.Nodes.Find("player.cpp", true).Length != 0)
            {
                this.label3.Text = "警告：player.cpp将被更新"; warning0 = true;
            }
            else if (e.Node.Text == "player.cpp" && !e.Node.Checked)
            {
                this.label3.Text = ""; warning0 = false;
            }
            if (e.Node.Text == "server.playback" && e.Node.Checked && this.treeView2.Nodes.Find("server.playback", true).Length != 0)
            {
                this.label3.Text = "警告：回放文件server.playback等将被更新"; warning1 = true;
            }
            else if (e.Node.Text == "server.playback" && !e.Node.Checked)
            {
                this.label3.Text = ""; warning1 = false;
            }
            if (e.Node.Text == "ClientConfig.json" && e.Node.Checked && this.treeView2.Nodes.Find("ClientConfig.json", true).Length != 0) warning2 = true;
            else if (e.Node.Text == "ClientConfig.json" && !e.Node.Checked) warning2 = false;
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
            if (warning0)
            {
                MessageBoxButtons mes = MessageBoxButtons.OKCancel;
                DialogResult dialogResult = MessageBox.Show("确认更新player.cpp?", "警告", mes);
                if (dialogResult != DialogResult.OK) return;
                else warning0 = false;
            }
            if (warning1)
            {
                MessageBoxButtons mes = MessageBoxButtons.OKCancel;
                DialogResult dialogResult = MessageBox.Show("确认更新server.playback?", "警告", mes);
                if (dialogResult != DialogResult.OK) return;
            }
            if (warning2)
            {
                MessageBoxButtons mes = MessageBoxButtons.OKCancel;
                DialogResult dialogResult = MessageBox.Show("确认更新ClientConfig.json?", "警告", mes);
                if (dialogResult != DialogResult.OK) return;
                else warning2 = warning1 = false;
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
            warning0 = false;warning1 = false;
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        private void label4_Click(object sender, EventArgs e)
        {
            SelectAll(this.treeView1.Nodes, true);
        }

        private void label5_Click(object sender, EventArgs e)
        {
            SelectAll(this.treeView2.Nodes, false);
        }

        private void label6_Click(object sender, EventArgs e)
        {
            DeSelectAll(this.treeView2.Nodes);
        }

        private void label7_Click(object sender, EventArgs e)
        {
            DeSelectAll(this.treeView1.Nodes);
        }
    }
}
