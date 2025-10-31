using System.Threading.Tasks;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18371 : _IssuesUITest
{
	public Issue18371(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "iOS Shell navigating from a modal results in navigation bar color disappearing";

	[Test]
	[Category(UITestCategories.Shell)]
	public void Issue18371Test()
	{
		App.WaitForElement("ShellPageButton");
		App.Tap("ShellPageButton");

		App.WaitForElement("WelcomePageButton");
		App.Tap("WelcomePageButton");

		App.WaitForElement("MainPageButton");
		App.Tap("MainPageButton");
		App.WaitForElement("WelcomePageButton");
		VerifyScreenshot();
	}
}

