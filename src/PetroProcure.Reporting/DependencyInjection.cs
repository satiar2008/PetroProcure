using Microsoft.Extensions.DependencyInjection;

namespace PetroProcure.Reporting;

public static class DependencyInjection
{
    public static IServiceCollection AddPetroProcureReporting(this IServiceCollection services)
    {
        services.AddScoped<IReportGenerator, ReportGenerator>();
        return services;
    }
}
