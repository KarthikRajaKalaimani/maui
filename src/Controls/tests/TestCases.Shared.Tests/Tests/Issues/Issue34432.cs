using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34432 : _IssuesUITest
{
	public override string Issue => "Content in a CollectionView header is not interactable when an EmptyView is displayed";

	public Issue34432(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void HeaderButtonInteractableWhenEmptyViewDisplayed()
	{
		// The CollectionView starts with no items, so the EmptyView is visible
		App.WaitForElement("HeaderButton");

		// Tap the header button while EmptyView is displayed
		App.Tap("HeaderButton");
		var newText = App.WaitForElement("ClickCountLabel").GetText();
		Assert.That(newText, Is.EqualTo("Clicked: 1"));
	}
}
