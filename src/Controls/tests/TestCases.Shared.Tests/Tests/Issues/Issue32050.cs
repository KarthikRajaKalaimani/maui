using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32050 : _IssuesUITest
{
	public Issue32050(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "IconOverride in Shell.BackButtonBehavior does not work.";

	[Test]
	[Category(UITestCategories.Shell)]
	public void BackButtonBehaviorIconOverrideShouldShowCustomIcon()
	{
		App.WaitForElement("SubPageLabel");
		VerifyScreenshot();
	}
}
