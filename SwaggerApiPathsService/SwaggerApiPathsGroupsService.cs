namespace SwaggerApiPathsService;

using SwaggerApiPathsService.Models;

public static class SwaggerApiPathsGroupsService
{
    public static IEnumerable<EndpointGroupSummary> GetEndpointGroupSummaries(
        IEnumerable<SwaggerApiEntry> swaggerApiEntries,
        IEnumerable<ApiEndpoint> apiEndpoints)
    {
        var prefixes = GetDistinctEndpointPrefixes(apiEndpoints);
        var aggregates = InitializeAggregates(prefixes);

        foreach (var entry in swaggerApiEntries)
        {
            if (entry.Result.Path is not { } path)
            {
                continue;
            }

            if (FindMatchingPrefix(path, prefixes) is { } matchedPrefix)
            {
                var (Count, NumberOfEndpoints) = aggregates[matchedPrefix];
                aggregates[matchedPrefix] = (
                    Count: Count + entry.Result.Count,
                    NumberOfEndpoints: NumberOfEndpoints + 1);
            }
        }

        return BuildSummaries(prefixes, aggregates);
    }

    private static Dictionary<string, (int Count, int NumberOfEndpoints)> InitializeAggregates(List<string> prefixes)
    {
        var aggregates = new Dictionary<string, (int Count, int NumberOfEndpoints)>(
            prefixes.Count,
            StringComparer.OrdinalIgnoreCase);

        foreach (var prefix in prefixes)
        {
            aggregates[prefix] = (Count: 0, NumberOfEndpoints: 0);
        }

        return aggregates;
    }

    private static string? FindMatchingPrefix(string path, List<string> prefixes)
    {
        foreach (var prefix in prefixes)
        {
            if (PathMatchesPrefix(path, prefix))
            {
                return prefix;
            }
        }

        return null;
    }

    private static bool PathMatchesPrefix(string path, string prefix) =>
        path.Equals(prefix, StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase);

    private static List<EndpointGroupSummary> BuildSummaries(
        List<string> prefixes,
        Dictionary<string, (int Count, int NumberOfEndpoints)> aggregates)
    {
        var summaries = new List<EndpointGroupSummary>(prefixes.Count);

        foreach (var prefix in prefixes)
        {
            var (count, numberOfEndpoints) = aggregates[prefix];
            summaries.Add(new EndpointGroupSummary(prefix, count, numberOfEndpoints));
        }

        return summaries;
    }

    private static List<string> GetDistinctEndpointPrefixes(IEnumerable<ApiEndpoint> apiEndpoints) =>
        [.. apiEndpoints
            .Select(ExtractPrefixFromSignature)
            .Distinct(StringComparer.OrdinalIgnoreCase)];

    private static string ExtractPrefixFromSignature(ApiEndpoint endpoint)
    {
        var signature = endpoint.Signature;
        var parameterIndex = signature.IndexOf("/{", StringComparison.Ordinal);
        return parameterIndex >= 0 ? signature[..parameterIndex] : signature;
    }
}
