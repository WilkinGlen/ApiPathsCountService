namespace SplunkApiPathsService;

using SplunkApiPathsService.Models;

/// <summary>
/// Provides methods for grouping and summarizing Splunk API entries by endpoint prefixes.
/// </summary>
public static class SplunkApiPathsGroupsService
{
    /// <summary>
    /// Gets endpoint group summaries by matching Splunk API entries to endpoint prefixes.
    /// </summary>
    /// <param name="splunkApiEntries">The Splunk API entries containing actual called paths and counts.</param>
    /// <param name="apiEndpoints">The API endpoint definitions with signature patterns.</param>
    /// <returns>A collection of summaries grouped by endpoint prefix.</returns>
    public static IEnumerable<EndpointGroupSummary> GetEndpointGroupSummaries(
        IEnumerable<SplunkApiEntry> splunkApiEntries,
        IEnumerable<ApiEndpoint> apiEndpoints)
    {
        var prefixes = GetDistinctEndpointPrefixes(apiEndpoints);
        var aggregates = BuildAggregates(splunkApiEntries, prefixes);

        return BuildSummaries(prefixes, aggregates);
    }

    private static List<string> GetDistinctEndpointPrefixes(IEnumerable<ApiEndpoint> apiEndpoints) =>
        [.. apiEndpoints
            .Select(ExtractPrefixFromSignature)
            .Distinct(StringComparer.OrdinalIgnoreCase)];

    private static Dictionary<string, (int Count, int NumberOfEndpoints)> BuildAggregates(
        IEnumerable<SplunkApiEntry> splunkApiEntries,
        List<string> prefixes)
    {
        var aggregates = prefixes.ToDictionary(
            prefix => prefix,
            _ => (Count: 0, NumberOfEndpoints: 0),
            StringComparer.OrdinalIgnoreCase);

        foreach (var entry in splunkApiEntries)
        {
            if (entry.Result.Path is not { } path)
            {
                continue;
            }

            if (FindMatchingPrefix(path, prefixes) is { } matchedPrefix)
            {
                var (count, numberOfEndpoints) = aggregates[matchedPrefix];
                aggregates[matchedPrefix] = (
                    Count: count + entry.Result.Count,
                    NumberOfEndpoints: numberOfEndpoints + 1);
            }
        }

        return aggregates;
    }

    private static List<EndpointGroupSummary> BuildSummaries(
        List<string> prefixes,
        Dictionary<string, (int Count, int NumberOfEndpoints)> aggregates) =>
        [.. prefixes.Select(prefix =>
        {
            var (count, numberOfEndpoints) = aggregates[prefix];
            return new EndpointGroupSummary(prefix, count, numberOfEndpoints);
        })];

    private static string? FindMatchingPrefix(string path, List<string> prefixes) =>
        prefixes.FirstOrDefault(prefix => PathMatchesPrefix(path, prefix));

    private static bool PathMatchesPrefix(string path, string prefix) =>
        path.Equals(prefix, StringComparison.OrdinalIgnoreCase) ||
        path.Length > prefix.Length &&
         path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
         path[prefix.Length] == '/';

    private static string ExtractPrefixFromSignature(ApiEndpoint endpoint)
    {
        var signature = endpoint.Signature;
        var parameterIndex = signature.IndexOf("/{", StringComparison.Ordinal);
        return parameterIndex >= 0 ? signature[..parameterIndex] : signature;
    }
}
