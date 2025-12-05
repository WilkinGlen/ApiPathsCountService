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

        _ = grouped.Should().HaveCount(1);
        _ = grouped[0].Should().HaveCount(3);
        _ = grouped[0].Key.Should().Be("/GetIt/v1/DirectReports/StandardId");
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

        _ = grouped.Should().HaveCount(1);
        _ = grouped[0].Key.Should().Be("/GetIt/v1/DirectReports/StandardId");
        _ = grouped[0].Should().HaveCount(4);
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

        _ = grouped.Should().HaveCount(2);
        _ = grouped.Should().Contain(g => g.Key == "/GetIt/v1/DirectReports/StandardId" && g.Count() == 2);
        _ = grouped.Should().Contain(g => g.Key == "/PostIt/v1/Users" && g.Count() == 2);
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

            _ = grouped.Should().HaveCount(2);
            _ = grouped.Should().Contain(g => g.Key == "/GetIt/v1/DirectReports/StandardId" && g.Count() == 3);
            _ = grouped.Should().Contain(g => g.Key == "/GetIt/v1/DirectReports/Name" && g.Count() == 3);
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

        _ = grouped.Should().HaveCount(1);
        _ = grouped[0].Key.Should().Be("/api/v1/users/profile/settings");
        _ = grouped[0].Should().HaveCount(3);
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
        _ = group.Key.Should().Be("/GetIt/v1/DirectReports/StandardId");
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
        _ = grouped.Should().Contain(g => g.Key == "/GetIt/v1/DirectReports/StandardId" && g.Count() == 2);
        _ = grouped.Should().Contain(g => g.Key == "/GetIt/v1/Users/Active" && g.Count() == 2);
    }

    [Fact]
    public void GroupQueryStringPaths_WhenPathsHaveQueryParameters()
    {
        var results = new[]
        {
            new ApiPathResult("/api/search?q=test&limit=10", "50"),
            new ApiPathResult("/api/search?q=test&limit=10", "75"),
            new ApiPathResult("/api/search?q=demo&limit=20", "30")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        // All paths have the same prefix "/api" after removing "search?..." part
        _ = grouped.Should().HaveCount(1);
        _ = grouped[0].Key.Should().Be("/api");
        _ = grouped[0].Should().HaveCount(3);
    }

    [Fact]
    public void GroupGuidPaths_WhenPathsContainGuids()
    {
        var results = new[]
        {
            new ApiPathResult("/api/sessions/550e8400-e29b-41d4-a716-446655440000", "25"),
            new ApiPathResult("/api/sessions/550e8400-e29b-41d4-a716-446655440000", "35"),
            new ApiPathResult("/api/sessions/7c9e6679-7425-40de-944b-e07fc1f90ae7", "45")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        // All three paths have the same prefix "/api/sessions" after removing the GUID
        _ = grouped.Should().HaveCount(1);
        _ = grouped[0].Key.Should().Be("/api/sessions");
        _ = grouped[0].Should().HaveCount(3);
    }

    [Fact]
    public void GroupVersionedPaths_WhenPathsHaveDifferentVersions()
    {
        var results = new[]
        {
            new ApiPathResult("/api/v1/users/list", "100"),
            new ApiPathResult("/api/v1/users/list", "50"),
            new ApiPathResult("/api/v2/users/list", "200"),
            new ApiPathResult("/api/v3/users/list", "300")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        _ = grouped.Should().HaveCount(3);
        var v1Group = grouped.FirstOrDefault(g => g.Key.Contains("/v1/"));
        _ = v1Group!.Should().HaveCount(2);
    }

    [Fact]
    public void GroupMatrixParameterPaths_WhenPathsUseMatrixNotation()
    {
        var results = new[]
        {
            new ApiPathResult("/api/products;color=red;size=large", "30"),
            new ApiPathResult("/api/products;color=red;size=large", "45"),
            new ApiPathResult("/api/products;color=blue;size=medium", "25")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        // All matrix parameter paths have the same prefix "/api" after removing products;... part
        _ = grouped.Should().HaveCount(1);
        _ = grouped[0].Key.Should().Be("/api");
        _ = grouped[0].Should().HaveCount(3);
    }

    [Fact]
    public void GroupFragmentPaths_WhenPathsIncludeFragments()
    {
        var results = new[]
        {
            new ApiPathResult("/api/documents/doc123#section1", "12"),
            new ApiPathResult("/api/documents/doc123#section1", "18"),
            new ApiPathResult("/api/documents/doc123#section2", "25")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        // All paths have the same prefix "/api/documents" after removing "doc123#..." part
        _ = grouped.Should().HaveCount(1);
        _ = grouped[0].Key.Should().Be("/api/documents");
        _ = grouped[0].Should().HaveCount(3);
    }

    [Fact]
    public void GroupNestedResourcePaths_WhenPathsHaveHierarchy()
    {
        var results = new[]
        {
            new ApiPathResult("/api/companies/123/departments/456/employees", "50"),
            new ApiPathResult("/api/companies/123/departments/456/employees", "75"),
            new ApiPathResult("/api/companies/123/departments/789/employees", "30")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        _ = grouped.Should().HaveCount(2);
        var dept456Group = grouped.FirstOrDefault(g => g.Key.Contains("456"));
        _ = dept456Group!.Should().HaveCount(2);
    }

    [Fact]
    public void GroupLocalePaths_WhenPathsContainLanguageCodes()
    {
        var results = new[]
        {
            new ApiPathResult("/api/content/en-US/articles", "100"),
            new ApiPathResult("/api/content/en-US/articles", "150"),
            new ApiPathResult("/api/content/fr-FR/articles", "80")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        _ = grouped.Should().HaveCount(2);
        var enUsGroup = grouped.FirstOrDefault(g => g.Key.Contains("en-US"));
        _ = enUsGroup!.Should().HaveCount(2);
    }

    [Fact]
    public void GroupDatePaths_WhenPathsContainDateParameters()
    {
        var results = new[]
        {
            new ApiPathResult("/api/reports/2024-01-01/2024-01-31", "500"),
            new ApiPathResult("/api/reports/2024-01-01/2024-01-31", "750"),
            new ApiPathResult("/api/reports/2024-02-01/2024-02-29", "600")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        _ = grouped.Should().HaveCount(2);
        var januaryGroup = grouped.FirstOrDefault(g => g.Key.Contains("2024-01-01"));
        _ = januaryGroup!.Should().HaveCount(2);
    }

    [Fact]
    public void GroupEncodedPaths_WhenPathsUseUrlEncoding()
    {
        var results = new[]
        {
            new ApiPathResult("/api/search?q=hello%20world", "40"),
            new ApiPathResult("/api/search?q=hello%20world", "60"),
            new ApiPathResult("/api/search?q=test%26demo", "30")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        // All paths have the same prefix "/api" after removing the search query part
        _ = grouped.Should().HaveCount(1);
        _ = grouped[0].Key.Should().Be("/api");
        _ = grouped[0].Should().HaveCount(3);
    }

    [Fact]
    public void GroupPaginationPaths_WhenPathsContainPageInfo()
    {
        var results = new[]
        {
            new ApiPathResult("/api/items?page=1&size=50", "1000"),
            new ApiPathResult("/api/items?page=1&size=50", "500"),
            new ApiPathResult("/api/items?page=2&size=50", "800")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        // All pagination paths have the same prefix "/api" after removing items?... part
        _ = grouped.Should().HaveCount(1);
        _ = grouped[0].Key.Should().Be("/api");
        _ = grouped[0].Should().HaveCount(3);
    }

    [Fact]
    public void HandlePathsWithBackslashes_WhenPathsUseBackslashes()
    {
        var results = new[]
        {
            new ApiPathResult(@"\api\windows\path", "10"),
            new ApiPathResult(@"\api\windows\path", "20"),
            new ApiPathResult(@"\api\unix/mixed", "30")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        _ = grouped.Should().HaveCount(2);
        var windowsGroup = grouped.FirstOrDefault(g => g.Key.Contains(@"\api\windows\path"));
        _ = windowsGroup!.Should().HaveCount(2);
    }

    [Fact]
    public void HandlePathsWithOnlySlash_WhenPathIsRootSlash()
    {
        var results = new[]
        {
            new ApiPathResult("/", "10"),
            new ApiPathResult("/", "20"),
            new ApiPathResult("/api", "30")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        // Both "/" and "/api" when split and have last segment removed become "" (empty prefix)
        _ = grouped.Should().HaveCount(1);
        _ = grouped[0].Key.Should().Be("");
        _ = grouped[0].Should().HaveCount(3);
    }

    [Fact]
    public void HandlePathsWithMultipleSlashes_WhenPathsHaveConsecutiveSlashes()
    {
        var results = new[]
        {
            new ApiPathResult("/api//double//slash", "10"),
            new ApiPathResult("/api//double//slash", "20"),
            new ApiPathResult("/api/single/slash", "30")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        _ = grouped.Should().HaveCount(2);
        var doubleSlashGroup = grouped.FirstOrDefault(g => g.Key.Contains("//"));
        _ = doubleSlashGroup!.Should().HaveCount(2);
    }

    [Fact]
    public void HandlePathsWithTrailingSlash_WhenPathsEndWithSlash()
    {
        var results = new[]
        {
            new ApiPathResult("/api/endpoint/", "10"),
            new ApiPathResult("/api/endpoint/", "20"),
            new ApiPathResult("/api/endpoint", "30")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        // "/api/endpoint/" splits to ["", "api", "endpoint", ""] - removing last gives ["", "api", "endpoint"] ? "/api/endpoint"
        // "/api/endpoint" splits to ["", "api", "endpoint"] - removing last gives ["", "api"] ? "/api"
        // So they end up in 2 different groups
        _ = grouped.Should().HaveCount(2);
        var trailingSlashGroup = grouped.FirstOrDefault(g => g.Key == "/api/endpoint");
        _ = trailingSlashGroup.Should().NotBeNull();
        _ = trailingSlashGroup!.Should().HaveCount(2);
        
        var noTrailingSlashGroup = grouped.FirstOrDefault(g => g.Key == "/api");
        _ = noTrailingSlashGroup.Should().NotBeNull();
        _ = noTrailingSlashGroup!.Should().HaveCount(1);
    }

    [Fact]
    public void HandleCaseSensitivePaths_WhenPathsDifferInCase()
    {
        var results = new[]
        {
            new ApiPathResult("/API/Users", "10"),
            new ApiPathResult("/api/users", "20"),
            new ApiPathResult("/api/users", "30")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        _ = grouped.Should().HaveCount(2);
        var lowercaseGroup = grouped.FirstOrDefault(g => g.Key == "/api");
        _ = lowercaseGroup!.Should().HaveCount(2);
    }

    [Fact]
    public void HandlePathsWithSpaces_WhenPathsContainSpaces()
    {
        var results = new[]
        {
            new ApiPathResult("/api/path with spaces", "10"),
            new ApiPathResult("/api/path with spaces", "20"),
            new ApiPathResult("/api/path_with_underscores", "30")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        // Both paths have the same prefix "/api" after removing the last segment
        _ = grouped.Should().HaveCount(1);
        _ = grouped[0].Key.Should().Be("/api");
        _ = grouped[0].Should().HaveCount(3);
    }

    [Fact]
    public void HandleSingleCharacterPaths_WhenPathsAreVeryShort()
    {
        var results = new[]
        {
            new ApiPathResult("a", "10"),
            new ApiPathResult("a", "20"),
            new ApiPathResult("b", "30")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        _ = grouped.Should().HaveCount(2);
        var aGroup = grouped.FirstOrDefault(g => g.Key == "a");
        _ = aGroup!.Should().HaveCount(2);
    }

    [Fact]
    public void HandleVeryLongPaths_WhenPathsExceedTypicalLength()
    {
        var longPath = "/api/" + string.Join("/", Enumerable.Range(1, 100).Select(i => $"seg{i}"));
        var results = new[]
        {
            new ApiPathResult(longPath, "10"),
            new ApiPathResult(longPath, "20"),
            new ApiPathResult("/short", "30")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        _ = grouped.Should().HaveCount(2);
        var longGroup = grouped.FirstOrDefault(g => g.Key.Contains("seg99"));
        _ = longGroup!.Should().HaveCount(2);
    }

    [Fact]
    public void HandlePathsWithOnlyNumbers_WhenPathsAreNumeric()
    {
        var results = new[]
        {
            new ApiPathResult("/123/456/789", "10"),
            new ApiPathResult("/123/456/789", "20"),
            new ApiPathResult("/999/888/777", "30")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        _ = grouped.Should().HaveCount(2);
        var numericGroup = grouped.FirstOrDefault(g => g.Key == "/123/456");
        _ = numericGroup!.Should().HaveCount(2);
    }

    [Fact]
    public void HandlePathsWithDots_WhenPathsContainFilenames()
    {
        var results = new[]
        {
            new ApiPathResult("/api/file.txt", "10"),
            new ApiPathResult("/api/file.txt", "20"),
            new ApiPathResult("/api/file.json", "30")
        };

        var grouped = ApiPathsCountService.GroupByPathPrefix(results).ToList();

        // All three paths have the same prefix "/api" after removing the filename
        _ = grouped.Should().HaveCount(1);
        _ = grouped[0].Key.Should().Be("/api");
        _ = grouped[0].Should().HaveCount(3);
    }
}
