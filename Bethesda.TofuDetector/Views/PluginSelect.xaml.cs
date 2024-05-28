using Bethesda.TofuDetector.ViewModels;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Skyrim;
using System.Windows;

namespace Bethesda.TofuDetector.Views
{
	/// <summary>
	/// Interaction logic for PluginSelect.xaml
	/// </summary>
	public partial class PluginSelect : Window
	{
		private readonly PluginSelectViewModel _viewModel;
		public ILoadOrder<IModListing<ISkyrimModGetter>> Result => _viewModel.FinalLoadOrder;

		public PluginSelect()
		{
			_viewModel = new PluginSelectViewModel();
			InitializeComponent();
			DataContext = _viewModel;
		}

		private void OKButton_Click(object sender, RoutedEventArgs e)
		{
			_viewModel.Commit();
			DialogResult = true;
			Close();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		private void EnableAll_OnClick(object sender, RoutedEventArgs e)
		{
			_viewModel.EnableAll();
		}

		private void DisableAll_OnClick(object sender, RoutedEventArgs e)
		{
			_viewModel.DisableAll();
		}

		private void InvertSelection_OnClick(object sender, RoutedEventArgs e)
		{
			_viewModel.InvertSelection();
		}
	}
}