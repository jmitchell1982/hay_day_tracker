using System.Windows.Controls;
using HayDayPlanner.ViewModels;

namespace HayDayPlanner.Views;

public partial class UpgradesView : UserControl
{
    public UpgradesView(UpgradesViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
