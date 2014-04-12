﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FrEee.WinForms.MogreCombatRender.StrategiesDesigner
{
    public partial class UserControlBaseObj : UserControl
    {
        bool leftmousegrab = false;
        StratMainForm parentForm;
        Point dragOffset;
        Point offset = new Point(10, 10);
        Canvasdata canvasdata;
        Point loc = new Point(0, 0);

        public UserControlBaseObj()
        {
            InitializeComponent();
        }

        public UserControlBaseObj(string name, StratMainForm parentForm, Canvasdata canvasdata) : base()
        {
            InitializeComponent();
            this.Name = name;
            this.lbl_FunctName.Text = name;

            this.parentForm = parentForm;
            this.canvasdata = canvasdata;           
        }

        public string name
        { get { return this.Name; } }

        protected TableLayoutPanel TableLayoutPanel1
        {
            get { return tableLayoutPanel1; }
            set { tableLayoutPanel1 = value; }
        }

        protected void UserControl_System_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                leftmousegrab = true;
                dragOffset = this.PointToScreen(e.Location);
                var formLocation = this.Location;
                dragOffset.X -= formLocation.X += offset.X;
                dragOffset.Y -= formLocation.Y += offset.Y;
            }
        }

        protected void UserControl_System_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftmousegrab)
            {
                Point newloc = this.PointToScreen(e.Location);
                newloc = canvasdata.sub(newloc, dragOffset);
                this.loc = canvasdata.reversecanvasLocation(newloc);
                location();
                parentForm.refreshlines();
                parentForm.refresh();
            }
        }

        protected void UserControl_System_MouseUp(object sender, MouseEventArgs e)
        {
            leftmousegrab = false;
            parentForm.refreshlines();
            parentForm.refresh();
        }

        protected void location()
        {
            this.Location = canvasdata.sub(canvasdata.canvasLocation(this.loc), offset);
        }
        public void location(Canvasdata canvasdata)
        {
            this.canvasdata = canvasdata;
            location();
        }
    }
}