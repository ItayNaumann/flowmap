using Server.Common.Models;

namespace Server.Communicators;

public class FlowBackendCommunicator
{
	public Node GetPackageData(string packageId, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public IEnumerable<string> GetPackagesIdentifiersInsidePackage(string pacakgeId,
		CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public IEnumerable<string> GetPackagesIdentifiersUsingPackage(string packageId,
		CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	// Could have another one splitted to id and names
	public IEnumerable<AutoCompleteOption> GetPackageAutoCompleteOptions(string search,
		CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}
}
