// This behavior isn't lit up for WinUI because it's never been supported on WinUI, event in Xamarin.Forms
// The primary purpose of this API is for XF migration purposes. 
// Ideally users would use behavior that's more accessible forward and consistent with platform expectations.
#if ANDROID || IOS
using System;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	partial class HideSoftInputOnTappedChangedManager
	{
		IDisposable? _watchingForTaps;
		WeakReference<IView>? _focusedView;
		WeakReference<ContentPage>? _focusedViewEnclosingPage;

		static ContentPage? GetEnclosingPage(IView? view)
		{
			Element? element = view as Element;
			while (element is not null)
			{
				if (element is ContentPage contentPage)
					return contentPage;
				element = element.Parent;
			}
			return null;
		}

		// A page can remain a valid logical descendant of its Window (and thus keep a
		// non-null Window/Parent chain) long after it has stopped being the page the
		// user is actually looking at - Shell, NavigationPage and FlyoutPage all retain
		// their inactive pages instead of tearing them down. Relying on Parent/Window
		// alone would let a stale cached page keep gating the tap watcher indefinitely
		// (e.g. Shell.GoToAsync switching ShellItem/ShellContent without ever firing
		// NavigatedFrom on the outgoing page - see Issue35890). So we additionally
		// require that the page is still the one actually being presented, by drilling
		// down from the page's Window through any Shell/NavigationPage/FlyoutPage
		// containers via GetCurrentPage().
		static bool IsCurrentlyDisplayedPage(ContentPage page)
		{
			if (page.Window?.Page is not Page rootPage)
				return false;

			return ReferenceEquals(rootPage.GetCurrentPage(), page);
		}

		bool FeatureEnabled => ResolveFocusedPage() is ContentPage page && page.HideSoftInputOnTapped && page.HasNavigatedTo;

		ContentPage? FocusedEnclosingPage =>
			_focusedViewEnclosingPage?.TryGetTarget(out var p) == true ? p : null;

		// Resolves the page that should currently gate the tap watcher for the
		// focused view.
		ContentPage? ResolveFocusedPage()
		{
			var focusedView = FocusedView;

			// Walk the live tree first. Even if this resolves a page, it might no
			// longer be the currently displayed one (see IsCurrentlyDisplayedPage).
			if (GetEnclosingPage(focusedView) is ContentPage livePage)
				return IsCurrentlyDisplayedPage(livePage) ? livePage : null;

			// FocusedView's logical Parent chain no longer resolves to a page. This can
			// happen transiently during navigation (the view is detached before its
			// LostFocus/NavigatedFrom fires) or permanently when the enclosing page was
			// removed from the visual tree without ever raising those events (e.g. the
			// top-level Window.Page/Application.MainPage was replaced directly). Only
			// trust the page cached when focus was set while the view is still attached
			// to a live platform Window, and only if that cached page is still the one
			// currently displayed - otherwise treat it as gone and stop tracking it so
			// the feature doesn't stay enabled indefinitely for a page that's no longer
			// the one the user is interacting with.
			if (focusedView is VisualElement { Window: not null } && FocusedEnclosingPage is ContentPage cachedPage)
				return IsCurrentlyDisplayedPage(cachedPage) ? cachedPage : null;

			if (_focusedView is not null || _focusedViewEnclosingPage is not null)
			{
				_focusedView = null;
				_focusedViewEnclosingPage = null;
				DisconnectFromPlatform();
			}

			return null;
		}

		internal void UpdatePage(ContentPage page)
		{
			// HideSoftInputOnTapped (or HasNavigatedTo) changed on this page.
			// FeatureEnabled is computed on-demand from the currently focused view's
			// enclosing page, so we just need to re-evaluate the tap watcher in case
			// the change flips FeatureEnabled for the focused view.
			SetupHideSoftInputOnTapped();
		}

		internal IDisposable? UpdateFocusForView(IView _view)
		{
			// Update to new focused view
			if (_view.IsFocused)
			{
				DisconnectFromPlatform();
				_focusedView = new WeakReference<IView>(_view);
				// Cache the enclosing page now, while the view is still in the logical
				// tree, so that FeatureEnabled can fall back to it if the view is later
				// detached before a focus-lost event fires.
				var enclosingPage = GetEnclosingPage(_view);
				_focusedViewEnclosingPage = enclosingPage is not null
					? new WeakReference<ContentPage>(enclosingPage)
					: null;
			}
			// If currently tracked view became unfocused then disconnect from it
			else if (_view == FocusedView)
			{
				DisconnectFromPlatform();
				_focusedView = null;
				_focusedViewEnclosingPage = null;
			}

			if (!FeatureEnabled)
			{
				DisconnectFromPlatform();
				return null;
			}

			if (_view is not VisualElement ve)
				return null;

			if (!_view.IsFocused)
				return null;

			DisconnectFromPlatform();

			// This view has been set as focused but it's not currently loaded
			var platformView = (_view.Handler as IPlatformViewHandler)?.PlatformView;
			if (platformView is null)
			{
				return null;
			}

			if (ve.Window is null)
			{
				// This means the xplat IsFocused value has lagged behind navigation events.
				// This might happen if navigated has fired on the incoming page but the
				// "LostFocus" event hasn't propagated from the previous one
				return null;
			}

			IDisposable? platformToken = SetupHideSoftInputOnTapped(platformView);

#if ANDROID
			var window = ve.Window;
			window.DispatchTouchEvent += OnWindowDispatchedTouch;
#endif
			_watchingForTaps = new ActionDisposable(() =>
			{
				platformToken?.Dispose();
				platformToken = null;
#if ANDROID
				window.DispatchTouchEvent -= OnWindowDispatchedTouch;
				window = null;
#endif
			});

			return _watchingForTaps;
		}

		void DisconnectFromPlatform()
		{
			_watchingForTaps?.Dispose();
			_watchingForTaps = null;
		}

		IView? FocusedView
		{
			get
			{
				if (_focusedView?.TryGetTarget(out IView? view) == true)
				{
					return view;
				}

				return null;
			}
		}
		internal void SetupHideSoftInputOnTapped()
		{
			if (FocusedView is not null)
			{
				UpdateFocusForView(FocusedView);
			}
		}
	}
}
#endif