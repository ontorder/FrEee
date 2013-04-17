﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace FrEee.Game
{
	/// <summary>
	/// A star. Normally found at the center of a star system.
	/// </summary>
	public class Star : ISpaceObject
	{
		/// <summary>
		/// The name of this star.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Stars cannot be obscured by fog of war.
		/// </summary>
		public bool CanBeFogged { get { return false; } }

		/// <summary>
		/// The size of this star.
		/// </summary>
		public Size Size { get; set; }

		/// <summary>
		/// The brightness of this star. (For flavor)
		/// </summary>
		public string Brightness { get; set; }

		/// <summary>
		/// The color of this star. (For flavor)
		/// </summary>
		public string Color { get; set; }

		/// <summary>
		/// The age of this star. (For flavor)
		/// </summary>
		public string Age { get; set; }

		/// <summary>
		/// Description of this star. (For flavor)
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Index of the picture to use to represent this star.
		/// </summary>
		public int PictureNumber { get; set; }

		/// <summary>
		/// A picture used to represent this star on the map.
		/// </summary>
		public Image Icon
		{
			get
			{
				return Pictures.GetStellarObjectIcon(PictureNumber);
			}
		}

		/// <summary>
		/// A picture used to represent this star in reports.
		/// </summary>
		public Image Portrait
		{
			get
			{
				return Pictures.GetStellarObjectPortrait(PictureNumber);
			}
		}
	}
}
