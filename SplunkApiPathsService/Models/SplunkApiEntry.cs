namespace SplunkApiPathsService.Models;

public record SplunkApiEntry(bool Preview, SplunkApiResult Result);

public record SplunkApiResult(string? Path, int Count);
