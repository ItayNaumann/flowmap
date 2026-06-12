namespace Server.Common.Models;

public record NodeConnections(IEnumerable<Node> Above, IEnumerable<Node> Below)
{
	public IEnumerable<Node> Above { get; } = Above;
	public IEnumerable<Node> Below { get; } = Below;
}
