using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FrEee.Objects.Civilization;
using FrEee.Objects.Space;
using FrEee.Objects.Technology;
using FrEee.Modding.Loaders;
using FrEee.Modding.Templates;
using FrEee.Utility;
using FrEee.Extensions;
using FrEee.Objects.GameState;
using FrEee.Extensions;
using FrEee.Utility;
using FrEee.Processes.AI;
using FrEee.Modding.Abilities;

namespace FrEee.Modding;

/// <summary>
/// A set of data files containing templates for game objects.
/// </summary>
[Serializable]
public class Mod : IDisposable
{
	public Mod()
	{
		Current = this;

		Errors = new List<DataParsingException>();

		Info = new ModInfo();
		Settings = new ModSettings();
		AbilityRules = new List<AbilityRule>();
		StarSystemNames = new List<string>();
		DesignRoles = new List<string>();
		Traits = new List<Trait>();
		Technologies = new List<Technology>();
		FacilityTemplates = new List<FacilityTemplate>();
		Hulls = new List<IHull>();
		DamageTypes = new List<DamageType>();
		ComponentTemplates = new List<ComponentTemplate>();
		Mounts = new List<Mount>();
		StellarObjectSizes = new List<StellarObjectSize>();
		StarSystemTemplates = new List<StarSystemTemplate>();
		StellarAbilityTemplates = new List<RandomAbilityTemplate>();
		GalaxyTemplates = new List<GalaxyTemplate>();
		StellarObjectTemplates = new List<StellarObject>();
		HappinessModels = new List<HappinessModel>();
		Cultures = new List<Culture>();
		EmpireAIs = new List<AI<Empire, Galaxy>>();
		EventTypes = new List<EventType>();
		EventTemplates = new List<EventTemplate>();

		// for redacted colonies
		FacilityTemplates.Add(FacilityTemplate.Unknown);
	}

	private object locker = new object();

	/// <summary>
	/// The currently loaded mod.
	/// </summary>
	public static Mod Current { get; set; }

	/// <summary>
	/// The file name being loaded. (For error reporting)
	/// </summary>
	public static string CurrentFileName { get; private set; }

	/// <summary>
	/// Errors encountered when loading the mod.
	/// </summary>
	public static IList<DataParsingException> Errors { get; private set; }

	/// <summary>
	/// Rules for grouping and stacking abilities.
	/// </summary>
	public ICollection<AbilityRule> AbilityRules { get; private set; }

	/// <summary>
	/// The components in the mod.
	/// </summary>
	public ICollection<ComponentTemplate> ComponentTemplates { get; private set; }

	/// <summary>
	/// The empire cultures in the game.
	/// </summary>
	public ICollection<Culture> Cultures { get; private set; }

	/// <summary>
	/// The damage types in the mod.
	/// </summary>
	public ICollection<DamageType> DamageTypes { get; private set; }

	/// <summary>
	/// Role names to use for vehicle designs.
	/// </summary>
	public ICollection<string> DesignRoles { get; private set; }

	/// <summary>
	/// The empire AIs in the game.
	/// </summary>
	public ICollection<AI<Empire, Galaxy>> EmpireAIs { get; private set; }

	/// <summary>
	/// The script which runs after each turn.
	/// </summary>
	public PythonScript EndTurnScript { get; set; }

	/// <summary>
	/// The event templates in the mod.
	/// </summary>
	public ICollection<EventTemplate> EventTemplates { get; private set; }

	/// <summary>
	/// The event types in the mod.
	/// </summary>
	public ICollection<EventType> EventTypes { get; private set; }

	/// <summary>
	/// The facilities in the mod.
	/// </summary>
	public ICollection<FacilityTemplate> FacilityTemplates { get; private set; }

	/// <summary>
	/// Templates for galaxies.
	/// </summary>
	public ICollection<GalaxyTemplate> GalaxyTemplates { get; private set; }

	/// <summary>
	/// The script which runs on game initialization, prior to the first turn.
	/// </summary>
	public PythonScript GameInitScript { get; set; }

	/// <summary>
	/// The global Python script module which is available to all scripts in the mod.
	/// </summary>
	public PythonScript GlobalScript { get; set; }

	/// <summary>
	/// The happiness models in the game.
	/// </summary>
	public ICollection<HappinessModel> HappinessModels { get; private set; }

	/// <summary>
	/// The vehicle hulls in the mod.
	/// </summary>
	public ICollection<IHull> Hulls { get; private set; }

	/// <summary>
	/// General info about the mod.
	/// </summary>
	public ModInfo Info { get; set; }

	/// <summary>
	/// The component mounts in the mod.
	/// </summary>
	public ICollection<Mount> Mounts { get; private set; }

	private IList<IModObject> objects = new List<IModObject>() { FacilityTemplate.Unknown };

	/// <summary>
	/// All mod objects.
	/// </summary>
	public IEnumerable<IModObject> Objects
		=> objects;

	/// <summary>
	/// The path to the mod's root folder, relative to the Mods folder.
	/// </summary>
	public string? RootPath { get; set; }

	/// <summary>
	/// General mod settings.
	/// </summary>
	public ModSettings Settings { get; set; }

	/// <summary>
	/// Names to use for star systems.
	/// </summary>
	public ICollection<string> StarSystemNames { get; private set; }

	/// <summary>
	/// Templates for star systems.
	/// </summary>
	public ICollection<StarSystemTemplate> StarSystemTemplates { get; private set; }

	/// <summary>
	/// Templates for stellar abilities.
	/// </summary>
	public ICollection<RandomAbilityTemplate> StellarAbilityTemplates { get; private set; }

	/// <summary>
	/// Planet and asteroid field sizes.
	/// </summary>
	public ICollection<StellarObjectSize> StellarObjectSizes { get; private set; }

	/// <summary>
	/// Templates for stellar objects.
	/// </summary>
	public ICollection<StellarObject> StellarObjectTemplates { get; private set; }

	/// <summary>
	/// The technologies in the game.
	/// </summary>
	public ICollection<Technology> Technologies { get; private set; }

	/// <summary>
	/// The race/empire traits in the game.
	/// </summary>
	public ICollection<Trait> Traits { get; private set; }

	/// <summary>
	/// Names of files containing lists of design names.
	/// e.g. Ravager would be loaded from Mods/CurrentMod/Dsgnname/Ravager.txt and also from Dsgnname/Ravager.txt.
	/// </summary>
	public IEnumerable<string> DesignNamesFiles
	{
		get
		{
			var list = new List<string>();
			string path;
			if (RootPath != null)
			{
				try
				{
					path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Mods", RootPath, "Dsgnname");
					foreach (var f in Directory.GetFiles(path))
						list.Add(Path.GetFileNameWithoutExtension(f));
				}
				catch (IOException)
				{
					// nothing to do, path probably doesn't exist
				}
			}
			path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Dsgnname");
			foreach (var f in Directory.GetFiles(path))
				list.Add(Path.GetFileNameWithoutExtension(f));
			return list.OrderBy(q => q).Distinct();
		}
	}

	/// <summary>
	/// Loads a mod.
	/// </summary>
	/// <param name="path">The mod root path, relative to the Mods folder. Or null to load the stock mod.</param>
	/// <param name="setCurrent">Set the current mod to the new mod?</param>
	/// <param name="status">A status object to report status back to the GUI.</param>
	/// <param name="desiredProgress">How much progress should we report back to the GUI when we're done loading the mod? 1.0 means all done with everything that needs to be done.</param>
	public static Mod Load(string? path, bool setCurrent = true, Status? status = null, double desiredProgress = 1.0)
	{
		var mod = new Mod();
		mod.RootPath = path;

		if (setCurrent)
			Current = mod;

		if (path != null && !Directory.Exists(Path.Combine("Mods", path)))
			throw new DirectoryNotFoundException($"Could not find mod {path} in the Mods folder.");

		var loaders = new Dictionary<ILoader, int>
		{
			{ new ModInfoLoader(path), 0 },
			{ new TextLoader(path, "SystemNames.txt", m => m.StarSystemNames), 0 },
			{ new DesignRoleLoader(path), 0 },
			{ new ScriptLoader(path), 0 },
			{ new AbilityRuleLoader(path), 0 },
			{ new ModSettingsLoader(path), 0 },
			{ new StellarObjectSizeLoader(path), 1 },
			{ new StellarAbilityLoader(path), 2 },
			{ new StellarObjectLoader(path), 3 },
			{ new TraitLoader(path), 1 },
			{ new TechnologyLoader(path), 2 },
			{ new FacilityLoader(path), 3 },
			{ new HullLoader(path), 3 },
			{ new DamageTypeLoader(path), 0 },
			{ new ComponentLoader(path), 3 },
			{ new MountLoader(path), 3 },
			{ new StarSystemLoader(path), 3 },
			{ new GalaxyLoader(path), 4 },
			{ new HappinessModelLoader(path), 0 },
			{ new CultureLoader(path), 0 },
			{ new EmpireAILoader(path) , 0 },
			{ new EventTypeLoader(path), 0 },
			{ new EventLoader(path), 1 },
		};

		var progressPerFile = (desiredProgress - (status == null ? 0 : status.Progress)) / loaders.Count;

		var used = new HashSet<string>();

		var minPriority = loaders.Values.Min();
		var maxPriority = loaders.Values.Max();

		for (var p = minPriority; p <= maxPriority; p++)
		{
			var files = loaders.Where(q => q.Value == p).Select(q => q.Key.FileName);
			CurrentFileName = string.Join(" / ", files);
			if (status != null)
				status.Message = "Loading " + CurrentFileName;

			loaders.Where(q => q.Value == p).ParallelSafeForeach(loader =>
			{
				foreach (var mo in loader.Key.Load(mod).ToArray())
				{
					mod.AssignID(mo, used);
				}
				if (status != null)
					status.Progress += progressPerFile;
			});
		}

		CurrentFileName = null;

		var dupes = mod.Objects.GroupBy(o => o.ModID).Where(g => g.Count() > 1);
		if (dupes.Any())
			throw new Exception("Multiple objects with mod ID {0} found ({1} total IDs with duplicates)".F(dupes.First().Key, dupes.Count()));

		return mod;
	}

	public void AssignID(IModObject mo, ICollection<string> used)
	{
		if (mo.ModID != null)
			return;
		lock (locker)
		{
			var fullname = mo.GetType().Name + " " + mo.Name;
			if (mo.Name != null && !used.Contains(fullname))
			{
				mo.ModID = fullname;
				used.Add(mo.ModID);
			}
			else
			{
				// tack a number on
				int lastnum;
				string name;
				if (mo.Name == null)
					name = "Generic " + mo.GetType().Name;
				else
					name = mo.GetType().Name + " " + mo.Name;
				var lastword = name.LastWord();
				if (int.TryParse(lastword, out lastnum))
				{
					// has a number, count from that number
				}
				else
				{
					lastnum = -1; // no number, start from 1
				}
				for (var num = lastnum + 1; num <= int.MaxValue; num++)
				{
					string exceptnum;
					if (lastnum < 0 && num == 0)
					{
						exceptnum = name;
						num = 1;
					}
					else if (lastnum < 0)
						exceptnum = name;
					else
						exceptnum = name.Substring(0, name.Length - lastword.Length - 1);
					var withnextnum = exceptnum + " " + num;
					if (!used.Contains(withnextnum))
					{
						mo.ModID = withnextnum;
						used.Add(withnextnum);
						break;
					}
					if (num == int.MaxValue)
						throw new Exception("Can't assign mod ID to " + name + "; there's a gazillion other mod objects with that name.");
				}
			}

			if (mo.ModID == null)
				throw new Exception("Failed to assign mod ID to {0}: {1}".F(mo.GetType(), mo));

			if (!objects.Contains(mo))
				objects.Add(mo);
		}
	}

	/// <summary>
	/// Assigns automatic IDs to all objects in the mod that lack IDs.
	/// </summary>
	public void AssignIDs()
	{
		var used = new HashSet<string>();
		foreach (var mo in Objects)
		{
			if (mo.ModID == null)
				AssignID(mo, used);
			var dupes = Objects.Where(q => q.ModID == mo.ModID); // with same mod ID
			if (dupes.Count() > 1)
			{
				foreach (var dupe in dupes)
					dupe.ModID = dupe.GetType().Name + " " + dupe.ModID;
			}
		}
	}

	public void Dispose()
	{
		foreach (var r in AbilityRules.ToArray())
			r.Dispose();
		foreach (var sos in StellarObjectSizes.ToArray())
			sos.Dispose();
		foreach (var x in StellarAbilityTemplates.ToArray())
			x.Dispose();
		foreach (var sot in StellarObjectTemplates.ToArray())
			sot.Dispose();
		foreach (var t in Traits.ToArray())
			t.Dispose();
		foreach (var t in Technologies.ToArray())
			t.Dispose();
		foreach (var f in FacilityTemplates.ToArray())
			f.Dispose();
		foreach (var h in Hulls.ToArray())
			h.Dispose();
		foreach (var c in ComponentTemplates.ToArray())
			c.Dispose();
		foreach (var m in Mounts.ToArray())
			m.Dispose();
		foreach (var sst in StarSystemTemplates.ToArray())
			sst.Dispose();
		foreach (var gt in GalaxyTemplates.ToArray())
			gt.Dispose();
		foreach (var h in HappinessModels.ToArray())
			h.Dispose();
		foreach (var c in Cultures.ToArray())
			c.Dispose();
		foreach (var ai in EmpireAIs.ToArray())
			ai.Dispose();
		if (this == Current)
			Current = null;
	}

	/// <summary>
	/// Do these ability names refer to the same ability, using aliases?
	/// </summary>
	/// <param name="n1"></param>
	/// <param name="n2"></param>
	/// <returns></returns>
	public bool DoAbilityNamesMatch(string n1, string n2)
	{
		if (n1 == n2)
			return true;
		var r1 = FindAbilityRule(n1);
		var r2 = FindAbilityRule(n2);
		return r1 != null && r2 != null && r1 == r2;
	}

	public T? Find<T>(string modid)
		where T : IModObject
	{
		T? result = default;
		lock (locker)
		{
			result = Objects.OfType<T>().SingleOrDefault(q => q.ModID == modid);
		}
		return result;
	}

	/// <summary>
	/// Finds an ability rule by name or alias.
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	public AbilityRule FindAbilityRule(string name)
	{
		return AbilityRules.SingleOrDefault(r => r.Matches(name));
	}

	public override string ToString()
	{
		return RootPath ?? "<Stock>";
	}
}
