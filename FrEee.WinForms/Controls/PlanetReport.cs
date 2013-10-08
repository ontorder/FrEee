using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FrEee.Game.Objects.Space;
using FrEee.Utility.Extensions;
using FrEee.Utility;
using FrEee.WinForms.Utility.Extensions;
using FrEee.WinForms.Interfaces;
using FrEee.Game.Objects.Technology;

namespace FrEee.WinForms.Controls
{
	public partial class PlanetReport : UserControl, IBindable<Planet>
	{
		public PlanetReport()
		{
			InitializeComponent();
		}

		public PlanetReport(Planet planet)
			: this()
		{
			Planet = planet;
		}

		private Planet planet;

		/// <summary>
		/// The planet for which to display a report.
		/// </summary>
		public Planet Planet
		{
			get { return planet; }
			set
			{
				planet = value;
				Bind();
			}
		}

		public void Bind()
		{
			if (Planet == null)
				Visible = false;
			else
			{
				Visible = true;

				bool showColonyInfo = planet.Colony != null;
				pnlColony.Visible = showColonyInfo;
				if (showColonyInfo)
				{
					if (!gameTabControl1.TabPages.Contains(pageFacil))
						gameTabControl1.TabPages.Insert(1, pageFacil);
					if (!gameTabControl1.TabPages.Contains(pageCargo))
						gameTabControl1.TabPages.Insert(2, pageCargo);
					if (!gameTabControl1.TabPages.Contains(pageOrders))
						gameTabControl1.TabPages.Insert(3, pageOrders);
				}
				else
				{
					gameTabControl1.TabPages.Remove(pageFacil);
					gameTabControl1.TabPages.Remove(pageCargo);
					gameTabControl1.TabPages.Remove(pageOrders);
				}


				picOwnerFlag.Image = Planet.Owner == null ? null : Planet.Owner.Icon;
				picPortrait.Image = Planet.Portrait;

				txtName.Text = Planet.Name;
				txtSizeSurface.Text = Planet.Size + " " + Planet.Surface + " Planet";
				txtAtmosphere.Text = Planet.Atmosphere;
				txtConditions.Text = ""; // TODO - load conditions

				txtValueMinerals.Text = Planet.ResourceValue[Resource.Minerals].ToUnitString();
				txtValueOrganics.Text = Planet.ResourceValue[Resource.Organics].ToUnitString();
				txtValueRadioactives.Text = Planet.ResourceValue[Resource.Radioactives].ToUnitString();

				txtDescription.Text = Planet.Description;

				txtColonyType.Text = Planet.Owner == null ? "" : Planet.Owner.Name + " Colony"; // TODO - load colony type
				if (Planet.Owner == null)
					txtPopulation.Text = "0";
				else
				{
					var pop = Planet.Colony.Population.Sum(kvp => kvp.Value);
					if (Planet.PopulationChangePerTurn > 0)
						txtPopulation.Text = pop.ToUnitString(true) + " / " + Planet.MaxPopulation.ToUnitString(true) + " (+" + Planet.PopulationChangePerTurn.ToUnitString(true) + ")";
					else if (Planet.PopulationChangePerTurn < 0)
						txtPopulation.Text = pop.ToUnitString(true) + " / " + Planet.MaxPopulation.ToUnitString(true) + " (" + Planet.PopulationChangePerTurn.ToUnitString(true) + ")";
					else
						txtPopulation.Text = pop.ToUnitString(true) + " / " + Planet.MaxPopulation.ToUnitString(true) + " (stagnant)";
				}

				txtMood.Text = ""; // TODO - load mood

				// load income
				var income = Planet.Income;
				resIncomeMinerals.Amount = income[Resource.Minerals];
				resIncomeOrganics.Amount = income[Resource.Organics];
				resIncomeRadioactives.Amount = income[Resource.Radioactives];
				resResearch.Amount = income[Resource.Research];
				resIntel.Amount = income[Resource.Intelligence];

				// load construction data
				if (Planet.Colony == null || Planet.ConstructionQueue.FirstItemEta == null)
				{
					txtConstructionItem.Text = "(None)";
					txtConstructionItem.BackColor = Color.Transparent;
					txtConstructionTime.Text = "";
					txtConstructionTime.BackColor = Color.Transparent;
				}
				else
				{
					txtConstructionItem.Text = Planet.ConstructionQueue.FirstItemName;
					txtConstructionItem.BackColor = Planet.ConstructionQueue.FirstItemEta <= 1d ? Color.DarkGreen : Color.Transparent;
					if (Planet.ConstructionQueue.Eta != Planet.ConstructionQueue.FirstItemEta)
						txtConstructionTime.Text = Planet.ConstructionQueue.FirstItemEta.ToString("f1") + " turns (" + Planet.ConstructionQueue.Eta.ToString("f1") + " turns for all)";
					else
						txtConstructionTime.Text = Planet.ConstructionQueue.FirstItemEta.ToString("f1") + " turns";
					txtConstructionTime.BackColor = Planet.ConstructionQueue.Eta <= 1d ? Color.DarkGreen : Color.Transparent;
				}

				// load orders
				// TODO - let player adjust orders here
				lstOrdersDetail.Items.Clear();
				foreach (var order in Planet.Orders)
					lstOrdersDetail.Items.Add(order);

				// load facilities
				lstFacilitiesDetail.Initialize(32, 32);
				if (Planet.Colony != null)
				{
					txtFacilitySlotsFree.Text = string.Format("{0} / {1} slots free", Planet.MaxFacilities - Planet.Colony.Facilities.Count, Planet.MaxFacilities);

					foreach (var fg in Planet.Colony.Facilities.GroupBy(f => f.Template))
						lstFacilitiesDetail.AddItemWithImage(fg.Key.Group, fg.Count() + "x " + fg.Key.Name, fg.Key, fg.Key.Icon);
				}
				else
					txtFacilitySlotsFree.Text = "";

				// load cargo
				txtCargoSpaceFree.Text = string.Format("{0} / {1} free", (Planet.CargoStorage - (Planet.Cargo == null ? 0 : Planet.Cargo.Size)).Kilotons(), Planet.CargoStorage.Kilotons());
				lstCargoDetail.Initialize(32, 32);
				if (Planet.Cargo != null)
				{
					foreach (var ug in Planet.Cargo.Units.GroupBy(u => u.Design))
						lstCargoDetail.AddItemWithImage(ug.Key.VehicleTypeName, ug.Count() + "x " + ug.Key.Name, ug, ug.First().Icon);
					foreach (var pop in Planet.Cargo.Population)
						lstCargoDetail.AddItemWithImage("Population", pop.Value.ToUnitString(true) + " " + pop.Key.Name, pop, pop.Key.Icon);
				}

				abilityTreeView.Abilities = Planet.UnstackedAbilities.StackToTree();
				if (Planet.Colony == null)
					abilityTreeView.IntrinsicAbilities = Planet.IntrinsicAbilities;
				else
					abilityTreeView.IntrinsicAbilities = Planet.IntrinsicAbilities.Concat(Planet.Colony.Abilities);

			}
		}

		public void Bind(Planet data)
		{
			Planet = data;
			Bind();
		}

		private void picPortrait_Click(object sender, System.EventArgs e)
		{
			picPortrait.ShowFullSize(Planet.Name);
		}

		private void lstFacilitiesDetail_MouseDown(object sender, MouseEventArgs e)
		{
			var item = lstFacilitiesDetail.GetItemAt(e.X, e.Y);
			if (item != null)
			{
				var facil = (FacilityTemplate)item.Tag;
				var report = new FacilityReport(facil);
				var form = report.CreatePopupForm(facil.Name);
				FindForm().ShowChildForm(form);
			}
		}
	}
}
