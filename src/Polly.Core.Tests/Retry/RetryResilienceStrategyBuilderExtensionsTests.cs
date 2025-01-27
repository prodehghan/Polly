using System.ComponentModel.DataAnnotations;
using Polly.Builder;
using Polly.Retry;
using Xunit;

namespace Polly.Core.Tests.Retry;

public class RetryResilienceStrategyBuilderExtensionsTests
{
    public static readonly TheoryData<Action<ResilienceStrategyBuilder>> OverloadsData = new()
    {
        builder =>
        {
            builder.AddRetry(retry=>retry.Result(10));
            AssertStrategy(builder, RetryBackoffType.Exponential, 3, TimeSpan.FromSeconds(2));
        },
        builder =>
        {
            builder.AddRetry(retry=>retry.Result(10), RetryBackoffType.Linear);
            AssertStrategy(builder, RetryBackoffType.Linear, 3, TimeSpan.FromSeconds(2));
        },
        builder =>
        {
            builder.AddRetry(retry=>retry.Result(10), RetryBackoffType.Linear, 2, TimeSpan.FromSeconds(1));
            AssertStrategy(builder, RetryBackoffType.Linear, 2, TimeSpan.FromSeconds(1));
        },
    };

    [MemberData(nameof(OverloadsData))]
    [Theory]
    public void AddRetry_Overloads_Ok(Action<ResilienceStrategyBuilder> configure)
    {
        var builder = new ResilienceStrategyBuilder();
        var options = new RetryStrategyOptions();

        builder.Invoking(b => configure(b)).Should().NotThrow();
    }

    [Fact]
    public void AddRetry_DefaultOptions_Ok()
    {
        var builder = new ResilienceStrategyBuilder();
        var options = new RetryStrategyOptions();

        builder.AddRetry(options);

        AssertStrategy(builder, options.BackoffType, options.RetryCount, options.BaseDelay);
    }

    private static void AssertStrategy(ResilienceStrategyBuilder builder, RetryBackoffType type, int retries, TimeSpan delay)
    {
        var strategy = (RetryResilienceStrategy)builder.Build();

        strategy.BackoffType.Should().Be(type);
        strategy.RetryCount.Should().Be(retries);
        strategy.BaseDelay.Should().Be(delay);
    }

    [Fact]
    public void AddRetry_InvalidOptions_Throws()
    {
        var builder = new ResilienceStrategyBuilder();

        builder
            .Invoking(b => b.AddRetry(new RetryStrategyOptions { ShouldRetry = null! }))
            .Should()
            .Throw<ValidationException>()
            .WithMessage("The retry strategy options are invalid.*");
    }
}
