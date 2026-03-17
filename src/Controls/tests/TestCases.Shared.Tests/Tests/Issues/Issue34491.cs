using System.Linq;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34491 : _IssuesUITest
{
	public Issue34491(TestDevice device) : base(device)
	{
	}

	public override string Issue => "[Android] CollectionView item selection not triggered when using PointerGestureRecognizer";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewSelectionShouldNotBeBlockedByPointerGestureRecognizer()
	{
		App.WaitForElement("TestCollectionView");
		App.WaitForElement("ItemContainer");

		Assert.That(App.WaitForElement("SelectionChangedLabel").GetText(), Is.EqualTo("SelectionChanged Count: 0"));
		Assert.That(App.WaitForElement("SelectedItemLabel").GetText(), Is.EqualTo("Selected Item: None"));

		var firstItemRect = App.FindElements("ItemContainer").First().GetRect();
		App.TapCoordinates(firstItemRect.X + firstItemRect.Width / 2, firstItemRect.Y + firstItemRect.Height / 2);

		Assert.That(App.WaitForElement("SelectionChangedLabel").GetText(), Is.EqualTo("SelectionChanged Count: 1"));
		Assert.That(App.WaitForElement("SelectedItemLabel").GetText(), Is.EqualTo("Selected Item: Item 1"));
	}
}
