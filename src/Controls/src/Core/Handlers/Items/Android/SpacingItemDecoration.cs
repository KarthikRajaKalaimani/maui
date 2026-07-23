#nullable disable
using System;
using Android.Content;
using AndroidX.RecyclerView.Widget;
using ARect = Android.Graphics.Rect;
using AView = Android.Views.View;
using ALayoutDirection = Android.Views.LayoutDirection;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public class SpacingItemDecoration : RecyclerView.ItemDecoration
	{
		public int HorizontalOffset { get; }

		public int VerticalOffset { get; }

		ItemsLayoutOrientation _orientation;

		public SpacingItemDecoration(Context context, IItemsLayout itemsLayout)
		{
			// The original "SpacingItemDecoration" applied spacing based on an item's current span index.
			// It did not apply any spacing to items currently at span index 0 which can create an issue for us with grid layouts.
			// If one of those items at span index 0 were to move to another column, it would result in misaligned items.
			// It's better to just apply equal spacing to all items so we can avoid that issue (even the ones at span index 0).
			// The reason they didn't do this originally, I suspect, is that they didn't want spacing around the edge of the RecyclerView.
			// That however can be corrected by adjusting the padding on the RecyclerView which we are now doing.

			if (itemsLayout == null)
			{
				throw new ArgumentNullException(nameof(itemsLayout));
			}

			double horizontalOffset;
			double verticalOffset;

			switch (itemsLayout)
			{
				case GridItemsLayout gridItemsLayout:
					horizontalOffset = gridItemsLayout.HorizontalItemSpacing;
					verticalOffset = gridItemsLayout.VerticalItemSpacing;
					_orientation = gridItemsLayout.Orientation;
					break;
				case LinearItemsLayout listItemsLayout:
					if (listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal)
					{
						horizontalOffset = listItemsLayout.ItemSpacing;
						verticalOffset = 0;
					}
					else
					{
						horizontalOffset = 0;
						verticalOffset = listItemsLayout.ItemSpacing;
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

			// In RTL layouts, AndroidX's layout managers keep items in logical/adapter order but flip their
			// physical on-screen position (e.g. span index 0 renders on the right, not the left). Horizontal
			// spacing must therefore be assigned to the opposite physical side so it still lands between
			// neighboring items instead of bleeding onto the new outer edge.
			bool isRtl = parent.LayoutDirection == ALayoutDirection.Rtl;

			// Apply spacing only between items, not on the outer edges.
			int rowCol;
			int lastRowCol;

			if (parent.GetLayoutManager() is GridLayoutManager gridLayoutManager)
			{
				var layoutParams = view.LayoutParameters as GridLayoutManager.LayoutParams;
				if (layoutParams == null)
					return;

				// Use SpanSizeLookup instead of position/spanCount so full-span items
				// (group headers, footers, etc.) are accounted for when determining rows.
				var spanSizeLookup = gridLayoutManager.GetSpanSizeLookup();
				int spanCount = gridLayoutManager.SpanCount;
				rowCol = spanSizeLookup.GetSpanGroupIndex(position, spanCount);
				lastRowCol = spanSizeLookup.GetSpanGroupIndex(itemCount - 1, spanCount);

				bool isLastCrossAxisItem = layoutParams.SpanIndex + layoutParams.SpanSize >= spanCount;

				if (_orientation == ItemsLayoutOrientation.Vertical)
				{
					outRect.Top = 0;
					outRect.Bottom = rowCol == lastRowCol ? 0 : VerticalOffset;
					SetHorizontalOffset(outRect, isLastCrossAxisItem ? 0 : HorizontalOffset, isRtl);
				}
				else
				{
					bool isFirstCrossAxisItem = layoutParams.SpanIndex == 0;

					outRect.Top = isFirstCrossAxisItem ? 0 : VerticalOffset;
					outRect.Bottom = 0;
					SetHorizontalOffset(outRect, rowCol == lastRowCol ? 0 : HorizontalOffset, isRtl);
				}

				return;
			}
			else
			{
				// Linear layout: each item occupies exactly one row/column.
				rowCol = position;
				lastRowCol = itemCount - 1;

				if (_orientation == ItemsLayoutOrientation.Vertical)
				{
					outRect.Left = 0;
					outRect.Top = 0;
					outRect.Right = 0;
					outRect.Bottom = rowCol == lastRowCol ? 0 : VerticalOffset;
				}
				else
				{
					outRect.Top = 0;
					outRect.Bottom = 0;
					SetHorizontalOffset(outRect, rowCol == lastRowCol ? 0 : HorizontalOffset, isRtl);
				}
			}
		}

		// Assigns a horizontal spacing value to the physical side that faces the next item in adapter
		// order. In LTR that's the item's right edge; in RTL, layout managers place the next item to the
		// left instead, so the spacing has to move to the left edge to stay between items.
		static void SetHorizontalOffset(ARect outRect, int offset, bool isRtl)
		{
			if (isRtl)
			{
				outRect.Left = offset;
				outRect.Right = 0;
			}
			else
			{
				outRect.Left = 0;
				outRect.Right = offset;
			}
		}
	}
}