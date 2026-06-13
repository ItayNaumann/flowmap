using Server.Common.Models;

namespace Server.Communicators;

public class ViewBackendCommunicator
{
	public Node GetDashboardData(string id)
	{
		throw new NotImplementedException();
	}

	public IEnumerable<string> GetDashboardConnectionsIdentifiers(string dashboardId)
	{
		throw new NotImplementedException();
	}

	public IEnumerable<string> GetPackageConnectedDashboardsIdentifiers(string packageId)
	{
		throw new NotImplementedException();
	}

	// Could have another one splitted to isd and names
	public IEnumerable<AutoCompleteOption> GetDashboardAutoCompleteOptions(string search,
		CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}
}
