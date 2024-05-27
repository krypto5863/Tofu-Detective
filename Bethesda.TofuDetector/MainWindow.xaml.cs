using Mutagen.Bethesda;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Mutagen.Bethesda.Plugins.Order;
using Mutagen.Bethesda.Plugins;
using Microsoft.Win32;
using Mutagen.Bethesda.Plugins.Binary.Parameters;

namespace Bethesda.TofuDetector
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public ObservableCollection<TofuSentence>? TofuSentences { get; private set; }
		public ObservableCollection<TrimmedSentence>? WhiteSpaceSentences { get; private set; }
		public SkyrimMod? CurrentPatchMod { get; private set; }
		public IGameEnvironment CurrentEnvironment { get; private set; }

		public MainWindow()
		{
			InitializeComponent();
			LoadOrderAndProcess();
		}
		private void SaveFixItem_OnClick(object sender, RoutedEventArgs e)
		{
			if (CurrentPatchMod == null || TofuSentences is null || TofuSentences.Count == 0)
			{
				return;
			}

			var dialog = new SaveFileDialog()
			{
				DefaultDirectory = CurrentEnvironment.DataFolderPath,
				FileName = CurrentPatchMod.ModKey.FileName,
				Filter = "Skyrim Plugin(*.esp)|*.esp|All(*.*)|*",
			};

			if (dialog.ShowDialog() == true)
			{
				CurrentPatchMod.WriteToBinary(dialog.FileName, new BinaryWriteParameters()
				{
					ModKey = ModKeyOption.NoCheck
				});
			}

			StatusLabel.Content = $"Patch plugin saved to: {dialog.FileName}";
		}
		private async void LoadLOItem_OnClick(object sender, RoutedEventArgs e)
		{
			CurrentEnvironment.Dispose();
			await LoadOrderAndProcess();
		}
		private void Exit_OnClick(object sender, RoutedEventArgs e)
		{
			Environment.Exit(0);
		}

		public async Task LoadOrderAndProcess()
		{
			CurrentPatchMod = new SkyrimMod(ModKey.FromFileName("TofuFixes.esp"), SkyrimRelease.SkyrimSE);
			CurrentEnvironment = GameEnvironment.Typical.Builder<ISkyrimMod, ISkyrimModGetter>(GameRelease.SkyrimSE)
				.TransformLoadOrderListings(m =>
				{
					var pluginDialogue = new PluginSelect(m);
					var dialogueResult = pluginDialogue.ShowDialog();
					if (dialogueResult is null or false)
					{
						return Enumerable.Empty<ILoadOrderListingGetter>();
					}

					return m
						.Where(r => pluginDialogue.PluginLoads.FirstOrDefault(plugin => plugin.PluginName.Equals(r.FileName)).WillLoad);
				})
				.WithOutputMod(CurrentPatchMod)
				.Build();

			try
			{
				Mouse.OverrideCursor = Cursors.Wait;
				IProgress<double> progress = new Progress<double>(percent =>
				{
					Debug.WriteLine(percent);
					ProgressBar.Value = percent;
				});
				IProgress<string> stringProgress = new Progress<string>(status =>
				{
					Debug.WriteLine(status);
					StatusLabel.Content = status;
				});

				var taskResults = await Task.Run(() =>
				{
					if (CurrentEnvironment is IGameEnvironment<ISkyrimMod, ISkyrimModGetter> skyrimGameEnvironment)
					{
						return LoadTofuSentences(skyrimGameEnvironment, progress, stringProgress);
					}
					return (null, null);
				});
				TofuSentences = new ObservableCollection<TofuSentence>(taskResults.Item1);
				WhiteSpaceSentences = new ObservableCollection<TrimmedSentence>(taskResults.Item2);
				TofuDataGrid.ItemsSource = TofuSentences;
				WhitespaceDataGrid.ItemsSource = WhiteSpaceSentences;
			}
			finally
			{
				Mouse.OverrideCursor = null;
			}
		}

		public void ProcessRecord(string textToScan, string FormID, string EditorID, string PluginFile, ICollection<TrimmedSentence> trimmedSentences, ICollection<TofuSentence> tofuSentences, Action<string> overrideAction)
		{
			if (textToScan.TryTrim(out var trimmedString))
			{
				var trimmedSentence = new TrimmedSentence(FormID, EditorID,
					PluginFile, textToScan);
				trimmedSentences.Add(trimmedSentence);
				textToScan = trimmedString;
			}

			if (TofuSentence.ContainsTofu(textToScan, out var chars))
			{
				var tofuSentence = new TofuSentence(FormID, EditorID,
					PluginFile,
					textToScan, chars.ToArray());
				tofuSentences.Add(tofuSentence);
				overrideAction(tofuSentence.FixedText);
			}
		}

		public (ICollection<TofuSentence>, ICollection<TrimmedSentence>) LoadTofuSentences(IGameEnvironment<ISkyrimMod, ISkyrimModGetter> env, IProgress<double> progress, IProgress<string> status)
		{
			var lastProgress = 0;
			var tofuSentences = new List<TofuSentence>();
			var trimmedSentences = new List<TrimmedSentence>();

			#region Topics
			status.Report("Scanning Topics...");

			var winningTopics = env
				.LoadOrder
				.PriorityOrder
				.DialogTopic()
				.WinningContextOverrides()
				.ToArray();

			for (var index = 0; index < winningTopics.Length; index++)
			{
				var topic = winningTopics[index];
				var currentProgress = index * 100 / winningTopics.Length;
				if (lastProgress != currentProgress)
				{
					progress.Report(currentProgress);
					lastProgress = currentProgress;
				}
				if (topic.Record.Name is not null && topic.Record.Name.String.IsNullOrWhitespace() == false)
				{
					ProcessRecord(topic.Record.Name.String,
						topic.Record.FormKey.IDString(),
						topic.Record.EditorID ?? string.Empty,
						topic.Record.FormKey.ModKey.FileName,
						trimmedSentences,
						tofuSentences,
						s =>
						{
							var patchedTopic = topic.GetOrAddAsOverride(CurrentPatchMod);
							patchedTopic.Name = s;
						});
				}
			}

			#endregion

			#region Responses
			status.Report("Scanning Dialogue...");

			var winningResponsesGetters = env
				.LoadOrder
				.PriorityOrder
				.DialogResponses()
				.WinningContextOverrides(env.LinkCache)
				.ToArray();

			for (var i = 0; i < winningResponsesGetters.Length; i++)
			{
				var currentProgress = i * 100 / winningResponsesGetters.Length;
				if (lastProgress != currentProgress)
				{
					progress.Report(currentProgress);
					lastProgress = currentProgress;
				}

				var responses = winningResponsesGetters[i];
				if (responses.Record.Prompt?.String != null && responses.Record.Prompt.String.IsNullOrWhitespace() == false)
				{
					ProcessRecord(responses.Record.Prompt.String,
						responses.Record.FormKey.IDString(),
						responses.Record.EditorID ?? string.Empty,
						responses.Record.FormKey.ModKey.FileName,
						trimmedSentences,
						tofuSentences,
						s =>
						{
							var patchedTopic = responses.GetOrAddAsOverride(CurrentPatchMod);
							patchedTopic.Prompt = s;
						});
				}
				for (var index = 0; index < responses.Record.Responses.Count; index++)
				{
					var response = responses.Record.Responses[index];
					if (response.Text.String != null && response.Text.String.IsNullOrWhitespace() == false)
					{
						ProcessRecord(response.Text.String,
							responses.Record.FormKey.IDString(),
							responses.Record.EditorID ?? string.Empty,
							responses.Record.FormKey.ModKey.FileName,
							trimmedSentences,
							tofuSentences,
							s =>
							{
								var patchedTopic = responses.GetOrAddAsOverride(CurrentPatchMod);
								patchedTopic.Responses[index].Text = s;
							});
					}
				}
			}

			#endregion

			#region NamedForms
			
			progress.Report(0);
			status.Report("Scanning Named Items...");

			var winningGetters = env
				.LoadOrder
				.PriorityOrder
				.WinningOverrides<ISkyrimMajorRecordGetter>()
				.Where(m => m is INamedGetter and not IDialogTopicGetter and not IDialogResponseGetter)
				.ToArray();

			for (var index = 0; index < winningGetters.Length; index++)
			{
				var currentProgress = index * 100 / winningGetters.Length;
				if (lastProgress != currentProgress)
				{
					progress.Report(currentProgress);
					lastProgress = currentProgress;
				}

				var majorGetter = winningGetters[index];

				if (majorGetter is not INamedGetter namedGetter || namedGetter.Name.IsNullOrWhitespace())
				{
					continue;
				}

				ProcessRecord(namedGetter.Name,
					majorGetter.FormKey.IDString(),
					majorGetter.EditorID ?? string.Empty,
					majorGetter.FormKey.ModKey.FileName,
					trimmedSentences,
					tofuSentences,
					s =>
					{
						var patchedText = env.LinkCache
							.ResolveContext<ISkyrimMajorRecord, ISkyrimMajorRecordGetter>(majorGetter.FormKey);
						patchedText.GetOrAddAsOverride(CurrentPatchMod);
						if (patchedText is INamed namedItem)
						{
							namedItem.Name = s;
						}
						else
						{
							Debug.Fail("An overriden record could not be edited afterwards!");
						}
					});
			}
			#endregion
			status.Report("Done!");

			return (tofuSentences, trimmedSentences);
		}
	}
}