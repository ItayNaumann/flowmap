using Server.Common.Models;

namespace Server.Communicators;

public class GiliApiCommunicator
{
	// Can be both display and unique name
	public Node GetGiliTableByName(string giliTableName, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public IEnumerable<AutoCompleteOption> GetPackageAutoCompleteOptions(string search,
		CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}
}
