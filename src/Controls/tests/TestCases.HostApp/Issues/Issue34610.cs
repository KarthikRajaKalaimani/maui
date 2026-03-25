using System.Globalization;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34610, "Shell titleview has unremovable horizontal and vertical margin", PlatformAffected.All)]
public class Issue34610 : TestShell
{
	const double TitleViewHeight = 60;

	protected override void Init()
	{
		var titleView = new Grid
		{
			AutomationId = "TitleViewGrid",
			BackgroundColor = Colors.Red,
			HeightRequest = TitleViewHeight,
			Margin = new Thickness(0),
			Padding = new Thickness(0),
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Fill
		};

		titleView.Add(new Label
		{
			Text = "TitleView should not have extra margin",
			AutomationId = "TitleViewLabel",
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center,
			TextColor = Colors.White
		});

		var titleViewMetrics = new Label
		{
			AutomationId = "TitleViewMetrics",
			Text = "TitleView=0x0"
		};

		var referenceView = new Grid
		{
			AutomationId = "ReferenceView",
			BackgroundColor = Colors.Blue,
			HeightRequest = TitleViewHeight,
			Margin = new Thickness(0),
			Padding = new Thickness(0),
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Fill
		};

		referenceView.Add(new Label
		{
			Text = "Reference content area",
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center,
			TextColor = Colors.White
		});

		var referenceMetrics = new Label
		{
			AutomationId = "ReferenceMetrics",
			Text = "Reference=0x0"
		};

		void UpdateMetrics()
		{
			if (titleView.Width <= 0 || titleView.Height <= 0 || referenceView.Width <= 0 || referenceView.Height <= 0)
				return;

			titleViewMetrics.Text = string.Create(
				CultureInfo.InvariantCulture,
				$"TitleView={titleView.Width:F1}x{titleView.Height:F1}");
			referenceMetrics.Text = string.Create(
				CultureInfo.InvariantCulture,
				$"Reference={referenceView.Width:F1}x{referenceView.Height:F1}");
		}

		titleView.SizeChanged += (_, _) => UpdateMetrics();
		referenceView.SizeChanged += (_, _) => UpdateMetrics();

		var page = new ContentPage
		{
			Title = "Issue 34610",
			Padding = new Thickness(0),
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(0),
				Spacing = 0,
				Children =
				{
					new Label
					{
						Text = "The red Shell TitleView should match the blue reference area when its Margin is zero.",
						AutomationId = "Instructions",
						Margin = new Thickness(12)
					},
					titleViewMetrics,
					referenceMetrics,
					referenceView,
					new Label
					{
						Text = "If the TitleView is narrower or shorter than the reference area, the issue is reproduced.",
						AutomationId = "ResultHint",
						Margin = new Thickness(12)
					}
				}
			}
		};

		Shell.SetTitleView(page, titleView);
		AddContentPage(page, "Issue 34610");
	}
}
