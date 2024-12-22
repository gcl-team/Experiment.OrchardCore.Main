using OrchardCore.AuditTrail.Models;

namespace OCBC.HeadlessCMS.Models;

public class CustomAuditEvent : AuditTrailEvent
{
    public string Message { get; set; } = "";
}