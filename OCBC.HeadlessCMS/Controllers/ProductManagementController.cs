using Microsoft.AspNetCore.Mvc;
using OCBC.ProductModule.Models;
using OrchardCore.ContentManagement;

namespace OCBC.HeadlessCMS.Controllers;

[ApiController]
[Route("api/v1/product-management")]
public class ProductManagementController(IContentManager contentManager) : Controller
{
    [HttpGet("create-sample")]
    public async Task<IActionResult> CreateSampleProductInformation()
    {
        var contentItem = await contentManager.NewAsync("ProductInformation");

        if (contentItem == null)
        {
            return BadRequest(new { Error = "Invalid content type." });
        }

        contentItem.DisplayText = $"New Product Information Created at {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        contentItem.Content.ProductInformationPart = new ProductInformationPart
        {
            ProductName = "Sample",
            Description = $"This is created at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.",
        };

        await contentManager.CreateAsync(contentItem, VersionOptions.Draft);
            
        return Ok();
    }
}