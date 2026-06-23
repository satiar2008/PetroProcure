using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PetroProcure.AI;
public static class DependencyInjection
{
    public static IServiceCollection AddPetroProcureAi(this IServiceCollection services,IConfiguration configuration)
    {
        services.Configure<AiOptions>(configuration.GetSection(AiOptions.SectionName));
        services.AddScoped<MockAiProvider>();
        services.AddHttpClient<OllamaProvider>();
        services.AddHttpClient<OpenAICompatibleProvider>();
        services.AddScoped<IAiChatProvider>(sp=>configuration[$"{AiOptions.SectionName}:Provider"] switch
        { "Ollama"=>sp.GetRequiredService<OllamaProvider>(), "OpenAICompatible"=>sp.GetRequiredService<OpenAICompatibleProvider>(), _=>sp.GetRequiredService<MockAiProvider>() });
        services.AddScoped<IProcurementRuleEvaluator,ProcurementRuleEvaluator>();
        services.AddScoped<IAiAgentService,AiAgentService>();
        return services;
    }
}
