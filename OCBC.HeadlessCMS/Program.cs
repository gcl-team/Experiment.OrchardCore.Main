using System.Globalization;
using Amazon.CloudWatch;
using Microsoft.AspNetCore.Localization;
using OCBC.HeadlessCMS.Models;
using OCBC.HeadlessCMS.Services;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OrchardCore.Media;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOrchardCms()
    // Orchard Specific Pipeline
    .ConfigureServices( services => {
        // services.PostConfigure<MediaOptions>(o => o.AllowedFileExtensions = [
        //     // Images
        //     ".jpg",
        //     ".jpeg",
        //     ".png",
        //     ".webp",
        //     ".heic",

        //     // Documents
        //     ".pdf",
        //     ".xls",
        //     ".xlsx",

        //     // Other
        //     ".json",
        //     ".zip",
        // ]);
        services.PostConfigure<MediaOptions>(o => {
            o.AllowedFileExtensions = [
                // Images
                ".jpg",
                ".jpeg",
                ".png",
                ".webp",
                ".heic",

                // Documents
                ".pdf",
                ".xls",
                ".xlsx",

                // Other
                ".json",
                ".zip",
            ];

            //o.CdnBaseUrl = "https://ocbc-cdn-demo.com";
        });
        //services.AddResponseCaching();
    })
    .Configure( (app, routes, services) => {
        
    });

builder.Services.AddResponseCaching();

builder.Services.AddHealthChecks();

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());

builder.Services.AddAWSService<IAmazonCloudWatch>();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("en-GB"), new CultureInfo("zh-CN") };

    options.DefaultRequestCulture = new RequestCulture("en-GB");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.Configure<MetricsOptions>(builder.Configuration.GetSection("MetricsOptions"));
builder.Services.AddSingleton<MetricsPublisher>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<MetricsPublisher>());

builder.Services.AddLogging(options =>
{
    options.AddConsole();
    options.SetMinimumLevel(LogLevel.Debug); // Capture debug-level logs
});

var tempoUrl = builder.Configuration["OpenTelemetry:TempoUrl"];

using var httpClient = new HttpClient();
try
{
    var response = await httpClient.GetAsync(tempoUrl);
    Console.WriteLine($"ADOT reachability test: {(int)response.StatusCode} {response.ReasonPhrase}");
}
catch (Exception ex)
{
    Console.WriteLine($"ADOT reachability test failed: {ex.GetType().Name} - {ex.Message}");
}

// Add OpenTelemetry tracing
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName: "cld-orchard-core"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(tempoUrl);
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        })
        .AddConsoleExporter());

Console.WriteLine($"Tracing endpoint: {tempoUrl}");

builder.Services.AddTransient<IDemoService, DemoService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseResponseCaching();

app.MapHealthChecks("/health");

app.UseMiddleware<EndpointStatisticsMiddleware>();

app.UseOrchardCore();

app.Run();
