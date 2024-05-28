using CommunityToolkit.Mvvm.ComponentModel;
using Mutagen.Bethesda.Plugins;

namespace Bethesda.TofuDetector.Models;

public partial class PluginLoad : ObservableObject
{
	public string PluginName { get; }

	[ObservableProperty]
	private bool _willLoad = true;

	public string Priority { get; }
	internal ModKey ModKey { get; }

	public PluginLoad(string pluginName, int priority, ModKey modKey)
	{
		PluginName = pluginName;
		ModKey = modKey;
		Priority = priority.ToString();
	}
}