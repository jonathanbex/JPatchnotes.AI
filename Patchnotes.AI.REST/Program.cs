using Domain.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using Patchnotes.AI.REST.Handler;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c=> {
  c.SwaggerDoc("v1", new OpenApiInfo { Title = "JPatchnotes.AI", Version = "v1" });
  c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
  c.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
  {
    Name = "Authorization",
    Description = "basic http authentication. Ex: Basic (apiKey:secret).ToBase64",
    Type = SecuritySchemeType.Http,
    Scheme = "basic",
    In = ParameterLocation.Header,
  });
  c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                      new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Basic"
                            }
                        },
                        new string[] {}
                }
            });


    var xmlDocPathRest = string.Format(@"{0}\Patchnotes.AI.REST.xml", System.AppDomain.CurrentDomain.BaseDirectory);

  c.IncludeXmlComments(xmlDocPathRest);
});
builder.Services.AddScoped<GitHubService>();
builder.Services.AddScoped<OpenAIService>();

builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", options => { });


var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.UseAuthentication();  // <-- Add this middleware to enable Authentication
app.UseAuthorization();


app.MapControllers();

app.Run();

