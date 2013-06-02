﻿using FrEee.Game.Interfaces;
using FrEee.Game.Objects.LogMessages;
using FrEee.Game.Objects.Space;
using FrEee.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace FrEee.Game.Objects.Civilization
{
	[Serializable]
	public class ConstructionQueue : IOrderable<ConstructionQueue, IConstructionOrder>
	{
		public ConstructionQueue()
		{
			Orders = new List<IConstructionOrder>();
			Galaxy.Current.Register(this);
		}

		/// <summary>
		/// Is this a space yard queue?
		/// </summary>
		public bool IsSpaceYardQueue { get; set; }

		/// <summary>
		/// Is this a colony queue?
		/// </summary>
		public bool IsColonyQueue { get { return Colony != null; } }

		/// <summary>
		/// The colony (if any) associated with this queue.
		/// </summary>
		public Colony Colony
		{
			get
			{
				if (SpaceObject is Planet)
					return ((Planet)SpaceObject).Colony;
				return null;
			}
		}

		/// <summary>
		/// Can this queue construct something?
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool CanConstruct(IConstructionTemplate item)
		{
			return (IsSpaceYardQueue || !item.RequiresSpaceYardQueue) && (IsColonyQueue || !item.RequiresColonyQueue);
		}

		/// <summary>
		/// The rate at which this queue can construct.
		/// </summary>
		public Resources Rate { get; set; }

		/// <summary>
		/// Unspent build rate for this turn.
		/// </summary>
		public Resources UnspentRate { get; set; }

		public IList<IConstructionOrder> Orders
		{
			get;
			private set;
		}

		public int ID
		{
			get;
			set;
		}

		public ISpaceObject SpaceObject { get; set; }

		public Image Icon
		{
			get
			{
				return SpaceObject.Icon;
			}
		}

		public Empire Owner
		{
			get { return SpaceObject.Owner; }
		}

		public string Name
		{
			get { return SpaceObject.Name; }
		}

		/// <summary>
		/// Executes orders for a turn.
		/// </summary>
		public void ExecuteOrders()
		{
			UnspentRate = Rate;
			var empty = new Resources();
			while (UnspentRate > empty && Orders.Any())
			{
				var numOrders = Orders.Count;

				var order = Orders[0];
				if (!CanConstruct(order.Template))
				{
					// can't build that here!
					Orders.RemoveAt(0);
					Owner.Log.Add(new PictorialLogMessage<ISpaceObject>(order.Template + " cannot be built at " + this + " because it requires a space yard and/or colony to construct it.", SpaceObject));
				}
				order.Execute(this);
				if (order.IsComplete)
				{
					order.Item.Place(SpaceObject);
					Orders.Remove(order);
				}

				if (Orders.Count == numOrders)
					break; // couldn't accomplish any orders
			}
		}

		/// <summary>
		/// The name of the first item.
		/// </summary>
		public string FirstItemName
		{
			get
			{
				if (!Orders.Any())
					return null;
				return Orders[0].Item.Name;
			}
		}

		/// <summary>
		/// The ETA for completion of the first item, in turns.
		/// </summary>
		public int? FirstItemEta
		{
			get
			{
				if (!Orders.Any())
					return null;
				var remainingCost = Orders[0].Template.Cost - (Orders[0].Item == null ? new Resources() : Orders[0].Item.ConstructionProgress);
				return (int)Math.Ceiling(remainingCost.Max(kvp => (double)kvp.Value / (double)Rate[kvp.Key]));
			}
		}

		/// <summary>
		/// The ETA for completion of the whole queue, in turns.
		/// </summary>
		public int? Eta
		{
			get
			{
				if (!Orders.Any())
					return null;
				var remainingCost = Orders.Select(o => o.Template.Cost - (o.Item == null ? new Resources() : o.Item.ConstructionProgress)).Aggregate((r1, r2) => r1 + r2);
				return (int)Math.Ceiling(remainingCost.Max(kvp => (double)kvp.Value / (double)Rate[kvp.Key]));
			}
		}
	}
}
