using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34257, "CollectionView vertical grid item spacing updates all rows and columns", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue34257 : ContentPage
{
	readonly GridItemsLayout _itemsLayoutHorizontal;
	readonly GridItemsLayout _itemsLayoutVertical;
	readonly Label _statusLabel;

	readonly Label _statusLabelHorizontal;

	public Issue34257()
	{
		_itemsLayoutHorizontal = new GridItemsLayout(2, ItemsLayoutOrientation.Horizontal)
		{
			HorizontalItemSpacing = 0,
			VerticalItemSpacing = 0
		};
		_itemsLayoutVertical = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical)
		{
			HorizontalItemSpacing = 0,
			VerticalItemSpacing = 0
		};

		_statusLabel = new Label
		{
			AutomationId = "StatusLabelVertical",
			Text = "Spacing=0,0"
		};
		_statusLabelHorizontal = new Label
		{
			AutomationId = "StatusLabelHorizontal",
			Text = "Spacing=0,0"
		};

		var applySpacingButton = new Button
		{
			AutomationId = "ApplyHorizontalSpacingButton",
			Text = "Apply horizontal spacing"
		};
		applySpacingButton.Clicked += OnApplyHorizontalSpacingClicked;

		var applyVerticalSpacingButton = new Button
		{
			AutomationId = "ApplyVerticalSpacingButton",
			Text = "Apply vertical spacing"
		};
		applyVerticalSpacingButton.Clicked += OnApplyVerticalSpacingClicked;

		var collectionViewV = new CollectionView
		{
			AutomationId = "TestCollectionViewVertical",
			HorizontalOptions = LayoutOptions.Center,
			ItemsLayout = _itemsLayoutVertical,
			ItemsSource = CreateItems()
		};
		collectionViewV.ItemTemplate = new DataTemplate(() =>
		{
			var titleLabel = new Label
			{
				FontAttributes = FontAttributes.Bold,
				LineBreakMode = LineBreakMode.TailTruncation
			};
			titleLabel.SetBinding(Label.TextProperty, nameof(SpacingIssueItem.Name));

			var locationLabel = new Label
			{
				FontAttributes = FontAttributes.Italic,
				LineBreakMode = LineBreakMode.TailTruncation,
				VerticalOptions = LayoutOptions.End
			};
			locationLabel.SetBinding(Label.TextProperty, nameof(SpacingIssueItem.Location));

			var textLayout = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto }
				}
			};
			textLayout.Add(titleLabel);
			textLayout.Add(locationLabel, 0, 1);

			var root = new Grid
			{
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = 70 },
					new ColumnDefinition { Width = GridLength.Star }
				},
				Padding = 10
			};
			root.SetBinding(AutomationIdProperty, nameof(SpacingIssueItem.AutomationId));
			root.SetBinding(BackgroundColorProperty, nameof(SpacingIssueItem.BackgroundColor));

			var imagePlaceholder = new Border
			{
				Background = Colors.DarkSlateBlue,
				HeightRequest = 60,
				StrokeThickness = 0,
				VerticalOptions = LayoutOptions.Center,
				WidthRequest = 60
			};

			root.Add(imagePlaceholder);
			root.Add(textLayout, 1, 0);

			return root;
		});

		var horizontalCollectionView = new CollectionView
		{
			AutomationId = "TestCollectionViewHorizontal",
			HorizontalOptions = LayoutOptions.Center,
			ItemsLayout = _itemsLayoutHorizontal,
			ItemsSource = CreateItemsForHorizontal()
		};
		horizontalCollectionView.ItemTemplate = new DataTemplate(() =>
		{
			var titleLabel = new Label
			{
				FontAttributes = FontAttributes.Bold,
				LineBreakMode = LineBreakMode.TailTruncation
			};
			titleLabel.SetBinding(Label.TextProperty, nameof(SpacingIssueItem.Name));

			var locationLabel = new Label
			{
				FontAttributes = FontAttributes.Italic,
				LineBreakMode = LineBreakMode.TailTruncation,
				VerticalOptions = LayoutOptions.End
			};
			locationLabel.SetBinding(Label.TextProperty, nameof(SpacingIssueItem.Location));

			var textLayout = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto }
				}
			};
			textLayout.Add(titleLabel);
			textLayout.Add(locationLabel, 0, 1);

			var root = new Grid
			{
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = 70 },
					new ColumnDefinition { Width = GridLength.Star }
				},
				Padding = 10
			};
			root.SetBinding(AutomationIdProperty, nameof(SpacingIssueItem.AutomationId));
			root.SetBinding(BackgroundColorProperty, nameof(SpacingIssueItem.BackgroundColor));

			var imagePlaceholder = new Border
			{
				Background = Colors.DarkSlateBlue,
				HeightRequest = 60,
				StrokeThickness = 0,
				VerticalOptions = LayoutOptions.Center,
				WidthRequest = 60
			};

			root.Add(imagePlaceholder);
			root.Add(textLayout, 1, 0);

			return root;
		});

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 12,
				Children =
				{
					new HorizontalStackLayout
					{
						Spacing = 12,
						Children =
						{
							applySpacingButton,
							applyVerticalSpacingButton
						}
					},
					_statusLabel,
					_statusLabelHorizontal,
					new Label { Text = "Vertical CollectionView" },
					collectionViewV,
					new Label { Text = "Horizontal CollectionView" },
					horizontalCollectionView
				}
			}
		};
	}

	void OnApplyHorizontalSpacingClicked(object sender, EventArgs e)
	{
		_itemsLayoutHorizontal.VerticalItemSpacing = 80;
		_itemsLayoutHorizontal.HorizontalItemSpacing = 80;
		_statusLabelHorizontal.Text = "Spacing=80,80";
	}

	void OnApplyVerticalSpacingClicked(object sender, EventArgs e)
	{
         _itemsLayoutVertical.VerticalItemSpacing = 80;
		_itemsLayoutVertical.HorizontalItemSpacing = 80;
		_statusLabel.Text = "Spacing=80,80";
	}

	static ObservableCollection<SpacingIssueItem> CreateItems()
	{
		return
		[
			new SpacingIssueItem("FirstColumnTopItemVertical", "Capuchin", "Central America", Colors.LightSkyBlue),
			new SpacingIssueItem("SecondColumnTopItemVertical", "Spider", "South America", Colors.LightSalmon),
			new SpacingIssueItem("FirstColumnBottomItemVertical", "Howler", "South America", Colors.PaleGreen),
			new SpacingIssueItem("SecondColumnBottomItemVertical", "Baboon", "Africa", Colors.Khaki)
		];
	}

	static ObservableCollection<SpacingIssueItem> CreateItemsForHorizontal()
	{
		return
		[
			new SpacingIssueItem("FirstColumnTopItemHorizontal", "Capuchin", "Central America", Colors.LightSkyBlue),
			new SpacingIssueItem("SecondColumnTopItemHorizontal", "Spider", "South America", Colors.LightSalmon),
			new SpacingIssueItem("FirstColumnBottomItemHorizontal", "Howler", "South America", Colors.PaleGreen),
			new SpacingIssueItem("SecondColumnBottomItemHorizontal", "Baboon", "Africa", Colors.Khaki)
		];
	}

	class SpacingIssueItem(string automationId, string name, string location, Color backgroundColor)
	{
		public string AutomationId { get; } = automationId;

		public Color BackgroundColor { get; } = backgroundColor;

		public string Location { get; } = location;

		public string Name { get; } = name;
	}
}
