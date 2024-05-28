using System.Windows.Controls;

namespace TofuDetective.MVVM.Contracts.Services;

public interface IPageService
{
    Type GetPageType(string key);

    Page GetPage(string key);
}
