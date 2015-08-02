﻿using System.IO;
using System.Threading;
using System.Windows.Data;
using FrEee.Modding;
using FrEee.Modding.Loaders;
using FrEee.Utility;
using FrEee.Wpf.ViewModels;

namespace FrEee.Wpf.Views
{
	/// <summary>
	/// Interaction logic for ModPickerView.xaml
	/// </summary>
	public partial class ModPickerView
	{
		public ModPickerView()
		{
			InitializeComponent();

			// load modinfos
			ModInfos = new ModPickerViewModel();
			var stock = new Mod();
			var loader = new ModInfoLoader(null);
			loader.Load(stock);
			ModInfos.Add(stock.Info);
			ModInfos.SelectedItem = stock.Info;
			if (Directory.Exists("Mods"))
			{
				foreach (var folder in Directory.GetDirectories("Mods"))
				{
					loader.ModPath = Path.GetFileName(folder);
					var mod = new Mod();
					loader.Load(mod);
					ModInfos.Add(mod.Info);
				}
			}
		}

		public ModPickerViewModel ModInfos
		{
			get
			{
				return (ModPickerViewModel)ViewModel;
			}
			set
			{
				ViewModel = value;
			}
		}

		private void btnLoad_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var thread = new Thread(new ThreadStart(() =>
			{
				Dispatcher.Invoke(() =>
						{
							Mod.Load(ModInfos.SelectedItem.Model.Folder, true, new Utility.Status());
							Close();
						});
			}));
			new StatusView(thread, new Status()).ShowDialog();
		}
	}
}
