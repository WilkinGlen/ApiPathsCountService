using ApiPathsCountService.Models;
using FluentAssertions;

namespace ApiPathsCountService.Tests;

public class ApiPathsCountServiceTests
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
    public async Task LoadFromFileAsync_ValidJsonFile_ReturnsPopulatedResponse()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, TestJsonContent);

            var result = await ApiPathsCountService.LoadFromFileAsync(tempFile);

            result.Should().NotBeNull();
            result!.Results.Should().NotBeNull();
            result.Results.Should().HaveCount(6);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadFromFileAsync_ValidJsonFile_CorrectlyDeserializesFirstResult()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, TestJsonContent);

            var result = await ApiPathsCountService.LoadFromFileAsync(tempFile);

            result.Should().NotBeNull();
            var firstResult = result!.Results[0];
            firstResult.Preview.Should().BeFalse();
            firstResult.Result.Should().NotBeNull();
            firstResult.Result.Path.Should().Be("/GetIt/v1/DirectReports/StandardId/ZKDPUWW");
            firstResult.Result.Count.Should().Be("1");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadFromFileAsync_ValidJsonFile_CorrectlyDeserializesAllCounts()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, TestJsonContent);

            var result = await ApiPathsCountService.LoadFromFileAsync(tempFile);

            result.Should().NotBeNull();
            result!.Results[0].Result.Count.Should().Be("1");
            result.Results[1].Result.Count.Should().Be("2");
            result.Results[2].Result.Count.Should().Be("3");
            result.Results[3].Result.Count.Should().Be("1");
            result.Results[4].Result.Count.Should().Be("1");
            result.Results[5].Result.Count.Should().Be("1");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadFromFileAsync_ValidJsonFile_CorrectlyDeserializesAllPaths()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, TestJsonContent);

            var result = await ApiPathsCountService.LoadFromFileAsync(tempFile);

            result.Should().NotBeNull();
            result!.Results[0].Result.Path.Should().Be("/GetIt/v1/DirectReports/StandardId/ZKDPUWW");
            result.Results[1].Result.Path.Should().Be("/GetIt/v1/DirectReports/StandardId/ZEDPUWW");
            result.Results[2].Result.Path.Should().Be("/GetIt/v1/DirectReports/StandardId/ZFDPUWW");
            result.Results[3].Result.Path.Should().Be("/GetIt/v1/DirectReports/Name/Glen");
            result.Results[4].Result.Path.Should().Be("/GetIt/v1/DirectReports/Name/Marcus");
            result.Results[5].Result.Path.Should().Be("/GetIt/v1/DirectReports/Name/Luke");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadFromFileAsync_ValidJsonFile_AllPreviewFlagsAreFalse()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, TestJsonContent);

            var result = await ApiPathsCountService.LoadFromFileAsync(tempFile);

            result.Should().NotBeNull();
            result!.Results.Should().AllSatisfy(r => r.Preview.Should().BeFalse());
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadFromFileAsync_EmptyResultsArray_ReturnsEmptyArray()
    {
        var tempFile = Path.GetTempFileName();
        var emptyJson = """{"results":[]}""";
        try
        {
            await File.WriteAllTextAsync(tempFile, emptyJson);

            var result = await ApiPathsCountService.LoadFromFileAsync(tempFile);

            result.Should().NotBeNull();
            result!.Results.Should().NotBeNull();
            result.Results.Should().BeEmpty();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadFromFileAsync_InvalidJson_ThrowsException()
    {
        var tempFile = Path.GetTempFileName();
        var invalidJson = """{"invalid json""";
        try
        {
            await File.WriteAllTextAsync(tempFile, invalidJson);

            var act = async () => await ApiPathsCountService.LoadFromFileAsync(tempFile);

            await act.Should().ThrowAsync<Exception>();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadFromFileAsync_NonExistentFile_ThrowsFileNotFoundException()
    {
        var nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".json");

        var act = async () => await ApiPathsCountService.LoadFromFileAsync(nonExistentFile);

        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task LoadFromFileAsync_CaseInsensitivePropertyNames_DeserializesCorrectly()
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

            result.Should().NotBeNull();
            result!.Results.Should().HaveCount(1);
            result.Results[0].Preview.Should().BeTrue();
            result.Results[0].Result.Path.Should().Be("/test/path");
            result.Results[0].Result.Count.Should().Be("99");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadFromFileAsync_MixedPreviewValues_DeserializesCorrectly()
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

            result.Should().NotBeNull();
            result!.Results.Should().HaveCount(3);
            result.Results[0].Preview.Should().BeFalse();
            result.Results[1].Preview.Should().BeTrue();
            result.Results[2].Preview.Should().BeFalse();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
