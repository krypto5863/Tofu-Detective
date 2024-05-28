using System.Windows.Controls;

namespace TofuDetective.MVVM.Contracts.Views;

public interface IShellWindow
{
    Frame GetNavigationFrame();

    void ShowWindow();

    void CloseWindow();
}
