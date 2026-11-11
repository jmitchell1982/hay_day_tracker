using System.Windows.Controls;
using HayDayPlanner.ViewModels;

namespace HayDayPlanner.Views;

public partial class ShoppingListView : UserControl
{
    public ShoppingListView(ShoppingListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
