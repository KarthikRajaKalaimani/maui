#nullable disable
using System;
using Android.Content;
using AndroidX.RecyclerView.Widget;
using ARect = Android.Graphics.Rect;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class SpacingItemDecoration : RecyclerView.ItemDecoration
	{
		public int HorizontalOffset { get; }

		public int VerticalOffset { get; }

		ItemsLayoutOrientation _orientation;

		bool _isGridItemsLayout;

		public SpacingItemDecoration(Context context, IItemsLayout itemsLayout)
		{
			// The original "SpacingItemDecoration" applied spacing based on an item's current span index.
			// It did not apply any spacing to items currently at span index 0 which can create an issue for us with grid layouts.
			// If one of those items at span index 0 were to move to another column, it would result in misaligned items.
			// It's better to just apply equal spacing to all items so we can avoid that issue (even the ones at span index 0).
			// Outer-edge items (first/last row or column) get double the offset in GetItemOffsets below, so the
			// gap at the edges visually matches the gaps between items. This is handled entirely by the decoration;
			// no RecyclerView padding is used (padding previously caused scroll/rendering issues, see #27093).

			if (itemsLayout == null)
			{
				throw new ArgumentNullException(nameof(itemsLayout));
			}

			double horizontalOffset;
			double verticalOffset;

			switch (itemsLayout)
			{
				case GridItemsLayout gridItemsLayout:
					horizontalOffset = gridItemsLayout.HorizontalItemSpacing / 2.0;
					verticalOffset = gridItemsLayout.VerticalItemSpacing / 2.0;
					_orientation = gridItemsLayout.Orientation;
					_isGridItemsLayout = true;
					break;
				case LinearItemsLayout listItemsLayout:
					if (listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
					{
						horizontalOffset = listItemsLayout.ItemSpacing / 2.0;
						verticalOffset = 0;
					}
					else
					{
						horizontalOffset = 0;
						verticalOffset = listItemsLayout.ItemSpacing / 2.0;
					}
					_orientation = listItemsLayout.Orientation;
					break;
				default:
					horizontalOffset = 0;
					verticalOffset = 0;
					_orientation = ItemsLayoutOrientation.Vertical;
					break;
			}

			HorizontalOffset = (int)context.ToPixels(horizontalOffset);
			VerticalOffset = (int)context.ToPixels(verticalOffset);
		}

		public override void GetItemOffsets(ARect outRect, AView view, RecyclerView parent, RecyclerView.State state)
		{
			base.GetItemOffsets(outRect, view, parent, state);

			int position = parent.GetChildAdapterPosition(view);
			if (position == RecyclerView.NoPosition)
				return;

			int itemCount = state.ItemCount;
			if (itemCount <= 0)
				return;

			outRect.Left = HorizontalOffset;
			outRect.Right = HorizontalOffset;
			outRect.Bottom = VerticalOffset;
			outRect.Top = VerticalOffset;

			// Interior items only get half of the requested spacing on each side (the neighboring
			// item contributes the other half), but the first/last row or column has no neighbor on
			// the outer side. For GridItemsLayout, double the offset there so the outer edge gets the
			// same visual gap as the gaps between items, instead of collapsing to zero. Linear layouts
			// (single row/column lists) keep the original edge-trimming behavior.
			int rowCol;
			int lastRowCol;
			int spanIndex = 0;
			int spanCount = 1;

			if (parent.GetLayoutManager() is GridLayoutManager gridLayoutManager)
			{
				// Use SpanSizeLookup instead of position/spanCount so full-span items
				// (group headers, footers, etc.) are accounted for when determining rows.
				var spanSizeLookup = gridLayoutManager.GetSpanSizeLookup();
				spanCount = gridLayoutManager.SpanCount;
				rowCol = spanSizeLookup.GetSpanGroupIndex(position, spanCount);
				lastRowCol = spanSizeLookup.GetSpanGroupIndex(itemCount - 1, spanCount);
				// spanIndex is the row position within each column (0 = first row, spanCount-1 = last row).
				spanIndex = spanSizeLookup.GetSpanIndex(position, spanCount);
			}
			else
			{
				// Linear layout: each item occupies exactly one row/column.
				rowCol = position;
				lastRowCol = itemCount - 1;
			}

			var edgeOffset = _isGridItemsLayout ? 2 : 0;

			if (_orientation == ItemsLayoutOrientation.Vertical)
			{
				if (rowCol == 0)
					outRect.Top = VerticalOffset * edgeOffset;
				if (rowCol == lastRowCol)
					outRect.Bottom = VerticalOffset * edgeOffset;
			}
			else
			{
				// Scroll-axis (left/right): only first/last column group.
				if (rowCol == 0)
					outRect.Left = HorizontalOffset * edgeOffset;
				if (rowCol == lastRowCol)
					outRect.Right = HorizontalOffset * edgeOffset;

				// Cross-axis (top/bottom): every item in the first/last row across all columns.
				if (spanIndex == 0)
					outRect.Top = VerticalOffset * edgeOffset;
				if (spanIndex == spanCount - 1)
					outRect.Bottom = VerticalOffset * edgeOffset;
			}
		}
	}
}