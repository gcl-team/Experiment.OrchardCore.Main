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