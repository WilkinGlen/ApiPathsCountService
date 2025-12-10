namespace ApiPathsCountService.Tests;

using FluentAssertions;
using SwaggerApiPathsService;
using SwaggerApiPathsService.Models;

public class GetEndpointGroupSummaries_Should
{
    [Fact]
    public void ReturnCorrectCount_WhenPathsMatchExactly()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/users")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users", 5)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users", 3))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].Path.Should().Be("/api/users");
        _ = summaries[0].Count.Should().Be(8);
        _ = summaries[0].NumberOfEndpoints.Should().Be(2);
    }

    [Fact]
    public void ReturnCorrectCount_WhenPathsStartWithPrefixFollowedBySlash()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/users/{userId}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/123", 10)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/456", 20)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/789", 30))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].Path.Should().Be("/api/users");
        _ = summaries[0].Count.Should().Be(60);
        _ = summaries[0].NumberOfEndpoints.Should().Be(3);
    }

    [Fact]
    public void ReturnCorrectCount_WhenMultipleParametersInEndpoint()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/users/{userId}/orders/{orderId}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/123/orders/A1", 5)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/456/orders/B2", 10)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/789/orders/C3", 15))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].Path.Should().Be("/api/users");
        _ = summaries[0].Count.Should().Be(30);
        _ = summaries[0].NumberOfEndpoints.Should().Be(3);
    }

    [Fact]
    public void ReturnMultipleSummaries_WhenMultipleDistinctEndpoints()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/users/{userId}"),
            new ApiEndpoint("server1", "/api/orders/{orderId}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/123", 10)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/456", 20)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/orders/A1", 5)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/orders/B2", 15))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(2);
        _ = summaries.Should().Contain(s => s.Path == "/api/users" && s.Count == 30 && s.NumberOfEndpoints == 2);
        _ = summaries.Should().Contain(s => s.Path == "/api/orders" && s.Count == 20 && s.NumberOfEndpoints == 2);
    }

    [Fact]
    public void ReturnZeroCount_WhenNoMatchingEntries()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/users/{userId}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/orders/123", 10)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/products/456", 20))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].Path.Should().Be("/api/users");
        _ = summaries[0].Count.Should().Be(0);
        _ = summaries[0].NumberOfEndpoints.Should().Be(0);
    }

    [Fact]
    public void ReturnEmptyList_WhenNoEndpointsProvided()
    {
        var apiEndpoints = Array.Empty<ApiEndpoint>();
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/123", 10))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().BeEmpty();
    }

    [Fact]
    public void HandleCaseInsensitiveMatching_WhenPathsHaveDifferentCasing()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/API/Users/{userId}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/123", 10)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/API/USERS/456", 20)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/Api/Users/789", 30))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].Count.Should().Be(60);
        _ = summaries[0].NumberOfEndpoints.Should().Be(3);
    }

    [Fact]
    public void HandleNullPaths_WhenSwaggerApiEntryPathIsNull()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/users/{userId}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult(null, 10)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/123", 20)),
            new SwaggerApiEntry(false, new SwaggerApiResult(null, 30))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].Count.Should().Be(20);
        _ = summaries[0].NumberOfEndpoints.Should().Be(1);
    }

    [Fact]
    public void NotMatchPartialPrefixes_WhenPathStartsWithPrefixButNotFollowedBySlash()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/user/{userId}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/123", 10)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/user/456", 20))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].Path.Should().Be("/api/user");
        _ = summaries[0].Count.Should().Be(20);
        _ = summaries[0].NumberOfEndpoints.Should().Be(1);
    }

    [Fact]
    public void DeduplicateEndpointPrefixes_WhenMultipleEndpointsHaveSamePrefix()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/users/{userId}"),
            new ApiEndpoint("server1", "/api/users/{userId}/profile")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/123", 10)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/456/profile", 20))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].Path.Should().Be("/api/users");
        _ = summaries[0].Count.Should().Be(30);
        _ = summaries[0].NumberOfEndpoints.Should().Be(2);
    }

    [Fact]
    public void HandleEndpointsWithoutParameters_WhenSignatureHasNoPlaceholders()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/health"),
            new ApiEndpoint("server1", "/api/status")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/health", 100)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/status", 50))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(2);
        _ = summaries.Should().Contain(s => s.Path == "/api/health" && s.Count == 100 && s.NumberOfEndpoints == 1);
        _ = summaries.Should().Contain(s => s.Path == "/api/status" && s.Count == 50 && s.NumberOfEndpoints == 1);
    }

    [Fact]
    public void HandleMixedEndpoints_WhenSomeHaveParametersAndSomeDoNot()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/users/{userId}"),
            new ApiEndpoint("server1", "/api/health")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/123", 10)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/456", 20)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/health", 5))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(2);
        _ = summaries.Should().Contain(s => s.Path == "/api/users" && s.Count == 30 && s.NumberOfEndpoints == 2);
        _ = summaries.Should().Contain(s => s.Path == "/api/health" && s.Count == 5 && s.NumberOfEndpoints == 1);
    }

    [Fact]
    public void HandleRealWorldApiPaths_WhenUsingTypicalRestEndpoints()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/GetIt/v1/DirectReports/StandardId/{standardId}"),
            new ApiEndpoint("server1", "/GetIt/v1/DirectReports/Name/{name}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/GetIt/v1/DirectReports/StandardId/ZKDPUWW", 1)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/GetIt/v1/DirectReports/StandardId/ZEDPUWW", 2)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/GetIt/v1/DirectReports/StandardId/ZFDPUWW", 3)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/GetIt/v1/DirectReports/Name/Glen", 1)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/GetIt/v1/DirectReports/Name/Marcus", 1)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/GetIt/v1/DirectReports/Name/Luke", 1))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(2);
        _ = summaries.Should().Contain(s => s.Path == "/GetIt/v1/DirectReports/StandardId" && s.Count == 6 && s.NumberOfEndpoints == 3);
        _ = summaries.Should().Contain(s => s.Path == "/GetIt/v1/DirectReports/Name" && s.Count == 3 && s.NumberOfEndpoints == 3);
    }

    [Fact]
    public void HandleGuidParameters_WhenPathsContainGuidValues()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/sessions/{sessionId}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/sessions/550e8400-e29b-41d4-a716-446655440000", 25)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/sessions/123e4567-e89b-12d3-a456-426614174000", 75))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].Path.Should().Be("/api/sessions");
        _ = summaries[0].Count.Should().Be(100);
        _ = summaries[0].NumberOfEndpoints.Should().Be(2);
    }

    [Fact]
    public void HandleNestedResourcePaths_WhenEndpointsHaveHierarchy()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/companies/{companyId}/departments/{departmentId}/employees/{employeeId}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/companies/123/departments/456/employees/789", 50)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/companies/ABC/departments/DEF/employees/GHI", 25))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].Path.Should().Be("/api/companies");
        _ = summaries[0].Count.Should().Be(75);
        _ = summaries[0].NumberOfEndpoints.Should().Be(2);
    }

    [Fact]
    public void HandleEmptySwaggerEntries_WhenNoEntriesProvided()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/users/{userId}")
        };
        var swaggerApiEntries = Array.Empty<SwaggerApiEntry>();

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].Count.Should().Be(0);
        _ = summaries[0].NumberOfEndpoints.Should().Be(0);
    }

    [Fact]
    public void HandleZeroCountEntries_WhenSwaggerApiResultCountIsZero()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/users/{userId}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/123", 0)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/456", 0)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/789", 0))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].Count.Should().Be(0);
        _ = summaries[0].NumberOfEndpoints.Should().Be(3);
    }

    [Fact]
    public void HandleVersionedApiPaths_WhenEndpointsContainVersionNumbers()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/v1/users/{userId}"),
            new ApiEndpoint("server1", "/api/v2/users/{userId}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/v1/users/123", 100)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/v1/users/456", 200)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/v2/users/123", 50)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/v2/users/456", 150))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(2);
        _ = summaries.Should().Contain(s => s.Path == "/api/v1/users" && s.Count == 300 && s.NumberOfEndpoints == 2);
        _ = summaries.Should().Contain(s => s.Path == "/api/v2/users" && s.Count == 200 && s.NumberOfEndpoints == 2);
    }

    [Fact]
    public void HandleLargeCountValues_WhenSummingManyEntries()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/metrics/{metricId}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/metrics/1", int.MaxValue / 2)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/metrics/2", int.MaxValue / 2))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].Count.Should().Be(int.MaxValue / 2 + int.MaxValue / 2);
        _ = summaries[0].NumberOfEndpoints.Should().Be(2);
    }

    [Fact]
    public void HandleNegativeCountValues_WhenSwaggerApiResultCountIsNegative()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/users/{userId}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/123", -5)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/456", 10))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].Count.Should().Be(5);
        _ = summaries[0].NumberOfEndpoints.Should().Be(2);
    }

    [Fact]
    public void HandleDuplicateSwaggerEntries_WhenSamePathAppearsMultipleTimes()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/users/{userId}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/123", 10)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/123", 20)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/123", 30))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].Count.Should().Be(60);
        _ = summaries[0].NumberOfEndpoints.Should().Be(3);
    }

    [Fact]
    public void HandleSpecialCharactersInPath_WhenPathContainsEncodedValues()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/search/{query}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/search/hello%20world", 5)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/search/test%2Fvalue", 10))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].Path.Should().Be("/api/search");
        _ = summaries[0].Count.Should().Be(15);
        _ = summaries[0].NumberOfEndpoints.Should().Be(2);
    }

    [Fact]
    public void HandleSingleEntryPerPrefix_WhenOnlyOneMatchingEntry()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/users/{userId}"),
            new ApiEndpoint("server1", "/api/orders/{orderId}"),
            new ApiEndpoint("server1", "/api/products/{productId}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/1", 100)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/orders/1", 200)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/products/1", 300))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(3);
        _ = summaries.Should().Contain(s => s.Path == "/api/users" && s.Count == 100 && s.NumberOfEndpoints == 1);
        _ = summaries.Should().Contain(s => s.Path == "/api/orders" && s.Count == 200 && s.NumberOfEndpoints == 1);
        _ = summaries.Should().Contain(s => s.Path == "/api/products" && s.Count == 300 && s.NumberOfEndpoints == 1);
    }

    [Fact]
    public void HandleEmptyStringPath_WhenSwaggerApiEntryPathIsEmpty()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/users/{userId}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("", 10)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/123", 20))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].Count.Should().Be(20);
        _ = summaries[0].NumberOfEndpoints.Should().Be(1);
    }

    [Fact]
    public void HandleTrailingSlashInSwaggerPath_WhenPathEndsWithSlash()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/users/{userId}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/123/", 10)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/456", 20))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].Count.Should().Be(30);
        _ = summaries[0].NumberOfEndpoints.Should().Be(2);
    }

    [Fact]
    public void HandleMultipleServers_WhenEndpointsFromDifferentServersHaveSamePath()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/users/{userId}"),
            new ApiEndpoint("server2", "/api/users/{userId}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/123", 10)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/456", 20))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].Path.Should().Be("/api/users");
        _ = summaries[0].Count.Should().Be(30);
        _ = summaries[0].NumberOfEndpoints.Should().Be(2);
    }

    [Fact]
    public void HandleRootPath_WhenEndpointIsAtRoot()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/{id}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/123", 10)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/456", 20))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].Path.Should().Be("");
        _ = summaries[0].Count.Should().Be(30);
        _ = summaries[0].NumberOfEndpoints.Should().Be(2);
    }

    [Fact]
    public void HandleQueryParametersInPath_WhenPathContainsQueryString()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/users/{userId}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/123?include=profile", 10)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/456", 20))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].Count.Should().Be(30);
        _ = summaries[0].NumberOfEndpoints.Should().Be(2);
    }

    [Fact]
    public void PreserveOrderOfPrefixes_WhenMultipleEndpointsProvided()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/zebra/{id}"),
            new ApiEndpoint("server1", "/api/alpha/{id}"),
            new ApiEndpoint("server1", "/api/beta/{id}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/zebra/1", 10)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/alpha/1", 20)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/beta/1", 30))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(3);
        _ = summaries[0].Path.Should().Be("/api/zebra");
        _ = summaries[1].Path.Should().Be("/api/alpha");
        _ = summaries[2].Path.Should().Be("/api/beta");
    }

    [Fact]
    public void HandleMixedFailedAndSuccessfulEntries_WhenSwaggerApiEntryHasFailedFlag()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/users/{userId}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(true, new SwaggerApiResult("/api/users/123", 10)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/456", 20)),
            new SwaggerApiEntry(true, new SwaggerApiResult("/api/users/789", 30))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        // Currently, failed flag is not considered - all entries are counted
        _ = summaries.Should().HaveCount(1);
        _ = summaries[0].Count.Should().Be(60);
        _ = summaries[0].NumberOfEndpoints.Should().Be(3);
    }

    [Fact]
    public void HandleSimilarPrefixes_WhenOnePrefixIsSubstringOfAnother()
    {
        var apiEndpoints = new[]
        {
            new ApiEndpoint("server1", "/api/user/{userId}"),
            new ApiEndpoint("server1", "/api/users/{userId}")
        };
        var swaggerApiEntries = new[]
        {
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/user/123", 10)),
            new SwaggerApiEntry(false, new SwaggerApiResult("/api/users/456", 20))
        };

        var summaries = SwaggerApiPathsGroupsService.GetEndpointGroupSummaries(swaggerApiEntries, apiEndpoints).ToList();

        _ = summaries.Should().HaveCount(2);
        _ = summaries.Should().Contain(s => s.Path == "/api/user" && s.Count == 10 && s.NumberOfEndpoints == 1);
        _ = summaries.Should().Contain(s => s.Path == "/api/users" && s.Count == 20 && s.NumberOfEndpoints == 1);
    }
}
