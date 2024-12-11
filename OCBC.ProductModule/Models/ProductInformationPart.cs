using OrchardCore.ContentManagement;
using OrchardCore.Media.Fields;

namespace OCBC.ProductModule.Models;

public class ProductInformationPart : ContentPart
{
    public string ProductName { get; set; }
    public string ChineseProductName { get; set; }
    public string Model { get; set; }
    public string Description { get; set; }
    public MediaField ProductImage1 { get; set; }
    public MediaField ProductImage2 { get; set; }
    public MediaField ProductImage3 { get; set; }
}