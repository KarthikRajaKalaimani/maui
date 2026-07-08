using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34257 : _IssuesUITest
{
	public Issue34257(TestDevice device)
		: base(device)
	{
	}

	public override string Issue => "CollectionView vertical grid item spacing updates all rows and columns";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void UpdateSpacingForHorizontalGridAndVerticalGrid()
	{
		var firstColumnBefore = App.WaitForElement("FirstColumnTopItemHorizontal").GetRect();
		App.Tap("ApplyHorizontalSpacingButton");
		App.WaitForElement("StatusLabelHorizontal", "Spacing=80,80");
		var firstColumnAfter = App.WaitForElement("FirstColumnTopItemHorizontal").GetRect();
		Assert.That(firstColumnBefore.X, Is.Not.EqualTo(firstColumnAfter.X), $"Expected the first column to move Horizontally");
		Assert.That(firstColumnBefore.Y, Is.Not.EqualTo(firstColumnAfter.Y), $"Expected the first column to move Vertically");
		var firstColumnBeforeVertical = App.WaitForElement("FirstColumnTopItemVertical").GetRect();
		App.Tap("ApplyVerticalSpacingButton");
		App.WaitForElement("StatusLabelVertical", "Spacing=80,80");
		var firstColumnAfterVertical = App.WaitForElement("FirstColumnTopItemVertical").GetRect();
		Assert.That(firstColumnBeforeVertical.Y, Is.Not.EqualTo(firstColumnAfterVertical.Y), $"Expected the first column to move Vertically");
		Assert.That(firstColumnBeforeVertical.X, Is.Not.EqualTo(firstColumnAfterVertical.X), $"Expected the first column to move Horizontally");

	}
}