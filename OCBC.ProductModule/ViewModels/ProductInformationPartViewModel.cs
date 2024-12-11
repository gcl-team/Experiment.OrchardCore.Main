using System.ComponentModel.DataAnnotations;
using OrchardCore.Media.Fields;

namespace OCBC.ProductModule.ViewModels;

public class ProductInformationPartViewModel
{
    [Required]
    public string ProductName { get; set; }
    [Required]
    public string ChineseProductName { get; set; }
    [Required]
    public string Model { get; set; }
    public string Description { get; set; }
    public MediaField ProductImage1 { get; set; }
    public MediaField ProductImage2 { get; set; }
    public MediaField ProductImage3 { get; set; }
}