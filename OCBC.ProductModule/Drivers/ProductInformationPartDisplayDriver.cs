using OCBC.ProductModule.Models;
using OCBC.ProductModule.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;

public class ProductInformationPartDisplayDriver : ContentPartDisplayDriver<ProductInformationPart>
{
    public override IDisplayResult Display(ProductInformationPart part, BuildPartDisplayContext context)=>
        Initialize<ProductInformationPartViewModel>(
            GetDisplayShapeType(context),
            viewModel => PopulateViewModel(part, viewModel))
        .Location("Detail", "Content:1");

    public override IDisplayResult Edit(ProductInformationPart part, BuildPartEditorContext context) =>
        Initialize<ProductInformationPartViewModel>(
            GetEditorShapeType(context),
            viewModel => PopulateViewModel(part, viewModel))
        .Location("Content:5");

    public override async Task<IDisplayResult> UpdateAsync(ProductInformationPart part, UpdatePartEditorContext context)
    {
        var viewModel = new ProductInformationPartViewModel();
        
        // Bind using the correct prefix
        if (await context.Updater.TryUpdateModelAsync(viewModel, Prefix))
        {
            part.ProductName = viewModel.ProductName;
            part.ChineseProductName = viewModel.ChineseProductName;
            part.Model = viewModel.Model;
            part.Description = viewModel.Description;
            part.ProductImage1 = viewModel.ProductImage1;
            part.ProductImage2 = viewModel.ProductImage2;
            part.ProductImage3 = viewModel.ProductImage3;

            var contentItem = part.ContentItem;
            contentItem.DisplayText = $"{viewModel.Model} - {viewModel.ProductName}";
        }
        
        return await EditAsync(part, context);
    }

    private static void PopulateViewModel(ProductInformationPart part, ProductInformationPartViewModel viewModel)
    {
        viewModel.ProductName = part.ProductName;
        viewModel.ChineseProductName = part.ChineseProductName;
        viewModel.Model = part.Model;
        viewModel.Description = part.Description;
        viewModel.ProductImage1 = part.ProductImage1;
        viewModel.ProductImage2 = part.ProductImage2;
        viewModel.ProductImage3 = part.ProductImage3;
    }
}
