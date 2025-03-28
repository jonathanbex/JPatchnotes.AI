using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text;

namespace Patchnotes.AI.REST.Handler
{
  public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
  {
    private readonly IConfiguration _configuration;

    public BasicAuthenticationHandler(IConfiguration configuration, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
      _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
      var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();

      if (string.IsNullOrEmpty(authorizationHeader))
      {
        return Task.FromResult(AuthenticateResult.Fail("Authorization header missing"));
      }

      
      if (!authorizationHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
      {
        return Task.FromResult(AuthenticateResult.Fail("Invalid authorization header"));
      }

      var encodedCredentials = authorizationHeader.Substring("Basic ".Length).Trim();
      var decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));

      var credentials = decodedCredentials.Split(':');
      if (credentials.Length != 2)
      {
        return Task.FromResult(AuthenticateResult.Fail("Invalid Basic Authentication header"));
      }

      var username = credentials[0];
      var password = credentials[1];

      // Fetch the username and password from configuration (replace with your keys)
      var configuredUsername = _configuration.GetValue<string>("Auth:Username");
      var configuredPassword = _configuration.GetValue<string>("Auth:Password");

      // Check if the provided credentials match the configured ones
      if (username == configuredUsername && password == configuredPassword)
      {
        var claims = new[] { new Claim(ClaimTypes.Name, username) };
        var identity = new ClaimsIdentity(claims, "BasicAuthentication");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "BasicAuthentication");

        return Task.FromResult(AuthenticateResult.Success(ticket));
      }

      return Task.FromResult(AuthenticateResult.Fail("Invalid username or password"));
    }
  }
}
