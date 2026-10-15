using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core)]
	public class KeyboardAutoCapitalizationTests
	{
		[Theory]
		[InlineData("", 0, 'h', 'H')]
		[InlineData("hello. ", 7, 'w', 'W')]
		[InlineData("hello!\t", 7, 'w', 'W')]
		[InlineData("hello\n", 6, 'w', 'W')]
		public void CapitalizeSentenceCapitalizesSentenceStarts(
			string text,
			int insertionIndex,
			char character,
			char expected)
		{
			var keyboard = Keyboard.Create(KeyboardFlags.CapitalizeSentence);

			var changed = KeyboardAutoCapitalization.TryCapitalizeCharacter(
				text,
				insertionIndex,
				character,
				keyboard,
				out var actual);

			Assert.True(changed);
			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData("hello ", 6)]
		[InlineData("hello-", 6)]
		public void CapitalizeWordCapitalizesWordStarts(string text, int insertionIndex)
		{
			var keyboard = Keyboard.Create(KeyboardFlags.CapitalizeWord);

			var changed = KeyboardAutoCapitalization.TryCapitalizeCharacter(
				text,
				insertionIndex,
				'w',
				keyboard,
				out var actual);

			Assert.True(changed);
			Assert.Equal('W', actual);
		}

		[Theory]
		[InlineData(KeyboardFlags.CapitalizeSentence, "hello ", 6)]
		[InlineData(KeyboardFlags.CapitalizeWord, "hello", 5)]
		[InlineData(KeyboardFlags.CapitalizeNone, "", 0)]
		public void DoesNotCapitalizeOutsideConfiguredBoundary(
			KeyboardFlags flags,
			string text,
			int insertionIndex)
		{
			var keyboard = Keyboard.Create(flags);

			var changed = KeyboardAutoCapitalization.TryCapitalizeCharacter(
				text,
				insertionIndex,
				'w',
				keyboard,
				out var actual);

			Assert.False(changed);
			Assert.Equal('w', actual);
		}

		[Fact]
		public void CapitalizeCharacterCapitalizesEveryLetter()
		{
			var keyboard = Keyboard.Create(KeyboardFlags.CapitalizeCharacter);

			var changed = KeyboardAutoCapitalization.TryCapitalizeCharacter(
				"hello",
				5,
				'w',
				keyboard,
				out var actual);

			Assert.True(changed);
			Assert.Equal('W', actual);
		}

		[Fact]
		public void CombinedFlagsPreserveSentencePriority()
		{
			var keyboard = Keyboard.Create(KeyboardFlags.All);

			var changed = KeyboardAutoCapitalization.TryCapitalizeCharacter(
				"hello ",
				6,
				'w',
				keyboard,
				out var actual);

			Assert.False(changed);
			Assert.Equal('w', actual);
		}
	}
}
