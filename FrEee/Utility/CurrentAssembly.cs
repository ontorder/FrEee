using System.IO;
using System.Reflection;

namespace FrEee.Utility;

public static class CurrentAssembly
{
	public static readonly string Location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
}
