using System.Windows;
using DynamicData;
using DynamicData.Binding;
using Mutagen.Bethesda.Plugins.Order;

namespace Bethesda.TofuDetector
{
	/// <summary>
	/// Interaction logic for PluginSelect.xaml
	/// </summary>
	public partial class PluginSelect : Window
	{
		public IObservableCollection<PluginLoad> PluginLoads { get; }
		public PluginSelect(IEnumerable<ILoadOrderListingGetter> loadOrdersEnumerable)
		{
			InitializeComponent();
			var pluginsToLoad = loadOrdersEnumerable
				.Select(loadOrderItem => new PluginLoad(loadOrderItem.FileName, loadOrdersEnumerable.IndexOf(loadOrderItem)) { WillLoad = loadOrderItem.Enabled })
				.ToList();

			PluginLoads = new ObservableCollectionExtended<PluginLoad>(pluginsToLoad);
			ListView.ItemsSource = pluginsToLoad;
		}

		private void OKButton_Click(object sender, RoutedEventArgs e)
		{
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
			foreach (var plugins in PluginLoads)
			{
				plugins.WillLoad = true;
			}
		}

		private void DisableAll_OnClick(object sender, RoutedEventArgs e)
		{
			foreach (var plugins in PluginLoads)
			{
				plugins.WillLoad = false;
			}
		}

		private void InvertSelection_OnClick(object sender, RoutedEventArgs e)
		{
			foreach (var plugins in PluginLoads)
			{
				plugins.WillLoad = !plugins.WillLoad;
			}
		}
	}
}
