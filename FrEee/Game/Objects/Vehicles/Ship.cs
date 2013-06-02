﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrEee.Game.Objects.Vehicles
{
	[Serializable]
	public class Ship : Vehicle<Ship>
	{
		public override bool RequiresSpaceYardQueue
		{
			get { return true; }
		}
	}
}
