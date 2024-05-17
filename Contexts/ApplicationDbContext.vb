Imports System.Linq

Imports Microsoft.EntityFrameworkCore
Imports Microsoft.Extensions.Configuration

Imports Models

Namespace Contexts
	Partial Public Class ApplicationDbContext
		Inherits DbContext

		Public Sub New()
		End Sub

		Public Sub New(ByVal options As DbContextOptions(Of ApplicationDbContext))
			MyBase.New(options)
		End Sub

		Public Overridable Property Products() As DbSet(Of Product)

		Protected Overrides Sub OnConfiguring(ByVal optionsBuilder As DbContextOptionsBuilder)
			If Not optionsBuilder.IsConfigured Then
				optionsBuilder.UseSqlServer(BuildConnection())
			End If
		End Sub
		''' <summary>
		''' Build connection string from appsettings.json in section database
		''' </summary>
		''' <returns>Connection string</returns>
		Private Shared Function BuildConnection() As String

			Dim configuration = (New ConfigurationBuilder()).AddJsonFile("appsettings.json", True, True).Build()

			Dim sections = configuration.GetSection("database").GetChildren().ToList()

			Dim connectionString =
					$"Data Source={sections(1).Value};" &
					$"Initial Catalog={sections(0).Value};" &
					$"Integrated Security={sections(2).Value}"


			Return connectionString

		End Function

		Protected Overrides Sub OnModelCreating(ByVal modelBuilder As ModelBuilder)
			modelBuilder.Entity(Of Product)(
				Sub(entity)
					entity.ToTable("Product")

				End Sub)

			OnModelCreatingPartial(modelBuilder)

		End Sub

		Partial Private Sub OnModelCreatingPartial(ByVal modelBuilder As ModelBuilder)
		End Sub
	End Class
End Namespace