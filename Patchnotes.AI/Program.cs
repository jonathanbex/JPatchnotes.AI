using Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Patchnotes.AI;
using Patchnotes.AI.Helpers;

public class Program
{
  public static async Task Main(string[] args)
  {
    var configuration = TryGetConfigurationHelper.LoadConfiguration("appsettings.json");

    var configurationValidator = new ConfigurationValidator(configuration);
    configurationValidator.ValidateAndUpdateConfiguration();

    // Create a HostBuilder for DI etc.
    var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((context, services) =>
        {
          services.AddSingleton<IConfiguration>(configuration);
#pragma warning disable CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.

#pragma warning restore CS8634 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'class' constraint.
          services.AddScoped<GitHubService>();
          services.AddScoped<OpenAIService>();


          services.AddSingleton<PatchBot>();


        })
        .Build();

    // Resolve the Bot service and run it
    var bot = host.Services.GetRequiredService<PatchBot>();
    await bot.RunAsync();
  }
}