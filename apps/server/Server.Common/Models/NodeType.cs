using System.Text.RegularExpressions;

namespace Server.Common.Models;

public enum NodeType
{
	Package,
	Dashboard,
	GiliTable
}

public static class NodeTypeExtensions
{
	public static bool IsIdPatternMatch(this NodeType type, string input)
	{
		string pattern = type switch
		{
			NodeType.Package => @"^[1-9]\d*$",
			NodeType.Dashboard => @"^[0-9a-f]{14}$",
			NodeType.GiliTable => @"[\S]*",
			_ => throw new ArgumentOutOfRangeException(nameof(type), "Unknown content type")
		};

		return Regex.IsMatch(input, pattern);
	}
}
