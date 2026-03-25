using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34610 : _IssuesUITest
{
	public Issue34610(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Shell titleview has unremovable horizontal and vertical margin";

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleViewZeroMarginShouldMatchReferenceArea()
	{
#if MACCATALYST
		if (App.FindElements("Instructions").Count == 0)
		{
			App.WaitForElement("SearchBar");
			App.EnterText("SearchBar", Issue);
			App.WaitForElement("GoToTestButton");
			App.Tap("GoToTestButton");
		}
#endif
		App.WaitForElement("Instructions");

		var titleView = App.WaitForElement("TitleViewGrid").GetRect();
		var referenceView = App.WaitForElement("ReferenceView").GetRect();

		Assert.Multiple(() =>
		{
			Assert.That(titleView.Width, Is.EqualTo(referenceView.Width).Within(5),
				$"Expected the TitleView width to match the reference width when Margin is zero. TitleView={titleView.Width}, Reference={referenceView.Width}");
			Assert.That(titleView.Height, Is.EqualTo(referenceView.Height).Within(5),
				$"Expected the TitleView height to match the reference height when Margin is zero. TitleView={titleView.Height}, Reference={referenceView.Height}");
		});
	}
}
