using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OCBC.HeadlessCMS.Models;
using OCBC.HeadlessCMS.Services;
using OrchardCore;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.Users.AuditTrail.Models;

namespace OCBC.HeadlessCMS.Controllers;

[ApiController]
[Route("api/v1/product")]
public class ProductController(
    IOrchardHelper orchard, 
    IAuditTrailManager auditTrailManager,
    IStringLocalizer<ProductController> stringLocalizer,
    IDemoService demoService) : Controller
{
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
}