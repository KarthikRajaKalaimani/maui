namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33613, "Mac Catalyst: NavigationPage.TitleView layout shifts and adds extra spacing when window is maximized", PlatformAffected.macOS)]
public class Issue33613NavPage : NavigationPage
{
	public Issue33613NavPage() : base(new Issue33613())
	{
	}
}

public partial class Issue33613 : ContentPage
{
	public Issue33613()
	{
		InitializeComponent();
		
		// Update TitleView position when layout changes
		TitleViewGrid.SizeChanged += (s, e) =>
		{
			// This helps with debugging - shows the TitleView position
			var bounds = TitleViewGrid.Bounds;
			TitleViewPositionLabel.Text = $"TitleView X: {bounds.X:F0}, Width: {bounds.Width:F0}";
		};
	}
	
	private async void OnPushClicked(object sender, EventArgs e)
	{
		StatusLabel.Text = "Pushing new page...";
		
		// Push a new page to trigger back button appearance
		var newPage = new ContentPage
		{
			Title = "Second Page",
			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 10,
				Children =
				{
					new Label 
					{ 
						Text = "Second Page - Has Back Button",
						FontSize = 20,
						FontAttributes = FontAttributes.Bold,
						HorizontalOptions = LayoutOptions.Center,
						AutomationId = "SecondPageLabel"
					},
					new Label
					{
						Text = "Maximize the window and observe TitleView spacing",
						FontSize = 14,
						AutomationId = "InstructionLabel"
					},
					new Button
					{
						Text = "Pop Back",
						AutomationId = "PopButton",
						HorizontalOptions = LayoutOptions.Center
					}
				}
			}
		};
		
		// Set TitleView on the new page too
		var secondTitleView = new Grid
		{
			BackgroundColor = Colors.LightGreen,
			HorizontalOptions = LayoutOptions.FillAndExpand,
			AutomationId = "SecondPageTitleViewGrid",
			Children =
			{
				new Label
				{
					Text = "Second Page Title",
					TextColor = Colors.White,
					FontSize = 18,
					FontAttributes = FontAttributes.Bold,
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					AutomationId = "SecondPageTitleLabel"
				}
			}
		};
		
		NavigationPage.SetTitleView(newPage, secondTitleView);
		
		((Button)((VerticalStackLayout)newPage.Content).Children[2]).Clicked += async (s, e) =>
		{
			await Navigation.PopAsync();
		};
		
		await Navigation.PushAsync(newPage);
		StatusLabel.Text = "Page pushed - now has back button";
	}
}
