using AnyAscii;
using JetBrains.Annotations;

namespace Bethesda.TofuDetector.Models;

public class TofuSentence : ISentence
{
	public string FormKey { get; }
	public string EditorId { get; }
	public string PluginName { get; }
	public string Text { get; }

	[UsedImplicitly]
	public string IndicatedText
	{
		get
		{
			var result = UnsupportedChars
				.Distinct()
				.Aggregate(Text, (current, character) => current.Replace($"{character}", $"(({character}))"));
			return result;
		}
	}

	public string FixedText
	{
		get
		{
			var result = UnsupportedChars
				.Distinct()
				.Aggregate(Text.Trim(), (current, character) => current.Replace($"{character}", $"{character}".Transliterate()));
			return result;
		}
	}

	public bool IsDifferent => !Text.Equals(FixedText);

	public char[] UnsupportedChars { get; }

	[UsedImplicitly]
	public string UnsupportedCharacters => string.Join(',', UnsupportedChars);

	[UsedImplicitly]
	public string UnsupportedCharactersUnicode => string.Join(' ', UnsupportedChars.Select(m => "U+" + ((int)m).ToString("X4") + $"({m})"));

	public TofuSentence(string formKey, string editorId, string pluginName, string text, char[] unsupportedCharacters)
	{
		FormKey = formKey;
		EditorId = editorId;
		PluginName = pluginName;
		Text = text;
		UnsupportedChars = unsupportedCharacters.Distinct().ToArray();
	}

	private const string ValidCharsString = "`1234567890-=~!@#$%^&*():_+QWERTYUIOP[]ASDFGHJKL;'\"ZXCVBNM,./qwertyuiop{}\\asdfghjklzxcvbnm<>?|¡¢£¤¥¦§¨©ª«®¯°²³´¶·¸¹º»¼½¾¿ÄÀÁÂÃÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþ ÿ ";

	public static bool ContainsTofu(string text, out ICollection<char> unsupportedChars)
	{
		unsupportedChars = new List<char>();
		var result = false;

		foreach (var character in text)
		{
			if (ValidCharsString.Contains(character))
			{
				continue;
			}

			unsupportedChars.Add(character);
			result = true;
		}

		unsupportedChars = unsupportedChars
			.Distinct()
			.ToArray();

		return result;
	}
}