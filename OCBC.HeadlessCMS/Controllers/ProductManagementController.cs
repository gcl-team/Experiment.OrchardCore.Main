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
            
        return Ok(new { contentItemId = contentItem.ContentItemId });
    }

    [HttpGet("update-sample/{contentItemId}")]
    public async Task<IActionResult> UpdateSampleProductInformation(string contentItemId)
    {
        var contentItem = await contentManager.GetAsync(contentItemId, VersionOptions.Latest);

        if (contentItem == null)
        {
            return BadRequest(new { Error = "Invalid content item id." });
        }

        contentItem.DisplayText += $" - Updated";
        contentItem.Content.ProductInformationPart.ProductName += " - Updated";
        contentItem.Content.ProductInformationPart.Description += $" This is updated at {DateTime.Now:yyyy-MM-dd HH:mm:ss}.";
        
        await contentManager.UpdateAsync(contentItem);
            
        return Ok();
    }

    [HttpGet("publish-unpublish-sample/{contentItemId}")]
    public async Task<IActionResult> PublishUnpublishSampleProductInformation(string contentItemId)
    {
        var contentItem = await contentManager.GetAsync(contentItemId, VersionOptions.Latest);

        if (contentItem == null)
        {
            return BadRequest(new { Error = "Invalid content item id." });
        }

        if (await contentManager.HasPublishedVersionAsync(contentItem))
        {
            await contentManager.UnpublishAsync(contentItem);
        } 
        else
        {
            await contentManager.PublishAsync(contentItem);
        }
            
        return Ok();
    }

    [HttpGet("remove-sample/{contentItemId}")]
    public async Task<IActionResult> RemoveSampleProductInformation(string contentItemId)
    {
        var contentItem = await contentManager.GetAsync(contentItemId, VersionOptions.Latest);

        if (contentItem == null)
        {
            return BadRequest(new { Error = "Invalid content item id." });
        }

        await contentManager.RemoveAsync(contentItem);
            
        return Ok();
    }

    [HttpGet("discard-draft-sample/{contentItemId}")]
    public async Task<IActionResult> DiscardDraftSampleProductInformation(string contentItemId)
    {
        var contentItem = await contentManager.GetAsync(contentItemId, VersionOptions.Latest);

        if (contentItem == null)
        {
            return BadRequest(new { Error = "Invalid content item id." });
        }

        await contentManager.DiscardDraftAsync(contentItem);
            
        return Ok();
    }
}