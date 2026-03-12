#if MACCATALYST // Window maximizing is specific to MacCatalyst for this issue
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue33613 : _IssuesUITest
	{
		public override string Issue => "Mac Catalyst: NavigationPage.TitleView layout shifts and adds extra spacing when window is maximized";
		
		public Issue33613(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.Navigation)]
		public void TitleViewLayoutShouldNotShiftWhenWindowMaximized()
		{
			// Wait for page to load
			App.WaitForElement("TitleViewGrid");
			App.WaitForElement("HeaderLabel");
			
			// Get initial TitleView bounds in normal window state
			var titleViewInitial = App.WaitForElement("TitleViewGrid").GetRect();
			var initialX = titleViewInitial.X;
			var initialWidth = titleViewInitial.Width;
			
			// Push a page to get the back button (which is where the spacing issue appears)
			App.WaitForElement("PushButton");
			App.Tap("PushButton");
			
			// Wait for second page to load
			App.WaitForElement("SecondPageTitleViewGrid");
			App.WaitForElement("SecondPageLabel");
			
			// Get TitleView position with back button visible (normal window)
			var titleViewWithBackButton = App.WaitForElement("SecondPageTitleViewGrid").GetRect();
			var normalX = titleViewWithBackButton.X;
			var normalWidth = titleViewWithBackButton.Width;
			
			// Maximize the window (enter full screen)
			App.EnterFullScreen();
			
			// Wait for fullscreen transition
			System.Threading.Thread.Sleep(1000);
			
			// Get TitleView position after maximizing
			var titleViewMaximized = App.WaitForElement("SecondPageTitleViewGrid").GetRect();
			var maximizedX = titleViewMaximized.X;
			var maximizedWidth = titleViewMaximized.Width;
			
			// The bug: Extra spacing appears between back button and TitleView when maximized
			// The TitleView X position should NOT shift significantly when window is maximized
			// Allow small tolerance (50px) for window chrome differences, but no major shifts
			Assert.That(maximizedX, Is.EqualTo(normalX).Within(50),
				$"TitleView X position should not shift significantly when maximized. Normal: {normalX}, Maximized: {maximizedX}");
			
			// The TitleView width should scale proportionally with window, not add extra empty space
			// In fullscreen, width should increase (larger window), but position should stay consistent
			Assert.That(maximizedWidth, Is.GreaterThanOrEqualTo(normalWidth),
				"TitleView width should expand or stay same in fullscreen, not shrink");
			
			// Exit fullscreen to verify it returns to normal
			App.ExitFullScreen();
			
			// Wait for exit transition
			System.Threading.Thread.Sleep(1000);
			
			// Verify TitleView returns to normal position
			var titleViewRestored = App.WaitForElement("SecondPageTitleViewGrid").GetRect();
			Assert.That(titleViewRestored.X, Is.EqualTo(normalX).Within(5),
				"TitleView should return to original position after exiting fullscreen");
			
			// Tap away from toolbar to prevent hover effects
			App.Tap("SecondPageLabel");
		}
	}
}
#endif
