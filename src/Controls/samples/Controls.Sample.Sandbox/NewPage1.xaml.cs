namespace Maui.Controls.Sample;

public partial class NewPage1 : ContentPage
{
	public NewPage1()
	{
		InitializeComponent();
	}

	private void OnGoBackClicked(object sender, EventArgs e) 
	{ 
		Navigation.PushAsync(new MainPage()); 
	}

}