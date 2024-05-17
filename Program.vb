Imports System
Imports System.Linq
Imports Google.Apis.Auth.OAuth2
Imports Google.Apis.Services
Imports Google.Apis.ShoppingContent.v2_1
Imports Google.Apis.ShoppingContent.v2_1.Data
Imports Contexts
Imports Google.Apis.Util.Store
Imports System.IO
Imports System.Threading

Friend Class Program
    Public Shared Sub Main(ByVal args() As String)
        Example1()
        Console.ReadLine()
    End Sub

    Private Shared Sub Example1()
        ' Path to your OAuth 2.0 credentials JSON file
        Dim credentialPath As String = "path/to/your/credentials.json"

        ' Set up OAuth 2.0 authorization
        Dim credential As UserCredential
        Using stream = New FileStream(credentialPath, FileMode.Open, FileAccess.Read)
            Dim credPath As String = "token.json"
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                {ShoppingContentService.Scope.Content},
                "user",
                CancellationToken.None,
                New FileDataStore(credPath, True)).Result
            Console.WriteLine("Credential file saved to: " & credPath)
        End Using

        ' Create the ShoppingContentService
        Dim service As New ShoppingContentService(New BaseClientService.Initializer() With {
            .HttpClientInitializer = credential,
            .ApplicationName = "Your Application Name"
        })

        ' Access your database context
        Using context = New ApplicationDbContext()
            ' Retrieve products with availability "in_stock"
            Dim products = context.Products.Where(Function(p) p.availability = "in_stock").ToList()
            Console.WriteLine($"Found {products.Count} products in stock.")

            ' Your Merchant Center account ID
            Dim accountId As ULong = 123456789 ' Replace with your actual Merchant Center account ID

            ' Loop through each product and upload to Google Shopping Center
            For Each dbProduct In products
                ' Create a new product for Google Shopping Center
                Dim product As New Product() With {
                    .OfferId = dbProduct.id,
                    .Title = dbProduct.title,
                    .Description = dbProduct.description,
                    .Link = dbProduct.link,
                    .ImageLink = dbProduct.image_link,
                    .ContentLanguage = "en",
                    .TargetCountry = "uk",
                    .Channel = "online",
                    .Availability = dbProduct.availability,
                    .Condition = dbProduct.condition,
                    .GoogleProductCategory = "Apparel & Accessories > Clothing > Shirts & Tops",
                    .Price = New Price() With {
                        .Value = dbProduct.price.ToString(),
                        .Currency = "GBP"
                    }
                }

                Try
                    Dim request = service.Products.Insert(product, accountId)
                    Dim response = request.Execute()
                    Console.WriteLine($"Product inserted with ID: {response.Id}")
                Catch ex As Exception
                    Console.WriteLine($"An error occurred while inserting product '{dbProduct.title}': {ex.Message}")
                End Try
            Next
        End Using
    End Sub
End Class
