using Server.Common.Models;

namespace Server.Common;

public record IdentifierStrategy(
	Func<string, CancellationToken, Task<Node?>> GetNodeData,
	Func<string, CancellationToken, Task<IEnumerable<ConnectionIdentifier>>> GetConnectedNodeIdentifiers);
