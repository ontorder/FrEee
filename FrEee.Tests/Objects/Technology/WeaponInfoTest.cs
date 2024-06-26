﻿using FrEee.Processes.Combat;
using FrEee.Modding;
using NUnit.Framework;
using System.Linq;
using FrEee.Objects.GameState;

namespace FrEee.Tests.Objects.Technology;

/// <summary>
/// Tests weapon info.
/// </summary>
public class WeaponInfoTest
{
	private static Galaxy gal = new Galaxy();

	private static Mod mod;

	[OneTimeSetUp]
	public static void ClassInit()
	{
		mod = Mod.Load("WeaponInfoTest");
	}

	/// <summary>
	/// Tests formula damage values.
	/// </summary>
	[Test]
	public void FormulaDamage()
	{
		var ct = mod.ComponentTemplates.Single(x => x.Name == "Formula Weapon");
		var comp = ct.Instantiate();
		Assert.AreEqual(3, ct.WeaponInfo.MinRange.Value);
		Assert.AreEqual(5, ct.WeaponInfo.MaxRange.Value);
		Assert.AreEqual(0, ct.WeaponInfo.GetDamage(new Shot(null, comp, null, 2)));
		Assert.AreEqual(20, ct.WeaponInfo.GetDamage(new Shot(null, comp, null, 4)));
		Assert.AreEqual(0, ct.WeaponInfo.GetDamage(new Shot(null, comp, null, 6)));
	}

	/// <summary>
	/// Tests non-formula damage values.
	/// </summary>
	[Test]
	public void NonFormulaDamage()
	{
		var ct = mod.ComponentTemplates.Single(x => x.Name == "Non-Formula Weapon");
		var comp = ct.Instantiate();
		Assert.AreEqual(3, ct.WeaponInfo.MinRange.Value);
		Assert.AreEqual(5, ct.WeaponInfo.MaxRange.Value);
		Assert.AreEqual(0, ct.WeaponInfo.GetDamage(new Shot(null, comp, null, 2)));
		Assert.AreEqual(20, ct.WeaponInfo.GetDamage(new Shot(null, comp, null, 4)));
		Assert.AreEqual(0, ct.WeaponInfo.GetDamage(new Shot(null, comp, null, 6)));
	}
}