namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32050, "IconOverride in Shell.BackButtonBehavior does not work.", PlatformAffected.Android)]
public class Issue32050 : Shell
{
	public Issue32050()
	{
		Items.Add(new ContentPage());
		Navigation.PushAsync(new Issue32050SubPage());
	}

	public class Issue32050SubPage : ContentPage
	{
		public Issue32050SubPage()
		{
			AutomationId = "Issue32050SubPage";
			Shell.SetBackButtonBehavior(this, new BackButtonBehavior
			{
				IconOverride = "coffee.png"
			});

			Content = new Label
			{
				AutomationId = "SubPageLabel",
				Text = "Sub Page - back button should show coffee icon",
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			};
		}
	}
}
