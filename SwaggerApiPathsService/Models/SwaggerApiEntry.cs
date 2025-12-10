namespace SwaggerApiPathsService.Models;

public record SwaggerApiEntry(bool Preview, SwaggerApiResult Result);

public record SwaggerApiResult(string? Path, int Count);
