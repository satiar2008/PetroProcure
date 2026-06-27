using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PetroProcure.Application.Ai;
using PetroProcure.Application.Legal;
using PetroProcure.Application.Rag;
using System;

namespace PetroProcure.AI;

public static class DependencyInjection
{
    public static IServiceCollection AddPetroProcureAi(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));
        services.Configure<AiCoreOptions>(configuration.GetSection(AiCoreOptions.SectionName));
        services.Configure<AiCoreIntegrationOptions>(configuration.GetSection(AiCoreIntegrationOptions.SectionName));
        services.AddScoped<MockAiProvider>();
        services.AddHttpClient<OllamaProvider>();
        services.AddHttpClient<ILocalOllamaAnalysisClient, LocalOllamaAnalysisClient>();
        services.AddHttpClient<OpenAICompatibleProvider>();
        services.AddHttpClient<AiCoreClient>();
        services.AddHttpClient<AiCoreEmbeddingGenerator>(client =>
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
        });
        services.AddScoped<IEmbeddingGenerator>(sp => sp.GetRequiredService<AiCoreEmbeddingGenerator>());
        services.AddScoped<IEmbeddingClient>(sp => sp.GetRequiredService<AiCoreEmbeddingGenerator>());
        services.AddHttpClient<IAiCoreJobClient, AiCoreJobClient>();
        services.AddHttpClient<IAiCoreClient, AiCoreClient>(client =>
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
        });
        services.AddScoped<IAiChatProvider>(sp => configuration[$"{AiOptions.SectionName}:Provider"] switch
        { "Ollama" => sp.GetRequiredService<OllamaProvider>(), "OpenAICompatible" => sp.GetRequiredService<OpenAICompatibleProvider>(), _ => sp.GetRequiredService<MockAiProvider>() });
        services.AddSingleton<IAiCoreCallbackAuthenticator, AiCoreCallbackAuthenticator>();
        // Stabilization: production AiCore implementations must win deterministically over the
        // Null fallbacks registered by AddApplication, regardless of registration order.
        services.Replace(ServiceDescriptor.Scoped<IAiLegalEvaluationService, AiCoreLegalEvaluationService>());
        services.Replace(ServiceDescriptor.Scoped<IGroundedAiAnswerGenerator, AiCoreGroundedAnswerGenerator>());
        services.AddScoped<IProcurementRuleEvaluator, ProcurementRuleEvaluator>();
        services.AddScoped<IAiAnalysisService, AiAnalysisService>();
        services.AddScoped<IAiAgentService, AiAgentService>();
        return services;
    }
}
