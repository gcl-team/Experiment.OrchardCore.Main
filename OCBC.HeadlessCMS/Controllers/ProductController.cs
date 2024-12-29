using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using OCBC.HeadlessCMS.Services;
using OrchardCore;
using YesSql;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.DynamicCache;
using OrchardCore.Environment.Cache;
using OrchardCore.Workflows.Services;
using OCBC.ProductModule.Models;

namespace OCBC.HeadlessCMS.Controllers;

[ApiController]
[Route("api/v1/product")]
public class ProductController(
    IOrchardHelper orchard,
    YesSql.ISession session,
    IMemoryCache memoryCache,
    ISignal orchardSignal,
    IDynamicCache dynamicCache,
    IWorkflowTypeStore workflowTypeStore,
    IWorkflowManager workflowManager,
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

    [HttpGet("trigger-workflow/{contentItemId}")]
    public async Task<IActionResult> TriggerWorkflowDemo(string contentItemId)
    {
        var workflowTypes = await workflowTypeStore.ListAsync();

        // Load the workflow definition.
        var workflowType = await workflowTypeStore.GetAsync(52);

        if (workflowType == null)
        {
            return BadRequest(new { Error = "Workflow not found." });
        }

        var productInformation = await orchard.GetContentItemByIdAsync(contentItemId);

        var input = new Dictionary<string, object>()
        {
            { "ContentItem", productInformation },
        };

        // Invoke the workflow.
        var workflowContext = await workflowManager.StartWorkflowAsync(workflowType, input: input);

        return Ok(new { workflowTypes, workflowContext });
    }

    [HttpGet("get-user-authentication")]
    public async Task<IActionResult> GetUserAuthenticationDemo()
    {
        bool isUserAuthenticated = HttpContext.User.Identity?.IsAuthenticated ?? false;
    
        var userClaims = HttpContext.User.Claims
            .Select(c => new { c.Type, c.Value })
            .ToList();

        var roles = HttpContext.User.Claims
            .Where(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
            .Select(c => c.Value)
            .ToList();

        bool isAdmin = HttpContext.User.IsInRole("Administrator");

        var claims = HttpContext.User.Claims
            .Where(c => c.Type == ClaimTypes.Role) // or use the full claim type if necessary
            .Select(c => c.Value)
            .ToList();

        return Ok(new { isUserAuthenticated, userClaims, roles, isAdmin, claims });
    }

    [Authorize(Roles = "Administrator")]
    [HttpGet("get-admin-only")]
    public async Task<IActionResult> GetAdminOnlyDemo()
    {
        return Ok();
    }

    [Authorize(Roles = "User")]
    [HttpGet("get-user-only")]
    public async Task<IActionResult> GetUserOnlyDemo()
    {
        return Ok();
    }

    [HttpGet("get-yessql-query-result")]
    public async Task<IActionResult> GetYesSqlQueryResultDemo()
    {
        var result = await session.Query<MyCustomTable>().ListAsync();
       
        return Ok(result);
    }

    [HttpGet("create-yessql-query-result/{customField}")]
    public async Task<IActionResult> CreateYesSqlQueryResultDemo(string customField)
    {
        var newEntry = new MyCustomTable
        {
            CustomField = customField,
            CreatedDate = DateTime.Now,
        };

        await session.SaveAsync(newEntry);
        await session.SaveChangesAsync();
       
        return Ok();
    }
}