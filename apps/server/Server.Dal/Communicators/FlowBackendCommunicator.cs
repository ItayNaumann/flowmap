using Server.Common.Models;

namespace Server.DAL.Communicators;

public class FlowBackendCommunicator
{
	public Node GetPackageData(string packageId)
	{
		throw new NotImplementedException();
	}

	public IEnumerable<string> GetPackagesIdentifiersInsidePackage(string pacakgeId)
	{
		throw new NotImplementedException();
	}

	public IEnumerable<string> GetPackagesIdentifiersUsingPackage(string packageId)
	{
		throw new NotImplementedException();
	}

	// Could have another one splitted to id and names
	public IEnumerable<AutoCompleteOption> GetPackageAutoCompleteOptions(string search)
	{
		throw new NotImplementedException();
	}
}
