namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33767, "Switch ThumbColor not applied correctly on iOS and MacCatalyst", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue33767 : ContentPage
{
	public Issue33767()
	{
		var switchControl = new Switch();
		switchControl.IsToggled = true;
		switchControl.ThumbColor = Colors.Orange;
		switchControl.AutomationId = "Switch";

		Content = new StackLayout
		{
			Children = {
				switchControl
			}
		};
	}
}
