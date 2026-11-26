namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35627, "Retained tab content must not unload on tab switches", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue35627 : TabbedPage
{
	public Issue35627()
	{
		var ready = new Label { AutomationId = "FirstTabReady", Text = "Waiting" };
		var status = new Label { AutomationId = "TabStatus", Text = "Waiting" };
		var view = new ContentView
		{
			Content = new Label { AutomationId = "TrackedContent", Text = "Content is visible" }
		};
		var firstLayout = new VerticalStackLayout { Children = { ready, view } };
		var first = new ContentPage { Title = "First", Content = firstLayout };
		var remove = new Button { AutomationId = "RemoveContent", Text = "Remove first tab content" };
		var second = new ContentPage
		{
			Title = "Second",
			Content = new VerticalStackLayout { Children = { status, remove } }
		};

		view.Loaded += (_, _) => ready.Text = "Loaded";
		view.Unloaded += (_, _) =>
		{
			var removed = view.Window is null;
			view.Handler?.DisconnectHandler();
			status.Text = removed ? "Removed" : "Unloaded on tab switch";
		};
		second.Appearing += (_, _) =>
		{
			int detachedChecks = 0;
			status.Text = "Waiting";
			second.Dispatcher.StartTimer(TimeSpan.FromMilliseconds(100), () =>
			{
				if (view.IsLoaded)
				{
					detachedChecks = 0;
					return true;
				}

				// Give the native unload callback a main-loop turn after the view leaves the window.
				if (++detachedChecks < 2)
					return true;

				status.Text = view.Handler is null ? "Settled: disconnected" : "Settled: retained";
				return false;
			});
		};
		remove.Clicked += (_, _) =>
		{
			status.Text = "Removing";
			firstLayout.Remove(view);
		};

		Children.Add(first);
		Children.Add(second);
	}
}
