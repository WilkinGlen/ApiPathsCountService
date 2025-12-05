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

            _ = summaries.Should().HaveCount(3);
            
            var zkdpuwwSummary = summaries.FirstOrDefault(s => s.PathPrefix.Contains("ZKDPUWW"));
            _ = zkdpuwwSummary.Should().NotBeNull();
            _ = zkdpuwwSummary!.TotalCount.Should().Be(3);
            _ = zkdpuwwSummary.OccurrenceCount.Should().Be(2);
            
            var zedpuwwSummary = summaries.FirstOrDefault(s => s.PathPrefix.Contains("ZEDPUWW"));
            _ = zedpuwwSummary.Should().NotBeNull();
            _ = zedpuwwSummary!.TotalCount.Should().Be(3);
            _ = zedpuwwSummary.OccurrenceCount.Should().Be(1);
            
            var glenSummary = summaries.FirstOrDefault(s => s.PathPrefix.Contains("Glen"));
            _ = glenSummary.Should().NotBeNull();
            _ = glenSummary!.TotalCount.Should().Be(15);
            _ = glenSummary.OccurrenceCount.Should().Be(2);
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

            _ = summaries.Should().HaveCount(6);
            _ = summaries.Sum(s => s.TotalCount).Should().Be(9);
            _ = summaries.Sum(s => s.OccurrenceCount).Should().Be(6);
            _ = summaries.Should().AllSatisfy(s => s.OccurrenceCount.Should().Be(1));
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

            _ = summaries.Should().HaveCount(2);
            
            var getSummary = summaries.FirstOrDefault(s => s.PathPrefix.Contains("get"));
            _ = getSummary.Should().NotBeNull();
            _ = getSummary!.TotalCount.Should().Be(600);
            _ = getSummary.OccurrenceCount.Should().Be(3);
            
            var postSummary = summaries.FirstOrDefault(s => s.PathPrefix.Contains("post"));
            _ = postSummary.Should().NotBeNull();
            _ = postSummary!.TotalCount.Should().Be(125);
            _ = postSummary.OccurrenceCount.Should().Be(2);
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

            _ = summaries.Should().HaveCount(2);
            
            var testSummary = summaries.FirstOrDefault(s => s.PathPrefix == "/api/test");
            _ = testSummary.Should().NotBeNull();
            _ = testSummary!.TotalCount.Should().Be(30);
            _ = testSummary.OccurrenceCount.Should().Be(3);
            
            var otherSummary = summaries.FirstOrDefault(s => s.PathPrefix == "/api/other");
            _ = otherSummary.Should().NotBeNull();
            _ = otherSummary!.TotalCount.Should().Be(5);
            _ = otherSummary.OccurrenceCount.Should().Be(2);
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

            _ = summaries.Should().HaveCount(7);
            _ = summaries.Sum(s => s.TotalCount).Should().Be(3915);
            
            var user1Summary = summaries.FirstOrDefault(s => s.PathPrefix == "/api/v1/users/1");
            _ = user1Summary.Should().NotBeNull();
            _ = user1Summary!.OccurrenceCount.Should().Be(2);
            _ = user1Summary.TotalCount.Should().Be(15);
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
            _ = grouped.Should().HaveCount(2);
            
            var summaries = ApiPathsCountService.GetGroupSummaries(grouped).ToList();
            _ = summaries.Should().HaveCount(2);
            
            var alphaSummary = summaries.FirstOrDefault(s => s.PathPrefix.Contains("alpha"));
            _ = alphaSummary.Should().NotBeNull();
            _ = alphaSummary!.TotalCount.Should().Be(50);
            _ = alphaSummary.OccurrenceCount.Should().Be(2);
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
            _ = summaries[0].PathPrefix.Should().Be("/single/path");
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
                {"preview":false,"result":{"Path":"/api/v2/different/path/structure","Count":"100"}}
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

            _ = summaries.Should().HaveCount(3);
            
            var fullSummary = summaries.FirstOrDefault(s => s.PathPrefix.Contains("full"));
            _ = fullSummary.Should().NotBeNull();
            _ = fullSummary!.TotalCount.Should().Be(30);
            _ = fullSummary.OccurrenceCount.Should().Be(2);
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

            _ = summaries.Should().HaveCount(3);
            _ = summaries.Sum(s => s.TotalCount).Should().Be(6160);
            _ = summaries.Sum(s => s.OccurrenceCount).Should().Be(7);
            
            var pathASummary = summaries.FirstOrDefault(s => s.PathPrefix == "/path/A");
            _ = pathASummary!.TotalCount.Should().Be(6000);
            _ = pathASummary.OccurrenceCount.Should().Be(3);
            
            var pathBSummary = summaries.FirstOrDefault(s => s.PathPrefix == "/path/B");
            _ = pathBSummary!.TotalCount.Should().Be(100);
            _ = pathBSummary.OccurrenceCount.Should().Be(1);
            
            var pathCSummary = summaries.FirstOrDefault(s => s.PathPrefix == "/path/C");
            _ = pathCSummary!.TotalCount.Should().Be(60);
            _ = pathCSummary.OccurrenceCount.Should().Be(3);
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

            _ = summaries.Should().HaveCount(2);
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

            _ = summariesWithHighCounts.Should().HaveCount(2);
            _ = summariesWithHighCounts[0].PathPrefix.Should().Contain("admin");
            _ = summariesWithHighCounts[0].TotalCount.Should().Be(300);
            _ = summariesWithHighCounts[1].PathPrefix.Should().Contain("list");
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

            var totalCount = summaries.Sum(s => s.TotalCount);
            var averageCount = summaries.Average(s => s.TotalCount);
            var maxCount = summaries.Max(s => s.TotalCount);
            var minCount = summaries.Min(s => s.TotalCount);

            _ = totalCount.Should().Be(150);
            _ = averageCount.Should().Be(50);
            _ = maxCount.Should().Be(90);
            _ = minCount.Should().Be(30);
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

            _ = projectedData.Should().HaveCount(3);
            _ = projectedData.First(p => p.Path.Contains('A')).Average.Should().Be(150);
            _ = projectedData.First(p => p.Path.Contains('B')).Average.Should().Be(300);
            _ = projectedData.First(p => p.Path.Contains('C')).Average.Should().Be(50);
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
                .Where(s => s.PathPrefix.Contains("/api/v1/"))
                .ToList();

            _ = v1Summaries.Should().HaveCount(2);
            _ = v1Summaries.Sum(s => s.TotalCount).Should().Be(70);
            _ = v1Summaries.Should().AllSatisfy(s => s.PathPrefix.Should().Contain("/api/v1/"));
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

            _ = summaries.Should().HaveCount(3);
            
            var testQuerySummary = summaries.FirstOrDefault(s => s.PathPrefix.Contains("q=test"));
            _ = testQuerySummary.Should().NotBeNull();
            _ = testQuerySummary!.TotalCount.Should().Be(125);
            _ = testQuerySummary.OccurrenceCount.Should().Be(2);
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

            _ = summaries.Should().HaveCount(3);
            
            var redLargeSummary = summaries.FirstOrDefault(s => s.PathPrefix.Contains("red") && s.PathPrefix.Contains("large"));
            _ = redLargeSummary.Should().NotBeNull();
            _ = redLargeSummary!.TotalCount.Should().Be(75);
            _ = redLargeSummary.OccurrenceCount.Should().Be(2);
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

            _ = summaries.Should().HaveCount(3);
            
            var section1Summary = summaries.FirstOrDefault(s => s.PathPrefix.Contains("#section1"));
            _ = section1Summary.Should().NotBeNull();
            _ = section1Summary!.TotalCount.Should().Be(30);
            _ = section1Summary.OccurrenceCount.Should().Be(2);
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

            var summaries = ApiPathsCountService.GetGroupSummaries(
                ApiPathsCountService.GroupByPathPrefix(
                    ApiPathsCountService.GetAllApiPathResults(
                        (await ApiPathsCountService.LoadFromFileAsync(tempFile))!))).ToList();

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

            _ = summaries.Should().HaveCount(3);
            _ = summaries[0].Action.Should().Be("create");
            _ = summaries[0].TotalCalls.Should().Be(95);
            _ = summaries[0].UniqueEndpoints.Should().Be(2);
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

            _ = summaries.Should().HaveCount(4);
            _ = summaries[0].PathPrefix.Should().Contain("en-US");
            _ = summaries[0].TotalCount.Should().Be(250);
            _ = summaries[1].PathPrefix.Should().Contain("de-DE");
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

            _ = summaries.Should().HaveCount(3);
            
            var firstSessionSummary = summaries.FirstOrDefault(s => s.PathPrefix.Contains("550e8400"));
            _ = firstSessionSummary.Should().NotBeNull();
            _ = firstSessionSummary!.TotalCount.Should().Be(60);
            _ = firstSessionSummary.OccurrenceCount.Should().Be(2);
            
            var tokenSummary = summaries.FirstOrDefault(s => s.PathPrefix.Contains("tokens"));
            _ = tokenSummary.Should().NotBeNull();
            _ = tokenSummary!.TotalCount.Should().Be(300);
            _ = tokenSummary.OccurrenceCount.Should().Be(2);
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

            _ = summaries.Should().HaveCount(3);
            
            var helloWorldSummary = summaries.FirstOrDefault(s => s.PathPrefix.Contains("hello%20world"));
            _ = helloWorldSummary.Should().NotBeNull();
            _ = helloWorldSummary!.TotalCount.Should().Be(100);
            _ = helloWorldSummary.OccurrenceCount.Should().Be(2);
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

            _ = summaries.Should().HaveCount(5);
            
            var page1Summary = summaries.FirstOrDefault(s => s.PathPrefix.Contains("page=1"));
            _ = page1Summary.Should().NotBeNull();
            _ = page1Summary!.TotalCount.Should().Be(1500);
            _ = page1Summary.OccurrenceCount.Should().Be(2);
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

            _ = summaries.Should().HaveCount(5000);
            _ = summaries.Sum(s => s.OccurrenceCount).Should().Be(5000);
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
            _ = summaries[0].OccurrenceCount.Should().Be(7);
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

            _ = summaries.Should().HaveCount(5);
            var uppercaseGroup = summaries.FirstOrDefault(s => s.PathPrefix.Contains("UPPERCASE"));
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
