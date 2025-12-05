namespace ApiPathsCountService.Tests;

using global::ApiPathsCountService.Models;
using FluentAssertions;

public class GetGroupSummaries_Should
{
    [Fact]
    public void CalculateSumOfCounts_WhenGroupHasMultipleItems()
    {
        var results = new[]
        {
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", "1"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZEDPUWW", "2"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZFDPUWW", "3")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results);
        var summaries = ApiPathsCountService.GetGroupSummaries(grouped).ToList();

        _ = summaries.Should().HaveCount(3);
        _ = summaries.Should().AllSatisfy(s => s.OccurrenceCount.Should().Be(1));
    }

    [Fact]
    public void CalculateSumOfCounts_WhenGroupHasMultipleItemsThatAreTheSame()
    {
        var results = new[]
        {
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", "1"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", "2"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", "3")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results);
        var summaries = ApiPathsCountService.GetGroupSummaries(grouped).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].PathPrefix.Should().Be("/GetIt/v1/DirectReports/StandardId/ZKDPUWW");
        _ = summaries[0].TotalCount.Should().Be(6);
        _ = summaries[0].OccurrenceCount.Should().Be(3);
    }

    [Fact]
    public void CalculateSumForMultipleGroups_WhenMultipleGroupsExist()
    {
        var results = new[]
        {
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", "1"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZEDPUWW", "2"),
            new ApiPathResult("/GetIt/v1/DirectReports/Name/Glen", "5"),
            new ApiPathResult("/GetIt/v1/DirectReports/Name/Marcus", "10")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results);
        var summaries = ApiPathsCountService.GetGroupSummaries(grouped).ToList();

        _ = summaries.Should().HaveCount(4);
        _ = summaries.Should().AllSatisfy(s => s.OccurrenceCount.Should().Be(1));
    }

    [Fact]
    public void ReturnEmptyEnumerable_WhenNoGroupsProvided()
    {
        var emptyGroups = Enumerable.Empty<IGrouping<string, ApiPathResult>>();

        var summaries = ApiPathsCountService.GetGroupSummaries(emptyGroups);

        _ = summaries.Should().BeEmpty();
    }

    [Fact]
    public void HandleSingleItemGroup_WhenGroupHasOneItem()
    {
        var results = new[]
        {
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", "42")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results);
        var summaries = ApiPathsCountService.GetGroupSummaries(grouped).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].TotalCount.Should().Be(42);
        _ = summaries[0].OccurrenceCount.Should().Be(1);
    }

    [Fact]
    public void HandleZeroCounts_WhenCountsAreZero()
    {
        var results = new[]
        {
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", "0"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZEDPUWW", "0"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZFDPUWW", "0")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results);
        var summaries = ApiPathsCountService.GetGroupSummaries(grouped).ToList();

        _ = summaries.Should().HaveCount(3);
        _ = summaries.Should().AllSatisfy(s => s.TotalCount.Should().Be(0));
        _ = summaries.Should().AllSatisfy(s => s.OccurrenceCount.Should().Be(1));
    }

    [Fact]
    public void HandleInvalidCounts_WhenCountsAreNotNumeric()
    {
        var results = new[]
        {
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", "invalid"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZEDPUWW", "5"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZFDPUWW", "not-a-number")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results);
        var summaries = ApiPathsCountService.GetGroupSummaries(grouped).ToList();

        _ = summaries.Should().HaveCount(3);
        
        var validSummary = summaries.FirstOrDefault(s => s.PathPrefix.Contains("ZEDPUWW"));
        _ = validSummary.Should().NotBeNull();
        _ = validSummary!.TotalCount.Should().Be(5);
        _ = validSummary.OccurrenceCount.Should().Be(1);
    }

    [Fact]
    public void CalculateLargeSums_WhenCountsAreLarge()
    {
        var results = new[]
        {
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", "1000"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", "2000"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", "3000")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results);
        var summaries = ApiPathsCountService.GetGroupSummaries(grouped).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].TotalCount.Should().Be(6000);
        _ = summaries[0].OccurrenceCount.Should().Be(3);
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
            var grouped = ApiPathsCountService.GroupByPathPrefix(allResults);
            var summaries = ApiPathsCountService.GetGroupSummaries(grouped).ToList();

            _ = summaries.Should().HaveCount(6);
            _ = summaries.Should().AllSatisfy(s => s.OccurrenceCount.Should().Be(1));
            
            var zkdpuwwSummary = summaries.FirstOrDefault(s => s.PathPrefix.Contains("ZKDPUWW"));
            _ = zkdpuwwSummary.Should().NotBeNull();
            _ = zkdpuwwSummary!.TotalCount.Should().Be(1);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void PreservePathPrefixInSummary_WhenCreatingSummaries()
    {
        var results = new[]
        {
            new ApiPathResult("/api/v1/users/profile/settings/display", "1"),
            new ApiPathResult("/api/v1/users/profile/settings/privacy", "2"),
            new ApiPathResult("/api/v1/users/profile/settings/security", "3")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results);
        var summaries = ApiPathsCountService.GetGroupSummaries(grouped).ToList();

        _ = summaries.Should().HaveCount(3);
        _ = summaries.Should().Contain(s => s.PathPrefix == "/api/v1/users/profile/settings/display");
        _ = summaries.Should().Contain(s => s.PathPrefix == "/api/v1/users/profile/settings/privacy");
        _ = summaries.Should().Contain(s => s.PathPrefix == "/api/v1/users/profile/settings/security");
    }

    [Fact]
    public void CalculateCorrectTotalsForEachGroup_WhenMultipleDifferentGroups()
    {
        var results = new[]
        {
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/A", "10"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/A", "20"),
            new ApiPathResult("/PostIt/v1/Users/Active/User1", "5"),
            new ApiPathResult("/PostIt/v1/Users/Active/User2", "15"),
            new ApiPathResult("/DeleteIt/v1/Items/Old/Item1", "100")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results);
        var summaries = ApiPathsCountService.GetGroupSummaries(grouped).ToList();

        _ = summaries.Should().HaveCount(4);
        
        var pathASummary = summaries.FirstOrDefault(s => s.PathPrefix.EndsWith("/A"));
        _ = pathASummary.Should().NotBeNull();
        _ = pathASummary!.TotalCount.Should().Be(30);
        _ = pathASummary.OccurrenceCount.Should().Be(2);
        
        _ = summaries.Should().Contain(s => s.PathPrefix.EndsWith("User1") && s.TotalCount == 5 && s.OccurrenceCount == 1);
        _ = summaries.Should().Contain(s => s.PathPrefix.EndsWith("User2") && s.TotalCount == 15 && s.OccurrenceCount == 1);
        _ = summaries.Should().Contain(s => s.PathPrefix.EndsWith("Item1") && s.TotalCount == 100 && s.OccurrenceCount == 1);
    }

    [Fact]
    public void ReturnEnumerableThatCanBeIteratedMultipleTimes()
    {
        var results = new[]
        {
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/A", "5"),
            new ApiPathResult("/GetIt/v1/DirectReports/StandardId/B", "10")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results);
        var summaries = ApiPathsCountService.GetGroupSummaries(grouped);

        var firstIteration = summaries.ToList();
        var secondIteration = summaries.ToList();

        _ = firstIteration.Should().HaveCount(2);
        _ = secondIteration.Should().HaveCount(2);
        _ = firstIteration[0].TotalCount.Should().Be(secondIteration[0].TotalCount);
    }
}
