using Server.Common;
using Server.Common.Models;

namespace Server.Services;

public class NodeService
{
	public async Task<IEnumerable<(Node?, NodeConnections?)>> GetUnknownNode(
		string id,
		IServiceProvider sp)
	{
		IEnumerable<NodeType> potentialNodeTypes = await DetermineNodeTypeAsync(id);

		IEnumerable<NodeType> nodeTypes = potentialNodeTypes.ToList();
		var potentialNodes = nodeTypes.Select(nodeType => GetNode(id, nodeType, sp)).ToList();

		await Task.WhenAll(potentialNodes);

		var nodesResult = potentialNodes.Select(input => input.Result).ToList();


		return nodesResult;
	}

	public async Task<(Node?, NodeConnections?)> GetNode(string id, NodeType nodeType,
		IServiceProvider sp)
	{
		var nodeData = GetNodeData(id, nodeType, sp);
		var nodeConnections = GetNodeConnections(id, nodeType, sp);

		await Task.WhenAll(nodeData, nodeConnections);

		return (nodeData.Result, nodeConnections.Result);
	}


	private async Task<Node?> GetNodeData(string id, NodeType nodeType, IServiceProvider sp)
	{
		var strategy = sp.GetKeyedService<IdentifierStrategy>(nodeType);
		if (strategy == null) return null;

		return await strategy.GetNodeData(id, CancellationToken.None);
	}

	public async Task<NodeConnections?> GetNodeConnections(string id, NodeType nodeType, IServiceProvider sp)
	{
		var strategy = sp.GetKeyedService<IdentifierStrategy>(nodeType);
		if (strategy == null) return null;

		var connectionIdentifiers = await strategy.GetConnectedNodeIdentifiers(id, CancellationToken.None);

		var aboveConnections = new List<Task<Node?>>();
		var belowConnections = new List<Task<Node?>>();

		foreach (var connectionIdentifier in connectionIdentifiers)
		{
			if (connectionIdentifier.IsAbove)
			{
				aboveConnections.Add(GetNodeData(id, nodeType, sp));
			}
			else
			{
				belowConnections.Add(GetNodeData(id, nodeType, sp));
			}
		}

		await Task.WhenAll(aboveConnections.Concat(belowConnections));

		IEnumerable<Node> nonNullAbove = aboveConnections
			.Select(t => t.Result)
			.Where(node => node != null)
			.ToList()!;

		IEnumerable<Node> nonNullBelow = belowConnections
			.Select(t => t.Result)
			.Where(node => node != null)
			.ToList()!;

		return new NodeConnections(nonNullAbove, nonNullBelow);
	}


	private static Task<IEnumerable<NodeType>> DetermineNodeTypeAsync(string id)
	{
		var possibleTypes = Enum.GetValues<NodeType>().Where(nodeType => nodeType.IsIdPatternMatch(id)).ToList();

		return Task.FromResult<IEnumerable<NodeType>>(possibleTypes);
	}
}
