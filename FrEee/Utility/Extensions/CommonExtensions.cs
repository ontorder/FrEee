using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using AutoMapper;
using FrEee.Game.Interfaces;
using FrEee.Game.Objects.Abilities;
using FrEee.Game.Objects.Space;
using FrEee.Modding;
using FrEee.Game.Objects.Vehicles;
using System.Text;
using System.IO;
using FrEee.Game.Objects.LogMessages;
using FrEee.Game.Objects.Civilization;
using System.Reflection;
using System.Collections;
using FrEee.Game.Objects.Commands;
using System.Drawing.Imaging;

namespace FrEee.Utility.Extensions
{
	public static class CommonExtensions
	{
		/// <summary>
		/// Shallow copies an object.
		/// </summary>
		/// <typeparam name="T">The type of object to copy.</typeparam>
		/// <param name="obj">The object to copy.</param>
		/// <returns>The copy.</returns>
		public static T Copy<T>(this T obj) where T : new()
		{
			if (obj == null)
				return default(T);
			var dest = new T();
			obj.CopyTo(dest);
			return (T)dest;
		}

		/// <summary>
		/// Shallow copies an object's data to another object.
		/// </summary>
		/// <typeparam name="T">The type of object to copy.</typeparam>
		/// <param name="src">The object to copy.</param>
		/// <param name="dest">The object to copy the source object's data to.</param>
		public static void CopyTo<T>(this T src, T dest)
		{
			if (!mappedTypes.Contains(typeof(T)))
			{
				mappedTypes.Add(typeof(T));
				Mapper.CreateMap<T, T>();
			}
			Mapper.Map(src, dest);
		}

		private static List<Type> mappedTypes = new List<Type>();

		/// <summary>
		/// Finds the largest space object out of a group of space objects.
		/// Stars are the largest space objects, followed by planets, asteroid fields, storms, fleets, ships/bases, and finally unit groups.
		/// Within a category, space objects are sorted by stellar size or tonnage as appropriate.
		/// </summary>
		/// <param name="objects">The group of space objects.</param>
		/// <returns>The largest space object.</returns>
		public static ISpaceObject Largest(this IEnumerable<ISpaceObject> objects)
		{
			if (objects.OfType<Star>().Any())
			{
				return objects.OfType<Star>().OrderByDescending(obj => obj.StellarSize).First();
			}
			if (objects.OfType<Planet>().Any())
			{
				return objects.OfType<Planet>().OrderByDescending(obj => obj.StellarSize).First();
			}
			if (objects.OfType<AsteroidField>().Any())
			{
				return objects.OfType<AsteroidField>().OrderByDescending(obj => obj.StellarSize).First();
			}
			if (objects.OfType<Storm>().Any())
			{
				return objects.OfType<Storm>().OrderByDescending(obj => obj.StellarSize).First();
			}
			if (objects.OfType<WarpPoint>().Any())
			{
				return objects.OfType<WarpPoint>().OrderByDescending(obj => obj.StellarSize).First();
			}
			// TODO - fleets
			if (objects.OfType<AutonomousSpaceVehicle>().Any())
			{
				return objects.OfType<AutonomousSpaceVehicle>().OrderByDescending(obj => obj.Design.Hull.Size).First();
			}
			// TODO - unit groups
			return null;
		}

		/// <summary>
		/// Determines if an object has a specified ability.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="abilityName"></param>
		/// <returns></returns>
		public static bool HasAbility(this IAbilityObject obj, string abilityName)
		{
			return obj.Abilities.Any(abil => abil.Name == abilityName);
		}

		/// <summary>
		/// Stacks any abilities of the same type according to the current mod's stacking rules.
		/// Keeps the original abilities in a handy tree format under the stacked abilities
		/// so you can tell which abilities contributed to which stacked abilities.
		/// </summary>
		/// <param name="abilities"></param>
		/// <returns></returns>
		public static ILookup<Ability, Ability> StackToTree(this IEnumerable<Ability> abilities)
		{
			var stacked = new List<Tuple<Ability, Ability>>();
			foreach (var rule in Mod.Current.AbilityRules)
			{
				var lookup = rule.GroupAndStack(abilities);
				foreach (var group in lookup)
				{
					foreach (var abil in group)
						stacked.Add(Tuple.Create(group.Key, abil));
				}
			}
			foreach (var abil in abilities.Where(a =>!Mod.Current.AbilityRules.Any(r => r.Name == a.Name)))
				stacked.Add(Tuple.Create(abil, abil));
			return stacked.ToLookup(t => t.Item1, t => t.Item2);
		}

		public static IEnumerable<Ability> Stack(this IEnumerable<Ability> abilities)
		{
			return abilities.StackToTree().Select(g => g.Key);
		}

		public static IEnumerable<Ability> StackAbilities(this IEnumerable<IAbilityObject> objs)
		{
			return objs.SelectMany(obj => obj.Abilities).Stack();
		}

		public static ILookup<Ability, Ability> StackAbilitiesToTree(this IEnumerable<IAbilityObject> objs)
		{
			return objs.SelectMany(obj => obj.Abilities).StackToTree();
		}

		/// <summary>
		/// Adds SI prefixes to a value and rounds it off.
		/// e.g. 25000 becomes 25.00k
		/// </summary>
		/// <param name="value"></param>
		public static string ToUnitString(this long value, bool bForBillions = false, int sigfigs = 4)
		{
			if (Math.Abs(value) >= 1e12 * Math.Pow(10, sigfigs - 3))
			{
				var log = (int)Math.Floor(Math.Log10(value / 1e12));
				var decimals = sigfigs - 1 - log;
				return (value / 1e12).ToString("f" + decimals) + "T";
			}
			if (Math.Abs(value) >= 1e9 * Math.Pow(10, sigfigs - 3))
			{
				var log = (int)Math.Floor(Math.Log10(value / 1e9));
				var decimals = sigfigs - 1 - log;
				return (value / 1e9).ToString("f" + decimals) + (bForBillions ? "B" : "G");
			}
			if (Math.Abs(value) >= 1e6 * Math.Pow(10, sigfigs - 3))
			{
				var log = (int)Math.Floor(Math.Log10(value / 1e6));
				var decimals = sigfigs - 1 - log;
				return (value / 1e6).ToString("f" + decimals) + "M";
			}
			if (Math.Abs(value) >= 1e3 * Math.Pow(10, sigfigs - 3))
			{
				var log = (int)Math.Floor(Math.Log10(value / 1e3));
				var decimals = sigfigs - 1 - log;
				return (value / 1e3).ToString("f" + decimals) + "k";
			}
			return value.ToString();
		}

		/// <summary>
		/// Adds SI prefixes to a value and rounds it off.
		/// e.g. 25000 becomes 25.00k
		/// </summary>
		/// <param name="value"></param>
		public static string ToUnitString(this int value, bool bForBillions = false, int sigfigs = 4)
		{
			return ((long)value).ToUnitString(bForBillions, sigfigs);
		}

		/// <summary>
		/// Displays a number in kT, MT, etc.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string Kilotons(this long value)
		{
			if (value < 10000)
				return value + "kT";
			return (value * 1000).ToUnitString() + "T";
		}

		/// <summary>
		/// Displays a number in kT, MT, etc.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static string Kilotons(this int value)
		{
			return ((long)value).Kilotons();
		}


		/// <summary>
		/// Converts a turn number to a stardate.
		/// </summary>
		/// <param name="turnNumber"></param>
		/// <returns></returns>
		public static string ToStardate(this int turnNumber)
		{
			// TODO - moddable starting stardate?
			return ((turnNumber + 23999) / 10.0).ToString("0.0");
		}

		/// <summary>
		/// Picks a random element from a sequence.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="src"></param>
		/// <returns></returns>
		public static T PickRandom<T>(this IEnumerable<T> src)
		{
			if (!src.Any())
				return default(T);
			return src.ElementAt(RandomHelper.Next(src.Count()));
		}

		/// <summary>
		/// Picks a random element from a weighted sequence.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="src"></param>
		/// <returns></returns>
		public static T PickWeighted<T>(this IDictionary<T, int> src)
		{
			var total = src.Sum(kvp => kvp.Value);
			var num = RandomHelper.Next(total);
			int sofar = 0;
			foreach (var kvp in src)
			{
				sofar += kvp.Value;
				if (num < sofar)
					return kvp.Key;
			}
			return default(T); // nothing to pick...
		}

		/// <summary>
		/// Picks a random element from a weighted sequence.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="src"></param>
		/// <returns></returns>
		public static T PickWeighted<T>(this IDictionary<T, long> src)
		{
			var total = src.Sum(kvp => kvp.Value);
			var num = RandomHelper.Next(total);
			long sofar = 0;
			foreach (var kvp in src)
			{
				sofar += kvp.Value;
				if (num < sofar)
					return kvp.Key;
			}
			return default(T); // nothing to pick...
		}

		/// <summary>
		/// Picks a random element from a weighted sequence.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="src"></param>
		/// <returns></returns>
		public static T PickWeighted<T>(this IDictionary<T, double> src)
		{
			var total = src.Sum(kvp => kvp.Value);
			var num = RandomHelper.Next(total);
			double sofar = 0;
			foreach (var kvp in src)
			{
				sofar += kvp.Value;
				if (num < sofar)
					return kvp.Key;
			}
			return default(T); // nothing to pick...
		}
		
		/// <summary>
		/// Orders elements randomly.
		/// </summary>
		/// <param name="src"></param>
		/// <returns></returns>
		public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> src)
		{
			return src.OrderBy(t => RandomHelper.Next(int.MaxValue));
		}

		public static T MinOrDefault<T>(this IEnumerable<T> stuff)
		{
			if (!stuff.Any())
				return default(T);
			return stuff.Min();
		}

		public static TProp MinOrDefault<TItem, TProp>(this IEnumerable<TItem> stuff, Func<TItem, TProp> selector)
		{
			return stuff.Select(selector).MinOrDefault();
		}

		public static T MaxOrDefault<T>(this IEnumerable<T> stuff)
		{
			if (!stuff.Any())
				return default(T);
			return stuff.Max();
		}

		public static TProp MaxOrDefault<TItem, TProp>(this IEnumerable<TItem> stuff, Func<TItem, TProp> selector)
		{
			return stuff.Select(selector).MaxOrDefault();
		}

		public static T Find<T>(this IEnumerable<T> stuff, string name) where T : INamed
		{
			return stuff.FirstOrDefault(item => item.Name == name);
		}

		/// <summary>
		/// Gets the points on the border of a rectangle.
		/// </summary>
		/// <param name="r"></param>
		/// <returns></returns>
		public static IEnumerable<Point> GetBorderPoints(this Rectangle r)
		{
			for (var x = r.Left; x <= r.Right; x++)
			{
				if (x == r.Left || x == r.Right)
				{
					// get left and right sides
					for (var y = r.Top; y <= r.Bottom; y++)
						yield return new Point(x, y);
				}
				else
				{
					// just get top and bottom
					yield return new Point(x, r.Top);
					if (r.Top != r.Bottom)
						yield return new Point(x, r.Bottom);
				}
			}
		}

		/// <summary>
		/// Gets points in the interior of a rectangle.
		/// </summary>
		/// <param name="r"></param>
		/// <returns></returns>
		public static IEnumerable<Point> GetInteriorPoints(this Rectangle r)
		{
			for (var x = r.Left + 1; x < r.Right; x++)
			{
				for (var y = r.Top + 1; y < r.Bottom; y++)
					yield return new Point(x, y);
			}
		}

		/// <summary>
		/// Gets points both on the border and in the interior of a rectangle.
		/// </summary>
		/// <param name="r"></param>
		/// <returns></returns>
		public static IEnumerable<Point> GetAllPoints(this Rectangle r)
		{
			for (var x = r.Left; x <= r.Right; x++)
			{
				for (var y = r.Top; y <= r.Bottom; y++)
					yield return new Point(x, y);
			}
		}

		/// <summary>
		/// Computes the Manhattan (4-way grid) distance between two points.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static int ManhattanDistance(this Point p, Point target)
		{
			return Math.Abs(target.X - p.X) + Math.Abs(target.Y - p.Y);
		}

		/// <summary>
		/// Computes the distance between two points along a grid with eight-way movement.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static int EightWayDistance(this Point p, Point target)
		{
			var dx = Math.Abs(target.X - p.X);
			var dy = Math.Abs(target.Y - p.Y);
			return Math.Max(dx, dy);
		}

		/// <summary>
		/// Computes the angle from one point to the other.
		/// Zero degrees is north, and positive is clockwise.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static double AngleTo(this Point p, Point target)
		{
			return Math.Atan2(target.X - p.X, target.Y - p.Y) * 180d / Math.PI;
		}

		/// <summary>
		/// Computes the angle from one point to the other.
		/// Zero degrees is north, and positive is clockwise.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static double AngleTo(this PointF p, PointF target)
		{
			return Math.Atan2(target.X - p.X, p.Y - target.Y) * 180d / Math.PI;
		}

		/// <summary>
		/// Removes points within a certain Manhattan distance of a certain point.
		/// </summary>
		/// <param name="points">The points to start with.</param>
		/// <param name="center">The point to block out.</param>
		/// <param name="distance">The distance to block out from the center.</param>
		/// <returns>The points that are left.</returns>
		public static IEnumerable<Point> BlockOut(this IEnumerable<Point> points, Point center, int distance)
		{
			foreach (var p in points)
			{
				if (center.ManhattanDistance(p) > distance)
					yield return p;
			}
		}

		/// <summary>
		/// Flattens groupings into a single sequence.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="lookup"></param>
		/// <returns></returns>
		public static IEnumerable<TValue> Flatten<TKey, TValue>(this IEnumerable<IGrouping<TKey, TValue>> lookup)
		{
			return lookup.SelectMany(g => g);
		}

		/// <summary>
		/// Flattens lookups into a single sequence.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="lookups"></param>
		/// <returns></returns>
		public static IEnumerable<TValue> Flatten<TKey, TValue>(this IEnumerable<ILookup<TKey, TValue>> lookups)
		{
			return lookups.SelectMany(g => g).Flatten();
		}

		/// <summary>
		/// "Squashes" a nested lookup into a collection of tuples.
		/// </summary>
		/// <typeparam name="TKey1"></typeparam>
		/// <typeparam name="TKey2"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="lookup"></param>
		/// <returns></returns>
		public static IEnumerable<Tuple<TKey1, TKey2, TValue>> Squash<TKey1, TKey2, TValue>(this ILookup<TKey1, ILookup<TKey2, TValue>> lookup)
		{
			foreach (var group1 in lookup)
			{
				foreach (var sublookup in group1)
				{
					foreach (var group2 in sublookup)
					{
						foreach (var item in group2)
							yield return Tuple.Create(group1.Key, group2.Key, item);
					}
				}
			}
		}

		/// <summary>
		/// Gets a capital letter from the English alphabet.
		/// </summary>
		/// <param name="i">1 to 26</param>
		/// <returns>A to Z</returns>
		/// <exception cref="ArgumentException">if i is not from 1 to 26</exception>
		public static char ToLetter(this int i)
		{
			if (i < 1 || i > 26)
				throw new ArgumentException("Only 26 letters in the alphabet, can't get letter #" + i + ".", "i");
			return (char)('A' + i - 1);
		}

		/// <summary>
		/// Gets a roman numeral.
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
		public static string ToRomanNumeral(this int i)
		{
			// do we already know this?
			if (!RomanNumeralCache.ContainsKey(i))
			{
				// get silly negative numbers and zeroes out of the way
				if (i < 0)
					RomanNumeralCache.Add(i, "-" + ToRomanNumeral(-i));
				else if (i == 0)
					RomanNumeralCache.Add(i, "");
				else
				{
					// scan the roman numeral parts list recursively
					foreach (var part in RomanNumeralParts.OrderByDescending(part => part.Item1))
					{
						if (i >= part.Item1)
						{
							RomanNumeralCache.Add(i, part.Item2 + (i - part.Item1).ToRomanNumeral());
							break;
						}
					}
				}
			}

			return RomanNumeralCache[i];
		}

		private static Tuple<int, string>[] RomanNumeralParts = new Tuple<int, string>[]
		{
			Tuple.Create(1000, "M"),
			Tuple.Create(900, "CM"),
			Tuple.Create(500, "D"),
			Tuple.Create(400, "CD"),
			Tuple.Create(100, "C"),
			Tuple.Create(90, "XC"),
			Tuple.Create(50, "L"),
			Tuple.Create(40, "XL"),
			Tuple.Create(10, "X"),
			Tuple.Create(9, "IX"),
			Tuple.Create(5, "V"),
			Tuple.Create(4, "IV"),
			Tuple.Create(1, "I"),
		};

		private static IDictionary<int, string> RomanNumeralCache = new Dictionary<int, string>();

		/// <summary>
		/// Determines if a string can be parsed as an integer.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static bool IsInt(this string s)
		{
			int i;
			return int.TryParse(s, out i);
		}

		/// <summary>
		/// Determines if a string can be parsed as a double.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="cultureCode">The LCID of the culture used to parse. Defaults to 127, which represents the invariant culture.</param>
		/// <returns></returns>
		public static bool IsDouble(this string s, int cultureCode = 127)
		{
			double d;
			return double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.GetCultureInfo(cultureCode), out d);
		}

		/// <summary>
		/// Determines if a string can be parsed as an boolean.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static bool IsBool(this string s)
		{
			bool b;
			return bool.TryParse(s, out b);
		}

		/// <summary>
		/// Parses a string as an integer. Returns 0 if it could not be parsed.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static int ToInt(this string s)
		{
			int i;
			int.TryParse(s, out i);
			return i;
		}

		/// <summary>
		/// Parses a string as a double. Returns 0 if it could not be parsed.
		/// </summary>
		/// <param name="s"></param>
		/// <param name="cultureCode">The LCID of the culture used to parse. Defaults to 127, which represents the invariant culture.</param>
		/// <returns></returns>
		public static double ToDouble(this string s, int cultureCode = 127)
		{
			double d;
			double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.GetCultureInfo(cultureCode), out d);
			return d;
		}

		/// <summary>
		/// Parses a string as a boolean. Returns false if it could not be parsed.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static bool ToBool(this string s)
		{
			bool b;
			bool.TryParse(s, out b);
			return b;
		}

		/// <summary>
		/// Gets an ability value.
		/// If the stacking rule in the mod is DoNotStack, an arbitrary matching ability will be chosen.
		/// If there are no values, null will be returned.
		/// </summary>
		/// <param name="name">The name of the ability.</param>
		/// <param name="obj">The object from which to get the value.</param>
		/// <param name="index">The ability value index (usually 1 or 2).</param>
		/// <param name="filter">A filter for the abilities. For instance, you might want to filter by the ability grouping rule's value.</param>
		/// <returns>The ability value.</returns>
		public static string GetAbilityValue(this IAbilityObject obj, string name, int index = 1, Func<Ability, bool> filter = null)
		{
			var abils = obj.Abilities.Where(a => a.Name == name && (filter == null || filter(a))).Stack();
			if (!abils.Any())
				return null;
			return abils.First().Values[index - 1];
		}

		public static string GetAbilityValue(this IEnumerable<IAbilityObject> objs, string name, int index = 1, Func<Ability, bool> filter = null)
		{
			var abils = objs.SelectMany(o => o.Abilities).Where(a => a.Name == name && (filter == null || filter(a))).Stack();
			if (!abils.Any())
				return null;
			return abils.First().Values[index - 1];
		}

		/// <summary>
		/// Copies an image and draws planet population bars on it.
		/// </summary>
		/// <param name="image">The image.</param>
		/// <param name="planet">The planet whose population bars should be drawn.</param>
		/// <returns>The copied image with the population bars.</returns>
		public static Image DrawPopulationBars(this Image image, Planet planet)
		{
			var img2 = (Image)image.Clone();
			planet.DrawPopulationBars(img2);
			return img2;
		}

		/// <summary>
		/// Resizes an image. The image should be square.
		/// </summary>
		/// <param name="image"></param>
		/// <param name="size"></param>
		/// <returns></returns>
		public static Image Resize(this Image image, int size)
		{
			if (image == null)
				return null;
			var result = new Bitmap(size, size, PixelFormat.Format32bppArgb);
			var g = Graphics.FromImage(result);
			g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
			g.DrawImage(image, 0, 0, size, size);
			return result;
		}

		/// <summary>
		/// Adds up a bunch of resources.
		/// </summary>
		/// <param name="resources"></param>
		/// <returns></returns>
		public static ResourceQuantity Sum(this IEnumerable<ResourceQuantity> resources)
		{
			if (!resources.Any())
				return new ResourceQuantity();
			return resources.Aggregate((r1, r2) => r1 + r2);
		}

		/// <summary>
		/// Adds up a bunch of resources.
		/// </summary>
		/// <param name="resources"></param>
		/// <returns></returns>
		public static ResourceQuantity Sum<T>(this IEnumerable<T> stuff, Func<T, ResourceQuantity> selector)
		{
			return stuff.Select(item => selector(item)).Sum();
		}

		public static IEnumerable<T> OnlyLatest<T>(this IEnumerable<T> stuff, Func<T, string> familySelector)
			where T : class
		{
			string family = null;
			T latest = null;
			foreach (var t in stuff)
			{
				if (family == null)
				{
					// first item
					latest = t;
					family = familySelector(t);
				}
				else if (family == familySelector(t))
				{
					// same family
					latest = t;
				}
				else
				{
					// different family
					yield return latest;
					latest = t;
					family = familySelector(t);
				}
			}
			if (stuff.Any())
				yield return stuff.Last();
		}

		/// <summary>
		/// Finds the sector containing a space object.
		/// </summary>
		/// <param name="sobj"></param>
		/// <returns></returns>
		public static Sector FindSector(this ISpaceObject sobj)
		{
			var results = Galaxy.Current.FindSpaceObjects<ISpaceObject>(s => s == sobj).Squash();
			if (!results.Any())
				return null;
			return results.First().Item1.Item.GetSector(results.First().Item2);
		}

		/// <summary>
		/// Finds the star system containing a space object.
		/// </summary>
		/// <param name="sobj"></param>
		/// <returns></returns>
		public static StarSystem FindStarSystem(this ISpaceObject sobj)
		{
			var results = Galaxy.Current.FindSpaceObjects<ISpaceObject>(s => s == sobj).Squash();
			if (!results.Any())
				return null;
			return results.First().Item1.Item;
		}

		/// <summary>
		/// Finds the coordinates of a space object within its star system.
		/// </summary>
		/// <param name="sobj"></param>
		/// <returns></returns>
		public static Point FindCoordinates(this ISpaceObject sobj)
		{
			return sobj.FindStarSystem().FindCoordinates(sobj);
		}

		/// <summary>
		/// Reads characters until the specified character is found or end of stream.
		/// Returns all characters read except the specified character.
		/// </summary>
		/// <param name="r"></param>
		/// <param name="c"></param>
		/// <returns></returns>
		public static string ReadTo(this TextReader r, char c, StringBuilder log)
		{
			var sb = new StringBuilder();
			int data = 0;
			do
			{
				data = r.Read();
				if (data > 0 && data != (int)c)
				{
					sb.Append((char)data);
					log.Append((char)data);
				}
			} while (data > 0 && data != (int)c);
			if (data == c)
				log.Append(c);
			return sb.ToString();
		}

		public static IEnumerable<T> Except<T>(this IEnumerable<T> src, T badguy)
		{
			return src.Except(new T[] { badguy });
		}

		public static Reference<T> Reference<T>(this T t) where T : IReferrable
		{
			return new Reference<T>(t);
		}

		public static PictorialLogMessage<T> CreateLogMessage<T>(this T context, string text, int? turnNumber = null)
			where T : IPictorial
		{
			if (turnNumber == null)
				return new PictorialLogMessage<T>(text, context);
			else
				return new PictorialLogMessage<T>(text, turnNumber.Value, context);
		}

		/// <summary>
		/// Returns the elements of a sequence that have the maximum of some selected value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TCompared"></typeparam>
		/// <param name="src"></param>
		/// <param name="getter"></param>
		/// <returns></returns>
		public static IEnumerable<T> WithMax<T, TCompared>(this IEnumerable<T> src, Func<T, TCompared> selector)
		{
			var list = src.Select(item => new { Item = item, Value = selector(item) });
			if (!list.Any())
				return Enumerable.Empty<T>();
			var max = list.Max(x => x.Value);
			return list.Where(x => x.Value.SafeEquals(max)).Select(x => x.Item);
		}

		/// <summary>
		/// Returns the elements of a sequence that have the minimum of some selected value.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="TCompared"></typeparam>
		/// <param name="src"></param>
		/// <param name="getter"></param>
		/// <returns></returns>
		public static IEnumerable<T> WithMin<T, TCompared>(this IEnumerable<T> src, Func<T, TCompared> selector)
		{
			var list = src.Select(item => new { Item = item, Value = selector(item) });
			if (!list.Any())
				return Enumerable.Empty<T>();
			var min = list.Min(x => x.Value);
			return list.Where(x => x.Value.SafeEquals(min)).Select(x => x.Item);
		}

		/// <summary>
		/// Is this type safe to pass from the client to the server?
		/// Primitives, strings, points and colors are client safe.
		/// So are types implementing IPromotable.
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static bool IsClientSafe(this Type t)
		{
			return
				t.IsPrimitive ||
				t == typeof(string) ||
				t == typeof(Point) ||
				t == typeof(Color) ||
				typeof(IEnumerable<object>).IsAssignableFrom(t) ||
				typeof(IEnumerable).IsAssignableFrom(t) || 
				typeof(IPromotable).IsAssignableFrom(t) ||
				t.BaseType != null && t.BaseType.IsClientSafe() ||
				t.GetInterfaces().Any(i => i.IsClientSafe());
		}

		public static int IndexOf<T>(this IEnumerable<T> haystack, T needle)
		{
			int i = 0;
			foreach (var item in haystack)
			{
				if (item.Equals(needle))
					return i;
				i++;
			}
			return -1;
		}

		/// <summary>
		/// Checks a command to make sure it doesn't contain any objects that are not client safe.
		/// </summary>
		/// <param name="cmd"></param>
		public static void CheckForClientSafety(this ICommand cmd)
		{
			var vals = cmd.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(f => !f.GetCustomAttributes(true).OfType<DoNotSerializeAttribute>().Any() && f.GetGetMethod(true) != null && f.GetSetMethod(true) != null).Select(prop => new { Name = prop.Name, Value = prop.GetValue(cmd, new object[0]) });
			var badVals = vals.Where(val => val.Value != null && !val.Value.GetType().IsClientSafe());
			if (badVals.Any())
				throw new Exception(cmd + " contained a non-client-safe type " + badVals.First().Value.GetType() + " in property " + badVals.First().Name);
		}

		/// <summary>
		/// Logs an exception in errorlog.txt. Overwrites the old errorlog.txt.
		/// </summary>
		/// <param name="ex"></param>
		public static void Log(this Exception ex)
		{
			var sw = new StreamWriter("errorlog.txt");
			sw.WriteLine(ex.GetType().Name + " occurred at " + DateTime.Now + ":");
			sw.WriteLine(ex.ToString());
			sw.Close();
		}

		/// <summary>
		/// Is this order a new order added this turn, or one the server already knows about?
		/// </summary>
		/// <param name="order"></param>
		/// <returns></returns>
		public static bool IsNew<T>(this IOrder<T> order) where T : IOrderable
		{
			return Galaxy.Current.Referrables.OfType<AddOrderCommand<T>>().Where(cmd => cmd.Order == order).Any();
		}

		/// <summary>
		/// Equals method that doesn't throw an exception when objects are null.
		/// Null is not equal to anything else, except other nulls.
		/// </summary>
		/// <param name="o1"></param>
		/// <param name="o2"></param>
		/// <returns></returns>
		public static bool SafeEquals(this object o1, object o2)
		{
			if (o1 == null && o2 == null)
				return true;
			if (o1 == null || o2 == null)
				return false;
			return o1.Equals(o2);
		}

		/// <summary>
		/// Gets a property value from an object using reflection.
		/// </summary>
		/// <param name="o"></param>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public static object GetPropertyValue(this object o, string propertyName)
		{
			return o.GetType().GetProperty(propertyName).GetValue(o, new object[0]);
		}

		/// <summary>
		/// Sets a property value on an object using reflection.
		/// </summary>
		/// <param name="o"></param>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public static void SetPropertyValue(this object o, string propertyName, object value)
		{
			o.GetType().GetProperty(propertyName).SetValue(o, value, new object[0]);
		}

		/// <summary>
		/// Tests if an object is null.
		/// Useful for writing == operators that don't infinitely recurse.
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public static bool IsNull(this object o)
		{
			return o == null;
		}

		public static void IssueOrder<T>(this T obj, IOrder<T> order) where T : IOrderable
		{
			if (obj.Owner != Empire.Current)
				throw new Exception("Cannot issue orders to another empire's objects.");
			Empire.Current.IssueOrder(obj, order);
		}
	}
}