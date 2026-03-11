namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34396, "UI becomes unresponsive when adding more than 200 Entry children to AbsoluteLayout", PlatformAffected.All)]
public class Issue34396 : ContentPage
{
	readonly Label _statusLabel;
	readonly Label _elapsedLabel;
	readonly AbsoluteLayout _canvas;
	System.Diagnostics.Stopwatch _sw = new System.Diagnostics.Stopwatch();

	public Issue34396()
	{
		var addButton = new Button
		{
			Text = "Add Editors",
			AutomationId = "AddEditors"
		};
		addButton.Clicked += OnAddEditorsClicked;

		_statusLabel = new Label
		{
			Text = "Ready",
			AutomationId = "StatusLabel"
		};

		_elapsedLabel = new Label
		{
			Text = "0",
			AutomationId = "ElapsedMs"
		};

		_canvas = new AbsoluteLayout
		{
			WidthRequest = 2000,
			HeightRequest = 3000,
			BackgroundColor = Color.FromArgb("#202020")
		};

		var root = new Grid
		{
			Padding = new Thickness(12),
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star },
			}
		};

		root.Add(addButton);

		Grid.SetRow(_statusLabel, 1);
		root.Add(_statusLabel);

		Grid.SetRow(_elapsedLabel, 2);
		root.Add(_elapsedLabel);

		var scroller = new ScrollView { Content = _canvas };
		Grid.SetRow(scroller, 3);
		root.Add(scroller);

		Content = root;
	}

	private void OnAddEditorsClicked(object sender, EventArgs e)
	{
		_statusLabel.Text = "Adding";
		_sw.Restart();

		var editors = new List<(Editor editor, Rect bounds)>();
		for (int i = 0; i < 200; i++)
		{
			double x = (i * 12) % 1800;
			double y = (i * 15) % 2800;

			var editor = new Editor
			{
				Text = $"Item {i}",
				FontSize = 12,
				BackgroundColor = Colors.Gray,
				TextColor = Colors.White,
				IsReadOnly = true
			};
			editors.Add((editor, new Rect(x, y, 120, 30)));
		}

		// Buggy pattern: 200 individual Dispatcher.Dispatch calls, each triggering a full
		// layout pass on the AbsoluteLayout. This floods the UI message queue and causes
		// the UI thread to be unresponsive for a significant amount of time.
		foreach (var (editor, bounds) in editors)
		{
			Dispatcher.Dispatch(() =>
			{
				_canvas.Children.Add(editor);
				AbsoluteLayout.SetLayoutBounds(editor, bounds);
			});
		}

		// Queued last - records elapsed wall-clock time when all editors are processed
		Dispatcher.Dispatch(() =>
		{
			_sw.Stop();
			_elapsedLabel.Text = _sw.ElapsedMilliseconds.ToString();
			_statusLabel.Text = "Done";
		});
	}
}

