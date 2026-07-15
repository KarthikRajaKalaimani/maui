using Android.Views;

namespace Microsoft.Maui.Platform
{
	public static class ScrollViewExtensions
	{
		internal static void HandleScrollBarVisibilityChange(this IScrollBarView scrollView)
		{
			// According to the Android Documentation
			// * <p>AwakenScrollBars method should be invoked every time a subclass directly updates
			// *the scroll parameters.</ p >

			// If AwakenScrollBars is never called there are cases where the ScrollDrawable is never called
			// which causes a crash during draw

			if (scrollView.ScrollBarsInitialized)
				scrollView.AwakenScrollBars();

			// The scrollbar drawable won't initialize if ScrollbarFadingEnabled == false
			if (!scrollView.ScrollbarFadingEnabled)
			{
				scrollView.ScrollbarFadingEnabled = true;
				scrollView.AwakenScrollBars();
				scrollView.ScrollbarFadingEnabled = false;
			}
			else
			{
				scrollView.AwakenScrollBars();
			}

			scrollView.ScrollBarsInitialized = true;
		}

		public static void UpdateContent(this MauiScrollView scrollView, IView? content, IMauiContext context)
		{
			var nativeContent = content?.ToPlatform(context);

			scrollView.RemoveAllViews();

			if (nativeContent != null)
			{
				scrollView.SetContent(nativeContent);
			}
		}

		internal static void HandleTouchEvent(MotionEvent ev, IViewParent? parent)
		{
			// requestDisallowInterceptTouchEvent propagates up through the ENTIRE ancestor chain
			// (not just the immediate parent), setting a flag on every ancestor ViewGroup that
			// prevents their OnInterceptTouchEvent from being called for the rest of the gesture.
			//
			// If any ancestor (not just the immediate parent) is another Maui ScrollView (e.g. a
			// horizontal ScrollView nested inside a vertical one, or vice versa - possibly with Maui
			// layout wrapper views such as ContentViewGroup in between), we must not request
			// disallow-intercept at all. Those ScrollView ancestors rely on their own
			// OnInterceptTouchEvent direction detection (touch slop on the initial gesture direction)
			// to decide which of the nested ScrollViews should own a given gesture. Forcing
			// disallow-intercept here would prevent that negotiation from happening, permanently
			// locking the gesture to the innermost ScrollView (see
			// https://github.com/dotnet/maui/issues/20920).
			//
			// For every other ancestor chain (e.g. DrawerLayout used by the Shell Flyout, with no
			// ScrollView ancestors above it), we still want to disallow interception so the
			// ScrollView can scroll instead of an ancestor stealing the gesture (see
			// https://github.com/dotnet/maui/issues/11764).
			if (HasScrollViewAncestor(parent))
			{
				return;
			}

			if (ev.Action == MotionEventActions.Down)
			{
				parent?.RequestDisallowInterceptTouchEvent(true);
			}
			else if (ev.Action == MotionEventActions.Up || ev.Action == MotionEventActions.Cancel)
			{
				parent?.RequestDisallowInterceptTouchEvent(false);
			}
		}

		static bool HasScrollViewAncestor(IViewParent? parent)
		{
			while (parent is not null)
			{
				if (parent is MauiScrollView or MauiHorizontalScrollView)
				{
					return true;
				}

				parent = (parent as View)?.Parent;
			}

			return false;
		}
	}
}