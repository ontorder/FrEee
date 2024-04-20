using FrEee.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using FrEee.Objects.GameState;

namespace FrEee.Modding;

/// <summary>
/// A record which takes parameters and is capable of generating multiple records using formulas.
/// </summary>
public sealed class MetaRecord : Record, ITemplate<IEnumerable<Record>>
{
	public MetaRecord()
		: base()
	{ }

	/// <summary>
	/// Creates a record by parsing some string data.
	/// </summary>
	/// <param name="lines"></param>
	public MetaRecord(IEnumerable<string> lines)
		: base(lines)
	{ }

	public string Filename { get; set; }

	public new IEnumerable<MetaRecordParameter> Parameters
	{
		get
		{
			string expecting = "Name";
			var recordPar = new MetaRecordParameter();
			var parameterFields = Fields.Where(f => f.Name.StartsWith("Parameter "));
			foreach (var foundField in parameterFields)
			{
				var what = foundField.Name.Substring("Parameter ".Length);
				switch (expecting)
				{
					case "Name":
						if (what == "Name")
						{
							recordPar.Name = foundField.Value;
							expecting = "Minimum";
						}
						else
						{
							Mod.Errors.Add(new DataParsingException($"Expected Parameter Name, found Parameter {what}.", Filename, this, foundField));
							yield break;
						}
						break;

					case "Minimum":
						if (what == "Minimum")
						{
							recordPar.Minimum = foundField.CreateFormula<int>(recordPar);
							expecting = "Maximum";
						}
						else if (what == "Maximum")
						{
							// it's OK, just set minimum to 1
							recordPar.Minimum = 1;
							recordPar.Maximum = foundField.CreateFormula<int>(recordPar);
							expecting = "Name";
							yield return recordPar;
							recordPar = new MetaRecordParameter();
						}
						else
						{
							Mod.Errors.Add(new DataParsingException($"Expected Parameter Minimum or Parameter Maximum, found Parameter {what}.", Filename, this, foundField));
							yield break;
						}
						break;

					case "Maximum":
						if (what == "Maximum")
						{
							recordPar.Maximum = foundField.CreateFormula<int>(recordPar);
							expecting = "Name";
							yield return recordPar;
							recordPar = new MetaRecordParameter();
						}
						else
						{
							Mod.Errors.Add(new DataParsingException("Expected Parameter Maximum, found Parameter " + what + ".", Filename, this, foundField));
							yield break;
						}
						break;
				}
			}
			if (expecting != "Name")
				Mod.Errors.Add(new DataParsingException("Expected Parameter Minimum or Parameter Maximum but no more parameter fields were found.", Filename, this));
		}
	}

	public IEnumerable<Record> Instantiate()
	{
		var parms = Parameters.ToArray();
		if (parms.Length == 0)
		{
			var rec = new Record();
			foreach (var f in Fields)
				rec.Fields.Add(f.Copy());
			yield return rec;
			yield break;
		}
		IList<IDictionary<string, int>>? permutations = null;
		foreach (var parm in parms)
			permutations = CreatePermutations(parm, permutations);
		foreach (var permutation in permutations)
		{
			var rec = new Record();
			rec.Parameters = new Dictionary<string, object>();
			foreach (var kvp in permutation)
				rec.Parameters.Add(kvp.Key, kvp.Value);

			foreach (var fieldItem in Fields)
			{
				if (fieldItem.Name.StartsWith("Parameter "))
					continue;

				if (fieldItem.Value.StartsWith("=="))
				{
					// dynamic formula field will be evaluated later
					rec.Fields.Add(fieldItem.Copy());
				}
				else if (fieldItem.Value.StartsWith('='))
				{
					// static formula field
					rec.Fields.Add(CreateStaticFormulaField(fieldItem, permutation));
				}
				else if (fieldItem.Value.Contains('{') && fieldItem.Value.Substring(fieldItem.Value.IndexOf('{')).Contains('}'))
				{
					// string interpolation formula
					var isDynamic = fieldItem.Value.Contains("{{") && fieldItem.Value.Substring(fieldItem.Value.IndexOf("{{")).Contains("}}");
					var replacedText = fieldItem.Value;
					if (isDynamic)
						replacedText = $"=='{replacedText}'"; // make it a string
					else
						replacedText = $"='{replacedText}'"; // make it a string
					replacedText = replacedText
						.Replace("{{", "' + str(")
						.Replace("}}", ") + '")
						.Replace("{", "' + str(")
						.Replace("}", ") + '");
					fieldItem.Value = replacedText;
					if (isDynamic)
						throw new NotImplementedException("Dynamic inline formulas are not yet supported.");
					else
						rec.Fields.Add(CreateStaticFormulaField(fieldItem, permutation));
				}
				else
				{
					// plain old field
					rec.Fields.Add(fieldItem.Copy());
				}
			}
			yield return rec;
		}
	}

	private static Field CreateStaticFormulaField(Field f, IDictionary<string, int> args)
	{
		var variables = args.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
		var result = PythonScriptEngine.EvaluateExpression<IConvertible>(f.Value.TrimStart('='), variables).ToStringInvariant();
		var field = new Field();
		field.Name = f.Name;
		field.Value = result;
		return field;
	}

	private static IList<IDictionary<string, int>> CreatePermutations(MetaRecordParameter recParam, IList<IDictionary<string, int>>? prevState = null)
	{
		if (prevState == null || prevState.Count == 0)
		{
			return Enumerable.Range(start: recParam.Minimum, count: recParam.Maximum - recParam.Minimum + 1)
				.Select(i => (IDictionary<string, int>)new Dictionary<string, int> { { recParam.Name, i } })
				.ToList();
		}

		var retDics = new List<IDictionary<string, int>>();
		foreach (var prevDict in prevState)
		{
			for (int i = recParam.Minimum; i <= recParam.Maximum; i++)
			{
				var newdict = new Dictionary<string, int>();
				foreach (var prevKvp in prevDict)
					newdict.Add(prevKvp.Key, prevKvp.Value);
				newdict.Add(recParam.Name, i);
				retDics.Add(newdict);
			}
		}
		return retDics;
	}
}
