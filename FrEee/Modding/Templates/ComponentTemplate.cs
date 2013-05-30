using System;
using System.Collections.Generic;
using System.Drawing;
using FrEee.Game.Interfaces;
using FrEee.Game.Objects.Abilities;
using FrEee.Utility;
using FrEee.Utility.Extensions;
using FrEee.Game.Objects.Space;
using FrEee.Game.Objects.Technology;
using FrEee.Game.Enumerations;

namespace FrEee.Modding.Templates
{
	/// <summary>
	/// A template for a vehicle component.
	/// </summary>
	[Serializable]
	public class ComponentTemplate : INamed, IResearchable, IAbilityObject, ITemplate<Component>
	{
		public ComponentTemplate()
		{
			Abilities = new List<Ability>();
			TechnologyRequirements = new List<TechnologyRequirement>();
			Cost = new Resources();
		}

		/// <summary>
		/// The name of the component.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// A description of the component.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Name of the picture used to represent this component, excluding the file extension.
		/// PNG files will be searched first, then BMP.
		/// </summary>
		public string PictureName { get; set; }

		public Image Icon
		{
			get { return Pictures.GetIcon(this); }
		}

		public Image Portrait
		{
			get { return Pictures.GetPortrait(this); }
		}

		/// <summary>
		/// The size of the component, in kilotons.
		/// </summary>
		public int Size { get; set; }

		/// <summary>
		/// The durability of the component, in kilotons. (Yes, kilotons.)
		/// </summary>
		public int Durability { get; set; }

		/// <summary>
		/// The cost to build the component.
		/// </summary>
		public Resources Cost { get; set; }

		/// <summary>
		/// The vehicle types on which this component can be installed.
		/// </summary>
		public VehicleTypes VehicleTypes { get; set; }

		/// <summary>
		/// Amount of supply consumed when this component is "used". (What "usage" means depends on the component's abilities.)
		/// </summary>
		public int SupplyUsage { get; set; }

		/// <summary>
		/// The maximum number of this component that can be installed on a vehicle, or null for no limit.
		/// </summary>
		public int? MaxPerVehicle { get; set; }

		/// <summary>
		/// The group that the component belongs to. Used for grouping on the design screen.
		/// </summary>
		public string Group { get; set; }

		/// <summary>
		/// The family that the component belongs to. Used for "Only Latest" on the design screen.
		/// </summary>
		public string Family { get; set; }

		/// <summary>
		/// The value of the Roman numeral that should be displayed on the component's icon.
		/// </summary>
		public int RomanNumeral { get; set; }

		/// <summary>
		/// Used by artificial world construction abilities.
		/// </summary>
		public string StellarConstructionGroup { get; set; }

		/// <summary>
		/// The technology requirements for this component.
		/// </summary>
		public IList<TechnologyRequirement> TechnologyRequirements { get; private set; }

		/// <summary>
		/// Abilities possessed by this component.
		/// </summary>
		public IList<Ability> Abilities { get; private set; }

		IEnumerable<Ability> IAbilityObject.Abilities
		{
			get { return Abilities; }
		}

		/// <summary>
		/// If this component is a weapon, info about it will be stored here.
		/// </summary>
		public WeaponInfo WeaponInfo { get; set; }

		/// <summary>
		/// Creates a component from this template.
		/// </summary>
		/// <returns></returns>
		public Component Instantiate()
		{
			return new Component(this);
		}
	}
}
