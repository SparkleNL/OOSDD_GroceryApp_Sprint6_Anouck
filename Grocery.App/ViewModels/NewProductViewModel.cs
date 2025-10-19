using CommunityToolkit.Mvvm.ComponentModel;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using System.Globalization;

namespace Grocery.App.ViewModels
{
    public partial class NewProductViewModel : BaseViewModel
    {
        private readonly IProductService _productService;
        public ObservableCollection<Product> Products { get; set; }

        [ObservableProperty]
        private string name = "";

        [ObservableProperty]
        private string stock = "";

        [ObservableProperty]
        private string shelfLife = "";

        [ObservableProperty]
        private string price = "";

        [ObservableProperty]
        private string errorMessage = "";

        [ObservableProperty]
        private string nameError = "";

        [ObservableProperty]
        private string stockError = "";

        [ObservableProperty]
        private string dateError = "";

        [ObservableProperty]
        private string priceError = "";


        public NewProductViewModel(IProductService productService)
        {
            _productService = productService;
            Products = new();
            foreach (Product p in _productService.GetAll()) Products.Add(p);
        }

        [RelayCommand]
        private async Task AddProduct()
        {
            ErrorMessage = "";
            NameError = "";
            StockError = "";
            DateError = "";
            PriceError = "";

            bool hasErrors = false;

            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Stock) ||
                string.IsNullOrWhiteSpace(ShelfLife) || string.IsNullOrWhiteSpace(Price))
            {
                ErrorMessage = "Vul alle velden in.";
                return;
            }

            if (Name.Length < 2 || Name.Length > 50)
            {
                NameError = "Naam moet tussen 2 en 50 tekens zijn.";
                hasErrors = true;
            }

            if (!int.TryParse(Stock, out int stockValue) || stockValue < 0)
            {
                StockError = "Voorraad moet een positief geheel getal zijn.";
                hasErrors = true;
            }

            // Parse date with specific format (dd-MM-yyyy)
            if (!DateOnly.TryParseExact(ShelfLife, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly shelfLifeValue))
            {
                DateError = "Voer datum in als dd-mm-yyyy";
                hasErrors = true;
            }
            else if (shelfLifeValue < DateOnly.FromDateTime(DateTime.Now))
            {
                DateError = "Datum moet in de toekomst liggen";
                hasErrors = true;
            }

            // Parse decimal with invariant culture (accepts both . and ,)
            if (!decimal.TryParse(Price.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out decimal priceValue) || priceValue < 0 || priceValue > 999.99m)
            {
                PriceError = "Prijs moet tussen 0 en 999.99 zijn";
                hasErrors = true;
            }

            // Only add product if there are no errors
            if (!hasErrors)
            {
                try
                {
                    var newProduct = new Product(0, Name.Trim(), stockValue, shelfLifeValue, priceValue);
                    var addedProduct = _productService.Add(newProduct);
                    Products.Add(addedProduct);

                    // Clear fields after successful addition
                    Name = "";
                    Stock = "";
                    ShelfLife = "";
                    Price = "";
                    
                    // Navigate back to ProductView to refresh the list
                    await Shell.Current.GoToAsync("..", true);
                }
                catch (Exception ex)
                {
                    ErrorMessage = $"Fout bij toevoegen product: {ex.Message}";
                }
            }
        }
    }
}
