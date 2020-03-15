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
using static Logic.Constant.MapInfo;

namespace GameForm
{
    public partial class Form1 : Form
    {
        public static readonly int LABEL_WIDTH = 14;
        public static readonly int LABEL_INTERVAL = 2;
        public static readonly int HALF_LABEL_INTERVAL = Convert.ToInt32(0.5 * Convert.ToDouble(LABEL_INTERVAL));
        public static readonly int FORM_WIDTH = WorldMap.Width * LABEL_WIDTH;
        public static readonly int FORM_HEIGHT = WorldMap.Height * LABEL_WIDTH;
        public static readonly int CONTROL_LABELS_WIDTH = 200;
        public static readonly int CONTROL_LABELS_HEIGHT = 120;
        private readonly ushort agentPort;

        public Form1(ushort agentPort)
        {
            this.agentPort = agentPort;
            InitializeComponent();
            Console.WriteLine("Start console output");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            for (int x = 0; x < WorldMap.Width; x++)
                for (int y = 0; y < WorldMap.Height; y++)
                {
                    if (this.mapLabels[x, y] == null)
                        continue;
                    this.mapLabels[x, y].Location = new System.Drawing.Point(x * LABEL_WIDTH + HALF_LABEL_INTERVAL, (WorldMap.Height - 1 - y) * LABEL_WIDTH + HALF_LABEL_INTERVAL);
                    this.mapLabels[x, y].Size = new System.Drawing.Size(LABEL_WIDTH - LABEL_INTERVAL, LABEL_WIDTH - LABEL_INTERVAL);
                }

            int i = 0;
            foreach (var label in ControlLabels)
            {
                label.Value.Location = new System.Drawing.Point(FORM_WIDTH, i * CONTROL_LABELS_HEIGHT);
                label.Value.Size = new System.Drawing.Size(CONTROL_LABELS_WIDTH, CONTROL_LABELS_HEIGHT);
                i++;
            }
            ControlLabels["Task"].Size = new System.Drawing.Size(CONTROL_LABELS_WIDTH, 3 * CONTROL_LABELS_HEIGHT);

            this.ClientSize = new System.Drawing.Size(FORM_WIDTH + CONTROL_LABELS_WIDTH, FORM_HEIGHT);

            new Player(15, 12, this.agentPort);
        }
    }
}
