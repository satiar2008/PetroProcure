using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PetroProcure.AI;
using PetroProcure.Application;
using PetroProcure.Application.Ai;
using PetroProcure.Application.Legal;

namespace PetroProcure.UnitTests.Application;

// Stabilization: AiCore implementations must win deterministically over the Null fallbacks,
// independent of the order in which AddApplication / AddPetroProcureAi are called.
public sealed class AiServiceRegistrationTests
{
    [Fact]
    public void ApplicationThenAi_ResolvesAiCoreImplementations()
    {
        var services = Build(applicationFirst: true);

        Assert.Equal(typeof(AiCoreLegalEvaluationService), ImplementationType<IAiLegalEvaluationService>(services));
        Assert.Equal(typeof(AiCoreGroundedAnswerGenerator), ImplementationType<IGroundedAiAnswerGenerator>(services));
    }

    [Fact]
    public void AiThenApplication_StillResolvesAiCoreImplementations()
    {
        var services = Build(applicationFirst: false);

        Assert.Equal(typeof(AiCoreLegalEvaluationService), ImplementationType<IAiLegalEvaluationService>(services));
        Assert.Equal(typeof(AiCoreGroundedAnswerGenerator), ImplementationType<IGroundedAiAnswerGenerator>(services));
    }

    private static IServiceCollection Build(bool applicationFirst)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var services = new ServiceCollection();
        if (applicationFirst)
        {
            services.AddApplication(configuration);
            services.AddPetroProcureAi(configuration);
        }
        else
        {
            services.AddPetroProcureAi(configuration);
            services.AddApplication(configuration);
        }
        return services;
    }

    private static Type? ImplementationType<TService>(IServiceCollection services)
    {
        var descriptors = services.Where(d => d.ServiceType == typeof(TService)).ToArray();
        Assert.Single(descriptors); // Replace/TryAdd must leave exactly one registration.
        return descriptors[0].ImplementationType;
    }
}
