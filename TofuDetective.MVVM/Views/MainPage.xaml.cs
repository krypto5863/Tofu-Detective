using System.Windows.Controls;

using TofuDetective.MVVM.ViewModels;

namespace TofuDetective.MVVM.Views;

public partial class MainPage : Page
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
