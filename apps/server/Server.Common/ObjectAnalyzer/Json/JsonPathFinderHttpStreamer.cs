using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Server.Common.ObjectAnalyzer.Json;

public static class JsonPathFinderHttpStreamer
{
	public static async IAsyncEnumerable<T> StreamFromPathAsync<T>(HttpClient httpClient, string url, string targetPath,
		[EnumeratorCancellation] CancellationToken token = default)
	{
		string normalizedTarget = targetPath.TrimStart('$').TrimStart('.');

		using var response =
			await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
		response.EnsureSuccessStatusCode();

		await using var stream = await response.Content.ReadAsStreamAsync(token);
		var pipeReader = PipeReader.Create(stream);

		var pathList = new List<string>();
		bool isFinalBlock = false;
		bool targetPathFound = false;

		while (!isFinalBlock)
		{
			ReadResult result = await pipeReader.ReadAsync(token);
			ReadOnlySequence<byte> buffer = result.Buffer;
			isFinalBlock = result.IsCompleted;

			var items = ProcessBuffer<T>(buffer, isFinalBlock, normalizedTarget, pathList, ref targetPathFound,
				out SequencePosition consumedPosition);

			foreach (var item in items)
			{
				yield return item;
			}

			pipeReader.AdvanceTo(consumedPosition, buffer.End);

			if (targetPathFound && pathList.Count == 0)
			{
				break;
			}
		}

		await pipeReader.CompleteAsync();
	}

	private static List<T> ProcessBuffer<T>(
		ReadOnlySequence<byte> buffer,
		bool isFinalBlock,
		string normalizedTarget,
		List<string> pathList,
		ref bool targetPathFound,
		out SequencePosition consumedPosition)
	{
		var extractedItems = new List<T>();
		var reader = new Utf8JsonReader(buffer, isFinalBlock, default);
		string currentPath = string.Join(".", pathList);

		while (reader.Read())
		{
			if (reader.TokenType == JsonTokenType.PropertyName)
			{
				string? propertyName = reader.GetString();
				if (propertyName != null)
				{
					pathList.Add(propertyName);
					currentPath = string.Join(".", pathList);
				}
			}
			else if (reader.TokenType == JsonTokenType.EndObject)
			{
				if (pathList.Count > 0)
				{
					pathList.RemoveAt(pathList.Count - 1);
					currentPath = string.Join(".", pathList);
				}
			}

			if (!targetPathFound && currentPath.Equals(normalizedTarget, StringComparison.OrdinalIgnoreCase))
			{
				targetPathFound = true;
			}

			if (targetPathFound)
			{
				if (reader.TokenType == JsonTokenType.StartObject)
				{
					T? item = JsonSerializer.Deserialize<T>(ref reader);
					if (item != null)
					{
						extractedItems.Add(item);
					}
				}

				if (reader.TokenType == JsonTokenType.EndArray &&
				    currentPath.Equals(normalizedTarget, StringComparison.OrdinalIgnoreCase))
				{
					break;
				}
			}
		}

		consumedPosition = reader.Position;
		return extractedItems;
	}
}
