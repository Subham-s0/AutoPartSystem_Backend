using Microsoft.Extensions.DependencyInjection;
using VehiStock.Application.Interfaces.IRepositories;
using VehiStock.Application.Interfaces.IServices;
using VehiStock.Infrastructure.Repositories;
using VehiStock.Infrastructure.Services;

namespace VehiStock.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IStaffAdministrationRepository, StaffAdministrationRepository>();
        services.AddScoped<ISalesInvoiceRepository, SalesInvoiceRepository>();
        services.AddScoped<ICustomerReportRepository, CustomerReportRepository>();
        services.AddScoped<IStaffAdministrationService, StaffAdministrationService>();
        services.AddScoped<ISalesInvoiceService, SalesInvoiceService>();
        services.AddScoped<ICustomerReportService, CustomerReportService>();

        return services;
    }
}
