
using Microsoft.Extensions.Configuration;

namespace Patchnotes.AI.Helpers
{


  public class ConfigurationValidator
  {
    private IConfiguration _configuration;

    public ConfigurationValidator(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    public void ValidateAndUpdateConfiguration()
    {
      ValidateAndPromptForToken();
    }

    private void ValidateAndPromptForToken()
    {
      var token = _configuration.GetValue<string>("Github:Token");
      if (string.IsNullOrEmpty(token))
      {
        Console.WriteLine("Missing Token for Github. Please enter your Github Token, See https://github.com/settings/tokens only need read :");
        token = Console.ReadLine();
        if (string.IsNullOrEmpty(token))
        {
          Console.WriteLine("Token is required to run the bot.");
          throw new ArgumentNullException(nameof(token));
        }
        JsonConfigurationHelper.UpdateAppSettings("Github:Token", token);
        ReloadConfiguration();
      }
    }






    private void ReloadConfiguration()
    {
      _configuration = TryGetConfigurationHelper.LoadConfiguration("appsettings.json");
    }
  }

}
