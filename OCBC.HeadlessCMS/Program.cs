using System.Globalization;
using Microsoft.AspNetCore.Localization;
using OCBC.HeadlessCMS.Services;
using OrchardCore.Media;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOrchardCms()
    // Orchard Specific Pipeline
    .ConfigureServices( services => {
        services.PostConfigure<MediaOptions>(o => o.AllowedFileExtensions = [
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
        ]);
    })
    .Configure( (app, routes, services) => {
        
    });

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("en-GB"), new CultureInfo("zh-CN") };

    options.DefaultRequestCulture = new RequestCulture("en-GB");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.AddTransient<IDemoService, DemoService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseOrchardCore();

app.Run();