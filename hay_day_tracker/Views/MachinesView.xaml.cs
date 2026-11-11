using System.Windows.Controls;
using HayDayPlanner.ViewModels;

namespace HayDayPlanner.Views;

public partial class MachinesView : UserControl
{
    public MachinesView(MachinesViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
