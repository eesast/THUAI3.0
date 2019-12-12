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

namespace GameForm
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

            for (int a = 0; a < Communication.Proto.Constants.AgentCount; a++)
                for (int c = 0; c < Communication.Proto.Constants.PlayerCount; c++)
                {
                    this.playerLabels[new Tuple<int, int>(a, c)].Location = new System.Drawing.Point(-10, -10);
                    this.playerLabels[new Tuple<int, int>(a, c)].Size = new System.Drawing.Size(LABEL_WIDTH, LABEL_WIDTH);
                }
            this.ClientSize = new System.Drawing.Size(FORM_WIDTH, FORM_HEIGHT);

            Player player = new Player(15, 12);
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

    }
}
