namespace Server.Common.Models;

public record AutoCompleteOption(string Identifier, NodeType Type)
{
	public string? DisplayName { get; init; }
}
