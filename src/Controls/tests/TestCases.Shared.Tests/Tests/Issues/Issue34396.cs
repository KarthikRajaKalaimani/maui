using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34396 : _IssuesUITest
{
	public override string Issue => "UI becomes unresponsive when adding more than 200 Entry children to AbsoluteLayout";

	public Issue34396(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Layout)]
	public void AddingManyChildrenToAbsoluteLayoutShouldNotBlockUIThread()
	{
		App.WaitForElement("AddEditors");

		// Tap the button that adds 200 editors via individual Dispatcher.Dispatch calls.
		// The buggy pattern queues 200 separate UI-thread operations (one Children.Add +
		// SetLayoutBounds each), causing 200 layout passes and blocking the UI thread.
		App.Tap("AddEditors");

		// Wait for all editors to finish being added
		App.WaitForElement("Done");

		// Read elapsed wall-clock time (measured from start of add to last dispatch completing)
		var elapsedText = App.FindElement("ElapsedMs").GetText() ?? "0";
		_ = long.TryParse(elapsedText, out var elapsed);

		// With the buggy pattern (200 individual dispatches) the UI thread is monopolised
		// for well over 500 ms. A correct implementation (e.g. BatchBegin/BatchEnd or a
		// single Children.AddRange equivalent) should complete in well under 500 ms.
		Assert.That(elapsed, Is.LessThan(500),
			$"Adding 200 editors took {elapsed}ms. Individual Dispatcher.Dispatch calls flood " +
			"the UI message queue and block the thread; use BatchBegin/BatchEnd instead.");
	}
}

