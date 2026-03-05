using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34148 : _IssuesUITest
{
	public Issue34148(TestDevice device) : base(device) { }

	public override string Issue => "Gesture doesn't recognize in the view's spanned region on all the platforms";

	[Test]
	[Category(UITestCategories.Gestures)]
	public void TapGestureWorksInSpannedRegion()
	{
		// SpanningCell is a child of row 0 arranged with height 200px so it
		// visually overlaps row 1 (the "spanned region"). The bug causes tap
		// gestures in that lower half to go unrecognized.
		App.WaitForElement("SpanningCell");
		App.WaitForElement("BottomLeftCell");

		var spanningRect = App.FindElement("SpanningCell").GetRect();
		var bottomLeftRect = App.FindElement("BottomLeftCell").GetRect();

		// Target the centre of the spanned (lower) half: X is in SpanningCell's column,
		// Y is in BottomLeftCell's row - the exact area where the bug manifests.
		var tapX = spanningRect.X + spanningRect.Width / 2;
		var tapY = bottomLeftRect.Y + bottomLeftRect.Height / 2;

		App.TapCoordinates(tapX, tapY);

		// If the bug is present the label stays "Not Tapped"; the fix makes it "Tapped".
		App.WaitForElement("StatusLabel");
		var statusText = App.FindElement("StatusLabel").GetText();
		Assert.That(statusText, Is.EqualTo("Tapped"),
			"Tap gesture should be recognized in the spanned (overflow) region of the custom layout child.");
	}
}
