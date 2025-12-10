namespace SwaggerApiPathsService;

using SwaggerApiPathsService.Models;

public static class SwaggerApiPathsGroupsService
{
    public static IEnumerable<EndpointGroupSummary> GetEndpointGroupSummaries(
        IEnumerable<SwaggerApiEntry> swaggerApiEntries, 
        IEnumerable<ApiEndpoint> apiEndpoints)
    {
        var distinctPrefixes = GetDistinctEndpointPrefixes(apiEndpoints);
        var aggregates = new Dictionary<string, (int Count, int NumberOfEndpoints)>(
            distinctPrefixes.Count, 
            StringComparer.OrdinalIgnoreCase);
        
        foreach (var prefix in distinctPrefixes)
        {
            aggregates[prefix] = (0, 0);
        }

        foreach (var entry in swaggerApiEntries)
        {
            var path = entry.Result.Path;
            if (path == null)
            {
                continue;
            }

            foreach (var prefix in distinctPrefixes)
            {
                if (path.Equals(prefix, StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase))
                {
                    var (Count, NumberOfEndpoints) = aggregates[prefix];
                    aggregates[prefix] = (Count + entry.Result.Count, NumberOfEndpoints + 1);
                    break;
                }
            }
        }

        List<EndpointGroupSummary> summaries = new(distinctPrefixes.Count);
        foreach (var prefix in distinctPrefixes)
        {
            var (count, numberOfEndpoints) = aggregates[prefix];
            summaries.Add(new EndpointGroupSummary(prefix, count, numberOfEndpoints));
        }

        return summaries;
    }

    private static List<string> GetDistinctEndpointPrefixes(IEnumerable<ApiEndpoint> apiEndpoints) =>
        [.. apiEndpoints
            .Select(e =>
            {
                var sig = e.Signature;
                var idx = sig.IndexOf("/{", StringComparison.Ordinal);
                return idx >= 0 ? sig[..idx] : sig;
            })
            .Distinct(StringComparer.OrdinalIgnoreCase)];
}
