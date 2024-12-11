using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace OCBC.ProductModule.Migrations
{
    public class ProductInformationMigration(IContentDefinitionManager contentDefinitionManager) : DataMigration
    {
        public int Create()
        {
            // Define the Product Information content type
            contentDefinitionManager.AlterTypeDefinitionAsync("ProductInformation", type => type
                .WithPart("CommonPart")  // Common Part that includes common fields (Author, Created, Modified)
                .WithPart("ProductInformationPart") // Custom Part (that we will define below)
            );

            // Define the ProductInformationPart with fields
            contentDefinitionManager.AlterPartDefinitionAsync("ProductInformationPart", part => part
                .WithField("ProductName", field => field
                    .OfType("TextField")
                    .WithDisplayName("Product Name"))
                .WithField("ChineseProductName", field => field
                    .OfType("TextField")
                    .WithDisplayName("产品名称"))
                .WithField("Model", field => field
                    .OfType("ContentPickerField")
                    .WithDisplayName("Model")
                    .WithSettings(new TextFieldSettings
                    {
                        Hint = "Please choose a model"
                    }))
                .WithField("Description", field => field
                    .OfType("TextField")
                    .WithDisplayName("Description")
                    .WithSettings(new TextFieldSettings
                    {
                        Hint = "Please provide a description"
                    }))
                .WithField("ProductImages", field => field
                    .OfType("MediaField")
                    .WithDisplayName("Product Images"))
            );

            return 1;
        }

        // Add a method for migrating later updates
        public int UpdateFrom1()
        {
            // contentDefinitionManager.AlterTypeDefinitionAsync("ProductInformation", type => type
            //     .Creatable()
            //     .Listable()
            // );

            return 2;
        }

        public int UpdateFrom2()
        {
            contentDefinitionManager.AlterTypeDefinitionAsync("ProductInformation", type => type
                .Creatable()
                .Listable()
            );

            return 3;
        }

        public int UpdateFrom3()
        {
            contentDefinitionManager.AlterTypeDefinitionAsync("ProductInformation", type => type
                .Creatable()
                .Listable()
                .Draftable()
            );

            return 4;
        }

        public int UpdateFrom4()
        {
            contentDefinitionManager.AlterTypeDefinitionAsync("ProductInformation", type => type
                .Creatable(false)
            );

            return 5;
        }

        public int UpdateFrom5()
        {
            contentDefinitionManager.AlterTypeDefinitionAsync("ProductInformation", type => type
                .Creatable()
            );

            return 6;
        }

        public int UpdateFrom6()
        {
            contentDefinitionManager.AlterPartDefinitionAsync("ProductInformationPart", part => part
                .RemoveField("ProductImages")
            );
            
            return 7;
        }

        public int UpdateFrom7()
        {
            contentDefinitionManager.AlterPartDefinitionAsync("ProductInformationPart", part => part
                .RemoveField("ProductName")
                .RemoveField("ChineseProductName")
                .RemoveField("Model")
                .RemoveField("Description")
            );
            
            return 8;
        }

        public int UpdateFrom8()
        {
            contentDefinitionManager.AlterPartDefinitionAsync("ProductInformationPart", part => part
                .WithField("ProductImages", field => field
                    .OfType("MediaField")
                    .WithDisplayName("Product Images")
                    .WithPosition("Editor:1"))
            );
            
            return 9;
        }

        public int UpdateFrom9()
        {
            contentDefinitionManager.AlterPartDefinitionAsync("ProductInformationPart", part => part
                .WithField("ProductImage1", field => field
                    .OfType("MediaField")
                    .WithDisplayName("Product Image 1")
                    .WithPosition("Editor:1"))
                .WithField("ProductImage2", field => field
                    .OfType("MediaField")
                    .WithDisplayName("Product Image 2")
                    .WithPosition("Editor:1"))
                .WithField("ProductImage3", field => field
                    .OfType("MediaField")
                    .WithDisplayName("Product Image 3")
                    .WithPosition("Editor:1"))
            );
            
            return 10;
        }

        public int UpdateFrom10()
        {
            contentDefinitionManager.AlterPartDefinitionAsync("ProductInformationPart", part => part
                .RemoveField("ProductImages")
                .WithField("ProductImage1", field => field
                    .OfType("MediaField")
                    .WithDisplayName("Product Image 1")
                    .WithPosition("Editor:1"))
                .WithField("ProductImage2", field => field
                    .OfType("MediaField")
                    .WithDisplayName("Product Image 2")
                    .WithPosition("Editor:2"))
                .WithField("ProductImage3", field => field
                    .OfType("MediaField")
                    .WithDisplayName("Product Image 3")
                    .WithPosition("Editor:3"))
            );
            
            return 11;
        }

        public int UpdateFrom11()
        {
            contentDefinitionManager.AlterPartDefinitionAsync("ProductInformationPart", part => part
                .RemoveField("ProductImage1")
                .RemoveField("ProductImage2")
                .RemoveField("ProductImage3")
            );
            
            return 12;
        }

        public int UpdateFrom12()
        {
            contentDefinitionManager.AlterPartDefinitionAsync("ProductInformationPart", part => part
                .WithField("ProductImage1", field => field
                    .OfType("MediaField")
                    .WithDisplayName("Product Image 1")
                    .WithPosition("Editor:1"))
                .WithField("ProductImage2", field => field
                    .OfType("MediaField")
                    .WithDisplayName("Product Image 2")
                    .WithPosition("Editor:2"))
                .WithField("ProductImage3", field => field
                    .OfType("MediaField")
                    .WithDisplayName("Product Image 3")
                    .WithPosition("Editor:3"))
            );
            
            return 13;
        }
    }
}
