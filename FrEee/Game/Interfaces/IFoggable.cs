﻿using FrEee.Game.Enumerations;
using FrEee.Game.Objects.Civilization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrEee.Game.Interfaces
{
	/// <summary>
	/// Something that can be obscured by fog of war.
	/// </summary>
	public interface IFoggable : IReferrable, IHistorical
	{
		/// <summary>
		/// Removes any data from this object that the specified empire cannot see.
		/// </summary>
		void Redact(Empire emp);

		/// <summary>
		/// Is this object just a memory, or a real object?
		/// </summary>
		bool IsMemory { get; set; }

		/// <summary>
		/// Is this object known to be destroyed?
		/// </summary>
		bool IsKnownToBeDestroyed { get; set; }
	}
}
