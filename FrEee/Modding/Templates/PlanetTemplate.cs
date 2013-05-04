﻿using FrEee.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrEee.Modding.Templates
{
	/// <summary>
	/// A template for generating planets.
	/// </summary>
	public class PlanetTemplate : ITemplate<Planet>
	{
		/// <summary>
		/// Abilities to assign to the planet.
		/// </summary>
		public RandomAbilityTemplate Abilities { get; set; }

		/// <summary>
		/// The size of the planet, or null to choose a size randomly.
		/// </summary>
		public Size? Size { get; set; }

		/// <summary>
		/// The atmosphere of the planet, or null to choose a planet randomly.
		/// </summary>
		public string Atmosphere { get; set; }

		/// <summary>
		/// The surface compositiion of the planet, or null to choose a surface randomly.
		/// </summary>
		public string Surface { get; set; }

		public Planet Instantiate()
		{
			var planet = new Planet();

			var abil = Abilities.Instantiate();
			if (abil != null)
				planet.IntrinsicAbilities.Add(abil);

			// TODO - use SectType.txt entries for instantiating planets
			planet.Size = Size ?? new Size[] { Game.Size.Tiny, Game.Size.Small, Game.Size.Medium, Game.Size.Large, Game.Size.Huge }.PickRandom();
			planet.Atmosphere = Atmosphere ?? new string[] { "None", "Methane", "Oxygen", "Hydrogen", "Carbon Dioxide" }.PickRandom();
			planet.Surface = Surface ?? new string[] { "Rock", "Ice", "Gas Giant" }.PickRandom();

			planet.ResourceValue["minerals"] = Rng.Range(Mod.Current.MinAsteroidResourceValue, Mod.Current.MaxAsteroidResourceValue);
			planet.ResourceValue["organics"] = Rng.Range(Mod.Current.MinAsteroidResourceValue, Mod.Current.MaxAsteroidResourceValue);
			planet.ResourceValue["radioactives"] = Rng.Range(Mod.Current.MinAsteroidResourceValue, Mod.Current.MaxAsteroidResourceValue);

			return planet;
		}
	}
}
