using System.Windows.Controls;

using MahApps.Metro.Controls;

using TofuDetective.MVVM.Contracts.Views;
using TofuDetective.MVVM.ViewModels;

namespace TofuDetective.MVVM.Views;

public partial class ShellWindow : MetroWindow, IShellWindow
{
    public ShellWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    public Frame GetNavigationFrame()
        => shellFrame;

    public void ShowWindow()
        => Show();

    public void CloseWindow()
        => Close();
}
