namespace Server.Communicators;

public class FlapiCommunicator
{
	public IEnumerable<string> GetConnectedPackagesIdentifiersToGiliTable(string uniqueGiliTableName,
		CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public IEnumerable<string> GetConnectedGiliTablesToPackage(string packageId,
		CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}
}
