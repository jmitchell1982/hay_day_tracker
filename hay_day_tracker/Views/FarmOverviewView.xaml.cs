using System.Windows.Controls;
using HayDayPlanner.ViewModels;

namespace HayDayPlanner.Views;

// No logic lives here on purpose — DataContext is injected from outside
// (e.g. via DI container or MainWindow), and all behavior lives in
// FarmOverviewViewModel.
public partial class FarmOverviewView : UserControl
{
    public FarmOverviewView(FarmOverviewViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
