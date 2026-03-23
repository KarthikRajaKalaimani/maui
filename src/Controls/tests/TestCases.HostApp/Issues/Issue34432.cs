using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34432, "Content in a CollectionView header is not interactable when an EmptyView is displayed", PlatformAffected.UWP)]
public class Issue34432 : ContentPage
{
	readonly ObservableCollection<Issue34432Item> _items = new();
	int _clickCount = 0;

	public Issue34432()
	{
		var countLabel = new Label
		{
			HeightRequest = 100,
			Text = "Clicked: 0",
			AutomationId = "ClickCountLabel"
		};

		var clearButton = new Button { Text = "Clear items", Margin = 10 };
		clearButton.Clicked += (s, e) => _items.Clear();

		var addButton = new Button { Text = "Add item", Margin = 10, AutomationId = "HeaderButton" };
		addButton.Clicked += (s, e) =>
		{
			_clickCount++;
			countLabel.Text = $"Clicked: {_clickCount}";
			if (_items.Count < 20)
				_items.Add(new Issue34432Item(RandomString(20), _items.Count + 1));
		};

		var removeButton = new Button { Text = "Remove item", Margin = 10 };
		removeButton.Clicked += (s, e) =>
		{
			if (_items.Count > 0)
				_items.RemoveAt(_items.Count - 1);
		};

		var generateButton = new Button { Text = "Generate items", Margin = 10 };
		generateButton.Clicked += (s, e) => GetTestData(10);

		var header = new Grid
		{
			ColumnDefinitions =
			{
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Star),
			}
		};
		header.Add(clearButton, 0, 0);
		header.Add(addButton, 1, 0);
		header.Add(removeButton, 2, 0);
		header.Add(generateButton, 3, 0);

		var emptyView = new ContentView
		{
			BackgroundColor = Colors.IndianRed,
			InputTransparent = true,
			Content = new Border
			{
				Stroke = new SolidColorBrush(Colors.White),
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Content = new Grid
				{
					Margin = new Thickness(100, 50),
					RowDefinitions =
					{
						new RowDefinition(GridLength.Auto),
					},
					Children =
					{
						new Label { Text = "No items!", TextColor = Colors.White }

					}
				}
			}
		};

		var itemTemplate = new DataTemplate(() =>
		{
			var grid = new Grid
			{
				BackgroundColor = Colors.White,
				ColumnDefinitions =
				{
					new ColumnDefinition(50),
					new ColumnDefinition(GridLength.Star),
					new ColumnDefinition(GridLength.Auto),
				},
				RowDefinitions =
				{
					new RowDefinition(GridLength.Star),
					new RowDefinition(GridLength.Auto),
				}
			};

			var valueLabel = new Label { TextColor = Colors.Black, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center };
			valueLabel.SetBinding(Label.TextProperty, nameof(Issue34432Item.Value));

			var nameLabel = new Label { TextColor = Colors.Black, VerticalOptions = LayoutOptions.Center };
			nameLabel.SetBinding(Label.TextProperty, nameof(Issue34432Item.Name));
			Grid.SetColumn(nameLabel, 1);

			var detailsButton = new Button { Text = "Details", HeightRequest = 40, Margin = 10 };
			detailsButton.SetBinding(Button.CommandProperty, nameof(Issue34432Item.ButtonDetailCommand));
			Grid.SetColumn(detailsButton, 2);

			var separator = new Border { Stroke = new SolidColorBrush(Colors.LightGray), StrokeThickness = 0.5 };
			Grid.SetRow(separator, 1);
			Grid.SetColumnSpan(separator, 3);

			grid.Add(valueLabel);
			grid.Add(nameLabel);
			grid.Add(detailsButton);
			grid.Add(separator);

			return grid;
		});

		var cv = new CollectionView
		{
			ItemsSource = _items,
			Header = header,
			EmptyView = emptyView,
			ItemTemplate = itemTemplate,
		};
		//Grid.SetColumnSpan(cv, 3);

		var bottomGenerateButton = new Button { Text = "Generate items", Margin = 10 };
		bottomGenerateButton.Clicked += (s, e) => GetTestData(10);
		//Grid.SetRow(bottomGenerateButton, 1);
		//Grid.SetColumn(bottomGenerateButton, 1);

		var root = new StackLayout
		{
			Orientation = StackOrientation.Vertical,
			Children = { countLabel, cv, bottomGenerateButton }
		};
		Content = root;
	}

	void GetTestData(int itemCount)
	{
		_items.Clear();
		for (int i = 1; i <= itemCount; i++)
			_items.Add(new Issue34432Item(RandomString(20), i));
	}

	static readonly Random _random = new();
	static string RandomString(int length)
	{
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		return new string(Enumerable.Repeat(chars, length).Select(s => s[_random.Next(s.Length)]).ToArray());
	}
}

public class Issue34432Item
{
	public int Value { get; set; }
	public string Name { get; set; }
	public Command ButtonDetailCommand { get; set; }

	public Issue34432Item(string name, int value)
	{
		Name = name;
		Value = value;
		ButtonDetailCommand = new Command(() => { });
	}
}
