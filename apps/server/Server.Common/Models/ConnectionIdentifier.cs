namespace Server.Common.Models;

public record ConnectionIdentifier(string Id, NodeType NodeType)
{
	public string Id = Id;
	public NodeType NodeType = NodeType;
}
