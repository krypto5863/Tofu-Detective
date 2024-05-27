namespace Bethesda.TofuDetector;

public class TrimmedSentence : ISentence
{
	public string FormKey { get; }
	public string EditorId { get; }
	public string PluginName { get; }
	public string Text { get; }

	public TrimmedSentence(string formKey, string editorId, string pluginName, string text)
	{
		FormKey = formKey;
		EditorId = editorId;
		PluginName = pluginName;
		Text = text;
	}
}

public static class StringExt
{
	public static bool TryTrim(this string currentString, out string trimmedString)
	{
		trimmedString = currentString.Trim();
		return !trimmedString.Equals(currentString);
	}
}