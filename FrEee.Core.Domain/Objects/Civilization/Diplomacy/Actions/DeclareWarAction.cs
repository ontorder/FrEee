﻿using FrEee.Extensions;

namespace FrEee.Objects.Civilization.Diplomacy.Actions;

/// <summary>
/// Declares war on the target empire.
/// </summary>
public class DeclareWarAction : DiplomaticAction
{
    public DeclareWarAction(Empire target)
        : base(target)
    {
    }

    public override string Description
    {
        get { return "Declare War"; }
    }

    public override void Execute()
    {
        foreach (var clause in Executor.GetTreaty(Target))
            clause.Dispose();
        // TODO - some sort of formal war state
        Executor.Log.Add(Target.CreateLogMessage("We have declared war on the " + Target + ".", LogMessages.LogMessageType.Diplomacy));
        Target.Log.Add(Executor.CreateLogMessage("The " + Executor + " has declared war on us!", LogMessages.LogMessageType.Diplomacy));
    }
}