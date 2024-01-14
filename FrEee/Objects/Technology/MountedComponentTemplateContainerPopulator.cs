using System.Linq;
using FrEee.Interfaces;
using FrEee.Objects.Space;
using FrEee.Utility;
namespace FrEee.Objects.Technology;

/// <summary>
/// Populates the <see cref="MountedComponentTemplate.Container"/> property.
/// </summary>
public class MountedComponentTemplateContainerPopulator
	: IPopulator
{
	public object? Populate(object? context)
	{
		if (context is MountedComponentTemplate mct)
		{
			return Galaxy.Current.Referrables.OfType<IDesign>().SingleOrDefault(q => q.Components.Contains(mct));
		}
		else
		{
			return null;
		}
	}
}
