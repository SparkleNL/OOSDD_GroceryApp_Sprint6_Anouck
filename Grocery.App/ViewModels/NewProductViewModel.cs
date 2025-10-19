using CommunityToolkit.Mvvm.ComponentModel;
using Grocery.Core.Interfaces.Services;
using Grocery.Core.Models;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

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
        private void AddProduct()
        {
            ErrorMessage = "";
            NameError = "";
            StockError = "";
            DateError = "";
            PriceError = "";


            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Stock) ||
                string.IsNullOrWhiteSpace(ShelfLife) || string.IsNullOrWhiteSpace(Price))
            {
                ErrorMessage = "Vul alle velden in.";
                return;
            }

            if (Name.Length < 2 || Name.Length > 50)
            {
                NameError = "Naam moet tussen 2 en 50 tekens zijn.";
            }
            

            if (!int.TryParse(Stock, out int stockValue) || stockValue < 0)
            {
                StockError = "Voorraad moet een positief geheel getal zijn.";
            }

            if (!DateOnly.TryParse(ShelfLife, out DateOnly shelfLifeValue))
            {
                DateError = "Voer datum in als dd-mm-yyyy";
            }
            else if (DateOnly.TryParse(ShelfLife, out DateOnly parsedDate) &&
                     parsedDate < DateOnly.FromDateTime(DateTime.Now))
            {
                DateError = "Datum moet in de toekomst liggen";
            }

            if (!decimal.TryParse(Price, out decimal priceValue) || priceValue < 0 || priceValue > 999.99m)
            {
                PriceError = "Prijs moet tussen 0 en 999.999 zijn";
            }
        }
    }
}
