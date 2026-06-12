using Server.Common.Models;

namespace Server.Common;

public class IdentifierStrategy(
	Func<string, Node> getNodeData,
	Func<string, IEnumerable<string>> getConnectedNodeIdentifiers)
{
	public Func<string, Node> GetNodeData = getNodeData;
	public Func<string, IEnumerable<string>> GetConnectedNodeIdentifiers = getConnectedNodeIdentifiers;
}
