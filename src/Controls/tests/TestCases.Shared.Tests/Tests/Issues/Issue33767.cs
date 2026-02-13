using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33767 : _IssuesUITest
{
	public override string Issue => "Switch ThumbColor not applied correctly on iOS and MacCatalyst";

	public Issue33767(TestDevice device) : base(device)
	{
	}

	
	[Test]
	[Category(UITestCategories.Switch)]
	public void Issue33767Test()
	{
		App.WaitForElement("Switch");
		VerifyScreenshot();
	}
}
