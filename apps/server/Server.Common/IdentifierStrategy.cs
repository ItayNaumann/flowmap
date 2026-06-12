using Server.Common.Models;

namespace Server.Common;

public class IdentifierStrategy(
	Func<string, Task<Node?>> getNodeData,
	Func<string, Task<IEnumerable<ConnectionIdentifier>>> getConnectedNodeIdentifiers)
{
	public readonly Func<string, Task<Node?>> GetNodeData = getNodeData;

	public readonly Func<string, Task<IEnumerable<ConnectionIdentifier>>> GetConnectedNodeIdentifiers =
		getConnectedNodeIdentifiers;
}
