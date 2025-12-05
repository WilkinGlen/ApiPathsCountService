using System.Text.Json;
using ApiPathsCountService.Models;

namespace ApiPathsCountService;

public static class ApiPathsCountService
{
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static async Task<ApiPathsResponse?> LoadFromFileAsync(string fileAddress)
    {
        var json = await File.ReadAllTextAsync(fileAddress);
        return JsonSerializer.Deserialize<ApiPathsResponse>(json, _jsonOptions);
    }
}
