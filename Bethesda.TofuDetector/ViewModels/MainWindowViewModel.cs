using Bethesda.TofuDetector.Models;
using Bethesda.TofuDetector.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using JetBrains.Annotations;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Environments;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Plugins.Aspects;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;

namespace Bethesda.TofuDetector.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
	[UsedImplicitly]
	public string Title { get; } = "Tofu Detective " + GetRunningVersion();

	private static Version GetRunningVersion()
	{
		return Assembly.GetExecutingAssembly().GetName().Version ?? new Version();
	}

	[ObservableProperty]
	private double _progress;

	[ObservableProperty]
	private string _status = "Idle";

	[ObservableProperty]
	private string _metrics = string.Empty;

	[ObservableProperty]
	private ObservableCollection<TofuSentence>? _tofuSentences;

	[ObservableProperty]
	private ObservableCollection<TrimmedSentence>? _whiteSpaceSentences;

	private readonly ISkyrimMutagenService _mutagenService;

	public MainWindowViewModel()
	{
		_mutagenService = SkyrimMutagenService.GetOrCreateInstance(GameRelease.SkyrimSE);
	}
	public bool CanSavePatch() => _mutagenService.PatchMod is not null;

	public async Task SavePatchPlugin(string path)
	{
		Status = _mutagenService.SavePatch(path) == false ? "Error: Patch could not be saved!" : $"Saved patch to: {path}";
		await Task.Delay(5000).ContinueWith(_ =>
		{
			Status = "Idle";
		});
	}

	public async Task LoadOrderAndProcess(ModKey[] modKeys)
	{
		_mutagenService.Load(modKeys);

		IProgress<double> progress = new Progress<double>(percent =>
		{
			Debug.WriteLine(percent);
			Progress = percent;
		});
		IProgress<string> stringProgress = new Progress<string>(status =>
		{
			Debug.WriteLine(status);
			Status = status;
		});

		var taskResults = await Task.Run(() =>
		{
			if (_mutagenService.SkyrimEnvironment is { } skyrimGameEnvironment)
			{
				Debug.Assert(_mutagenService.PatchMod != null, "_mutagenService.PatchMod != null");
				return LoadTofuSentences(skyrimGameEnvironment, _mutagenService.PatchMod, progress, stringProgress);
			}
			return (Enumerable.Empty<TofuSentence>(), Enumerable.Empty<TrimmedSentence>());
		});
		TofuSentences = new ObservableCollection<TofuSentence>(taskResults.Item1);
		WhiteSpaceSentences = new ObservableCollection<TrimmedSentence>(taskResults.Item2);

		Metrics = $"Whitespace: {WhiteSpaceSentences.Count} | Tofu: {TofuSentences.Count}";
	}

	private static void ProcessRecord(string textToScan, string formId, string editorId, string pluginFile, ICollection<TrimmedSentence> trimmedSentences, ICollection<TofuSentence> tofuSentences, Action<string> overrideAction)
	{
		var shouldSave = false;
		if (textToScan.TryTrim(out var trimmedString))
		{
			var trimmedSentence = new TrimmedSentence(formId, editorId,
				pluginFile, textToScan);
			trimmedSentences.Add(trimmedSentence);
			textToScan = trimmedString;
			shouldSave = true;
		}

		if (TofuSentence.ContainsTofu(textToScan, out var chars))
		{
			var tofuSentence = new TofuSentence(formId, editorId,
				pluginFile,
				textToScan, chars.ToArray());
			tofuSentences.Add(tofuSentence);
			textToScan = tofuSentence.FixedText;
			if (tofuSentence.IsDifferent)
			{
				shouldSave = true;
			}
		}

		if (shouldSave)
		{
			overrideAction(textToScan);
		}
	}

	private static (IEnumerable<TofuSentence>, IEnumerable<TrimmedSentence>) LoadTofuSentences(IGameEnvironment<ISkyrimMod, ISkyrimModGetter> environment, ISkyrimMod currentPatchMod, IProgress<double> progress, IProgress<string> status)
	{
		var sw = Stopwatch.StartNew();

		var lastProgress = 0;
		var tofuSentences = new List<TofuSentence>();
		var trimmedSentences = new List<TrimmedSentence>();

		/*
		#region Topics

		status.Report("Scanning Topic Forms...");

		var winningTopics = environment
			.LoadOrder
			.PriorityOrder
			.DialogTopic()
			.WinningContextOverrides()
			.ToArray();

		for (var index = 0; index < winningTopics.Length; index++)
		{
			var topic = winningTopics[index];
			var currentProgress = (index + 1) * 100 / winningTopics.Length;
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
						var patchedTopic = topic.GetOrAddAsOverride(currentPatchMod);
						patchedTopic.Name = s;
					});
			}
		}

		#endregion Topics

		#region Responses

		status.Report("Scanning Response Forms...");

		var winningResponsesGetters = environment
			.LoadOrder
			.PriorityOrder
			.DialogResponses()
			.WinningContextOverrides(environment.LinkCache)
			.ToArray();

		for (var i = 0; i < winningResponsesGetters.Length; i++)
		{
			var currentProgress = (i + 1) * 100 / winningResponsesGetters.Length;
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
						var patchedTopic = responses.GetOrAddAsOverride(currentPatchMod);
						patchedTopic.Prompt = s;
					});
			}
			for (var index = 0; index < responses.Record.Responses.Count; index++)
			{
				var response = responses.Record.Responses[index];
				if (response.Text.String != null && response.Text.String.IsNullOrWhitespace() == false)
				{
					var index1 = index;
					ProcessRecord(response.Text.String,
						responses.Record.FormKey.IDString(),
						responses.Record.EditorID ?? string.Empty,
						responses.Record.FormKey.ModKey.FileName,
						trimmedSentences,
						tofuSentences,
						s =>
						{
							var patchedTopic = responses.GetOrAddAsOverride(currentPatchMod);
							patchedTopic.Responses[index1].Text = s;
						});
				}
			}
		}

		#endregion Responses
		*/

		#region NamedForms

		progress.Report(0);
		status.Report("Loading Forms...");

		var winningGetters = environment
			.LoadOrder
			.PriorityOrder
			.WinningOverrides<ISkyrimMajorRecordGetter>()
			.Where(m => m is INamedGetter or IDialogResponsesGetter)
			.ToArray();

		status.Report("Scanning Forms...");

		for (var index = 0; index < winningGetters.Length; index++)
		{
			var currentProgress = (index + 1) * 100 / winningGetters.Length;
			if (lastProgress != currentProgress)
			{
				progress.Report(currentProgress);
				lastProgress = currentProgress;
			}

			var majorGetter = winningGetters[index];
			if (majorGetter is INamedGetter namedGetter && namedGetter.Name.IsNullOrWhitespace() == false)
			{
				ProcessRecord(namedGetter.Name,
					majorGetter.FormKey.IDString(),
					majorGetter.EditorID ?? string.Empty,
					majorGetter.FormKey.ModKey.FileName,
					trimmedSentences,
					tofuSentences,
					s =>
					{
						var patchedText = environment.LinkCache
							.ResolveContext<ISkyrimMajorRecord, ISkyrimMajorRecordGetter>(majorGetter.FormKey);
						var overrideText = patchedText.GetOrAddAsOverride(currentPatchMod);
						if (overrideText is INamed namedItem)
						{
							namedItem.Name = s;
						}
						else
						{
							Debug.Fail("An overriden record could not be edited afterwards!");
						}
					});
			}

			if (majorGetter is IDialogResponsesGetter dialogResponses)
			{
				if (dialogResponses.Prompt?.String != null &&
				    dialogResponses.Prompt.String.IsNullOrWhitespace() == false)
				{
					ProcessRecord(dialogResponses.Prompt.String,
						dialogResponses.FormKey.IDString(),
						dialogResponses.EditorID ?? string.Empty,
						dialogResponses.FormKey.ModKey.FileName,
						trimmedSentences,
						tofuSentences,
						s =>
						{
							var patchedText = environment.LinkCache
								.ResolveContext<ISkyrimMajorRecord, ISkyrimMajorRecordGetter>(dialogResponses.FormKey);
							var newMajorRecord = patchedText.GetOrAddAsOverride(currentPatchMod);
							if (newMajorRecord is IDialogResponses overrideDialogResponses)
							{
								overrideDialogResponses.Prompt = s;
							}
							else
							{
								Debug.Fail("An overriden record could not be edited afterwards!");
							}
						});
				}

				for (var index1 = 0; index1 < dialogResponses.Responses.Count; index1++)
				{
					var response = dialogResponses.Responses[index1];
					if (response.Text.String == null || response.Text.String.IsNullOrWhitespace())
					{
						continue;
					}

					var localIndex = index1;
					ProcessRecord(response.Text.String,
						dialogResponses.FormKey.IDString(),
						dialogResponses.EditorID ?? string.Empty,
						dialogResponses.FormKey.ModKey.FileName,
						trimmedSentences,
						tofuSentences,
						s =>
						{
							var patchedText = environment.LinkCache
								.ResolveContext<ISkyrimMajorRecord, ISkyrimMajorRecordGetter>(dialogResponses.FormKey);
							var newMajorRecord = patchedText.GetOrAddAsOverride(currentPatchMod);
							if (newMajorRecord is IDialogResponses overrideDialogResponses)
							{
								overrideDialogResponses.Responses[localIndex].Text = s;
							}
							else
							{
								Debug.Fail("An overriden record could not be edited afterwards!");
							}
						});
				}
			}
		}

		#endregion NamedForms

		status.Report($"Done! {sw.Elapsed}");

		return (tofuSentences, trimmedSentences);
	}
}