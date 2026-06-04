namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27126, "Line control prevents tapping of other controls on iOS", PlatformAffected.iOS)]
public partial class Issue27126 : ContentPage
{
	public Issue27126()
	{
		InitializeComponent();
	}

	void OnLabel1Tapped(object sender, TappedEventArgs e)
	{
		TapResultLabel.Text = "Label1";
	}

	void OnLabel2Tapped(object sender, TappedEventArgs e)
	{
		TapResultLabel.Text = "Label2";
	}
}
