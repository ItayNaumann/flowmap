namespace Server.Common.ObjectAnalyzer.Json.Models;

public record CheckItem(string NodePath)
{
	public IEnumerable<string>? AllowableData { get; init; }
}
