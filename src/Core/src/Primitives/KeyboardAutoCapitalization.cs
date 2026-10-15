using System.Globalization;

namespace Microsoft.Maui
{
	internal static class KeyboardAutoCapitalization
	{
		public static bool TryCapitalizeCharacter(
			string text,
			int insertionIndex,
			char character,
			Keyboard keyboard,
			out char capitalizedCharacter)
		{
			capitalizedCharacter = character;

			if (keyboard is not CustomKeyboard customKeyboard || !char.IsLetter(character))
				return false;

			var flags = customKeyboard.Flags;
			var shouldCapitalize =
				flags.HasFlag(KeyboardFlags.CapitalizeSentence) ? IsSentenceStart(text, insertionIndex) :
				flags.HasFlag(KeyboardFlags.CapitalizeWord) ? IsWordStart(text, insertionIndex) :
				flags.HasFlag(KeyboardFlags.CapitalizeCharacter);

			if (!shouldCapitalize)
				return false;

			capitalizedCharacter = char.ToUpper(character, CultureInfo.CurrentCulture);
			return capitalizedCharacter != character;
		}

		static bool IsSentenceStart(string text, int insertionIndex)
		{
			for (var i = insertionIndex - 1; i >= 0; i--)
			{
				var character = text[i];

				if (character is '\r' or '\n')
					return true;

				if (!char.IsWhiteSpace(character))
					return character is '.' or '!' or '?';
			}

			return true;
		}

		static bool IsWordStart(string text, int insertionIndex)
		{
			if (insertionIndex == 0)
				return true;

			var previousCharacter = text[insertionIndex - 1];
			return char.IsWhiteSpace(previousCharacter) ||
				(char.IsPunctuation(previousCharacter) && previousCharacter is not '\'' and not '’');
		}
	}
}
