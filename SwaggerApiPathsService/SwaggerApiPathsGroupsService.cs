namespace SwaggerApiPathsService;

using SwaggerApiPathsService.Models;

public static class SwaggerApiPathsGroupsService
{
    public static IEnumerable<EndpointGroupSummary> GetEndpointGroupSummaries(
        IEnumerable<SwaggerApiEntry> swaggerApiEntries,
        IEnumerable<ApiEndpoint> apiEndpoints)
    {
        var prefixes = GetDistinctEndpointPrefixes(apiEndpoints);
        var aggregates = BuildAggregates(swaggerApiEntries, prefixes);

        return BuildSummaries(prefixes, aggregates);
    }

    private static List<string> GetDistinctEndpointPrefixes(IEnumerable<ApiEndpoint> apiEndpoints) =>
        [.. apiEndpoints
            .Select(ExtractPrefixFromSignature)
            .Distinct(StringComparer.OrdinalIgnoreCase)];

    private static Dictionary<string, (int Count, int NumberOfEndpoints)> BuildAggregates(
        IEnumerable<SwaggerApiEntry> swaggerApiEntries,
        List<string> prefixes)
    {
        var initialAggregates = prefixes.ToDictionary(
            prefix => prefix,
            _ => (Count: 0, NumberOfEndpoints: 0),
            StringComparer.OrdinalIgnoreCase);

        return swaggerApiEntries
            .Where(entry => entry.Result.Path is not null)
            .Select(entry => (Path: entry.Result.Path!, entry.Result.Count))
            .Select(entry => (Prefix: FindMatchingPrefix(entry.Path, prefixes), entry.Count))
            .Where(match => match.Prefix is not null)
            .Aggregate(initialAggregates, (aggregates, match) =>
            {
                var (Count, NumberOfEndpoints) = aggregates[match.Prefix!];
                aggregates[match.Prefix!] = (Count: Count + match.Count, NumberOfEndpoints: NumberOfEndpoints + 1);
                return aggregates;
            });
    }

    private static List<EndpointGroupSummary> BuildSummaries(
        List<string> prefixes,
        Dictionary<string, (int Count, int NumberOfEndpoints)> aggregates) =>
        [.. prefixes
            .Select(prefix => new EndpointGroupSummary(
                prefix,
                aggregates[prefix].Count,
                aggregates[prefix].NumberOfEndpoints))];

    private static string? FindMatchingPrefix(string path, List<string> prefixes) =>
        prefixes.FirstOrDefault(prefix => PathMatchesPrefix(path, prefix));

    private static bool PathMatchesPrefix(string path, string prefix) =>
        path.Equals(prefix, StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase);

    private static string ExtractPrefixFromSignature(ApiEndpoint endpoint)
    {
        var signature = endpoint.Signature;
        var parameterIndex = signature.IndexOf("/{", StringComparison.Ordinal);
        return parameterIndex >= 0 ? signature[..parameterIndex] : signature;
    }
}
