using Server.Common.Models;

namespace Server.Common;

public class IdentifierStrategy(
	Func<string, CancellationToken, Task<Node?>> getNodeData,
	Func<string, CancellationToken, Task<IEnumerable<ConnectionIdentifier>>> getConnectedNodeIdentifiers)
{
	public readonly Func<string, CancellationToken, Task<Node?>> GetNodeData = getNodeData;

	public readonly Func<string, CancellationToken, Task<IEnumerable<ConnectionIdentifier>>>
		GetConnectedNodeIdentifiers =
			getConnectedNodeIdentifiers;
}
