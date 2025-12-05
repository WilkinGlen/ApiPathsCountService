namespace ApiPathsCountService.Tests;

using global::ApiPathsCountService.Models;
using FluentAssertions;

public class GetAllApiPathResults_Should
{
    [Fact]
    public void ReturnAllApiPathResults_WhenResponseHasMultipleResults()
    {
        var response = new ApiPathsResponse(
        [
            new ApiPathResultWrapper(false, new ApiPathResult("/path1", "1")),
            new ApiPathResultWrapper(false, new ApiPathResult("/path2", "2")),
            new ApiPathResultWrapper(true, new ApiPathResult("/path3", "3"))
        ]);

        var results = ApiPathsCountService.GetAllApiPathResults(response).ToList();

        _ = results.Should().HaveCount(3);
        _ = results[0].Path.Should().Be("/path1");
        _ = results[0].Count.Should().Be("1");
        _ = results[1].Path.Should().Be("/path2");
        _ = results[1].Count.Should().Be("2");
        _ = results[2].Path.Should().Be("/path3");
        _ = results[2].Count.Should().Be("3");
    }

    [Fact]
    public void ReturnEmptyEnumerable_WhenResponseHasNoResults()
    {
        var response = new ApiPathsResponse([]);

        var results = ApiPathsCountService.GetAllApiPathResults(response);

        _ = results.Should().BeEmpty();
    }

    [Fact]
    public void ReturnSingleResult_WhenResponseHasOneResult()
    {
        var response = new ApiPathsResponse(
        [
            new ApiPathResultWrapper(true, new ApiPathResult("/single/path", "42"))
        ]);

        var results = ApiPathsCountService.GetAllApiPathResults(response).ToList();

        _ = results.Should().HaveCount(1);
        _ = results[0].Path.Should().Be("/single/path");
        _ = results[0].Count.Should().Be("42");
    }

    [Fact]
    public void UnwrapResultsRegardlessOfPreviewFlag_WhenMixedPreviewValues()
    {
        var response = new ApiPathsResponse(
        [
            new ApiPathResultWrapper(false, new ApiPathResult("/preview-false", "1")),
            new ApiPathResultWrapper(true, new ApiPathResult("/preview-true", "2"))
        ]);

        var results = ApiPathsCountService.GetAllApiPathResults(response).ToList();

        _ = results.Should().HaveCount(2);
        _ = results.Should().AllSatisfy(r => r.Should().NotBeNull());
    }

    [Fact]
    public void ReturnCorrectPathsAndCounts_WhenCalledWithRealWorldData()
    {
        var response = new ApiPathsResponse(
        [
            new ApiPathResultWrapper(false, new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", "1")),
            new ApiPathResultWrapper(false, new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZEDPUWW", "2")),
            new ApiPathResultWrapper(false, new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZFDPUWW", "3")),
            new ApiPathResultWrapper(false, new ApiPathResult("/GetIt/v1/DirectReports/Name/Glen", "1")),
            new ApiPathResultWrapper(false, new ApiPathResult("/GetIt/v1/DirectReports/Name/Marcus", "1")),
            new ApiPathResultWrapper(false, new ApiPathResult("/GetIt/v1/DirectReports/Name/Luke", "1"))
        ]);

        var results = ApiPathsCountService.GetAllApiPathResults(response).ToList();

        _ = results.Should().HaveCount(6);
        _ = results[0].Path.Should().Be("/GetIt/v1/DirectReports/StandardId/ZKDPUWW");
        _ = results[3].Path.Should().Be("/GetIt/v1/DirectReports/Name/Glen");
        _ = results[5].Count.Should().Be("1");
    }

    [Fact]
    public async Task WorkWithLoadFromFileAsync_WhenIntegrated()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/test/path1","Count":"10"}},
                {"preview":true,"result":{"Path":"/test/path2","Count":"20"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var response = await ApiPathsCountService.LoadFromFileAsync(tempFile);
            var results = ApiPathsCountService.GetAllApiPathResults(response!).ToList();

            _ = results.Should().HaveCount(2);
            _ = results[0].Path.Should().Be("/test/path1");
            _ = results[0].Count.Should().Be("10");
            _ = results[1].Path.Should().Be("/test/path2");
            _ = results[1].Count.Should().Be("20");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ReturnEnumerableThatCanBeIteratedMultipleTimes()
    {
        var response = new ApiPathsResponse(
        [
            new ApiPathResultWrapper(false, new ApiPathResult("/path1", "1")),
            new ApiPathResultWrapper(false, new ApiPathResult("/path2", "2"))
        ]);

        var results = ApiPathsCountService.GetAllApiPathResults(response);

        var firstIteration = results.ToList();
        var secondIteration = results.ToList();

        _ = firstIteration.Should().HaveCount(2);
        _ = secondIteration.Should().HaveCount(2);
        _ = firstIteration.Should().BeEquivalentTo(secondIteration);
    }

    [Fact]
    public void PreserveOrderOfResults_WhenMultipleResultsPresent()
    {
        var response = new ApiPathsResponse(
        [
            new ApiPathResultWrapper(false, new ApiPathResult("/first", "1")),
            new ApiPathResultWrapper(false, new ApiPathResult("/second", "2")),
            new ApiPathResultWrapper(false, new ApiPathResult("/third", "3")),
            new ApiPathResultWrapper(false, new ApiPathResult("/fourth", "4"))
        ]);

        var results = ApiPathsCountService.GetAllApiPathResults(response).ToList();

        _ = results[0].Path.Should().Be("/first");
        _ = results[1].Path.Should().Be("/second");
        _ = results[2].Path.Should().Be("/third");
        _ = results[3].Path.Should().Be("/fourth");
    }

    [Fact]
    public void ExtractQueryStringPaths_WhenResponseContainsQueryParameters()
    {
        var response = new ApiPathsResponse(
        [
            new ApiPathResultWrapper(false, new ApiPathResult("/api/search?q=test&limit=10", "50")),
            new ApiPathResultWrapper(false, new ApiPathResult("/api/filter?category=books", "100"))
        ]);

        var results = ApiPathsCountService.GetAllApiPathResults(response).ToList();

        _ = results.Should().HaveCount(2);
        _ = results[0].Path.Should().Contain("?q=test");
        _ = results[1].Path.Should().Contain("category=books");
    }

    [Fact]
    public void ExtractGuidPaths_WhenResponseContainsGuidIdentifiers()
    {
        var response = new ApiPathsResponse(
        [
            new ApiPathResultWrapper(false, new ApiPathResult("/api/sessions/550e8400-e29b-41d4-a716-446655440000", "25")),
            new ApiPathResultWrapper(false, new ApiPathResult("/api/tokens/123e4567-e89b-12d3-a456-426614174000", "100"))
        ]);

        var results = ApiPathsCountService.GetAllApiPathResults(response).ToList();

        _ = results.Should().HaveCount(2);
        _ = results.Should().AllSatisfy(r => r.Path.Should().MatchRegex(@"[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}"));
    }

    [Fact]
    public void ExtractVersionedPaths_WhenResponseContainsApiVersions()
    {
        var response = new ApiPathsResponse(
        [
            new ApiPathResultWrapper(false, new ApiPathResult("/api/v1/users/list", "100")),
            new ApiPathResultWrapper(false, new ApiPathResult("/api/v2/users/list", "200")),
            new ApiPathResultWrapper(false, new ApiPathResult("/api/v3/users/list", "300"))
        ]);

        var results = ApiPathsCountService.GetAllApiPathResults(response).ToList();

        _ = results.Should().HaveCount(3);
        _ = results.Should().Contain(r => r.Path.Contains("/v1/"));
        _ = results.Should().Contain(r => r.Path.Contains("/v2/"));
        _ = results.Should().Contain(r => r.Path.Contains("/v3/"));
    }

    [Fact]
    public void ExtractLocalePaths_WhenResponseContainsLanguageCodes()
    {
        var response = new ApiPathsResponse(
        [
            new ApiPathResultWrapper(false, new ApiPathResult("/api/content/en-US/articles", "100")),
            new ApiPathResultWrapper(false, new ApiPathResult("/api/content/fr-FR/articles", "80")),
            new ApiPathResultWrapper(false, new ApiPathResult("/api/content/de-DE/articles", "90"))
        ]);

        var results = ApiPathsCountService.GetAllApiPathResults(response).ToList();

        _ = results.Should().HaveCount(3);
        _ = results.Should().Contain(r => r.Path.Contains("en-US"));
        _ = results.Should().Contain(r => r.Path.Contains("fr-FR"));
        _ = results.Should().Contain(r => r.Path.Contains("de-DE"));
    }

    [Fact]
    public void ExtractNestedResourcePaths_WhenResponseContainsHierarchy()
    {
        var response = new ApiPathsResponse(
        [
            new ApiPathResultWrapper(false, new ApiPathResult("/api/companies/123/departments/456/employees", "50")),
            new ApiPathResultWrapper(false, new ApiPathResult("/api/schools/ABC/classes/101/students", "25"))
        ]);

        var results = ApiPathsCountService.GetAllApiPathResults(response).ToList();

        _ = results.Should().HaveCount(2);
        _ = results[0].Path.Split('/').Should().HaveCountGreaterThan(5);
        _ = results[1].Path.Split('/').Should().HaveCountGreaterThan(5);
    }

    [Fact]
    public void ExtractPaginationPaths_WhenResponseContainsPagingParameters()
    {
        var response = new ApiPathsResponse(
        [
            new ApiPathResultWrapper(false, new ApiPathResult("/api/items?page=1&size=50", "1000")),
            new ApiPathResultWrapper(false, new ApiPathResult("/api/users?offset=0&limit=25", "300"))
        ]);

        var results = ApiPathsCountService.GetAllApiPathResults(response).ToList();

        _ = results.Should().HaveCount(2);
        _ = results[0].Path.Should().Contain("page=");
        _ = results[1].Path.Should().Contain("offset=");
    }
}
