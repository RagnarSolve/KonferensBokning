using KonferenscentrumVast.Data;
using KonferenscentrumVast.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace KonferenscentrumVast
{
    public class Program
    {
        public static void Main()
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<ApplicationDbContext>(opt =>
                        opt.UseCosmos(
                            Environment.GetEnvironmentVariable("Cosmos--Endpoint")!,
                            Environment.GetEnvironmentVariable("Cosmos--Key")!,
                            Environment.GetEnvironmentVariable("Cosmos--Database")!
                        ));


                    services.AddScoped<BookingService>();
                    services.AddScoped<BookingContractService>();
                    services.AddScoped<CustomerService>();
                    services.AddScoped<FacilityService>();
                });

            var host = builder.Build();
            host.Run();
        }
    }
}
