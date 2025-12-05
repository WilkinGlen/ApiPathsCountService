namespace ApiPathsCountService;

using global::ApiPathsCountService.Models;
using System.Text.Json;

public static class ApiPathsCountService
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static async Task<ApiPathsResponse?> LoadFromFileAsync(string fileAddress)
    {
        var json = await File.ReadAllTextAsync(fileAddress);
        return JsonSerializer.Deserialize<ApiPathsResponse>(json, _jsonOptions);
    }

    public static IEnumerable<ApiPathResult> GetAllApiPathResults(ApiPathsResponse response)
    {
        return response.Results.Select(wrapper => wrapper.Result);
    }

    public static IEnumerable<IGrouping<string, ApiPathResult>> GroupByPathPrefix(IEnumerable<ApiPathResult> results)
    {
        return results.GroupBy(result => result.Path);
    }

    public static IEnumerable<PathGroupSummary> GetGroupSummaries(IEnumerable<IGrouping<string, ApiPathResult>> groups)
    {
        return groups.Select(group => new PathGroupSummary(
            group.Key,
            group.Sum(result => int.TryParse(result.Count, out var count) ? count : 0),
            group.Count()
        ));
    }
}
