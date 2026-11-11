using System.Windows.Controls;
using HayDayPlanner.ViewModels;

namespace HayDayPlanner.Views;

public partial class ProductsView : UserControl
{
    public ProductsView(ProductsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
