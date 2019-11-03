using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Logic.Constant;
using Client;
using static Map;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        public static readonly int LABEL_WIDTH = 15;
        public static readonly int LABEL_INTERVAL = 2;
        public static readonly int HALF_LABEL_INTERVAL = Convert.ToInt32(0.5 * Convert.ToDouble(LABEL_INTERVAL));
        public static readonly int FORM_WIDTH = WORLD_MAP_WIDTH * LABEL_WIDTH;
        public static readonly int FORM_HEIGHT = WORLD_MAP_HEIGHT * LABEL_WIDTH;

        public Form1()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            Console.WriteLine("Start console output");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            for (int x = 0; x < WORLD_MAP_WIDTH; x++)
                for (int y = 0; y < WORLD_MAP_HEIGHT; y++)
                {
                    this.mapLabels[x, y].Location = new System.Drawing.Point(x * LABEL_WIDTH + HALF_LABEL_INTERVAL, (WORLD_MAP_HEIGHT - 1 - y) * LABEL_WIDTH + HALF_LABEL_INTERVAL);
                    this.mapLabels[x, y].Size = new System.Drawing.Size(LABEL_WIDTH - LABEL_INTERVAL, LABEL_WIDTH - LABEL_INTERVAL);
                }

            this.playerLabel.Location = new System.Drawing.Point(15 * LABEL_WIDTH, 12 * LABEL_WIDTH);
            this.playerLabel.Size = new System.Drawing.Size(LABEL_WIDTH - LABEL_INTERVAL, LABEL_WIDTH - LABEL_INTERVAL);

            this.ClientSize = new System.Drawing.Size(FORM_WIDTH, FORM_HEIGHT);

            Player player = new Player(15, 12);
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        public void moveLabel(XY_Position xy)
        {
            playerLabel.Location = new System.Drawing.Point(Convert.ToInt32((xy.x - 0.5) * Convert.ToDouble(LABEL_WIDTH)), Convert.ToInt32((Convert.ToDouble(WORLD_MAP_HEIGHT) - xy.y - 0.5) * Convert.ToDouble(LABEL_WIDTH)));
        }
    }
}
