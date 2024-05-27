namespace Bethesda.TofuDetector;

public interface ISentence
{
	public string FormKey { get; }
	public string EditorId { get; }
	public string PluginName { get; }
	public string Text { get; }
}