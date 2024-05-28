# Overview
I built this tool while aiding with the development of Serana Dialog Addon, the mod had quite a number of unsupported characters that caused rectangles to show up in text. The issue was kind of nuanced, so I came up with this tool to resolve the problem, and decided I could clean it up for release.

- Detects and attempts to convert unsupported characters to the supported ASCII range using Transliteration.
- Checks if text has pointless trailing or leading white spaces (that is, spaces at the start of end of the sentence).
- Shows a detailed list of the sentences, what was fixed, how it was fixed, what the issue is, etc,.
- Allows you to save a patch file with the fixes for every selected plugin.

Powered by [Mutagen](https://github.com/Mutagen-Modding/Mutagen)
## How is the useful
This will be particularly useful for developers who make English mods while having a keyboard for a different language where the characters can be slightly different causing Tofu.

This will probably be useless, if not harmful, to developers making mods in different languages, but if asked, I can look into supporting it.
## What is Tofu?
[Google's Explanation](https://fonts.google.com/knowledge/glossary/tofu)
The short answer is, tofu is just a name for the glyph not found character. It's typically shaped like a rectangle, and it's been named tofu because of it's rectangular shape.

## Why Does Tofu Occur in Skyrim?
While the font itself supports more characters than Skyrim is willing to use, Skyrim has it's own restrictions on what characters it may use. Typically, every character on a standard US Qwerty keyboard will display fine. However, some users from other countries have characters with slightly different diacritics that look basically the same, but Skyrim will refuse to display properly. Here's some to name a few, they have been appended with their Unicode code-points so you can understand the programmatic difference, if you look closely enough, you'll even notice they're shaped slightly differently:

    U+2018: ‘
    U+2019: ’
    U+201C: “
    U+201D: ”
    U+2026: …
    U+2010: ‐

Their basic Latin and supported counterparts would be:

    U+0027: '
    U+0022: "
    U+002E: .
    U+002D: -

## How is this fixed?
All the tool does is load the selected plugins and check all the dialogue forms and then the named forms and checks if a character not on Skyrim's own validNameChars string is in there. Once found, it will use the AnyASCII library to transliterate the character to a basic counterpart and save the result to a patch mod. The results have so far been excellent.

As extra, the tool also trims any text of latent white space at the ends of the text.

## Usage?
You need the [Net 8.0 Desktop Runtime x64](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
Afterwards, just add Tofu Detective to MO2 as a new executable and run it. It's very intuitive afterwards.
