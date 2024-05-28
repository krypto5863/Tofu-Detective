using Bethesda.TofuDetector.Services;
using Bethesda.TofuDetector.ViewModels;
using Microsoft.Win32;
using Mutagen.Bethesda;
using System.Reflection;
using System.Windows;

namespace Bethesda.TofuDetector.Views
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly MainWindowViewModel _viewModel;
		public MainWindow()
		{
			_viewModel = new MainWindowViewModel();

			DataContext = _viewModel;

			InitializeComponent();
		}

		private async void SaveFixItem_OnClick(object sender, RoutedEventArgs e)
		{
			var saveDialog = new SaveFileDialog()
			{
				Filter = "Plugin File | *.esp",
				DefaultDirectory = SkyrimMutagenService.GetOrCreateInstance(GameRelease.SkyrimSE).SkyrimEnvironment?.DataFolderPath,
				FileName = "TofuPatch.esp"
			};

			if (saveDialog.ShowDialog() == false)
			{
				return;
			}

			await _viewModel.SavePatchPlugin(saveDialog.FileName);
		}

		private async void LoadLOItem_OnClick(object sender, RoutedEventArgs e)
		{
			MenuBar.IsEnabled = false;
			var pluginDialogue = new PluginSelect
			{
				Owner = this
			};
			var dialogueResult = pluginDialogue.ShowDialog();
			if (dialogueResult is null or false)
			{
				return;
			}
			await _viewModel.LoadOrderAndProcess(pluginDialogue.Result.Keys.ToArray());
			MenuBar.IsEnabled = true;
		}

		private void Exit_OnClick(object sender, RoutedEventArgs e)
		{
			Environment.Exit(0);
		}
	}
}