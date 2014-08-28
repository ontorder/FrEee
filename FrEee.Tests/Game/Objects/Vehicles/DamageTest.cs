﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using FrEee.Game.Objects.Space;
using FrEee.Game.Objects.Vehicles;
using System.Drawing;
using FrEee.Game.Objects.Technology;
using FrEee.Game.Objects.Civilization;
using FrEee.Utility.Extensions;
using FrEee.Game.Objects.Abilities;
using FrEee.Game.Interfaces;
using FrEee.Modding;
using FrEee.Modding.Templates;
using FrEee.Game.Objects.Combat;

namespace FrEee.Tests.Game.Objects.Vehicles
{
	/// <summary>
	/// Tests damage to vehicles.
	/// </summary>
	[TestClass]
	public class DamageTest
	{
		#region variables
		/// <summary>
		/// The ship that is taking damage.
		/// </summary>
		Ship ship;

		/// <summary>
		/// They're controlling the ship.
		/// </summary>
		Empire empire;

		/// <summary>
		/// The template for the engine component.
		/// </summary>
		ComponentTemplate engineTemplate;

		/// <summary>
		/// Number of engines to give the ship.
		/// </summary>
		const int numEngines = 3;

		#endregion

		[TestInitialize]
		public void Setup()
		{
			// initialize galaxy
			Mod.Load(null);
			new Galaxy();

			// initialize empire
			empire = new Empire();
			empire.Name = "Masochists";

			// initialize engine template
			engineTemplate = new ComponentTemplate();
			engineTemplate.Name = "Gotta-Go-Fast Engine";
			engineTemplate.Abilities.Add(new Ability(engineTemplate, Mod.Current.AbilityRules.FindByName("Standard Ship Movement"), "Lets the ship go fast.", "1"));
			engineTemplate.Abilities.Add(new Ability(engineTemplate, Mod.Current.AbilityRules.FindByName("Movement Bonus"), "Lets the ship go REALLY fast.", "1"));
			engineTemplate.Abilities.Add(new Ability(engineTemplate, Mod.Current.AbilityRules.FindByName("Extra Movement Generation"), "Lets the ship go REALLY REALLY fast.", "1"));
			engineTemplate.Abilities.Add(new Ability(engineTemplate, Mod.Current.AbilityRules.FindByName("Vehicle Speed"), "Lets the ship go REALLY FREAKIN' UNGODLY LUDICROUSLY fast.", "1"));
			engineTemplate.Abilities.Add(new Ability(engineTemplate, Mod.Current.AbilityRules.FindByName("Quantum Reactor"), "Infinite supplies! Wheeee!"));
			engineTemplate.SupplyUsage = 0;
			engineTemplate.Durability = 10;

			// initialize ship's design
			var design = new Design<Ship>();
			design.BaseName = "Punching Bag";
			design.Hull = new Hull<Ship>();
			design.Hull.Mass = 1;
			design.Owner = empire;
			for (var i = 0; i < numEngines; i++)
				design.Components.Add(new MountedComponentTemplate(design, engineTemplate));

			// TODO - account for C&C and supply requirements once those are a thing

			// initialize ship
			ship = design.Instantiate();
			ship.Owner = empire;
		}

		/// <summary>
		/// Ship speed should degrade as engines take damage.
		/// Also, ships should go the proper speed when undamaged.
		/// </summary>
		[TestMethod]
		public void EngineDamage()
		{
			// sanity check
			Assert.AreNotEqual(0, GetExpectedSpeed(ship));

			Assert.AreEqual(GetExpectedSpeed(ship), ship.Speed);

			for (var i = 0; i < numEngines; i++)
			{
				// ouchies!
				ship.Components.Where(c => c.Template.ComponentTemplate == engineTemplate && c.Hitpoints > 0).First().Hitpoints = 0;

				Assert.AreEqual(GetExpectedSpeed(ship), ship.Speed);
			}
		}

		private int GetExpectedSpeed(Ship ship)
		{
			// add up thrust of all working engines, and divide by hull mass (engines per move, not tonnage)
			// HACK - assumes standard ability rules!
			// TODO - worry about supplies
			var thrust = ship.Components.Where(c => c.Hitpoints > 0).Sum(c => c.GetAbilityValue("Standard Ship Movement").ToInt());
			if (thrust < ship.Hull.Mass)
				return 0;
			return thrust / ship.Hull.Mass
				+ ship.Components.Where(c => c.Hitpoints > 0).MaxOrDefault(c => c.GetAbilityValue("Movement Bonus").ToInt())
				+ ship.Components.Where(c => c.Hitpoints > 0).MaxOrDefault(c => c.GetAbilityValue("Extra Movement Generation").ToInt())
				+ ship.Components.Where(c => c.Hitpoints > 0).MaxOrDefault(c => c.GetAbilityValue("Vehicle Speed").ToInt());
		}
	}
}