﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FrEee.Game.Objects.Combat2;

namespace FrEee.WinForms.MogreCombatRender.StrategiesDesigner
{
    public partial class funclist : Form
    {

        Dictionary<string, UserControlBaseObj> functItems = new Dictionary<string, UserControlBaseObj>();

        public UserControlBaseObj ReturnCtrlObj { get; set; }

        public funclist(StratMainForm parent, Canvasdata canvasdata)
        {
            InitializeComponent();
            /*
            UCFunct_Loc functloc = new UCFunct_Loc(parent, canvasdata);
            UCFunct_Vel functvel = new UCFunct_Vel(parent, canvasdata);
            UCData_This datathis = new UCData_This(parent, canvasdata);
            UCData_Clock dataclk = new UCData_Clock(parent, canvasdata);
            UCData_NearestEnemy nearenme = new UCData_NearestEnemy(parent, canvasdata);
            UCData_EnemyShips nmeshps = new UCData_EnemyShips(parent, canvasdata);
            UCData_FrendlyShips frndshps = new UCData_FrendlyShips(parent, canvasdata);
            UCData_NearestComObj nearObj = new UCData_NearestComObj(parent, canvasdata);
            functItems.Add(nearObj.name, nearObj);
            functItems.Add(functloc.name, functloc);
            functItems.Add(functvel.name, functvel);
            functItems.Add(datathis.name, datathis);
            functItems.Add(dataclk.name, dataclk);
            functItems.Add(nearenme.name, nearenme);
            functItems.Add(nmeshps.name, nmeshps);
            functItems.Add(frndshps.name, frndshps);
            */

            UCStratBlock ThisObj = new UCStratBlock(new StrategyThisObj(), parent, canvasdata);
            UCStratBlock Closesed = new UCStratBlock(new StrategyClosest(null, null), parent, canvasdata);
            UCStratBlock thisobjEnemys = new UCStratBlock(new StrategyThisEnemys(), parent, canvasdata);
            UCStratBlock loc = new UCStratBlock(new StrategyLocdata(), parent, canvasdata);
            UCStratBlock vel = new UCStratBlock(new StrategyVeldata(), parent, canvasdata);

            functItems.Add(ThisObj.name, ThisObj);
            functItems.Add(Closesed.name, Closesed);
            functItems.Add(thisobjEnemys.name, thisobjEnemys);
            functItems.Add(loc.name, loc);
            functItems.Add(vel.name, vel);


            this.listBox1.DataSource = functItems.Keys.ToList();
            
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //ReturnCtrlObj = functItems[listBox1.SelectedItem.ToString()];
            //this.Close();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            ReturnCtrlObj = functItems[listBox1.SelectedItem.ToString()];
            this.Close();
        }
    }

    
}
