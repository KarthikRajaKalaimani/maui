#nullable enable

namespace Microsoft.Maui.Platform
{
	public static class LayoutPanelExtensions
	{
		public static void UpdateClipsToBounds(this LayoutPanel layoutPanel, ILayout layout)
		{
			layoutPanel.ClipsToBounds = layout.ClipsToBounds;
			layoutPanel.InvalidateArrange();

			// ClipsToBounds affects whether Background should be null (hit-test transparent)
			// or a Transparent brush (hit-test opaque). Re-evaluate the background so that
			// a dynamic ClipsToBounds change is reflected immediately.
			layoutPanel.UpdatePlatformViewBackground(layout);
		}
	}
}
