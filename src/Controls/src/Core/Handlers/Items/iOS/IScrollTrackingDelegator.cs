namespace Microsoft.Maui.Controls.Handlers.Items;

// Implemented by delegators to allow explicit scroll-tracking updates after native offset changes.
internal interface IScrollTrackingDelegator
{
	void ResetScrollTracking();
	void SetScrollTracking(double horizontalOffset, double verticalOffset);
}
