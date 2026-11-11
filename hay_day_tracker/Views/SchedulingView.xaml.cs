using System.Windows.Controls;
using HayDayPlanner.ViewModels;

namespace HayDayPlanner.Views;

public partial class SchedulingView : UserControl
{
    public SchedulingView(SchedulingViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}