using FrEee.Utility;
using System.Collections.Generic;
using System.IO;

namespace FrEee.Modding.Loaders;

/// <summary>
/// Loads the mod's general purpose scripts (not AI scripts).
/// </summary>
public sealed class ScriptLoader : ILoader
{
	public ScriptLoader(string modPath)
	{
		ModPath = modPath;
		FileName = "*.py";
	}

	public string FileName
	{
		get;
		set;
	}

	public string ModPath
	{
		get;
		set;
	}

	public IEnumerable<IModObject> Load(Mod mod)
	{
		// TODO - should scripts be mod objects?
		{
			var name = "builtins";
			string filename;
			string stockFilename = Path.Combine(CurrentAssembly.Location, "Scripts", name);
			if (ModPath == null)
				filename = stockFilename;
			else
				filename = Path.Combine(CurrentAssembly.Location, "Mods", ModPath, "Scripts", name);
			mod.GlobalScript = PythonScript.Load(filename) ?? PythonScript.Load(stockFilename) ?? new PythonScript(name, "");
		}
		{
			var name = "GameInit";
			string filename;
			string stockFilename = Path.Combine(CurrentAssembly.Location, "Scripts", name);
			if (ModPath == null)
				filename = stockFilename;
			else
				filename = Path.Combine(CurrentAssembly.Location, "Mods", ModPath, "Scripts", name);
			mod.GameInitScript = PythonScript.Load(filename) ?? PythonScript.Load(stockFilename) ?? new PythonScript(name, "");
		}
		{
			var name = "EndTurn";
			string filename;
			string stockFilename = Path.Combine(CurrentAssembly.Location, "Scripts", name);
			if (ModPath == null)
				filename = stockFilename;
			else
				filename = Path.Combine(CurrentAssembly.Location, "Mods", ModPath, "Scripts", name);
			mod.EndTurnScript = PythonScript.Load(filename) ?? PythonScript.Load(stockFilename) ?? new PythonScript(name, "");
		}

		yield break;
	}
}