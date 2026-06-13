namespace Server.Common.ObjectAnalyzer.Json;

public interface IJsonStreamProvider
{
	IAsyncEnumerable<T> ProvideStream<T>(CancellationToken token);
}
