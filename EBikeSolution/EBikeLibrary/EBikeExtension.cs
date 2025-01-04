using EBikeLibrary.BLL;
using EBikeLibrary.BLL.SaleReturnServices;
using EBikeLibrary.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EBikeLibrary;

public static class EBikeExtension
{
    public static void AddBackendDependencies(
           this IServiceCollection services,
           Action<DbContextOptionsBuilder> options)
    {
        // Register the HogWildContext class, which is the DBContext for your connection.
        services.AddDbContext<eBike_DMIT2018Context>(options);

        services.AddTransient<PurchasingService>((ServiceProvider) =>
        {
            var context = ServiceProvider.GetRequiredService<eBike_DMIT2018Context>();
            //create an instance of the service and return the instance
            return new PurchasingService(context);
        });

        services.AddTransient<ReceivingService>((ServiceProvider) =>
        {
            //  Retrieve an instance of ebikeContext from the service provider.
            var context = ServiceProvider.GetService<eBike_DMIT2018Context>();

            // Create a new instance of ReceivingService,
            //   passing the ebikeContext instance as a parameter.
            return new ReceivingService(context);

        });

        services.AddTransient<SaleService>((ServiceProvider) =>
        {
            // get instance of eBikeContext
            var context = ServiceProvider.GetService<eBike_DMIT2018Context>();
            // create new instance of SaleService with context
            return new SaleService(context);
        });

        services.AddTransient<ReturnService>((ServiceProvider) =>
        {
            // get instance of eBikeContext
            var context = ServiceProvider.GetService<eBike_DMIT2018Context>();
            // create new instance of ReturnService with context
            return new ReturnService(context);
        });


        services.AddTransient<ServicingService>((ServiceProvider) =>
        {
            return new(ServiceProvider.GetService<eBike_DMIT2018Context>()!);
        });
    }
}