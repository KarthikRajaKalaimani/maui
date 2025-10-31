namespace Controls.TestCases.Sample.Issues;

[Issue(IssueTracker.Github, "18371", "iOS Shell navigating from a modal results in navigation bar color disappearing", PlatformAffected.iOS)]
public class Issue18371 : ContentPage
{
	public Issue18371()
	{
		Title = "Issue 18371 - Shell Navigation Test";
		
		var button = new Button
		{
			Text = "Launch Shell Test",
			Margin = new Thickness(20),
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center,
			AutomationId="ShellPageButton"
		};
		
		button.Clicked += (sender, e) =>
		{
			// Replace the current window's page with our shell
			var shell = new Issue18371Shell();
			Application.Current.Windows[0].Page = shell;
		};
		
		Content = new StackLayout
		{
			Children = { 
				new Label
				{
					Text = "This test demonstrates Shell navigation issues on iOS.\nTap the button below to launch the Shell test.",
					HorizontalOptions = LayoutOptions.Center,
					HorizontalTextAlignment = TextAlignment.Center,
					Margin = new Thickness(20)
				},
				button 
			},
			VerticalOptions = LayoutOptions.Center
		};
	}
}

public class Issue18371Shell : Shell
{
	public Issue18371Shell()
	{
		Title = "MauiAppShellFlyoutItemsNavBarColor";
		Routing.RegisterRoute("Issue18371_MainPage", typeof(Issue18371_MainPage));
		Routing.RegisterRoute("Issue18371_StartUpPage", typeof(Issue18371_StartUpPage));
		Routing.RegisterRoute("Issue18371_WelcomePage", typeof(Issue18371_WelcomePage));
		FlyoutBehavior = FlyoutBehavior.Disabled;

		var shellStyle = new Style(typeof(Shell))
		{
			Setters =
			{
				new Setter
				{
					Property = Shell.BackgroundColorProperty,
					Value = Colors.LightSteelBlue
				},
			}
		};
		Resources = new ResourceDictionary
		{
			{ "ExplicitShellStyle", shellStyle }
		};
		Style = (Style)Resources["ExplicitShellStyle"];

		var mainPageFlyout = new FlyoutItem
		{
			Items =
			{
				new ShellContent
					{
						ContentTemplate = new DataTemplate(typeof(Issue18371_MainPage)),
						Route = "Issue18371_MainPage"
				}
			}
		};
		var startUpPageFlyout = new FlyoutItem
		{
			Items =
			{
				new ShellContent
				{
					ContentTemplate = new DataTemplate(typeof(Issue18371_StartUpPage)),
					Route = "Issue18371_StartUpPage"
				}
			}
		};
		Items.Add(mainPageFlyout);
		Items.Add(startUpPageFlyout);
	}
}

public class Issue18371_MainPage : ContentPage
{
	public Issue18371_MainPage()
	{
		Title = "MainPage";
		var button = new Button
		{
			Margin = new Thickness(32),
            AutomationId= "WelcomePageButton",
			Text = "Press me to go to StartUpPage/WelcomePage"
		};
		button.Clicked += Button_Clicked;
		var layout = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.End,
			Children = { button }
		};
		Content = layout;
	}
	
	private void Button_Clicked(object sender, EventArgs e)
    {
		Dispatcher.Dispatch(async () =>
		{
			await Shell.Current.GoToAsync("//Issue18371_StartUpPage/Issue18371_WelcomePage");
		});
    }
}

public class Issue18371_StartUpPage : ContentPage
{
	public Issue18371_StartUpPage()
	{
		Title = "StartUpPage";
        Shell.SetNavBarIsVisible(this, false);
	}
}

public class Issue18371_WelcomePage : ContentPage
{
	public Issue18371_WelcomePage()
	{
		Title = "WelcomePage";
		Shell.SetPresentationMode(this, PresentationMode.Modal);
		var button = new Button
		{
			Margin = new Thickness(32),
            AutomationId= "MainPageButton",
			Text = "Now press me!"
		};
		button.Clicked += Button_Clicked;
		var layout = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.End,
			Children = { button }
		};
		Content = layout;
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		Dispatcher.Dispatch(async () =>
		{
			await Shell.Current.GoToAsync("//Issue18371_MainPage");
		});
	}
}