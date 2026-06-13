using System.Text.Json.Nodes;
using System.Threading.Channels;
using Json.Path;
using Server.Common.ObjectAnalyzer.Json.Models;

namespace Server.Common.ObjectAnalyzer.Json;

public static class JsonObjectAnalyzer
{
	// Example data:
	// var checkItems = new List<CheckItem>
	// {
	// 	new(NodePath: "$.BodyChanges.Name"),
	// 	new(NodePath: "$.UniqueName") { AllowableData = ["giliCrud", "packageCube", "insertToLoop"] }
	// };
	// var extractedFields = new ExtractedData
	// (
	// 	ElementPathToName: new Dictionary<string, string>
	// 	{
	// 		{ "$.UniqueName", "CubeType" },
	// 		{ "$.BodyChanges.PackageId", "PackageId" },
	// 		{ "$.BodyChanges.Name", "UniqueName" },
	// 	}
	// );

	public static async IAsyncEnumerable<ExtractedData> ExecuteAsync(
		IJsonStreamProvider streamProvider,
		List<CheckItem> checkItems,
		ExtractedData extractedFields)
	{
		var semaphore = new SemaphoreSlim(10, 10);
		var tasks = new List<Task>();

		var channel =
			Channel.CreateUnbounded<ExtractedData>(new UnboundedChannelOptions
			{
				SingleReader = true, SingleWriter = false
			});

		var intakeTask = Task.Run(async () =>
		{
			try
			{
				await foreach (var item in streamProvider.ProvideStream<JsonNode>(CancellationToken.None))
				{
					var task = Task.Run(async () =>
					{
						await semaphore.WaitAsync();

						try
						{
							var result = await ProcessItemAsync(item, checkItems, extractedFields);
							if (result != null)
							{
								await channel.Writer.WriteAsync(result);
							}
						}
						finally
						{
							semaphore.Release();
						}
					});

					tasks.Add(task);
					tasks.RemoveAll(t => t.IsCompleted);
				}

				await Task.WhenAll(tasks);
			}
			finally
			{
				channel.Writer.Complete();
			}
		});

		await foreach (var extractedResult in channel.Reader.ReadAllAsync())
		{
			yield return extractedResult;
		}

		await intakeTask;
	}


	private static async Task<ExtractedData?> ProcessItemAsync(JsonNode item, List<CheckItem> checkItems,
		ExtractedData extractedFields)
	{
		await Task.Yield();
		var extractedValues = new ExtractedData(new Dictionary<string, string>());

		foreach (var check in checkItems)
		{
			var path = JsonPath.Parse(check.NodePath);
			var evaluation = path.Evaluate(item);

			var node = evaluation.Matches.FirstOrDefault()?.Value;

			if (node == null)
			{
				continue;
			}

			string nodeValue = node.ToString();
			if (check.AllowableData == null || check.AllowableData.Contains(nodeValue))
			{
				continue;
			}

			// TODO: Add to logs
			Console.WriteLine($"Value '{nodeValue}' at path '{check.NodePath}' is not in the allowable data list.");
			return null;
		}

		foreach ((string key, string value) in extractedFields.ElementPathToName)
		{
			var path = JsonPath.Parse(key);
			var evaluation = path.Evaluate(item);
			var node = evaluation.Matches.FirstOrDefault()?.Value;

			if (node != null)
			{
				extractedValues.ElementPathToName[value] = node.ToString();
			}
		}

		return extractedValues;
	}
}
