using System.Diagnostics.CodeAnalysis;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using Secura.Infrastructure.Autofac;
using Secura.Infrastructure.Logging.NLog;
using Secura.Infrastructure.Core.WebApi.Middleware;
using TestPipeline.Domain;
using TestPipeline.Domain.Auth;
using TestPipeline.Domain.Settings;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using NLogLoggerFactory = Secura.Infrastructure.Logging.NLog.NLogLoggerFactory;
using TestPipeline.Configure.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

var builder = WebApplication.CreateBuilder(args);

var environment = builder.Environment;
IWebHostBuilder webHostBuilder = builder.WebHost;

webHostBuilder.ConfigureLogging(x =>
{
    x.ClearProviders();
    x.SetMinimumLevel(LogLevel.Trace);
}).UseNLog();


var environmentConfiguration = new ConfigurationBuilder()
               .SetBasePath(environment.ContentRootPath)
               .AddJsonFile("appsettings.json", true, true)
               .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true)
               .AddEnvironmentVariables();

IConfiguration configuration = environmentConfiguration.Build();

LogManager.Configuration = new NLogLoggingConfiguration(configuration.GetSection("NLog"));

builder.Services.AddControllers();

builder.Services.AddApiVersioning(o =>
{
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    o.ReportApiVersions = true;
    o.Conventions.Add(new VersionByNamespaceConvention());
    o.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version"));
});

builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule<LoggingModule<NLogLoggerFactory, NLogRetrieveLogs>>();
    containerBuilder.RegisterModule<TestPipelineDomainAutofacRegistration>();
});

builder.Services.Configure<DatabaseSettings>(configuration.GetSection("Database"));
builder.Services.Configure<ErrorHandlingSettings>(configuration.GetSection("ErrorHandling"));
builder.Services.Configure<RequestResponseLogSettings>(configuration.GetSection("RequestResponseLog"));

builder.Services.Configure<BasicAuthenticationSettings>(configuration.GetSection("BasicAuthentication"));
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);

builder.Services.AddHttpClient("PropagateHeaders").AddHeaderPropagation();
builder.Services.AddHeaderPropagation(options => {options.Headers.Add("X-Correlation-Id");});

var app = builder.Build();

app.UseAuthentication();
app.UseSwagger();

var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "swagger";
    options.DocExpansion(DocExpansion.List);
    // build a Swagger endpoint for each discovered API version
    foreach (var description in provider.ApiVersionDescriptions.Reverse())
    {
        options.SwaggerEndpoint($"{description.GroupName}/swagger.json",
            description.GroupName.ToUpperInvariant());
    }
    options.DocumentTitle = environment.IsProduction() ? "TestPipeline API" : $"{environment.EnvironmentName} - TestPipeline API";
});

app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<UnhandledExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseHeaderPropagation();

app.MapControllers();

app.Run();

#pragma warning disable CA1050
[ExcludeFromCodeCoverage]
[UsedImplicitly]
public partial class Program { }
#pragma warning restore CA1050
