﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FrEee.Gui.Controls;
using FrEee.Game;
using FrEee.Modding;

namespace FrEee
{
	public partial class GameForm : Form
	{
		public GameForm()
		{
			InitializeComponent();

			// set up resource display
			var pnlResources = new FlowLayoutPanel();
			pnlResources.FlowDirection = FlowDirection.LeftToRight;
			pnlResources.WrapContents = false;
			pnlResources.Controls.Add(new ResourceDisplay { ResourceColor = Color.Blue, Amount = 500000, Change = -25000});
			pnlResources.Controls.Add(new ResourceDisplay { ResourceColor = Color.Green, Amount = 250000, Change = +10000});
			pnlResources.Controls.Add(new ResourceDisplay { ResourceColor = Color.Red, Amount = 0, Change = -5000});
			var pnlResIntel = new FlowLayoutPanel();
			pnlResIntel.FlowDirection = FlowDirection.LeftToRight;
			pnlResIntel.WrapContents = false;
			pnlResIntel.Controls.Add(new ResourceDisplay { ResourceColor = Color.Magenta, Amount = 50000 });
			pnlResIntel.Controls.Add(new ResourceDisplay { ResourceColor = Color.White, Amount = 10000 });
			pagResources.Content = new List<Control>();
			pagResources.Content.Add(pnlResources);
			pagResources.Content.Add(pnlResIntel);
			pagResources.CurrentPage = 0;

			// set up system view
			// uncomment this to see a star system instead of test mod loading until mods can store space object templates
			/*var starsys = new StarSystem(8);
			starsys.GetSector(0, 0).SpaceObjects.Add(new Star { Name = "Tudran Star", Age = "Old", Brightness = "Dim", Color = "Red", Description = "A red giant star.", Size = Game.Size.Large });
			starsys.GetSector(6, 3).SpaceObjects.Add(new Planet { Name = "Tudran I" });
			starsys.GetSector(1, 5).SpaceObjects.Add(new Planet { Name = "Tudran II" });
			starsys.GetSector(1, 5).SpaceObjects.Add(new Planet { Name = "Tudran IIa" });
			starsys.GetSector(0, 8).SpaceObjects.Add(new WarpPoint { Name = "Warp Point to Fizbon" });
			starSystemView.StarSystem = starsys;*/

			// set up system view
			// comment this when showing a star system instead of testing mod loading until mods can store space object templates
			Mod.Load("Stock");
			var starsys = Mod.Current.StarSystemTemplates.First().Value.Instantiate();
			starSystemView.StarSystem = starsys;
		}

		private void starSystemView_SectorClicked(StarSystemView sender, Sector sector)
		{
			// select the sector that was clicked
			starSystemView.SelectedSector = sector;
		}

		private void starSystemView_SectorSelected(StarSystemView sender, Sector sector)
		{
			// remove old report, if any
			pnlDetailReport.Controls.Clear();

			if (sector.SpaceObjects.Count > 0)
			{
				// add new report
				Control newReport = null;
				if (sector.SpaceObjects.Count == 1)
				{
					// add new report
					newReport = CreateSpaceObjectReport(sector.SpaceObjects.Single());
				}
				else
				{
					// add list view
					var lv = new ListView();
					newReport = lv;
					lv.View = View.Tile;
					lv.BackColor = Color.Black;
					lv.ForeColor = Color.White;
					lv.BorderStyle = BorderStyle.None;
					var il = new ImageList();
					lv.LargeImageList = il;
					lv.SmallImageList = il;
					int i = 0;
					foreach (var sobj in sector.SpaceObjects)
					{
						var item = new ListViewItem();
						item.Text = sobj.Name;
						item.Tag = sobj;
						il.Images.Add(sobj.Icon);
						item.ImageIndex = i;
						i++;
						lv.Items.Add(item);
					}
					lv.MouseDoubleClick += SpaceObjectListReport_MouseDoubleClick;
				}

				if (newReport != null)
				{
					// align control
					pnlDetailReport.Controls.Add(newReport);
					newReport.Left = newReport.Margin.Left;
					newReport.Width = pnlDetailReport.Width - newReport.Margin.Right - newReport.Margin.Left;
					newReport.Top = newReport.Margin.Top;
					newReport.Height = pnlDetailReport.Height - newReport.Margin.Bottom - newReport.Margin.Top;
					newReport.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
				}
			}
		}

		void SpaceObjectListReport_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			var lv = (ListView)sender;
			if (lv.SelectedItems.Count > 0)
			{
				// remove list view
				pnlDetailReport.Controls.Clear();

				// add new report
				var item = lv.GetItemAt(e.X, e.Y);
				pnlDetailReport.Controls.Add(CreateSpaceObjectReport((ISpaceObject)item.Tag));
			}
		}

		private Control CreateSpaceObjectReport(ISpaceObject sobj)
		{
			if (sobj is Star)
				return new StarReport((Star)sobj);
			// TODO - other typeos of space objects
			return null;
		}
	}
}
