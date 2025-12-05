namespace ApiPathsCountService.Tests;

using FluentAssertions;

public class LoadFromFileAsync_Should
{
    private const string TestJsonContent = """
    {
        "results":[
            {"preview":false,"result":{"Path":"/GetIt/v1/DirectReports/StandardId/ZKDPUWW","Count":"1"}},
            {"preview":false,"result":{"Path":"/GetIt/v1/DirectReports/StandardId/ZEDPUWW","Count":"2"}},
            {"preview":false,"result":{"Path":"/GetIt/v1/DirectReports/StandardId/ZFDPUWW","Count":"3"}},
            {"preview":false,"result":{"Path":"/GetIt/v1/DirectReports/Name/Glen","Count":"1"}},
            {"preview":false,"result":{"Path":"/GetIt/v1/DirectReports/Name/Marcus","Count":"1"}},
            {"preview":false,"result":{"Path":"/GetIt/v1/DirectReports/Name/Luke","Count":"1"}}
        ]
    }
    """;

    [Fact]
    public async Task ReturnPopulatedResponse_WhenValidJsonFile()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, TestJsonContent);

            var result = await ApiPathsCountService.LoadFromFileAsync(tempFile);

            _ = result.Should().NotBeNull();
            _ = result!.Results.Should().NotBeNull();
            _ = result.Results.Should().HaveCount(6);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task CorrectlyDeserializeFirstResult_WhenValidJsonFile()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, TestJsonContent);

            var result = await ApiPathsCountService.LoadFromFileAsync(tempFile);

            _ = result.Should().NotBeNull();
            var firstResult = result!.Results[0];
            _ = firstResult.Preview.Should().BeFalse();
            _ = firstResult.Result.Should().NotBeNull();
            _ = firstResult.Result.Path.Should().Be("/GetIt/v1/DirectReports/StandardId/ZKDPUWW");
            _ = firstResult.Result.Count.Should().Be("1");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task CorrectlyDeserializeAllCounts_WhenValidJsonFile()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, TestJsonContent);

            var result = await ApiPathsCountService.LoadFromFileAsync(tempFile);

            _ = result.Should().NotBeNull();
            _ = result!.Results[0].Result.Count.Should().Be("1");
            _ = result.Results[1].Result.Count.Should().Be("2");
            _ = result.Results[2].Result.Count.Should().Be("3");
            _ = result.Results[3].Result.Count.Should().Be("1");
            _ = result.Results[4].Result.Count.Should().Be("1");
            _ = result.Results[5].Result.Count.Should().Be("1");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task CorrectlyDeserializeAllPaths_WhenValidJsonFile()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, TestJsonContent);

            var result = await ApiPathsCountService.LoadFromFileAsync(tempFile);

            _ = result.Should().NotBeNull();
            _ = result!.Results[0].Result.Path.Should().Be("/GetIt/v1/DirectReports/StandardId/ZKDPUWW");
            _ = result.Results[1].Result.Path.Should().Be("/GetIt/v1/DirectReports/StandardId/ZEDPUWW");
            _ = result.Results[2].Result.Path.Should().Be("/GetIt/v1/DirectReports/StandardId/ZFDPUWW");
            _ = result.Results[3].Result.Path.Should().Be("/GetIt/v1/DirectReports/Name/Glen");
            _ = result.Results[4].Result.Path.Should().Be("/GetIt/v1/DirectReports/Name/Marcus");
            _ = result.Results[5].Result.Path.Should().Be("/GetIt/v1/DirectReports/Name/Luke");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task HaveAllPreviewFlagsFalse_WhenValidJsonFile()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, TestJsonContent);

            var result = await ApiPathsCountService.LoadFromFileAsync(tempFile);

            _ = result.Should().NotBeNull();
            _ = result!.Results.Should().AllSatisfy(r => r.Preview.Should().BeFalse());
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReturnEmptyArray_WhenEmptyResultsArray()
    {
        var tempFile = Path.GetTempFileName();
        var emptyJson = """{"results":[]}""";
        try
        {
            await File.WriteAllTextAsync(tempFile, emptyJson);

            var result = await ApiPathsCountService.LoadFromFileAsync(tempFile);

            _ = result.Should().NotBeNull();
            _ = result!.Results.Should().NotBeNull();
            _ = result.Results.Should().BeEmpty();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ThrowException_WhenInvalidJson()
    {
        var tempFile = Path.GetTempFileName();
        var invalidJson = """{"invalid json""";
        try
        {
            await File.WriteAllTextAsync(tempFile, invalidJson);

            var act = async () => await ApiPathsCountService.LoadFromFileAsync(tempFile);

            _ = await act.Should().ThrowAsync<Exception>();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ThrowFileNotFoundException_WhenNonExistentFile()
    {
        var nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");

        var act = async () => await ApiPathsCountService.LoadFromFileAsync(nonExistentFile);

        _ = await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task DeserializeCorrectly_WhenCaseInsensitivePropertyNames()
    {
        var tempFile = Path.GetTempFileName();
        var mixedCaseJson = """
        {
            "RESULTS":[
                {"PREVIEW":true,"RESULT":{"PATH":"/test/path","COUNT":"99"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, mixedCaseJson);

            var result = await ApiPathsCountService.LoadFromFileAsync(tempFile);

            _ = result.Should().NotBeNull();
            _ = result!.Results.Should().HaveCount(1);
            _ = result.Results[0].Preview.Should().BeTrue();
            _ = result.Results[0].Result.Path.Should().Be("/test/path");
            _ = result.Results[0].Result.Count.Should().Be("99");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task DeserializeCorrectly_WhenMixedPreviewValues()
    {
        var tempFile = Path.GetTempFileName();
        var mixedPreviewJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/path1","Count":"1"}},
                {"preview":true,"result":{"Path":"/path2","Count":"2"}},
                {"preview":false,"result":{"Path":"/path3","Count":"3"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, mixedPreviewJson);

            var result = await ApiPathsCountService.LoadFromFileAsync(tempFile);

            _ = result.Should().NotBeNull();
            _ = result!.Results.Should().HaveCount(3);
            _ = result.Results[0].Preview.Should().BeFalse();
            _ = result.Results[1].Preview.Should().BeTrue();
            _ = result.Results[2].Preview.Should().BeFalse();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadQueryStringPaths_WhenJsonContainsQueryParameters()
    {
        var tempFile = Path.GetTempFileName();
        var queryJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/search?q=test&limit=10","Count":"50"}},
                {"preview":false,"result":{"Path":"/api/filter?category=books&sort=asc","Count":"100"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, queryJson);

            var result = await ApiPathsCountService.LoadFromFileAsync(tempFile);

            _ = result.Should().NotBeNull();
            _ = result!.Results.Should().HaveCount(2);
            _ = result.Results[0].Result.Path.Should().Contain("?");
            _ = result.Results[1].Result.Path.Should().Contain("category=books");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadGuidPaths_WhenJsonContainsGuidIdentifiers()
    {
        var tempFile = Path.GetTempFileName();
        var guidJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/sessions/550e8400-e29b-41d4-a716-446655440000","Count":"25"}},
                {"preview":false,"result":{"Path":"/api/tokens/123e4567-e89b-12d3-a456-426614174000","Count":"100"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, guidJson);

            var result = await ApiPathsCountService.LoadFromFileAsync(tempFile);

            _ = result.Should().NotBeNull();
            _ = result!.Results.Should().HaveCount(2);
            _ = result.Results[0].Result.Path.Should().Contain("550e8400-e29b-41d4-a716-446655440000");
            _ = result.Results[1].Result.Path.Should().Contain("123e4567-e89b-12d3-a456-426614174000");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadVersionedPaths_WhenJsonContainsApiVersions()
    {
        var tempFile = Path.GetTempFileName();
        var versionJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/v1/users/list","Count":"100"}},
                {"preview":false,"result":{"Path":"/api/v2/users/list","Count":"200"}},
                {"preview":false,"result":{"Path":"/api/v3/users/list","Count":"300"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, versionJson);

            var result = await ApiPathsCountService.LoadFromFileAsync(tempFile);

            _ = result.Should().NotBeNull();
            _ = result!.Results.Should().HaveCount(3);
            _ = result.Results.Should().Contain(r => r.Result.Path.Contains("/api/v1/"));
            _ = result.Results.Should().Contain(r => r.Result.Path.Contains("/api/v2/"));
            _ = result.Results.Should().Contain(r => r.Result.Path.Contains("/api/v3/"));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadEncodedPaths_WhenJsonContainsUrlEncoding()
    {
        var tempFile = Path.GetTempFileName();
        var encodedJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/search?q=hello%20world","Count":"40"}},
                {"preview":false,"result":{"Path":"/api/files/document%2Epdf","Count":"15"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, encodedJson);

            var result = await ApiPathsCountService.LoadFromFileAsync(tempFile);

            _ = result.Should().NotBeNull();
            _ = result!.Results.Should().HaveCount(2);
            _ = result.Results[0].Result.Path.Should().Contain("%20");
            _ = result.Results[1].Result.Path.Should().Contain("%2E");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadDateTimePaths_WhenJsonContainsDateParameters()
    {
        var tempFile = Path.GetTempFileName();
        var dateJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/reports/2024-01-01/2024-01-31","Count":"500"}},
                {"preview":false,"result":{"Path":"/api/analytics/2024-01-15","Count":"200"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, dateJson);

            var result = await ApiPathsCountService.LoadFromFileAsync(tempFile);

            _ = result.Should().NotBeNull();
            _ = result!.Results.Should().HaveCount(2);
            _ = result.Results[0].Result.Path.Should().Contain("2024-01-01");
            _ = result.Results[1].Result.Path.Should().Contain("2024-01-15");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
