namespace ApiPathsCountService.Tests;

using FluentAssertions;

public class ApiPathsCountServiceIntegration_Should
{
    [Fact]
    public async Task LoadGroupAndSummarize_WhenProcessingCompleteWorkflow()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/GetIt/v1/DirectReports/StandardId/ZKDPUWW","Count":"1"}},
                {"preview":false,"result":{"Path":"/GetIt/v1/DirectReports/StandardId/ZKDPUWW","Count":"2"}},
                {"preview":false,"result":{"Path":"/GetIt/v1/DirectReports/StandardId/ZEDPUWW","Count":"3"}},
                {"preview":false,"result":{"Path":"/GetIt/v1/DirectReports/Name/Glen","Count":"5"}},
                {"preview":false,"result":{"Path":"/GetIt/v1/DirectReports/Name/Glen","Count":"10"}}
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

            // All paths group under two prefixes: /GetIt/v1/DirectReports/StandardId and /GetIt/v1/DirectReports/Name
            _ = summaries.Should().HaveCount(2);
            
            var standardIdSummary = summaries.FirstOrDefault(s => s.PathPrefix == "/GetIt/v1/DirectReports/StandardId");
            _ = standardIdSummary.Should().NotBeNull();
            _ = standardIdSummary!.TotalCount.Should().Be(6);
            _ = standardIdSummary.OccurrenceCount.Should().Be(3);
            
            var nameSummary = summaries.FirstOrDefault(s => s.PathPrefix == "/GetIt/v1/DirectReports/Name");
            _ = nameSummary.Should().NotBeNull();
            _ = nameSummary!.TotalCount.Should().Be(15);
            _ = nameSummary.OccurrenceCount.Should().Be(2);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadGroupAndSummarize_WhenProcessingCompleteWorkflowDifferenceInMiddel()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/GetIt/v1/DirectReports/StandardId/ZKDPUWW","Count":"1"}},
                {"preview":false,"result":{"Path":"/GetIt/v1/DirectReports/StandardId/ZKDPUWW","Count":"2"}},
                {"preview":false,"result":{"Path":"/GetIt/v1/DirectReports/StandardId/ZKDPUWW","Count":"3"}},
                {"preview":false,"result":{"Path":"/GetIt/v1/DirectReports/Name/ZKDPUWW","Count":"5"}},
                {"preview":false,"result":{"Path":"/GetIt/v1/DirectReports/Name/ZKDPUWW","Count":"10"}}
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

            // All paths group under two prefixes: /GetIt/v1/DirectReports/StandardId and /GetIt/v1/DirectReports/Name
            _ = summaries.Should().HaveCount(2);

            var standardIdSummary = summaries.FirstOrDefault(s => s.PathPrefix == "/GetIt/v1/DirectReports/StandardId");
            _ = standardIdSummary.Should().NotBeNull();
            _ = standardIdSummary!.TotalCount.Should().Be(6);
            _ = standardIdSummary.OccurrenceCount.Should().Be(3);

            var nameSummary = summaries.FirstOrDefault(s => s.PathPrefix == "/GetIt/v1/DirectReports/Name");
            _ = nameSummary.Should().NotBeNull();
            _ = nameSummary!.TotalCount.Should().Be(15);
            _ = nameSummary.OccurrenceCount.Should().Be(2);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ProcessRealWorldData_WhenHandlingMultipleGroups()
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

            // Two groups: StandardId (3 items) and Name (3 items)
            _ = summaries.Should().HaveCount(2);
            _ = summaries.Sum(s => s.TotalCount).Should().Be(9);
            _ = summaries.Sum(s => s.OccurrenceCount).Should().Be(6);
            _ = summaries.Should().Contain(s => s.PathPrefix == "/GetIt/v1/DirectReports/StandardId" && s.OccurrenceCount == 3);
            _ = summaries.Should().Contain(s => s.PathPrefix == "/GetIt/v1/DirectReports/Name" && s.OccurrenceCount == 3);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task HandleDuplicatePathsAcrossWorkflow_WhenChainingAllMethods()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/users/get","Count":"100"}},
                {"preview":false,"result":{"Path":"/api/users/get","Count":"200"}},
                {"preview":false,"result":{"Path":"/api/users/get","Count":"300"}},
                {"preview":false,"result":{"Path":"/api/users/post","Count":"50"}},
                {"preview":false,"result":{"Path":"/api/users/post","Count":"75"}}
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

            // All paths group under "/api/users" prefix
            _ = summaries.Should().HaveCount(1);
            _ = summaries[0].PathPrefix.Should().Be("/api/users");
            _ = summaries[0].TotalCount.Should().Be(725);
            _ = summaries[0].OccurrenceCount.Should().Be(5);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ProcessEmptyResults_WhenFileHasNoData()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var response = await ApiPathsCountService.LoadFromFileAsync(tempFile);
            var allResults = ApiPathsCountService.GetAllApiPathResults(response!);
            var grouped = ApiPathsCountService.GroupByPathPrefix(allResults);
            var summaries = ApiPathsCountService.GetGroupSummaries(grouped).ToList();

            _ = summaries.Should().BeEmpty();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task HandleMixedValidAndInvalidCounts_WhenProcessingFullPipeline()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/test","Count":"10"}},
                {"preview":false,"result":{"Path":"/api/test","Count":"invalid"}},
                {"preview":false,"result":{"Path":"/api/test","Count":"20"}},
                {"preview":false,"result":{"Path":"/api/other","Count":"not-a-number"}},
                {"preview":false,"result":{"Path":"/api/other","Count":"5"}}
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

            // All paths group under "/api" prefix
            _ = summaries.Should().HaveCount(1);
            _ = summaries[0].PathPrefix.Should().Be("/api");
            _ = summaries[0].TotalCount.Should().Be(35);
            _ = summaries[0].OccurrenceCount.Should().Be(5);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ProcessLargeDataset_WhenHandlingManyPaths()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/v1/users/1","Count":"10"}},
                {"preview":false,"result":{"Path":"/api/v1/users/2","Count":"20"}},
                {"preview":false,"result":{"Path":"/api/v1/users/3","Count":"30"}},
                {"preview":false,"result":{"Path":"/api/v1/users/1","Count":"5"}},
                {"preview":false,"result":{"Path":"/api/v1/products/100","Count":"100"}},
                {"preview":false,"result":{"Path":"/api/v1/products/200","Count":"200"}},
                {"preview":false,"result":{"Path":"/api/v1/products/100","Count":"50"}},
                {"preview":false,"result":{"Path":"/api/v2/orders/A","Count":"1000"}},
                {"preview":false,"result":{"Path":"/api/v2/orders/B","Count":"2000"}},
                {"preview":false,"result":{"Path":"/api/v2/orders/A","Count":"500"}}
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

            // Three groups: /api/v1/users, /api/v1/products, /api/v2/orders
            _ = summaries.Should().HaveCount(3);
            _ = summaries.Sum(s => s.TotalCount).Should().Be(3915);
            
            var usersSum = summaries.FirstOrDefault(s => s.PathPrefix == "/api/v1/users");
            _ = usersSum.Should().NotBeNull();
            _ = usersSum!.TotalCount.Should().Be(65);
            _ = usersSum.OccurrenceCount.Should().Be(4);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PreserveDataIntegrity_WhenChainingAllOperations()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":true,"result":{"Path":"/test/path/alpha","Count":"42"}},
                {"preview":false,"result":{"Path":"/test/path/beta","Count":"24"}},
                {"preview":true,"result":{"Path":"/test/path/alpha","Count":"8"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var response = await ApiPathsCountService.LoadFromFileAsync(tempFile);
            
            _ = response.Should().NotBeNull();
            _ = response!.Results.Should().HaveCount(3);
            
            var allResults = ApiPathsCountService.GetAllApiPathResults(response).ToList();
            _ = allResults.Should().HaveCount(3);
            
            var grouped = ApiPathsCountService.GroupByPathPrefix(allResults).ToList();
            // All paths group under "/test/path" prefix
            _ = grouped.Should().HaveCount(1);
            
            var summaries = ApiPathsCountService.GetGroupSummaries(grouped).ToList();
            _ = summaries.Should().HaveCount(1);
            _ = summaries[0].PathPrefix.Should().Be("/test/path");
            _ = summaries[0].TotalCount.Should().Be(74);
            _ = summaries[0].OccurrenceCount.Should().Be(3);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task HandleSinglePath_WhenOnlyOneUniquePathExists()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/single/path","Count":"1"}},
                {"preview":false,"result":{"Path":"/single/path","Count":"2"}},
                {"preview":false,"result":{"Path":"/single/path","Count":"3"}},
                {"preview":false,"result":{"Path":"/single/path","Count":"4"}},
                {"preview":false,"result":{"Path":"/single/path","Count":"5"}}
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

            _ = summaries.Should().HaveCount(1);
            _ = summaries[0].PathPrefix.Should().Be("/single");
            _ = summaries[0].TotalCount.Should().Be(15);
            _ = summaries[0].OccurrenceCount.Should().Be(5);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ProcessComplexPaths_WhenHandlingDeepHierarchy()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/v1/resource/sub/item/detail/full","Count":"10"}},
                {"preview":false,"result":{"Path":"/api/v1/resource/sub/item/detail/full","Count":"20"}},
                {"preview":false,"result":{"Path":"/api/v1/resource/sub/item/detail/summary","Count":"5"}},
                {"preview":false,"result":{"Path":"/api/v2/orders/A","Count":"1000"}},
                {"preview":false,"result":{"Path":"/api/v2/orders/B","Count":"2000"}},
                {"preview":false,"result":{"Path":"/api/v2/orders/A","Count":"500"}}
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

            // Two groups: /api/v1/resource/sub/item/detail (3 items) and /api/v2/different/path (1 item)
            _ = summaries.Should().HaveCount(2);
            
            var detailSummary = summaries.FirstOrDefault(s => s.PathPrefix == "/api/v1/resource/sub/item/detail");
            _ = detailSummary.Should().NotBeNull();
            _ = detailSummary!.TotalCount.Should().Be(35);
            _ = detailSummary.OccurrenceCount.Should().Be(3);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task CalculateCorrectTotals_WhenMultipleGroupsWithVariousCounts()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/path/A","Count":"1000"}},
                {"preview":false,"result":{"Path":"/path/A","Count":"2000"}},
                {"preview":false,"result":{"Path":"/path/A","Count":"3000"}},
                {"preview":false,"result":{"Path":"/path/B","Count":"100"}},
                {"preview":false,"result":{"Path":"/path/C","Count":"10"}},
                {"preview":false,"result":{"Path":"/path/C","Count":"20"}},
                {"preview":false,"result":{"Path":"/path/C","Count":"30"}}
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

            // All paths group under "/path" prefix
            _ = summaries.Should().HaveCount(1);
            _ = summaries.Sum(s => s.TotalCount).Should().Be(6160);
            _ = summaries.Sum(s => s.OccurrenceCount).Should().Be(7);
            _ = summaries[0].PathPrefix.Should().Be("/path");
            _ = summaries[0].TotalCount.Should().Be(6160);
            _ = summaries[0].OccurrenceCount.Should().Be(7);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ChainMethodsFluentlyInSingleStatement_WhenProcessingData()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/fluent/api/test","Count":"10"}},
                {"preview":false,"result":{"Path":"/fluent/api/test","Count":"20"}},
                {"preview":false,"result":{"Path":"/fluent/api/prod","Count":"30"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);
            var summaries = ApiPathsCountService.GetGroupSummaries(
                ApiPathsCountService.GroupByPathPrefix(
                    ApiPathsCountService.GetAllApiPathResults(
                        (await ApiPathsCountService.LoadFromFileAsync(tempFile))!))).ToList();

            // All paths group under "/fluent/api" prefix
            _ = summaries.Should().HaveCount(1);
            _ = summaries.Sum(s => s.TotalCount).Should().Be(60);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ChainMethodsWithLinqStyleFluency_WhenFilteringResults()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/users/admin","Count":"100"}},
                {"preview":false,"result":{"Path":"/api/users/admin","Count":"200"}},
                {"preview":false,"result":{"Path":"/api/users/guest","Count":"5"}},
                {"preview":false,"result":{"Path":"/api/products/list","Count":"50"}},
                {"preview":false,"result":{"Path":"/api/products/list","Count":"75"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var summariesWithHighCounts = ApiPathsCountService.GetGroupSummaries(
                    ApiPathsCountService.GroupByPathPrefix(
                        ApiPathsCountService.GetAllApiPathResults(
                            (await ApiPathsCountService.LoadFromFileAsync(tempFile))!)))
                .Where(s => s.TotalCount > 50)
                .OrderByDescending(s => s.TotalCount)
                .ToList();

            // Two groups: /api/users (total 305) and /api/products (total 125)
            _ = summariesWithHighCounts.Should().HaveCount(2);
            _ = summariesWithHighCounts[0].PathPrefix.Should().Be("/api/users");
            _ = summariesWithHighCounts[0].TotalCount.Should().Be(305);
            _ = summariesWithHighCounts[1].PathPrefix.Should().Be("/api/products");
            _ = summariesWithHighCounts[1].TotalCount.Should().Be(125);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ChainWithAggregationOperations_WhenCalculatingStatistics()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/stats/endpoint1","Count":"10"}},
                {"preview":false,"result":{"Path":"/stats/endpoint1","Count":"20"}},
                {"preview":false,"result":{"Path":"/stats/endpoint2","Count":"30"}},
                {"preview":false,"result":{"Path":"/stats/endpoint3","Count":"40"}},
                {"preview":false,"result":{"Path":"/stats/endpoint3","Count":"50"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var summaries = ApiPathsCountService.GetGroupSummaries(
                ApiPathsCountService.GroupByPathPrefix(
                    ApiPathsCountService.GetAllApiPathResults(
                        (await ApiPathsCountService.LoadFromFileAsync(tempFile))!))).ToList();

            // All paths group under "/stats" prefix
            _ = summaries.Should().HaveCount(1);
            var totalCount = summaries.Sum(s => s.TotalCount);
            var averageCount = summaries.Average(s => s.TotalCount);
            var maxCount = summaries.Max(s => s.TotalCount);
            var minCount = summaries.Min(s => s.TotalCount);

            _ = totalCount.Should().Be(150);
            _ = averageCount.Should().Be(150);
            _ = maxCount.Should().Be(150);
            _ = minCount.Should().Be(150);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ChainWithGroupingAndProjection_WhenTransformingData()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/transform/data/A","Count":"100"}},
                {"preview":false,"result":{"Path":"/transform/data/A","Count":"200"}},
                {"preview":false,"result":{"Path":"/transform/data/B","Count":"300"}},
                {"preview":false,"result":{"Path":"/transform/data/C","Count":"50"}},
                {"preview":false,"result":{"Path":"/transform/data/C","Count":"50"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var projectedData = ApiPathsCountService.GetGroupSummaries(
                    ApiPathsCountService.GroupByPathPrefix(
                        ApiPathsCountService.GetAllApiPathResults(
                            (await ApiPathsCountService.LoadFromFileAsync(tempFile))!)))
                .Select(s => new
                {
                    Path = s.PathPrefix,
                    Total = s.TotalCount,
                    Occurrences = s.OccurrenceCount,
                    Average = s.TotalCount / (double)s.OccurrenceCount
                })
                .ToList();

            // All paths group under "/transform/data" prefix
            _ = projectedData.Should().HaveCount(1);
            _ = projectedData[0].Path.Should().Be("/transform/data");
            _ = projectedData[0].Total.Should().Be(700);
            _ = projectedData[0].Average.Should().Be(140);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ChainWithConditionalFiltering_WhenProcessingSelectivePaths()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/v1/users","Count":"10"}},
                {"preview":false,"result":{"Path":"/api/v1/users","Count":"20"}},
                {"preview":false,"result":{"Path":"/api/v2/users","Count":"30"}},
                {"preview":false,"result":{"Path":"/api/v1/products","Count":"40"}},
                {"preview":false,"result":{"Path":"/api/v2/products","Count":"50"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var v1Summaries = ApiPathsCountService.GetGroupSummaries(
                    ApiPathsCountService.GroupByPathPrefix(
                        ApiPathsCountService.GetAllApiPathResults(
                            (await ApiPathsCountService.LoadFromFileAsync(tempFile))!)))
                .Where(s => s.PathPrefix.Contains("/api/v1"))
                .ToList();

            // One group with v1: /api/v1 (for both users and products)
            _ = v1Summaries.Should().HaveCount(1);
            _ = v1Summaries[0].PathPrefix.Should().Be("/api/v1");
            _ = v1Summaries.Sum(s => s.TotalCount).Should().Be(70);
            _ = v1Summaries.Should().AllSatisfy(s => s.PathPrefix.Should().Contain("/api/v1"));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ChainWithComplexTransformation_WhenBuildingReportData()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/report/sales/Q1","Count":"1000"}},
                {"preview":false,"result":{"Path":"/report/sales/Q1","Count":"2000"}},
                {"preview":false,"result":{"Path":"/report/sales/Q2","Count":"1500"}},
                {"preview":false,"result":{"Path":"/report/revenue/Q1","Count":"5000"}},
                {"preview":false,"result":{"Path":"/report/revenue/Q2","Count":"6000"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var report = ApiPathsCountService.GetGroupSummaries(
                    ApiPathsCountService.GroupByPathPrefix(
                        ApiPathsCountService.GetAllApiPathResults(
                            (await ApiPathsCountService.LoadFromFileAsync(tempFile))!)))
                .GroupBy(s => s.PathPrefix.Contains("sales") ? "Sales" : "Revenue")
                .Select(g => new
                {
                    Category = g.Key,
                    TotalValue = g.Sum(s => s.TotalCount),
                    ItemCount = g.Count(),
                    AverageValue = g.Average(s => s.TotalCount)
                })
                .OrderByDescending(r => r.TotalValue)
                .ToList();

            _ = report.Should().HaveCount(2);
            _ = report[0].Category.Should().Be("Revenue");
            _ = report[0].TotalValue.Should().Be(11000);
            _ = report[1].Category.Should().Be("Sales");
            _ = report[1].TotalValue.Should().Be(4500);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ProcessQueryStringParameters_WhenPathsContainQueries()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/search?q=test&limit=10","Count":"50"}},
                {"preview":false,"result":{"Path":"/api/search?q=test&limit=10","Count":"75"}},
                {"preview":false,"result":{"Path":"/api/search?q=demo&limit=20","Count":"30"}},
                {"preview":false,"result":{"Path":"/api/filter?category=books&sort=asc","Count":"100"}},
                {"preview":false,"result":{"Path":"/api/filter?category=books&sort=asc","Count":"150"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var summaries = ApiPathsCountService.GetGroupSummaries(
                ApiPathsCountService.GroupByPathPrefix(
                    ApiPathsCountService.GetAllApiPathResults(
                        (await ApiPathsCountService.LoadFromFileAsync(tempFile))!))).ToList();

            // All query string paths group under "/api" prefix (query strings are part of last segment)
            _ = summaries.Should().HaveCount(1);
            _ = summaries[0].PathPrefix.Should().Be("/api");
            _ = summaries[0].TotalCount.Should().Be(405);
            _ = summaries[0].OccurrenceCount.Should().Be(5);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ProcessRestfulResourceIds_WhenPathsUseResourceIdentifiers()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/v1/users/12345/profile","Count":"10"}},
                {"preview":false,"result":{"Path":"/api/v1/users/12345/profile","Count":"15"}},
                {"preview":false,"result":{"Path":"/api/v1/users/67890/profile","Count":"20"}},
                {"preview":false,"result":{"Path":"/api/v1/orders/ORD-001/status","Count":"5"}},
                {"preview":false,"result":{"Path":"/api/v1/orders/ORD-002/status","Count":"8"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var summaries = ApiPathsCountService.GetGroupSummaries(
                ApiPathsCountService.GroupByPathPrefix(
                    ApiPathsCountService.GetAllApiPathResults(
                        (await ApiPathsCountService.LoadFromFileAsync(tempFile))!))).ToList();

            _ = summaries.Should().HaveCount(4);
            
            var user12345Summary = summaries.FirstOrDefault(s => s.PathPrefix.Contains("12345"));
            _ = user12345Summary.Should().NotBeNull();
            _ = user12345Summary!.TotalCount.Should().Be(25);
            _ = user12345Summary.OccurrenceCount.Should().Be(2);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ProcessMatrixParameters_WhenPathsUseMatrixNotation()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/products;color=red;size=large","Count":"30"}},
                {"preview":false,"result":{"Path":"/api/products;color=red;size=large","Count":"45"}},
                {"preview":false,"result":{"Path":"/api/products;color=blue;size=medium","Count":"25"}},
                {"preview":false,"result":{"Path":"/api/items;type=electronics;brand=sony","Count":"100"}},
                {"preview":false,"result":{"Path":"/api/items;type=electronics;brand=sony","Count":"200"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var summaries = ApiPathsCountService.GetGroupSummaries(
                ApiPathsCountService.GroupByPathPrefix(
                    ApiPathsCountService.GetAllApiPathResults(
                        (await ApiPathsCountService.LoadFromFileAsync(tempFile))!))).ToList();

            // All paths group under "/api" prefix
            _ = summaries.Should().HaveCount(1);
            _ = summaries[0].PathPrefix.Should().Be("/api");
            _ = summaries[0].TotalCount.Should().Be(400);
            _ = summaries[0].OccurrenceCount.Should().Be(5);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ProcessPathWithFragments_WhenPathsIncludeHashFragments()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/documents/doc123#section1","Count":"12"}},
                {"preview":false,"result":{"Path":"/api/documents/doc123#section1","Count":"18"}},
                {"preview":false,"result":{"Path":"/api/documents/doc123#section2","Count":"25"}},
                {"preview":false,"result":{"Path":"/api/pages/home#intro","Count":"50"}},
                {"preview":false,"result":{"Path":"/api/pages/home#intro","Count":"75"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var summaries = ApiPathsCountService.GetGroupSummaries(
                ApiPathsCountService.GroupByPathPrefix(
                    ApiPathsCountService.GetAllApiPathResults(
                        (await ApiPathsCountService.LoadFromFileAsync(tempFile))!))).ToList();

            // Two groups: /api/documents and /api/pages
            _ = summaries.Should().HaveCount(2);
            
            var docsSummary = summaries.FirstOrDefault(s => s.PathPrefix == "/api/documents");
            _ = docsSummary.Should().NotBeNull();
            _ = docsSummary!.TotalCount.Should().Be(55);
            _ = docsSummary.OccurrenceCount.Should().Be(3);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ProcessVersionedApiPaths_WhenMultipleVersionsExist()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/v1/users/list","Count":"100"}},
                {"preview":false,"result":{"Path":"/api/v2/users/list","Count":"200"}},
                {"preview":false,"result":{"Path":"/api/v3/users/list","Count":"300"}},
                {"preview":false,"result":{"Path":"/api/v1/users/list","Count":"50"}},
                {"preview":false,"result":{"Path":"/api/v2/users/list","Count":"75"}},
                {"preview":false,"result":{"Path":"/api/v3/users/list","Count":"125"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var summaries = ApiPathsCountService.GetGroupSummaries(
                ApiPathsCountService.GroupByPathPrefix(
                    ApiPathsCountService.GetAllApiPathResults(
                        (await ApiPathsCountService.LoadFromFileAsync(tempFile))!)))
                .OrderBy(s => s.PathPrefix)
                .ToList();

            _ = summaries.Should().HaveCount(3);
            _ = summaries[0].PathPrefix.Should().Contain("/api/v1/");
            _ = summaries[0].TotalCount.Should().Be(150);
            _ = summaries[1].PathPrefix.Should().Contain("/api/v2/");
            _ = summaries[1].TotalCount.Should().Be(275);
            _ = summaries[2].PathPrefix.Should().Contain("/api/v3/");
            _ = summaries[2].TotalCount.Should().Be(425);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ProcessNestedResourcePaths_WhenUsingNestedRelationships()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/companies/123/departments/456/employees","Count":"50"}},
                {"preview":false,"result":{"Path":"/api/companies/123/departments/456/employees","Count":"75"}},
                {"preview":false,"result":{"Path":"/api/companies/123/departments/789/employees","Count":"30"}},
                {"preview":false,"result":{"Path":"/api/schools/ABC/classes/101/students","Count":"25"}},
                {"preview":false,"result":{"Path":"/api/schools/ABC/classes/102/students","Count":"28"}}
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

            _ = summaries.Should().HaveCount(4);
            
            var dept456Summary = summaries.FirstOrDefault(s => s.PathPrefix.Contains("456"));
            _ = dept456Summary.Should().NotBeNull();
            _ = dept456Summary!.TotalCount.Should().Be(125);
            _ = dept456Summary.OccurrenceCount.Should().Be(2);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ProcessHttpMethodsInPaths_WhenPathsIncludeActions()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/users/create","Count":"10"}},
                {"preview":false,"result":{"Path":"/api/users/create","Count":"15"}},
                {"preview":false,"result":{"Path":"/api/users/update","Count":"20"}},
                {"preview":false,"result":{"Path":"/api/users/delete","Count":"5"}},
                {"preview":false,"result":{"Path":"/api/products/create","Count":"30"}},
                {"preview":false,"result":{"Path":"/api/products/create","Count":"40"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var summaries = ApiPathsCountService.GetGroupSummaries(
                ApiPathsCountService.GroupByPathPrefix(
                    ApiPathsCountService.GetAllApiPathResults(
                        (await ApiPathsCountService.LoadFromFileAsync(tempFile))!)))
            .GroupBy(s => s.PathPrefix.Split('/')[^1])
            .Select(g => new
            {
                Action = g.Key,
                TotalCalls = g.Sum(s => s.TotalCount),
                UniqueEndpoints = g.Count()
            })
            .OrderByDescending(x => x.TotalCalls)
            .ToList();

        // Two prefix groups: /api/users and /api/products
        // After re-grouping by last segment: "users" and "products"
        _ = summaries.Should().HaveCount(2);
        _ = summaries[0].Action.Should().Be("products");
        _ = summaries[0].TotalCalls.Should().Be(70);
        _ = summaries[0].UniqueEndpoints.Should().Be(1);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ProcessLocaleParameters_WhenPathsIncludeLanguageCodes()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/content/en-US/articles","Count":"100"}},
                {"preview":false,"result":{"Path":"/api/content/en-US/articles","Count":"150"}},
                {"preview":false,"result":{"Path":"/api/content/fr-FR/articles","Count":"80"}},
                {"preview":false,"result":{"Path":"/api/content/es-ES/articles","Count":"60"}},
                {"preview":false,"result":{"Path":"/api/content/de-DE/articles","Count":"90"}},
                {"preview":false,"result":{"Path":"/api/content/de-DE/articles","Count":"110"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var summaries = ApiPathsCountService.GetGroupSummaries(
                ApiPathsCountService.GroupByPathPrefix(
                    ApiPathsCountService.GetAllApiPathResults(
                        (await ApiPathsCountService.LoadFromFileAsync(tempFile))!)))
            .OrderByDescending(s => s.TotalCount)
            .ToList();

            // Four groups by locale: en-US, de-DE, fr-FR, es-ES
            _ = summaries.Should().HaveCount(4);
            _ = summaries[0].PathPrefix.Should().Be("/api/content/en-US");
            _ = summaries[0].TotalCount.Should().Be(250);
            _ = summaries[1].PathPrefix.Should().Be("/api/content/de-DE");
            _ = summaries[1].TotalCount.Should().Be(200);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ProcessGuidIdentifiers_WhenPathsUseGuids()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/sessions/550e8400-e29b-41d4-a716-446655440000","Count":"25"}},
                {"preview":false,"result":{"Path":"/api/sessions/550e8400-e29b-41d4-a716-446655440000","Count":"35"}},
                {"preview":false,"result":{"Path":"/api/sessions/7c9e6679-7425-40de-944b-e07fc1f90ae7","Count":"45"}},
                {"preview":false,"result":{"Path":"/api/tokens/123e4567-e89b-12d3-a456-426614174000","Count":"100"}},
                {"preview":false,"result":{"Path":"/api/tokens/123e4567-e89b-12d3-a456-426614174000","Count":"200"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var summaries = ApiPathsCountService.GetGroupSummaries(
                ApiPathsCountService.GroupByPathPrefix(
                    ApiPathsCountService.GetAllApiPathResults(
                        (await ApiPathsCountService.LoadFromFileAsync(tempFile))!))).ToList();

            // Two groups: /api/sessions (3 items) and /api/tokens (2 items)
            _ = summaries.Should().HaveCount(2);
            
            var sessionsSummary = summaries.FirstOrDefault(s => s.PathPrefix == "/api/sessions");
            _ = sessionsSummary.Should().NotBeNull();
            _ = sessionsSummary!.TotalCount.Should().Be(105);
            _ = sessionsSummary.OccurrenceCount.Should().Be(3);
            
            var tokensSummary = summaries.FirstOrDefault(s => s.PathPrefix == "/api/tokens");
            _ = tokensSummary.Should().NotBeNull();
            _ = tokensSummary!.TotalCount.Should().Be(300);
            _ = tokensSummary.OccurrenceCount.Should().Be(2);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ProcessDateTimeParameters_WhenPathsIncludeDateRanges()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/reports/2024-01-01/2024-01-31","Count":"500"}},
                {"preview":false,"result":{"Path":"/api/reports/2024-01-01/2024-01-31","Count":"750"}},
                {"preview":false,"result":{"Path":"/api/reports/2024-02-01/2024-02-29","Count":"600"}},
                {"preview":false,"result":{"Path":"/api/analytics/2024-01-15","Count":"200"}},
                {"preview":false,"result":{"Path":"/api/analytics/2024-01-15","Count":"300"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var summaries = ApiPathsCountService.GetGroupSummaries(
                ApiPathsCountService.GroupByPathPrefix(
                    ApiPathsCountService.GetAllApiPathResults(
                        (await ApiPathsCountService.LoadFromFileAsync(tempFile))!))).ToList();

            _ = summaries.Should().HaveCount(3);
            
            var januaryReportSummary = summaries.FirstOrDefault(s => s.PathPrefix.Contains("2024-01-01"));
            _ = januaryReportSummary.Should().NotBeNull();
            _ = januaryReportSummary!.TotalCount.Should().Be(1250);
            _ = januaryReportSummary.OccurrenceCount.Should().Be(2);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ProcessEncodedParameters_WhenPathsUseUrlEncoding()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/search?q=hello%20world","Count":"40"}},
                {"preview":false,"result":{"Path":"/api/search?q=hello%20world","Count":"60"}},
                {"preview":false,"result":{"Path":"/api/search?q=test%26demo","Count":"30"}},
                {"preview":false,"result":{"Path":"/api/files/document%2Epdf","Count":"15"}},
                {"preview":false,"result":{"Path":"/api/files/document%2Epdf","Count":"25"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var summaries = ApiPathsCountService.GetGroupSummaries(
                ApiPathsCountService.GroupByPathPrefix(
                    ApiPathsCountService.GetAllApiPathResults(
                        (await ApiPathsCountService.LoadFromFileAsync(tempFile))!))).ToList();

            // Two groups: /api (for search queries) and /api/files
            _ = summaries.Should().HaveCount(2);
            
            var searchSummary = summaries.FirstOrDefault(s => s.PathPrefix == "/api");
            _ = searchSummary.Should().NotBeNull();
            _ = searchSummary!.TotalCount.Should().Be(130);
            _ = searchSummary.OccurrenceCount.Should().Be(3);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ProcessPaginationParameters_WhenPathsIncludePageInfo()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/items?page=1&size=50","Count":"1000"}},
                {"preview":false,"result":{"Path":"/api/items?page=2&size=50","Count":"800"}},
                {"preview":false,"result":{"Path":"/api/items?page=3&size=50","Count":"600"}},
                {"preview":false,"result":{"Path":"/api/items?page=1&size=50","Count":"500"}},
                {"preview":false,"result":{"Path":"/api/users?offset=0&limit=25","Count":"300"}},
                {"preview":false,"result":{"Path":"/api/users?offset=25&limit=25","Count":"250"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var summaries = ApiPathsCountService.GetGroupSummaries(
                ApiPathsCountService.GroupByPathPrefix(
                    ApiPathsCountService.GetAllApiPathResults(
                        (await ApiPathsCountService.LoadFromFileAsync(tempFile))!)))
            .OrderByDescending(s => s.TotalCount)
            .ToList();

            // All paths group under "/api" prefix
            _ = summaries.Should().HaveCount(1);
            _ = summaries[0].PathPrefix.Should().Be("/api");
            _ = summaries[0].TotalCount.Should().Be(3450);
            _ = summaries[0].OccurrenceCount.Should().Be(6);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task HandleStressTest_WhenProcessingThousandsOfPaths()
    {
        var tempFile = Path.GetTempFileName();
        var results = new System.Text.StringBuilder("{\"results\":[");
        for (var i = 0; i < 5000; i++)
        {
            if (i > 0)
            {
                _ = results.Append(',');
            }

            var path = $"/api/path{i % 100}/item{i}";
            _ = results.Append($"{{\"preview\":false,\"result\":{{\"Path\":\"{path}\",\"Count\":\"{i % 10}\"}}}}");
        }
        
        _ = results.Append("]}");

        try
        {
            await File.WriteAllTextAsync(tempFile, results.ToString());

            var summaries = ApiPathsCountService.GetGroupSummaries(
                ApiPathsCountService.GroupByPathPrefix(
                    ApiPathsCountService.GetAllApiPathResults(
                        (await ApiPathsCountService.LoadFromFileAsync(tempFile))!))).ToList();

            // 100 groups (path0 through path99), each with 50 items
            _ = summaries.Should().HaveCount(100);
            _ = summaries.Sum(s => s.OccurrenceCount).Should().Be(5000);
            _ = summaries.Should().AllSatisfy(s => s.OccurrenceCount.Should().Be(50));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task HandleUnicodePaths_WhenProcessingInternationalCharacters()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/??/??","Count":"10"}},
                {"preview":false,"result":{"Path":"/api/??/??","Count":"20"}},
                {"preview":false,"result":{"Path":"/api/????????????/??????","Count":"30"}},
                {"preview":false,"result":{"Path":"/api/??????/??????","Count":"40"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var summaries = ApiPathsCountService.GetGroupSummaries(
                ApiPathsCountService.GroupByPathPrefix(
                    ApiPathsCountService.GetAllApiPathResults(
                        (await ApiPathsCountService.LoadFromFileAsync(tempFile))!))).ToList();

            _ = summaries.Should().HaveCount(3);
            var chineseSummary = summaries.FirstOrDefault(s => s.PathPrefix.Contains("??"));
            _ = chineseSummary!.TotalCount.Should().Be(30);
            _ = chineseSummary.OccurrenceCount.Should().Be(2);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task HandleEdgeCaseCounts_WhenCountsHaveVariousFormats()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/test","Count":"100"}},
                {"preview":false,"result":{"Path":"/api/test","Count":"abc"}},
                {"preview":false,"result":{"Path":"/api/test","Count":""}},
                {"preview":false,"result":{"Path":"/api/test","Count":"  50  "}},
                {"preview":false,"result":{"Path":"/api/test","Count":"10.5"}},
                {"preview":false,"result":{"Path":"/api/test","Count":"-20"}},
                {"preview":false,"result":{"Path":"/api/test","Count":"999999999999999"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var summaries = ApiPathsCountService.GetGroupSummaries(
                ApiPathsCountService.GroupByPathPrefix(
                    ApiPathsCountService.GetAllApiPathResults(
                        (await ApiPathsCountService.LoadFromFileAsync(tempFile))!))).ToList();

            _ = summaries.Should().HaveCount(1);
            _ = summaries[0].TotalCount.Should().BeGreaterOrEqualTo(100);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task HandleMixedPathFormats_WhenProcessingDiversePaths()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/","Count":"10"}},
                {"preview":false,"result":{"Path":"single","Count":"20"}},
                {"preview":false,"result":{"Path":"/api//double//slash","Count":"30"}},
                {"preview":false,"result":{"Path":"/path with spaces","Count":"40"}},
                {"preview":false,"result":{"Path":"/UPPERCASE/path","Count":"50"}},
                {"preview":false,"result":{"Path":"/UPPERCASE/path","Count":"60"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var summaries = ApiPathsCountService.GetGroupSummaries(
                ApiPathsCountService.GroupByPathPrefix(
                    ApiPathsCountService.GetAllApiPathResults(
                        (await ApiPathsCountService.LoadFromFileAsync(tempFile))!))).ToList();

            _ = summaries.Should().HaveCount(4);
            var uppercaseGroup = summaries.FirstOrDefault(s => s.PathPrefix == "/UPPERCASE");
            _ = uppercaseGroup!.TotalCount.Should().Be(110);
            _ = uppercaseGroup.OccurrenceCount.Should().Be(2);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task HandleAllInvalidCounts_WhenNoValidCountsExist()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/test","Count":"invalid"}},
                {"preview":false,"result":{"Path":"/api/test","Count":"not-a-number"}},
                {"preview":false,"result":{"Path":"/api/test","Count":"abc123"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var summaries = ApiPathsCountService.GetGroupSummaries(
                ApiPathsCountService.GroupByPathPrefix(
                    ApiPathsCountService.GetAllApiPathResults(
                        (await ApiPathsCountService.LoadFromFileAsync(tempFile))!))).ToList();

            _ = summaries.Should().HaveCount(1);
            _ = summaries[0].TotalCount.Should().Be(0);
            _ = summaries[0].OccurrenceCount.Should().Be(3);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task HandlePathsWithOnlySpecialCharacters_WhenProcessingSymbols()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"@#$%^&*()","Count":"10"}},
                {"preview":false,"result":{"Path":"@#$%^&*()","Count":"20"}},
                {"preview":false,"result":{"Path":"!@#$%","Count":"30"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var summaries = ApiPathsCountService.GetGroupSummaries(
                ApiPathsCountService.GroupByPathPrefix(
                    ApiPathsCountService.GetAllApiPathResults(
                        (await ApiPathsCountService.LoadFromFileAsync(tempFile))!))).ToList();

            _ = summaries.Should().HaveCount(2);
            var specialGroup = summaries.FirstOrDefault(s => s.PathPrefix.Contains("@#$%^&*()"));
            _ = specialGroup!.TotalCount.Should().Be(30);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ChainWithComplexFiltering_WhenApplyingMultipleConditions()
    {
        var tempFile = Path.GetTempFileName();
        var testJson = """
        {
            "results":[
                {"preview":false,"result":{"Path":"/api/v1/high/priority","Count":"1000"}},
                {"preview":false,"result":{"Path":"/api/v1/high/priority","Count":"2000"}},
                {"preview":false,"result":{"Path":"/api/v1/low/priority","Count":"10"}},
                {"preview":false,"result":{"Path":"/api/v2/high/priority","Count":"500"}},
                {"preview":false,"result":{"Path":"/api/v2/low/priority","Count":"5"}}
            ]
        }
        """;
        try
        {
            await File.WriteAllTextAsync(tempFile, testJson);

            var filteredSummaries = ApiPathsCountService.GetGroupSummaries(
                    ApiPathsCountService.GroupByPathPrefix(
                        ApiPathsCountService.GetAllApiPathResults(
                            (await ApiPathsCountService.LoadFromFileAsync(tempFile))!)))
                .Where(s => s.PathPrefix.Contains("v1") && s.PathPrefix.Contains("high"))
                .OrderByDescending(s => s.TotalCount)
                .ToList();

            _ = filteredSummaries.Should().HaveCount(1);
            _ = filteredSummaries[0].TotalCount.Should().Be(3000);
            _ = filteredSummaries[0].OccurrenceCount.Should().Be(2);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
