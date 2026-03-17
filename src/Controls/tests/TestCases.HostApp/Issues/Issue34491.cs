using System.Collections.Generic;
using System.Linq;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34491, "[Android] CollectionView item selection not triggered when using PointerGestureRecognizer", PlatformAffected.Android)]
public class Issue34491 : ContentPage
{
	int _selectionChangedCount;

	public Issue34491()
	{
		var selectionChangedLabel = new Label
		{
			AutomationId = "SelectionChangedLabel",
			Text = "SelectionChanged Count: 0"
		};

		var selectedItemLabel = new Label
		{
			AutomationId = "SelectedItemLabel",
			Text = "Selected Item: None"
		};

		var pointerStateLabel = new Label
		{
			AutomationId = "PointerStateLabel",
			Text = "Pointer State: None"
		};

		var collectionView = new CollectionView
		{
			AutomationId = "TestCollectionView",
			SelectionMode = SelectionMode.Single,
			ItemsSource = new List<string> { "Item 1", "Item 2", "Item 3" },
			ItemTemplate = new DataTemplate(() =>
			{
				var itemLabel = new Label
				{
					AutomationId = "ItemLabel",
					VerticalOptions = LayoutOptions.Center,
					Margin = new Thickness(12, 8)
				};
				itemLabel.SetBinding(Label.TextProperty, ".");

				var container = new Grid
				{
					AutomationId = "ItemContainer",
					BackgroundColor = Colors.LightGray,
					HeightRequest = 56,
					Padding = new Thickness(8)
				};

				var pointerGestureRecognizer = new PointerGestureRecognizer();
				pointerGestureRecognizer.PointerEntered += (_, _) => pointerStateLabel.Text = "Pointer State: Entered";
				pointerGestureRecognizer.PointerExited += (_, _) => pointerStateLabel.Text = "Pointer State: Exited";

				container.GestureRecognizers.Add(pointerGestureRecognizer);
				container.Add(itemLabel);

				return container;
			})
		};

		collectionView.SelectionChanged += (_, e) =>
		{
			_selectionChangedCount++;
			selectionChangedLabel.Text = $"SelectionChanged Count: {_selectionChangedCount}";

			var selectedItem = e.CurrentSelection.FirstOrDefault()?.ToString() ?? "None";
			selectedItemLabel.Text = $"Selected Item: {selectedItem}";
		};

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(12),
			Spacing = 12,
			Children =
			{
				new Label
				{
					Text = "Tap the first CollectionView item. The item should be selected and SelectionChanged should fire.",
					AutomationId = "InstructionsLabel"
				},
				selectionChangedLabel,
				selectedItemLabel,
				pointerStateLabel,
				collectionView
			}
		};
	}
}
