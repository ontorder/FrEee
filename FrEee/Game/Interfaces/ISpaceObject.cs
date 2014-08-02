using System;
using System.Collections.Generic;
using System.Drawing;
using FrEee.Game.Enumerations;
using FrEee.Game.Objects.Abilities;
using FrEee.Game.Objects.Civilization;
using FrEee.Game.Objects.Space;
using FrEee.Game.Objects.Technology;
using FrEee.Game.Objects.Combat;
using FrEee.Utility;

namespace FrEee.Game.Interfaces
{
	public interface ISpaceObject : IOwnableAbilityObject, IPictorial, IReferrable, IFoggable, INamed, ILocated
	{
		/// <summary>
		/// The name of this space object.
		/// </summary>
		new string Name { get; set; }

		/// <summary>
		/// Is this space object hostile to an empire?
		/// </summary>
		bool IsHostileTo(Empire emp);

		/// <summary>
		/// This space object's construction queue, if any.
		/// </summary>
		ConstructionQueue ConstructionQueue { get; }

		/// <summary>
		/// Can this space object be placed in a fleet?
		/// </summary>
		bool CanBeInFleet { get; }

		/// <summary>
		/// Supply storage capacity.
		/// </summary>
		int SupplyStorage { get; }

		/// <summary>
		/// Does this space object have infinite supplies?
		/// TODO - make supply a resource?
		/// </summary>
		bool HasInfiniteSupplies { get; }

		/// <summary>
		/// Can this space object traverse warp points?
		/// </summary>
		bool CanWarp { get; }

		/// <summary>
		/// Is this space object idle?
		/// Space objects are idle if they have no orders (but they have speed greater than zero, and are not in a fleet) or their construction queue has a fractional ETA less than 1.
		/// </summary>
		bool IsIdle { get; }

		/// <summary>
		/// Resources stored on this space object.
		/// For a ship, this could include supplies.
		/// </summary>
		ResourceQuantity StoredResources { get; }
	}
}
