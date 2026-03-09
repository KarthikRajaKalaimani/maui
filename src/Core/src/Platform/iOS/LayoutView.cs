using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class LayoutView : MauiView
	{
		bool _userInteractionEnabled;

		public override void SubviewAdded(UIView uiview)
		{
			InvalidateConstraintsCache();
			base.SubviewAdded(uiview);
		}

		public override void WillRemoveSubview(UIView uiview)
		{
			InvalidateConstraintsCache();
			base.WillRemoveSubview(uiview);
		}

		public override UIView? HitTest(CGPoint point, UIEvent? uievent)
		{
			// When this layout does not clip its children, a child view may be arranged
			// beyond this layout's own bounds. UIKit's standard hitTest stops traversal
			// whenever a subview's PointInside returns false, so those overflow children are
			// never reached. We work around this by directly inspecting each subview's own
			// children (grandchildren of this layout) before falling back to the standard
			// algorithm. Converting the touch point from this layout's coordinate system
			// straight to the grandchild bypasses the intermediate PointInside check.
			if (!ClipsToBounds)
			{
				// Iterate direct subviews in reverse z-order (last added = topmost).
				for (int i = Subviews.Length - 1; i >= 0; i--)
				{
					var subview = Subviews[i];

					if (subview.Hidden || subview.Alpha < 0.01f || !subview.UserInteractionEnabled)
						continue;

					var hit = HitTestInSubtree(subview, point, uievent);
					if (hit is not null)
						return hit;
				}
			}

			// Standard hit-testing: handles the common in-bounds case and ClipsToBounds=true.
			var result = base.HitTest(point, uievent);

			if (result is null)
			{
				return null;
			}

			if (!_userInteractionEnabled && Equals(result))
			{
				// If user interaction is disabled (IOW, if the corresponding Layout is InputTransparent),
				// then we exclude the LayoutView itself from hit testing. But it's children are valid
				// hit testing targets.

				return null;
			}

			if (result is LayoutView layoutView && !layoutView.UserInteractionEnabledOverride)
			{
				// If the child is a layout then we need to check the UserInteractionEnabledOverride
				// since layouts always have user interaction enabled.

				return null;
			}

			return result;
		}

		// Searches the subview's own children for a hit, converting touch coordinates
		// directly from this layout's coordinate system to bypass any intermediate
		// PointInside bounds check on the subview itself.
		UIView? HitTestInSubtree(UIView subview, CGPoint point, UIEvent? uievent)
		{
			if (subview.Subviews is null || subview.Subviews.Length == 0)
				return null;

			// Iterate the subview's children in reverse z-order.
			for (int i = subview.Subviews.Length - 1; i >= 0; i--)
			{
				var grandchild = subview.Subviews[i];

				if (grandchild.Hidden || grandchild.Alpha < 0.01f || !grandchild.UserInteractionEnabled)
					continue;

				// Convert the touch point from *this* layout's coordinate system directly
				// to the grandchild's coordinate system, skipping the subview's own bounds.
				var grandchildPoint = grandchild.ConvertPointFromView(point, this);
				var hit = grandchild.HitTest(grandchildPoint, uievent);

				if (hit is not null)
					return hit;
			}

			return null;
		}

		internal bool UserInteractionEnabledOverride => _userInteractionEnabled;

		public override bool UserInteractionEnabled
		{
			get => base.UserInteractionEnabled;
			set
			{
				// We leave the base UIE value true no matter what, so that hit testing will find children
				// of the LayoutView. But we track the intended value so we can use it during hit testing
				// to ignore the LayoutView itself, if necessary.

				base.UserInteractionEnabled = true;
				_userInteractionEnabled = value;
			}
		}
	}
}