using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OCBC.HeadlessCMS.Services;
using OrchardCore;

namespace OCBC.HeadlessCMS.Controllers;

[ApiController]
[Route("api/v1/product")]
public class ProductController(
    IOrchardHelper orchard, 
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
}