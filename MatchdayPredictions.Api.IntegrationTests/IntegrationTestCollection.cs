using MatchdayPredictions.Api.IntegrationTests.Infrastructure;
using Xunit;

namespace MatchdayPredictions.Api.IntegrationTests;

[CollectionDefinition("integration-tests", DisableParallelization = true)]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
}
