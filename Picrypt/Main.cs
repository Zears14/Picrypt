﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text;

namespace Picrypt
{
    public partial class Main : Form
    {

        public Main()
        {
            InitializeComponent();

        }



        private bool dragging = false;
        private Point dcp;
        private Point dfp;


        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void label1_Click(object sender, EventArgs e)
        {
        }
        private void button4_Click(object sender, EventArgs e)
        {
            LOGIC_MAIN.EncryptFile();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            LOGIC_MAIN.DecryptFile();
        }

        private void label2_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dcp = Cursor.Position;
            dfp = this.Location;
        }

        private void label2_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point diff = Point.Subtract(Cursor.Position, new Size(dcp));
                this.Location = Point.Add(dfp, new Size(diff));
            }
        }

        private void label2_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }
    }
}