using System.ComponentModel;

namespace Bethesda.TofuDetector;

public class PluginLoad : INotifyPropertyChanged
{
	public string PluginName { get; }
	public bool WillLoad { get; set; } = true;
	public event PropertyChangedEventHandler? PropertyChanged;
	public PluginLoad(string pluginName)
	{
		PluginName = pluginName;
	}
}