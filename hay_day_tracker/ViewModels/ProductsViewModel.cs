using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using HayDayPlanner.Models;
using HayDayPlanner.Services;

namespace HayDayPlanner.ViewModels;

public partial class ProductsViewModel : ObservableObject
{
    private readonly ICollectionView _productsView;

    public ObservableCollection<ProductRow> Products { get; }

    public ProductDetailViewModel Detail { get; }

    /// <summary>
    /// "All" plus every machine name that has at least one product -
    /// bound to the filter dropdown in the view.
    /// </summary>
    public List<string> MachineFilterOptions { get; }

    [ObservableProperty]
    private string _selectedMachineFilter = AllMachinesOption;

    [ObservableProperty]
    private int _levelFilter;

    [ObservableProperty]
    private ProductRow? _selectedProduct;

    private const string AllMachinesOption = "All";

    public ProductsViewModel(FarmStateService farmStateService, List<ProductInfo> products,
        List<BuildingInfo> buildings)
    {
        _levelFilter = farmStateService.Current.FarmLevel;
        
        ProductAnalysisContext context = new ProductAnalysisContext(products, buildings, farmStateService);
        Detail = new ProductDetailViewModel(context);

        List<ProductRow> rows = new List<ProductRow>();

        foreach (ProductInfo product in products)
        {
            BuildingInfo buildingInfo = context.BuildingsById[product.BuildingId];

            int mastery = context.GetMastery(product);
            int quantity = context.GetQuantity(product);
            double ingredientValue = context.ValueResolver.IngredientValueOf(product);

            ProductCalculationResult result = ProductCalculator.Calculate(
                product,
                ingredientValue,
                mastery,
                quantity);

            rows.Add(new ProductRow(product, buildingInfo.Name, result));
        }

        Products = new ObservableCollection<ProductRow>(rows);

        MachineFilterOptions = new List<string> { AllMachinesOption }
            .Concat(rows.Select(r => r.MachineName).Distinct().OrderBy(name => name))
            .ToList();

        _productsView = CollectionViewSource.GetDefaultView(Products);
        _productsView.Filter = Filter;
    }

    partial void OnSelectedMachineFilterChanged(string value)
    {
        _productsView.Refresh();
    }

    partial void OnLevelFilterChanged(int value)
    {
        _productsView.Refresh();
    }

    partial void OnSelectedProductChanged(ProductRow? value)
    {
        Detail.ShowProduct(value?.Product);
    }

    private bool Filter(object item)
    {
        return FilterByLevel(item) && FilterByMachine(item);
    }

    private bool FilterByMachine(object item)
    {
        if (SelectedMachineFilter == AllMachinesOption)
        {
            return true;
        }

        return item is ProductRow row && row.MachineName == SelectedMachineFilter;
    }

    private bool FilterByLevel(object item)
    {
        return item is ProductRow row && row.Level <= LevelFilter;
    }
}