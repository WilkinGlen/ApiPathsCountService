namespace ApiPathsCountService.Tests;

using global::ApiPathsCountService.Models;
using FluentAssertions;

public class GroupByPathPrefix_Should
{
    [Fact]
    public void GroupByCompletePath_WhenPathsAreDifferent()
    {
        var results = new[]
        {
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", "1"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZEDPUWW", "2"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZFDPUWW", "3")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        _ = grouped.Should().HaveCount(3);
        _ = grouped.Should().AllSatisfy(g => g.Should().HaveCount(1));
    }

    [Fact]
    public void GroupIdenticalPaths_WhenPathsAreSame()
    {
        var results = new[]
        {
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", "1"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", "2"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZEDPUWW", "3"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZEDPUWW", "4")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        _ = grouped.Should().HaveCount(2);
        
        var zkdpuwwGroup = grouped.FirstOrDefault(g => g.Key.Contains("ZKDPUWW"));
        _ = zkdpuwwGroup.Should().NotBeNull();
        _ = zkdpuwwGroup!.Should().HaveCount(2);
        
        var zedpuwwGroup = grouped.FirstOrDefault(g => g.Key.Contains("ZEDPUWW"));
        _ = zedpuwwGroup.Should().NotBeNull();
        _ = zedpuwwGroup!.Should().HaveCount(2);
    }

    [Fact]
    public void ReturnEmptyEnumerable_WhenNoResultsProvided()
    {
        var results = Array.Empty<ApiPathResult>();

        var grouped = ApiPathsCountService.GroupByPathPrefix(results);

        _ = grouped.Should().BeEmpty();
    }

    [Fact]
    public void CreateSeparateGroups_WhenAllPathsAreDifferent()
    {
        var results = new[]
        {
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", "1"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZEDPUWW", "2"),
            new ApiPathResult("/PostIt/v1/Users/Create", "5"),
            new ApiPathResult("/PostIt/v1/Users/Update", "3")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        _ = grouped.Should().HaveCount(4);
        _ = grouped.Should().AllSatisfy(g => g.Should().HaveCount(1));
    }

    [Fact]
    public void ReturnSingleGroup_WhenOnlyOneResult()
    {
        var results = new[]
        {
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", "1")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        _ = grouped.Should().HaveCount(1);
        _ = grouped[0].Should().HaveCount(1);
    }

    [Fact]
    public void PreserveAllResultsInGroups_WhenGrouping()
    {
        var results = new[]
        {
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", "1"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", "2"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", "3")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        _ = grouped.Should().HaveCount(1);
        var group = grouped[0].ToList();
        _ = group[0].Count.Should().Be("1");
        _ = group[1].Count.Should().Be("2");
        _ = group[2].Count.Should().Be("3");
    }

    [Fact]
    public async Task WorkWithFullPipeline_WhenIntegrated()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
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
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var response = await ApiPathsCountService.LoadFromFileAsync(tempFile);
            var allResults = ApiPathsCountService.GetAllApiPathResults(response!);
            var grouped = ApiPathsCountService.GroupByPathPrefix(allResults).ToList();

            _ = grouped.Should().HaveCount(6);
            _ = grouped.Should().AllSatisfy(g => g.Should().HaveCount(1));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void HandleEmptyPath_WhenPathIsEmpty()
    {
        var results = new[]
        {
            new ApiPathResult("", "1"),
            new ApiPathResult("/GetIt/v1/DirectReports", "2")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        _ = grouped.Should().HaveCount(2);
    }

    [Fact]
    public void GroupExactMatchesOnly_WhenPathsSharePrefix()
    {
        var results = new[]
        {
            new ApiPathResult("/api/v1/users/profile/settings/display", "1"),
            new ApiPathResult("/api/v1/users/profile/settings/privacy", "2"),
            new ApiPathResult("/api/v1/users/profile/settings/security", "3")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        _ = grouped.Should().HaveCount(3);
        _ = grouped.Should().AllSatisfy(g => g.Should().HaveCount(1));
    }

    [Fact]
    public void GroupIdenticalPathsWithDifferentCounts()
    {
        var results = new[]
        {
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/AAA", "1"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/AAA", "2"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/AAA", "3")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        _ = grouped.Should().HaveCount(1);
        var group = grouped[0];
        _ = group.Should().HaveCount(3);
        _ = group.Key.Should().Be("/GetIt/v1/DirectReports/StandardId/AAA");
    }

    [Fact]
    public void CreateMultipleGroups_WhenSomePathsMatch()
    {
        var results = new[]
        {
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", "1"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", "2"),
            new ApiPathResult("/GetIt/v1/Users/Active/User1", "3"),
            new ApiPathResult("/GetIt/v1/Users/Active/User1", "4")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        _ = grouped.Should().HaveCount(2);
        _ = grouped.Should().AllSatisfy(g => g.Should().HaveCount(2));
    }
}
