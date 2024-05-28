using Bethesda.TofuDetector.Services;
using Bethesda.TofuDetector.ViewModels;
using Microsoft.Win32;
using Mutagen.Bethesda;
using System.Windows;
using System.Windows.Input;

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
			if (_viewModel.CanSavePatch() == false)
			{
				return;
			}

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
			try
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
				Mouse.OverrideCursor = Cursors.Wait;
				await _viewModel.LoadOrderAndProcess(pluginDialogue.Result.Keys.ToArray());
			}
			finally
			{
				Mouse.OverrideCursor = null;
				MenuBar.IsEnabled = true;
			}
		}

		private void Exit_OnClick(object sender, RoutedEventArgs e)
		{
			Environment.Exit(0);
		}
	}
}