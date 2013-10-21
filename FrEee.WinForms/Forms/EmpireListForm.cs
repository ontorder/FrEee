﻿using FrEee.Game.Objects.Civilization;
using FrEee.Game.Objects.Space;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FrEee.Utility.Extensions;
using FrEee.WinForms.Utility.Extensions;
using FrEee.Utility;

namespace FrEee.WinForms.Forms
{
	public partial class EmpireListForm : Form
	{
		public EmpireListForm()
		{
			InitializeComponent();
			try { base.Icon = new Icon(FrEee.WinForms.Properties.Resources.FrEeeIcon); }
			catch { }
			BindEmpires();
		}

		private void BindEmpires()
		{
			lstEmpires.Initialize(128, 128);
			foreach (var emp in Galaxy.Current.Empires)
			{
				var item = lstEmpires.AddItemWithImage(null, emp.Name, emp, emp.Portrait);
				if (emp == Empire.Current)
					item.Selected = true;
			}
		}

		private Empire empire;

		private void BindEmpire(Empire emp)
		{
			empire = emp;
			if (emp == Empire.Current && tabs.SelectedTab == tabDiplomacy)
				tabs.SelectedTab = tabBudget;
			else if (emp != Empire.Current && tabs.SelectedTab == tabBudget)
				tabs.SelectedTab = tabDiplomacy;

			report.Empire = emp;

			if (emp == null)
			{
				txtTreaty.Text = "N/A";
				txtTreaty.ForeColor = Color.White;
			}
			else
			{
				if (emp == Empire.Current)
				{
					txtTreaty.Text = "Self";
					txtTreaty.ForeColor = Color.Green;
				}
				else
				{
					// TODO - diplomacy
					txtTreaty.Text = "None";
					txtTreaty.ForeColor = Color.Yellow;
				}

				if (emp == Empire.Current)
					rqdConstruction.ResourceQuantity = emp.ConstructionQueues.Sum(rq => rq.UpcomingSpending);
				else
					// assume other empires' construction queues are running at full capacity
					rqdConstruction.ResourceQuantity = emp.ConstructionQueues.Sum(rq => rq.Rate);
				rqdExtraction.ResourceQuantity = emp.ColonizedPlanets.Sum(p => p.Income); // TODO - remote mining
				rqdIncome.ResourceQuantity = emp.GrossIncome;
				rqdMaintenance.ResourceQuantity = emp.Maintenance;
				rqdNet.ResourceQuantity = emp.NetIncome;
				rqdSpoiled.ResourceQuantity = ResourceQuantity.Max(new ResourceQuantity(), emp.StoredResources + emp.NetIncome - emp.ResourceStorage);
				rqdStored.ResourceQuantity = emp.StoredResources;
				rqdTrade.ResourceQuantity = new ResourceQuantity(); // TODO - trade
				rqdTributesIn.ResourceQuantity = new ResourceQuantity(); // TODO - tributes
				rqdTributesOut.ResourceQuantity = new ResourceQuantity(); // TODO - tributes
				rqExpenses.ResourceQuantity = rqdConstruction.ResourceQuantity + rqdMaintenance.ResourceQuantity + rqdTributesOut.ResourceQuantity;

				lblBudgetWarning.Visible = emp != Empire.Current;
			}
		}

		private void lstEmpires_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
		{
			var item = e.Item;
			if (e.IsSelected)
				BindEmpire((Empire)e.Item.Tag);
			else
				BindEmpire(null);
		}

		private void btnClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void btnCompose_Click(object sender, EventArgs e)
		{
			this.ShowChildForm(new DiplomacyForm(empire));
		}

		private void btnReply_Click(object sender, EventArgs e)
		{
			// TODO - reply to message
		}

		private void btnRescind_Click(object sender, EventArgs e)
		{
			// TODO - rescind message
		}
	}
}