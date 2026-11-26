#if IOS || MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35627 : _IssuesUITest
{
	public Issue35627(TestDevice device) : base(device) { }

	public override string Issue => "Retained tab content must not unload on tab switches";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void RetainedContentSurvivesTabSwitchAndUnloadsWhenRemoved()
	{
		App.WaitForTextToBePresentInElement("FirstTabReady", "Loaded");
		App.WaitForElement("TrackedContent");

		App.TapTab("Second");
		App.WaitForTextToBePresentInElement("TabStatus", "Settled:");
		Assert.That(App.FindElement("TabStatus").GetText(), Is.EqualTo("Settled: retained"));

		App.TapTab("First");
		App.WaitForElement("TrackedContent");

		App.TapTab("Second");
		App.WaitForTextToBePresentInElement("TabStatus", "Settled:");
		Assert.That(App.FindElement("TabStatus").GetText(), Is.EqualTo("Settled: retained"));

		App.Tap("RemoveContent");
		App.WaitForTextToBePresentInElement("TabStatus", "Removed");
	}
}
#endif
