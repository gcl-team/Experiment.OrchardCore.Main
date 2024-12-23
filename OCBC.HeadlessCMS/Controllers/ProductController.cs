using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using OCBC.HeadlessCMS.Services;
using OrchardCore;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.DynamicCache;
using OrchardCore.Environment.Cache;

namespace OCBC.HeadlessCMS.Controllers;

[ApiController]
[Route("api/v1/product")]
public class ProductController(
    IOrchardHelper orchard, 
    IMemoryCache memoryCache,
    ISignal orchardSignal,
    IDynamicCache dynamicCache,
    IAuditTrailManager auditTrailManager,
    IStringLocalizer<ProductController> stringLocalizer,
    IDemoService demoService) : Controller
{
    private const string OrchardSignalKey = "OCBC.HeadlessCMS.Controllers.ProductController.OrchardSignalKey";

    [HttpGet("product-info")]
    public async Task<IActionResult> GetProductInformation()
    {
        var productInformation = await orchard.QueryContentItemsAsync(q => 
            q.Where(c => c.ContentType == "ProductInformation"));
            
        return Ok(productInformation);
    }

    [HttpGet("product-info/published")]
    public async Task<IActionResult> GetPublishedProductInformation()
    {
        var productInformation = await orchard.QueryContentItemsAsync(q => 
            q.Where(c => c.ContentType == "ProductInformation" && c.Published));
            
        return Ok(productInformation);
    }

    [HttpGet("product-info/{contentItemId}")]
    public async Task<IActionResult> GetProductInformationById(string contentItemId)
    {
        var productInformation = await orchard.GetContentItemByIdAsync(contentItemId);
            
        return Ok(productInformation);
    }

    [HttpGet("po")]
    public async Task<IActionResult> GetPortableObjectDemoInformation()
    {
        // Hardcode the locale for testing
        var testCulture = "zh-CN";
        CultureInfo.CurrentCulture = new CultureInfo(testCulture);
        CultureInfo.CurrentUICulture = new CultureInfo(testCulture);

        // Use the localizer to get a string
        var localizedString = stringLocalizer["weather_Hot"];

        // Return the localized string
        return Ok(new { Locale = testCulture, Translation = localizedString });
    }

    [HttpGet("po-with-service")]
    public IActionResult GetPortableObjectServiceDemoInformation()
    {
        return Ok(demoService.GetDemoTranslatedText());
    }

    [HttpGet("audit-demo/{contentItemId}")]
    public async Task<IActionResult> GetAuditCreationDemo(string contentItemId)
    {
        var productInformation = await orchard.GetContentItemByIdAsync(contentItemId);
        
        await auditTrailManager.RecordEventAsync(
            new AuditTrailContext<AuditTrailContentEvent>
            (
                name: "Published",
                category: "Content",
                correlationId: Guid.NewGuid().ToString(),
                userId: "4gyfgah3k3ness5adpmffgntnc",
                userName: "ADMIN",
                auditTrailEventItem: new AuditTrailContentEvent
                {
                    ContentItem = productInformation,
                    VersionNumber = 1,
                    Comment = $"This is created at {DateTime.Now:yyyy-MM-dd HH:mm:ss}."
                }
            )
        );

        return Ok(new { Message = "Audit log entry created." });
    }

    [HttpGet("read-cache/{contentItemId}")]
    public async Task<IActionResult> ReadCacheDemo(string contentItemId)
    {
        ContentItem? productInformation;
        if (!memoryCache.TryGetValue(contentItemId, out productInformation))
        {
            productInformation = await orchard.GetContentItemByIdAsync(contentItemId);
            productInformation.DisplayText = $"Cached at {DateTime.Now:yyyy-MM-dd-HH:mm:ss}!";

            memoryCache.Set(contentItemId, productInformation, orchardSignal.GetToken(OrchardSignalKey));
        }

        return Ok(productInformation);
    }

    [HttpGet("invalidate-cache/{contentItemId}")]
    public IActionResult InvalidateCacheDemo(string contentItemId)
    {
        memoryCache.Remove(contentItemId);

        return Ok(new { Message = $"Removed memory cache for {contentItemId}." });
    }

    [HttpGet("invalidate-cache-with-signal/{contentItemId}")]
    public async Task<IActionResult> InvalidateCacheWithSignalDemo(string contentItemId)
    {
        // Raise a signal
        await orchardSignal.SignalTokenAsync(OrchardSignalKey);

        return Ok(new { Message = $"Raised signal to invalidate memory cache for {contentItemId}." });
    }

    [HttpGet("read-dynamic-cache/{contentItemId}")]
    public async Task<IActionResult> ReadDynamicCacheDemo(string contentItemId)
    {
        ContentItem? productInformation;
        
        var cachedContentBytes = await dynamicCache.GetAsync(contentItemId);

        if (cachedContentBytes == null)
        {
            productInformation = await orchard.GetContentItemByIdAsync(contentItemId);
            productInformation.DisplayText = $"Dynamic Cached at {DateTime.Now:yyyy-MM-dd-HH:mm:ss}!";

            var serializedContentItem = JsonSerializer.Serialize(productInformation);

            await dynamicCache.SetAsync(contentItemId, Encoding.UTF8.GetBytes(serializedContentItem), 
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                }
            );
        } 
        else 
        {
            productInformation = JsonSerializer.Deserialize<ContentItem>(Encoding.UTF8.GetString(cachedContentBytes));
        }

        return Ok(productInformation);
    }

    [HttpGet("read-traditional-cache/{contentItemId}")]
    [ResponseCache(Duration = 60)]
    public async Task<IActionResult> ReadTraditionalCacheDemo(string contentItemId)
    {
        var productInformation = await orchard.GetContentItemByIdAsync(contentItemId);
        productInformation.DisplayText = $"Traditional Cached at {DateTime.Now:yyyy-MM-dd-HH:mm:ss}!";

        Response.Headers["ETag"] = Guid.NewGuid().ToString();
        return Ok(productInformation);
    }
}