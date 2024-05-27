using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;

namespace Bethesda.TofuDetector;

public class PluginLoad : INotifyPropertyChanged
{
	public string PluginName { get; }
	public bool WillLoad { get; set; } = true;
	public string Priority { get; }
	public event PropertyChangedEventHandler? PropertyChanged;
	public PluginLoad(string pluginName, int priority)
	{
		PluginName = pluginName;
		Priority = priority.ToString();
	}
}