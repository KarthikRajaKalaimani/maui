using System;
using System.Collections.Generic;
using System.Text;
#if IOS || MACCATALYST
using PlatformView = UIKit.UIView;
#elif MONOANDROID
using PlatformView = Android.Views.View;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#endif

namespace Microsoft.Maui.Controls
{
	public partial class VisualElement
	{
		IDisposable? _loadedUnloadedToken;
#if IOS || MACCATALYST
		Page? _deferredUnloadedPage;
		TabbedPage? _deferredUnloadedTabbedPage;
#endif

		partial void HandlePlatformUnloadedLoaded()
		{
			_loadedUnloadedToken?.Dispose();
			_loadedUnloadedToken = null;

			// Window and this VisualElement both have a handler to work with
			if (Window?.Handler?.PlatformView is not null &&
				Handler?.PlatformView is PlatformView view)
			{
				if (view.IsLoaded())
				{
#if IOS || MACCATALYST
					ClearDeferredUnloaded();
#endif
					SendLoaded(false);
#if IOS || MACCATALYST
					SendPendingLoaded();
#endif

					// If SendLoaded caused the unloaded tokens to wire up
					_loadedUnloadedToken?.Dispose();
					_loadedUnloadedToken = null;
					_loadedUnloadedToken = this.OnUnloaded(OnPlatformUnloaded);
				}
				else
				{
#if IOS || MACCATALYST
					if (!DeferUnloadedForRetainedTab(view))
#endif
					SendUnloaded(false);

					// If SendUnloaded caused the unloaded tokens to wire up
					_loadedUnloadedToken?.Dispose();
					_loadedUnloadedToken = null;
					if (Handler is not null)
					{
						_loadedUnloadedToken = this.OnLoaded(OnPlatformLoaded);
					}
				}
			}
			// This VisualElement has a handler with MauiContext but no MAUI Window.
			// This happens when the view is hosted inside a native container via ToPlatform().
			// We still need to fire Loaded/Unloaded based on the platform view's attach state.
			else if (Window is null &&
				Handler?.MauiContext is not null &&
				Handler?.PlatformView is PlatformView nativeHostedView)
			{
				if (nativeHostedView.IsLoaded())
				{
#if IOS || MACCATALYST
					ClearDeferredUnloaded();
#endif
					SendLoaded(false);
#if IOS || MACCATALYST
					SendPendingLoaded();
#endif

					// If SendLoaded caused the unloaded tokens to wire up
					_loadedUnloadedToken?.Dispose();
					_loadedUnloadedToken = null;
					_loadedUnloadedToken = this.OnUnloaded(OnPlatformUnloaded);
				}
				else
				{
#if IOS || MACCATALYST
					if (_deferredUnloadedPage is not null)
					{
						// Let the parent remove the native view before an Unloaded subscriber disconnects its handler.
						nativeHostedView.BeginInvokeOnMainThread(() =>
						{
							if (_deferredUnloadedPage is null)
								return;

							if (Window is null &&
								(Handler?.PlatformView is not PlatformView currentView || !currentView.IsLoaded()))
							{
								ClearDeferredUnloaded();
								_loadedUnloadedToken?.Dispose();
								_loadedUnloadedToken = null;
								SendUnloaded();
							}
							else
							{
								UpdatePlatformUnloadedLoadedWiring(Window);
							}
						});
						return;
					}
#endif
					// Not yet attached to a native parent; wait for the platform loaded event
					_loadedUnloadedToken?.Dispose();
					_loadedUnloadedToken = null;
					if (Handler is not null)
					{
						_loadedUnloadedToken = this.OnLoaded(OnPlatformLoaded);
					}
				}
			}
			else
			{
#if IOS || MACCATALYST
				ClearDeferredUnloaded();
#endif
				// My handler is still set but the window handler isn't set.
				// This means I'm starting to detach from the platform window
				// So we wait for the platform detach events to fire before calling 
				// OnUnloaded
				if (Handler?.PlatformView is PlatformView detachingView &&
					detachingView.IsLoaded())
				{
					_loadedUnloadedToken = this.OnUnloaded(OnPlatformUnloaded);
				}
				else
				{
					SendUnloaded();
				}
			}
		}

		void OnPlatformLoaded()
		{
#if IOS || MACCATALYST
			if (_deferredUnloadedPage is not null)
			{
				ClearDeferredUnloaded();
				HandlePlatformUnloadedLoaded();
				return;
			}
#endif
			SendLoaded();
		}

		void OnPlatformUnloaded()
		{
#if IOS || MACCATALYST
			if (Handler?.PlatformView is PlatformView view && DeferUnloadedForRetainedTab(view))
			{
				HandlePlatformUnloadedLoaded();
				return;
			}

			ClearDeferredUnloaded();
#endif
			SendUnloaded();
		}

#if IOS || MACCATALYST
		bool DeferUnloadedForRetainedTab(PlatformView view)
		{
			if (_isLoadedFired && Window?.Handler?.PlatformView is not null)
			{
				Element child = this;
				while (child.Parent is Element parent)
				{
					// UIKit detaches an inactive tab's native page while its MAUI descendants remain parented.
					if (child is Page page && parent is TabbedPage tabbedPage &&
						tabbedPage.Handler?.PlatformView is PlatformView tabbedView && tabbedView.Window is not null &&
						page.Handler?.PlatformView is PlatformView pageView && pageView.Window is null &&
						(view == pageView || view.IsDescendantOfView(pageView)))
					{
						if (_deferredUnloadedPage != page || _deferredUnloadedTabbedPage != tabbedPage)
						{
							ClearDeferredUnloaded();
							_deferredUnloadedPage = page;
							_deferredUnloadedTabbedPage = tabbedPage;
							page.HandlerChanged += OnDeferredTabHandlerChanged;
							tabbedPage.HandlerChanged += OnDeferredTabHandlerChanged;
						}

						return true;
					}

					child = parent;
				}
			}

			ClearDeferredUnloaded();
			return false;
		}

		void OnDeferredTabHandlerChanged(object? sender, EventArgs e) => UpdatePlatformUnloadedLoadedWiring(Window);

		void ClearDeferredUnloaded()
		{
			if (_deferredUnloadedPage is not null)
				_deferredUnloadedPage.HandlerChanged -= OnDeferredTabHandlerChanged;
			if (_deferredUnloadedTabbedPage is not null)
				_deferredUnloadedTabbedPage.HandlerChanged -= OnDeferredTabHandlerChanged;

			_deferredUnloadedPage = null;
			_deferredUnloadedTabbedPage = null;
		}

		// Delivers the initial Loaded callback to subscribers added while a retained tab was detached.
		void SendPendingLoaded()
		{
			var pendingLoaded = _pendingLoaded;
			_pendingLoaded = null;
			pendingLoaded?.Invoke(this, EventArgs.Empty);
		}
#endif
	}
}
