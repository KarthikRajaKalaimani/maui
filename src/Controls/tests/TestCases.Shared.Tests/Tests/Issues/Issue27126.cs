using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue27126 : _IssuesUITest
{
	public Issue27126(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Line control prevents tapping of other controls on iOS";

	[Test]
	[Category(UITestCategories.Gestures)]
	public void LineShouldNotBlockTapsOnControlsUnderneath()
	{
		// Label1 and Label2 sit inside the Line shape's native frame (0,0,width,250).
		// Without the fix, the Line absorbs all touches and neither label is tappable.
		App.WaitForElement("Label1");

		App.Tap("Label1");
		Assert.That(App.WaitForElement("TapResult").GetText(), Is.EqualTo("Label1"),
			"Label1 should receive the tap even though a Line shape is layered on top.");

		App.Tap("Label2");
		Assert.That(App.WaitForElement("TapResult").GetText(), Is.EqualTo("Label2"),
			"Label2 should receive the tap even though a Line shape is layered on top.");
	}
}
