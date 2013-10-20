﻿using System;
using System.Threading;
using System.Linq;
using System.Windows.Forms;
using FrEee.WinForms.Forms;
using FrEee.Game.Objects.Space;
using FrEee.Utility.Extensions;
using System.IO;
using FrEee.Game.Objects.Civilization;
using FrEee.Utility;
using System.Text.RegularExpressions;
using System.Reflection;

namespace FrEee.WinForms
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// Return values:
		/// 0 for success
		/// 1 for syntax error in command line
		/// 2 for missing GAM or PLR file specified to load
		/// 3 for crash
		/// 1xxx for missing PLR file for player xxx when running in "safe processing" mode
		/// </summary>
		[STAThread]
		static int Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			if (args.Length == 0)
			{
				Application.Run(MainMenuForm.GetInstance());
				return 0;
			}
			else if (args.Length == 1)
			{
				if (args[0].TrimStart('-').ToLower() == "help")
				{
					DisplaySyntax();
					return 0;
				}
				else
				{
					var file = args[0];
					return ProcessArgs(file);
				}
			}
			else if (args.Length == 2)
			{
				var operation = args[0];
				var file = args[1];
				return ProcessArgs(file, operation.TrimStart('-').ToLower());
			}
			else
			{
				return DisplaySyntax();
			}
		}

		private static int DisplaySyntax()
		{
			MessageBox.Show(
@"Syntax:

FrEee: display main menu

FrEee --help: display this help

FrEee --host gamename_turnnumber.gam: load host console
	Shortcut: FrEee gamename_turnnumber.gam
FrEee --process gamename_turnnumber.gam: process turn
FrEee --process-safe gamename_turnnumber.gam: process turn, halting if any plr files are missing

FrEee --play gamename_turnnumber_playernumber.gam: play a turn, resuming from where you left off if a plr file is present
	Shortcut: FrEee gamename_turnnumber_playernumber.gam
FrEee --play gamename_turnnumber_playernumber.plr: play a turn, resuming from where you left off
	Shortcut: FrEee gamename_turnnumber_playernumber.plr
FrEee --load gamename_turnnumber_playernumber.gam: play a turn, prompting to resume if plr file is present
FrEee --restart gamename_turnnumber_playernumber.gam: play a turn, restarting from the beginning of the turn");
			return 1;
		}

		private static int ProcessArgs(string file, string operation = null)
		{
			string gamfile = null, plrfile = null;

			// guess operation from filename
			// TODO - move these regexes to the Galaxy class?
			var playerRegex = @"(?i).*_.*_.*\.gam";
			var hostRegex = @"(?i).*_.*\.gam";
			var cmdRegex = @"(?i).*_.*_.*\.plr";

			if (Regex.IsMatch(file, playerRegex))
				operation = "play";
			else if (Regex.IsMatch(file, hostRegex))
				operation = "host";
			else if (Regex.IsMatch(file, cmdRegex))
				operation = "play";
				
			else
				return DisplaySyntax();

			gamfile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Savegame", Path.GetFileNameWithoutExtension(file) + ".gam");
			plrfile = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Savegame", Path.GetFileNameWithoutExtension(file) + ".plr");

			if (!File.Exists(gamfile))
			{
				MessageBox.Show(gamfile + " does not exist.");
				return 2;
			}

			// load GAM file, see if it's a host or player view
			Galaxy.Load(gamfile);
			if (Empire.Current == null)
			{
				// host view
				if (operation == "host")
				{
					// display host console
					var form = new HostConsoleForm();
					form.StartPosition = FormStartPosition.CenterScreen;
					Application.Run(form);
					return 0;
				}
				else if (operation == "process")
					return ProcessTurn(false);
				else if (operation == "process-safe")
					return ProcessTurn(true);
				else
					return DisplaySyntax();
			}
			else
			{
				// player view
				if (operation == "play")
				{
					if (File.Exists(plrfile))
						return PlayTurn(plrfile);
					else
						return PlayTurn();
				}
				else if (operation == "load")
				{
					// load turn and prompt player to resume if PLR file exists
					if (File.Exists(plrfile))
					{
						var result = MessageBox.Show("Player commands file exists for this turn. Resume turn from where you left off?", "Resume Turn", MessageBoxButtons.YesNo);
						if (result == DialogResult.Yes)
							return PlayTurn(plrfile);
						else
							return PlayTurn();
					}
					else
						return PlayTurn();
				}
				else if (operation == "restart")
					return PlayTurn();
				else
					return DisplaySyntax();
			}
		}

		private static int ProcessTurn(bool safe)
		{
			try
			{
				var status = new Status();
				Action status_Changed = () =>
				{
					Console.WriteLine(status.Progress.ToString("p0") + ": " + status.Message);
					if (status.Exception != null)
						Console.Error.WriteLine(status.Exception);
				};
				status.Changed += new Status.ChangedDelegate(status_Changed);

				Console.WriteLine("Processing turn...");
				var emps = Galaxy.ProcessTurn(false, status);
				foreach (var emp in emps)
					Console.WriteLine(emp + " did not submit a PLR file.");
				if (safe && emps.Any())
				{
					Console.Error.WriteLine("Halting turn processing due to missing PLR file(s).");
					return 1001 + Galaxy.Current.Empires.IndexOf(emps.First());
				}
				Galaxy.SaveAll();
				Console.WriteLine("Turn processed successfully. It is now turn " + Galaxy.Current.TurnNumber + " (stardate " + Galaxy.Current.Stardate + ").");
				Application.Exit();
				return 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return 3;
			}
		}

		private static int PlayTurn(string plrfile = null)
		{
			if (plrfile != null)
			{
				if (File.Exists(plrfile))
					Galaxy.Current.LoadCommands();
				else
					MessageBox.Show(plrfile + " does not exist. You will need to start your turn from the beginning.");
			}
			var form = new GameForm(false);
			form.StartPosition = FormStartPosition.CenterScreen;
			Application.Run(form);
			return 0;
		}
	}
}
