using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue20654 : _IssuesUITest
{
	public Issue20654(TestDevice device) : base(device) { }

	public override string Issue => "Improve the CollectionView FindEstimatedSizeUsingWidth to handle grouping and testing";

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void LastItemShouldVisible()
    {
        App.WaitForElement("ScrollButton");
        App.Tap("ScrollButton");
        App.WaitForElement("21");
	}
}