﻿using System.Text;
using AnyAscii;

namespace Bethesda.TofuDetector;

public class TofuSentence : ISentence
{
	public string FormKey { get; }
	public string EditorId { get; }
	public string PluginName { get; }
	public string Text { get; }
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

	public char[] UnsupportedChars { get; }
	public string UnsupportedCharacters => string.Join(',', UnsupportedChars);
	public string UnsupportedCharactersUnicode => string.Join(' ', UnsupportedChars.Select(m => "U+" + ((int)m).ToString("X4") + $"({m})"));

	private static string LatinToAscii(string inString)
	{
		var newStringBuilder = new StringBuilder();
		newStringBuilder.Append(inString.Normalize(NormalizationForm.FormKD)
			.ToArray());
		return newStringBuilder.ToString();
	}

	public TofuSentence(string formKey, string editorId, string pluginName, string text, char[] unsupportedCharacters)
	{
		FormKey = formKey;
		EditorId = editorId;
		PluginName = pluginName;
		Text = text;
		UnsupportedChars = unsupportedCharacters.Distinct().ToArray();
	}
	private static readonly string ValidCharsString =
		"`1234567890-=~!@#$%^&*():_+QWERTYUIOP[]ASDFGHJKL;'\"ZXCVBNM,./qwertyuiop{}\\asdfghjklzxcvbnm<>?|¡¢£¤¥¦§¨©ª«®¯°²³´¶·¸¹º»¼½¾¿ÄÀÁÂÃÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþ ÿ ";
	/*
	private static readonly List<int> SupportedChars = new()
	{
		32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64,
		65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96,
		97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122,
		123, 124, 125, 126, 160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 170, 171, 174, 175, 176, 178, 179, 180, 182, 183, 184,
		185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200, 201, 202, 203, 204, 205, 206, 207, 208, 209,
		210, 211, 212, 213, 214, 215, 216, 217, 218, 219, 220, 221, 222, 223, 224, 225, 226, 227, 228, 229, 230, 231, 232, 233, 234,
		235, 236, 237, 238, 239, 240, 241, 242, 243, 244, 245, 246, 247, 248, 249, 250, 251, 252, 253, 254, 255, 339, 8209, 8211,
		8212, 8216, 8217, 8218, 8220, 8221, 8222, 8224, 8225, 8226, 8230, 8240, 8249, 8250, 8260, 8482
	};
	*/

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