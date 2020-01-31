using static Logic.Constant.Map;
using THUnity2D;
using System;
using System.Collections.Generic;

namespace GameForm
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mapLabels = new System.Windows.Forms.Label[WorldMap.Width, WorldMap.Height];
            this.playerLabels = new Dictionary<Int64, System.Windows.Forms.Label>();
            this.SuspendLayout();
            //
            //mapLabels
            //
            for (int x = 0; x < WorldMap.Width; x++)
                for (int y = 0; y < WorldMap.Height; y++)
                {
                    if (map[x, y] == 0)
                        continue;

                    this.mapLabels[x, y] = new System.Windows.Forms.Label();
                    switch (map[x, y])
                    {
                        case 0:
                            break;
                        case 1:
                            this.mapLabels[x, y].BackColor = System.Drawing.Color.White;
                            break;
                        case 2:
                            this.mapLabels[x, y].BackColor = System.Drawing.Color.White;
                            break;
                        case 3:
                            this.mapLabels[x, y].BackColor = System.Drawing.Color.Green;
                            break;
                        case 4:
                            this.mapLabels[x, y].BackColor = System.Drawing.Color.Green;
                            break;
                        case 5:
                            this.mapLabels[x, y].BackColor = System.Drawing.Color.Blue;
                            break;
                        case 6:
                            this.mapLabels[x, y].BackColor = System.Drawing.Color.Purple;
                            break;
                        default:
                            this.mapLabels[x, y].BackColor = System.Drawing.Color.Black;
                            break;
                    }

                    this.mapLabels[x, y].Location = new System.Drawing.Point(x * LABEL_WIDTH + HALF_LABEL_INTERVAL, (WorldMap.Height - 1 - y) * LABEL_WIDTH + HALF_LABEL_INTERVAL);
                    this.mapLabels[x, y].Name = "Label" + x.ToString() + "," + y.ToString();
                    this.mapLabels[x, y].Size = new System.Drawing.Size(LABEL_WIDTH - LABEL_INTERVAL, LABEL_WIDTH - LABEL_INTERVAL);

                    this.mapLabels[x, y].TabIndex = 1;
                    this.Controls.Add(this.mapLabels[x, y]);
                }
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(FORM_WIDTH, FORM_HEIGHT);
            //this.Controls.Add(this.label2);
            //this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Label[,] mapLabels;
        public Dictionary<Int64, System.Windows.Forms.Label> playerLabels;
    }
}

