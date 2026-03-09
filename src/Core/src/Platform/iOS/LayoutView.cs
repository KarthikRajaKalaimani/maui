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

		// Searches a subview's subtree for a hit, converting touch coordinates from
		// *this* layout's coordinate system to bypass any intermediate PointInside
		// bounds check on the subview itself.
		UIView? HitTestInSubtree(UIView subview, CGPoint point, UIEvent? uievent)
		{
			// When the subview is itself a LayoutView its overridden HitTest already
			// handles ClipsToBounds=false and deeper overflow children. Call it directly
			// with the touch point converted from *this* layout's space — bypassing the
			// UIKit PointInside check that would wrongly reject out-of-bounds overflow
			// touches. We only accept the result when it is a *descendant*; if HitTest
			// returns the subview itself that means UIKit found no accepting child and
			// fell back to the container, so we must continue the grandchild search below.
			if (subview is LayoutView layoutSubview)
			{
				var subviewPoint = layoutSubview.ConvertPointFromView(point, this);
				var layoutHit = layoutSubview.HitTest(subviewPoint, uievent);
				if (layoutHit is not null && !ReferenceEquals(layoutHit, layoutSubview))
					return layoutHit;
			}

			// For non-LayoutView subviews (e.g. native UIView wrappers), or when the
			// LayoutView's own HitTest found no accepting descendant, iterate the
			// subview's direct children in reverse z-order. Converting the touch from
			// *this* layout's coordinate system straight to each grandchild skips the
			// intermediate PointInside check on the subview.
			if (subview.Subviews is null || subview.Subviews.Length == 0)
				return null;

			for (int i = subview.Subviews.Length - 1; i >= 0; i--)
			{
				var grandchild = subview.Subviews[i];

				if (grandchild.Hidden || grandchild.Alpha < 0.01f || !grandchild.UserInteractionEnabled)
					continue;

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