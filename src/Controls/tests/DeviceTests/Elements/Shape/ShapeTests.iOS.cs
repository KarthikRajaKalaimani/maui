using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ShapeTests
	{
		Task PerformClick(IButton button)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				GetNativeButton(CreateHandler<ButtonHandler>(button)).SendActionForControlEvents(UIControlEvent.TouchUpInside);
			});
		}

		UIButton GetNativeButton(ButtonHandler buttonHandler) =>
			buttonHandler.PlatformView;

		// Regression test for https://github.com/dotnet/maui/issues/27126
		// A Line (MauiShapeView) with no gesture recognizers should return null from HitTest
		// so touches pass through to controls placed beneath the shape.
		[Fact(DisplayName = "Line HitTest passes through when no gesture recognizers")]
		public async Task LineHitTestPassesThroughWithNoGestureRecognizers()
		{
			SetupBuilder();

			var line = new Line
			{
				X1 = 0, Y1 = 0, X2 = 100, Y2 = 0,
				WidthRequest = 100, HeightRequest = 10,
				Stroke = Colors.Black, StrokeThickness = 2
			};

			await AttachAndRun<LineHandler>(line, handler =>
			{
				var platformView = handler.PlatformView;
				Assert.NotNull(platformView);

				// Ensure the platform view has a frame so HitTest can find hits within it.
				platformView.Frame = new CoreGraphics.CGRect(0, 0, 100, 10);

				// With no gesture recognizers the shape should pass the touch through (return null).
				var hitResult = platformView.HitTest(new CGPoint(50, 5), null);
				Assert.Null(hitResult);

				return Task.CompletedTask;
			});
		}

		// A Line with a TapGestureRecognizer should still receive the touch (return self from HitTest).
		[Fact(DisplayName = "Line HitTest returns self when gesture recognizers are attached")]
		public async Task LineHitTestReturnsSelfWithGestureRecognizers()
		{
			SetupBuilder();

			var line = new Line
			{
				X1 = 0, Y1 = 0, X2 = 100, Y2 = 0,
				WidthRequest = 100, HeightRequest = 10,
				Stroke = Colors.Black, StrokeThickness = 2
			};

			await AttachAndRun<LineHandler>(line, handler =>
			{
				var platformView = handler.PlatformView;
				Assert.NotNull(platformView);

				// Set a frame so HitTest can find hits within bounds.
				platformView.Frame = new CoreGraphics.CGRect(0, 0, 100, 10);

				// Simulate a gesture recognizer being attached (as MAUI does via GesturePlatformManager).
				var gr = new UITapGestureRecognizer();
				platformView.AddGestureRecognizer(gr);

				try
				{
					// With a gesture recognizer present the shape should receive the touch (return self).
					var hitResult = platformView.HitTest(new CGPoint(50, 5), null);
					Assert.NotNull(hitResult);
				}
				finally
				{
					platformView.RemoveGestureRecognizer(gr);
				}

				return Task.CompletedTask;
			});
		}

		// A Line with a Background creates a WrapperView. The WrapperView should also
		// pass touches through when it has no gesture recognizers (IsInteractionTransparent=true).
		[Fact(DisplayName = "Line WrapperView HitTest passes through when no gesture recognizers")]
		public async Task LineWrapperViewHitTestPassesThroughWithNoGestureRecognizers()
		{
			SetupBuilder();

			var line = new Line
			{
				X1 = 0, Y1 = 0, X2 = 100, Y2 = 0,
				WidthRequest = 100, HeightRequest = 10,
				Stroke = Colors.Black, StrokeThickness = 2,
				Background = new SolidColorBrush(Colors.Transparent) // forces NeedsContainer=true
			};

			await AttachAndRun<LineHandler>(line, handler =>
			{
				var containerView = handler.ContainerView as WrapperView;
				Assert.NotNull(containerView);

				// Verify the flag was set by SetupContainer().
				Assert.True(containerView.IsInteractionTransparent);

				// Set frames so HitTest operates within bounds.
				containerView.Frame = new CoreGraphics.CGRect(0, 0, 100, 10);
				handler.PlatformView.Frame = containerView.Bounds;

				// WrapperView with IsInteractionTransparent=true and no GRs should pass through.
				var hitResult = containerView.HitTest(new CGPoint(50, 5), null);
				Assert.Null(hitResult);

				return Task.CompletedTask;
			});
		}
	}
}