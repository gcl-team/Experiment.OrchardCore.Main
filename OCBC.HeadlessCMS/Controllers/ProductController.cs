using Microsoft.AspNetCore.Mvc;
using OrchardCore;

namespace OCBC.HeadlessCMS.Controllers;

[ApiController]
[Route("api/v1/product")]
public class ProductController(IOrchardHelper orchard) : Controller
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
}