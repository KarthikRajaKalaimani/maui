#nullable enable
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Handlers
{
	public partial class EditorHandler : ViewHandler<IEditor, TextBox>
	{
		bool _set;
		bool _updatingAutoCapitalization;
		string _previousText = string.Empty;

		protected override TextBox CreatePlatformView() =>
			new TextBox
			{
				AcceptsReturn = true,
				TextWrapping = TextWrapping.Wrap,
			};

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			if (!_set)
				PlatformView.SelectionChanged += OnSelectionChanged;

			_set = true;
		}

		protected override void ConnectHandler(TextBox platformView)
		{
			_previousText = platformView.Text;
			platformView.TextChanged += OnTextChanged;
			platformView.LostFocus += OnLostFocus;
			platformView.SizeChanged += OnPlatformViewSizeChanged;
		}

		protected override void DisconnectHandler(TextBox platformView)
		{
			platformView.SizeChanged -= OnPlatformViewSizeChanged;
			platformView.TextChanged -= OnTextChanged;
			platformView.LostFocus -= OnLostFocus;

			_previousText = string.Empty;
			_updatingAutoCapitalization = false;

			if (_set)
				platformView.SelectionChanged -= OnSelectionChanged;

			_set = false;
		}

		public static void MapText(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateText(editor);

		public static void MapPlaceholder(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdatePlaceholder(editor);

		public static void MapPlaceholderColor(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdatePlaceholderColor(editor);

		public static void MapCharacterSpacing(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateCharacterSpacing(editor);

		public static void MapMaxLength(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateMaxLength(editor);

		public static void MapIsTextPredictionEnabled(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateIsTextPredictionEnabled(editor);

		public static void MapIsSpellCheckEnabled(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateIsSpellCheckEnabled(editor);

		public static void MapFont(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateFont(editor, handler.GetRequiredService<IFontManager>());

		public static void MapIsReadOnly(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateIsReadOnly(editor);

		public static void MapBackground(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateBackground(editor);

		public static void MapTextColor(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateTextColor(editor);

		public static void MapHorizontalTextAlignment(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateHorizontalTextAlignment(editor);

		public static void MapCursorPosition(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateCursorPosition(editor);

		public static void MapSelectionLength(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateSelectionLength(editor);

		public static void MapVerticalTextAlignment(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateVerticalTextAlignment(editor);

		public static void MapKeyboard(IEditorHandler handler, IEditor editor) =>
			handler.PlatformView?.UpdateKeyboard(editor);

		void OnTextChanged(object sender, TextChangedEventArgs args)
		{
			if (_updatingAutoCapitalization)
				return;

			var currentText = PlatformView.Text;

			if (VirtualView is IEditor editor &&
				PlatformView.FocusState != FocusState.Unfocused &&
				TryGetSingleCharacterInsertion(_previousText, currentText, out var insertionIndex, out var insertedCharacter) &&
				KeyboardAutoCapitalization.TryCapitalizeCharacter(
					_previousText,
					insertionIndex,
					insertedCharacter,
					editor.Keyboard,
					out var capitalizedCharacter))
			{
				_updatingAutoCapitalization = true;
				currentText = currentText.Remove(insertionIndex, 1)
					.Insert(insertionIndex, capitalizedCharacter.ToString());
				PlatformView.Text = currentText;
				PlatformView.SelectionStart = insertionIndex + 1;
				_updatingAutoCapitalization = false;
			}

			_previousText = currentText;
			VirtualView?.UpdateText(currentText);
		}

		static bool TryGetSingleCharacterInsertion(
			string previousText,
			string currentText,
			out int insertionIndex,
			out char insertedCharacter)
		{
			insertionIndex = -1;
			insertedCharacter = default;

			if (currentText.Length != previousText.Length + 1)
				return false;

			var index = 0;
			while (index < previousText.Length && previousText[index] == currentText[index])
				index++;

			if (!currentText.AsSpan(index + 1).SequenceEqual(previousText.AsSpan(index)))
				return false;

			insertionIndex = index;
			insertedCharacter = currentText[index];
			return true;
		}

		void OnLostFocus(object? sender, RoutedEventArgs e) =>
			VirtualView?.Completed();

		void OnPlatformViewSizeChanged(object sender, SizeChangedEventArgs e) =>
			MauiTextBox.InvalidateAttachedProperties(PlatformView);

		private void OnSelectionChanged(object sender, RoutedEventArgs e)
		{
			var cursorPosition = PlatformView.GetCursorPosition();
			var selectedTextLength = PlatformView.SelectionLength;

			if (VirtualView.CursorPosition != cursorPosition)
				VirtualView.CursorPosition = cursorPosition;

			if (VirtualView.SelectionLength != selectedTextLength)
				VirtualView.SelectionLength = selectedTextLength;
		}
	}
}