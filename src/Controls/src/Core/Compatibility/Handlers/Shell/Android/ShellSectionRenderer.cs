#nullable disable
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.CoordinatorLayout.Widget;
using AndroidX.Core.View;
using AndroidX.Fragment.App;
using AndroidX.ViewPager.Widget;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.AppBar;
using Google.Android.Material.Tabs;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Platform;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellSectionRenderer : Fragment, IShellSectionRenderer//, ViewPager.IOnPageChangeListener
		, AView.IOnClickListener, IShellObservableFragment, IAppearanceObserver, TabLayoutMediator.ITabConfigurationStrategy
	{
		#region ITabConfigurationStrategy

		void TabLayoutMediator.ITabConfigurationStrategy.OnConfigureTab(TabLayout.Tab tab, int position)
		{
			tab.SetText(new String(SectionController.GetItems()[position].Title));
		}

		void UpdateCurrentItem(ShellContent content)
		{
			if (_toolbarTracker == null)
				return;

			var page = ((IShellContentController)content).GetOrCreateContent();
			if (page == null)
				throw new ArgumentNullException(nameof(page), "Shell Content Page is Null");

			ShellSection.SetValueFromRenderer(ShellSection.CurrentItemProperty, content);
			_toolbarTracker.Page = page;
		}

		#endregion IOnPageChangeListener

		#region IAppearanceObserver

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			if (appearance == null)
				ResetAppearance();
			else
				SetAppearance(appearance);
		}

		#endregion IAppearanceObserver

		#region IOnClickListener

		void AView.IOnClickListener.OnClick(AView v)
		{
		}

		#endregion IOnClickListener

		readonly IShellContext _shellContext;
		CoordinatorLayout _rootView;
		bool _selecting;
		TabLayout _tablayout;
		IShellTabLayoutAppearanceTracker _tabLayoutAppearanceTracker;
		AToolbar _toolbar;
		IShellToolbarAppearanceTracker _toolbarAppearanceTracker;
		IShellToolbarTracker _toolbarTracker;
		ViewPager2 _viewPager;
		bool _disposed;
		ItemViewAccessibilityLayoutListener _a11yLayoutListener;
		IShellController ShellController => _shellContext.Shell;
		public event EventHandler AnimationFinished;
		Fragment IShellObservableFragment.Fragment => this;
		public ShellSection ShellSection { get; set; }
		protected IShellContext ShellContext => _shellContext;
		IShellSectionController SectionController => (IShellSectionController)ShellSection;
		IMauiContext MauiContext => ShellContext.Shell.Handler.MauiContext;
		Toolbar _shellToolbar;

		public ShellSectionRenderer(IShellContext shellContext)
		{
			_shellContext = shellContext;
		}


		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			var shellSection = ShellSection;
			if (shellSection == null)
				return null;

			if (shellSection.CurrentItem == null)
				throw new InvalidOperationException($"Content not found for active {shellSection}. Title: {shellSection.Title}. Route: {shellSection.Route}.");

			var context = Context;
			var root = PlatformInterop.CreateShellCoordinatorLayout(context);
			var appbar = PlatformInterop.CreateShellAppBar(context, Resource.Attribute.appBarLayoutStyle, root);

			MauiWindowInsetListener.SetupViewWithLocalListener(root);

			int actionBarHeight = context.GetActionBarHeight();

			_shellToolbar = new Toolbar(shellSection);
			ShellToolbarTracker.ApplyToolbarChanges(_shellContext.Shell.Toolbar, _shellToolbar);
			_toolbar = (AToolbar)_shellToolbar.ToPlatform(_shellContext.Shell.FindMauiContext());
			appbar.AddView(_toolbar);
			_tablayout = PlatformInterop.CreateShellTabLayout(context, appbar, actionBarHeight);

			var pagerContext = MauiContext.MakeScoped(layoutInflater: inflater, fragmentManager: ChildFragmentManager);
			var adapter = new ShellFragmentStateAdapter(shellSection, ChildFragmentManager, pagerContext);
			var pageChangedCallback = new ViewPagerPageChanged(this);
			_viewPager = PlatformInterop.CreateShellViewPager(context, root, _tablayout, this, adapter, pageChangedCallback);

			Page currentPage = null;
			int currentIndex = -1;
			var currentItem = shellSection.CurrentItem;
			var items = SectionController.GetItems();

			while (currentIndex < 0 && items.Count > 0 && shellSection.CurrentItem != null)
			{
				currentItem = shellSection.CurrentItem;
				currentPage = ((IShellContentController)shellSection.CurrentItem).GetOrCreateContent();

				// current item hasn't changed
				if (currentItem == shellSection.CurrentItem)
					currentIndex = items.IndexOf(currentItem);
			}

			_toolbarTracker = _shellContext.CreateTrackerForToolbar(_toolbar);
			_toolbarTracker.SetToolbar(_shellToolbar);
			_toolbarTracker.Page = currentPage;

			_viewPager.CurrentItem = currentIndex;

			if (items.Count == 1)
			{
				UpdateTablayoutVisibility();
			}

			_tablayout.LayoutChange += OnTabLayoutChange;

			_tabLayoutAppearanceTracker = _shellContext.CreateTabLayoutAppearanceTracker(ShellSection);
			_toolbarAppearanceTracker = _shellContext.CreateToolbarAppearanceTracker();

			HookEvents();

			return _rootView = root;
		}

		void OnShellContentPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == ShellContent.TitleProperty.PropertyName && sender is ShellContent shellContent)
			{
				UpdateTabTitle(shellContent);
			}
		}

		void UpdateTabTitle(ShellContent shellContent)
		{
			if (_tablayout == null || SectionController.GetItems().Count == 0)
				return;

			int index = SectionController.GetItems().IndexOf(shellContent);
			if (index >= 0)
			{
				var tab = _tablayout.GetTabAt(index);
				tab?.SetText(new string(shellContent.Title));
			}
		}

		void OnTabLayoutChange(object sender, AView.LayoutChangeEventArgs e)
		{
			if (_disposed)
				return;

			var items = SectionController.GetItems();
			for (int i = 0; i < _tablayout.TabCount; i++)
			{
				if (items.Count <= i)
					break;

				var tab = _tablayout.GetTabAt(i);

				if (tab.View != null)
					AutomationPropertiesProvider.AccessibilitySettingsChanged(tab.View, items[i]);
			}
		}

		void Destroy()
		{
			// Remove the a11y layout listener BEFORE tearing down ViewPager2 so we can still
			// reach the RecyclerView's ViewTreeObserver.
			RemoveAccessibilityLayoutListener();

			if (_rootView != null)
			{
				// Clean up the coordinator layout and local listener first
				if (_rootView is not null)
				{
					MauiWindowInsetListener.RemoveViewWithLocalListener(_rootView);
				}

				UnhookEvents();

				_shellContext?.Shell?.Toolbar?.Handler?.DisconnectHandler();

				//_viewPager.RemoveOnPageChangeListener(this);
				var adapter = _viewPager.Adapter;
				_viewPager.Adapter = null;
				adapter.Dispose();

				_tablayout.LayoutChange -= OnTabLayoutChange;
				_toolbarAppearanceTracker.Dispose();
				_tabLayoutAppearanceTracker.Dispose();
				_toolbarTracker.Dispose();
				_tablayout.Dispose();
				_toolbar.Dispose();
				_viewPager.Dispose();
				_rootView.Dispose();
			}

			_toolbarAppearanceTracker = null;
			_tabLayoutAppearanceTracker = null;
			_toolbarTracker = null;
			_tablayout = null;
			_toolbar = null;
			_shellToolbar = null;
			_viewPager = null;
			_rootView = null;

		}

		// Use OnDestroy instead of OnDestroyView because OnDestroyView will be
		// called before the animation completes. This causes tons of tiny issues.
		public override void OnDestroy()
		{
			Destroy();
			base.OnDestroy();
		}
		
		public override void OnHiddenChanged(bool hidden)
		{
			base.OnHiddenChanged(hidden);
			
			if (!hidden && _shellToolbar?.Handler != null)
			{
				_shellToolbar.Handler.UpdateValue(nameof(Toolbar.TitleView));
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				Destroy();
			}
		}

		protected virtual void OnAnimationFinished(EventArgs e)
		{
			AnimationFinished?.Invoke(this, e);
		}

		protected virtual void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateTablayoutVisibility();

			if (_viewPager?.Adapter is ShellFragmentStateAdapter adapter)
			{
				adapter.OnItemsCollectionChanged(sender, e);
				SafeNotifyDataSetChanged();
			}
		}

		void SafeNotifyDataSetChanged(int iteration = 0)
		{
			if (_disposed)
				return;

			if (!_viewPager.IsAlive())
				return;

			if (iteration >= 10)
			{
				// It's very unlikely this will happen but just in case there's a scenario
				// where we might hit an infinite loop we're adding an exit strategy
				MauiContext.CreateLogger<ShellSectionRenderer>()
					.LogWarning("ViewPager2 stuck in layout, unable to NotifyDataSetChanged;");

				return;
			}

			if (_viewPager?.Adapter is ShellFragmentStateAdapter adapter)
			{
				// https://stackoverflow.com/questions/43221847/cannot-call-this-method-while-recyclerview-is-computing-a-layout-or-scrolling-wh
				// ViewPager2 is based on RecyclerView which really doesn't like NotifyDataSetChanged when a layout is happening
				if (!_viewPager.IsInLayout)
				{
					adapter.NotifyDataSetChanged();
				}
				else
				{
					_viewPager.Post(() => SafeNotifyDataSetChanged(++iteration));
				}
			}
		}

		void UpdateTablayoutVisibility()
		{
			_tablayout.Visibility = (SectionController.GetItems().Count > 1) ? ViewStates.Visible : ViewStates.Gone;
			if (_tablayout.Visibility == ViewStates.Gone)
			{
				SetViewPager2UserInputEnabled(false);
			}
			else
			{
				SetViewPager2UserInputEnabled(true);
			}

			// Exclude ViewPager2 and its internal RecyclerView from the accessibility tree so
			// TalkBack does not treat them as a focus boundary that merges all descendant text
			// (multiple Labels in a VerticalStackLayout) into a single accessibility announcement.
			// See https://github.com/dotnet/maui/issues/36304.
			// This does NOT remove descendants — only the pager and its RecyclerView themselves —
			// so each Label remains individually focusable by TalkBack.
			_viewPager.Focusable = false;
			_viewPager.ImportantForAccessibility = ImportantForAccessibility.No;

			var recyclerView = GetViewPagerRecyclerView();
			if (recyclerView != null)
			{
				recyclerView.Focusable = false;

				// The RecyclerView's item view (the direct container of the fragment's content)
				// is another merge point. Item views are recycled/replaced under adapter changes,
				// so we can't set the flag once at startup and be done — a newly attached item
				// view has ImportantForAccessibility=Yes by default. Use a lightweight global
				// layout listener that re-applies the flag on every layout pass. The listener
				// only holds a WeakReference to the RecyclerView (no reference to this renderer),
				// so it cannot extend renderer lifetime; it is explicitly removed in Destroy().
				EnsureAccessibilityLayoutListener(recyclerView);
			}
		}

		void EnsureAccessibilityLayoutListener(AndroidX.RecyclerView.Widget.RecyclerView recyclerView)
		{
			if (_a11yLayoutListener != null)
			{
				// Re-apply immediately (in case the current item view was just replaced during
				// an adapter change and no layout pass has happened yet).
				_a11yLayoutListener.MarkAttachedItemViews();
				return;
			}

			_a11yLayoutListener = new ItemViewAccessibilityLayoutListener(recyclerView);
			var vto = recyclerView.ViewTreeObserver;
			if (vto != null && vto.IsAlive)
				vto.AddOnGlobalLayoutListener(_a11yLayoutListener);

			// Apply once now so the very first item view (already attached before the first
			// layout pass fires the listener) is marked immediately for the test / initial focus.
			_a11yLayoutListener.MarkAttachedItemViews();
		}

		// ViewPager2 always has exactly one direct child: its internal RecyclerView.
		AndroidX.RecyclerView.Widget.RecyclerView GetViewPagerRecyclerView()
		{
			if (_viewPager == null || _viewPager.ChildCount == 0)
				return null;

			return _viewPager.GetChildAt(0) as AndroidX.RecyclerView.Widget.RecyclerView;
		}

		void RemoveAccessibilityLayoutListener()
		{
			if (_a11yLayoutListener == null)
				return;

			var rv = _a11yLayoutListener.TryGetRecyclerView();
			if (rv != null)
			{
				var vto = rv.ViewTreeObserver;
				if (vto != null && vto.IsAlive)
					vto.RemoveOnGlobalLayoutListener(_a11yLayoutListener);
			}

			_a11yLayoutListener.Dispose();
			_a11yLayoutListener = null;
		}

		sealed class ItemViewAccessibilityLayoutListener : Java.Lang.Object, ViewTreeObserver.IOnGlobalLayoutListener
		{
			readonly WeakReference<AndroidX.RecyclerView.Widget.RecyclerView> _rvRef;

			public ItemViewAccessibilityLayoutListener(AndroidX.RecyclerView.Widget.RecyclerView rv)
			{
				_rvRef = new WeakReference<AndroidX.RecyclerView.Widget.RecyclerView>(rv);
			}

			public AndroidX.RecyclerView.Widget.RecyclerView TryGetRecyclerView()
				=> _rvRef.TryGetTarget(out var rv) ? rv : null;

			public void OnGlobalLayout() => MarkAttachedItemViews();

			public void MarkAttachedItemViews()
			{
				if (!_rvRef.TryGetTarget(out var rv) || rv == null)
					return;

				int count = rv.ChildCount;
				for (int i = 0; i < count; i++)
				{
					var itemView = rv.GetChildAt(i);
					if (itemView != null && itemView.ImportantForAccessibility != ImportantForAccessibility.No)
						itemView.ImportantForAccessibility = ImportantForAccessibility.No;
				}
			}
		}

		protected virtual void SetViewPager2UserInputEnabled(bool value)
		{
			_viewPager.UserInputEnabled = value;
		}

		protected virtual void OnShellItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (_rootView == null)
				return;

			if (e.PropertyName == ShellSection.CurrentItemProperty.PropertyName)
			{
				var newIndex = SectionController.GetItems().IndexOf(ShellSection.CurrentItem);

				if (SectionController.GetItems().Count != _viewPager.ChildCount)
				{
					SafeNotifyDataSetChanged();
				}

				if (newIndex >= 0)
				{
					_viewPager.CurrentItem = newIndex;
				}
			}
		}

		protected virtual void ResetAppearance()
		{
			_toolbarAppearanceTracker.ResetAppearance(_toolbar, _toolbarTracker);
			_tabLayoutAppearanceTracker.ResetAppearance(_tablayout);
		}

		protected virtual void SetAppearance(ShellAppearance appearance)
		{
			_toolbarAppearanceTracker.SetAppearance(_toolbar, _toolbarTracker, appearance);
			_tabLayoutAppearanceTracker.SetAppearance(_tablayout, appearance);
		}

		void HookEvents()
		{
			SectionController.ItemsCollectionChanged += OnItemsCollectionChanged;
			((IShellController)_shellContext.Shell).AddAppearanceObserver(this, ShellSection);
			ShellSection.PropertyChanged += OnShellItemPropertyChanged;
			foreach (var item in SectionController.GetItems())
			{
				item.PropertyChanged += OnShellContentPropertyChanged;
			}
		}

		void UnhookEvents()
		{
			SectionController.ItemsCollectionChanged -= OnItemsCollectionChanged;
			((IShellController)_shellContext?.Shell)?.RemoveAppearanceObserver(this);
			ShellSection.PropertyChanged -= OnShellItemPropertyChanged;
			foreach (var item in SectionController.GetItems())
			{
				item.PropertyChanged -= OnShellContentPropertyChanged;
			}
		}

		protected virtual void OnPageSelected(int position)
		{
			if (_selecting)
				return;

			var shellSection = ShellSection;
			var visibleItems = SectionController.GetItems();

			// This mainly happens if all of the items that are part of this shell section 
			// vanish. Android calls `OnPageSelected` with position zero even though the view pager is
			// empty
			if (position >= visibleItems.Count)
				return;

			var shellContent = visibleItems[position];

			if (shellContent == shellSection.CurrentItem)
				return;

			var stack = shellSection.Stack.ToList();
			bool result = ShellController.ProposeNavigation(ShellNavigationSource.ShellContentChanged,
				(ShellItem)shellSection.Parent, shellSection, shellContent, stack, true);

			if (result)
			{
				UpdateCurrentItem(shellContent);
			}
			else if (shellSection?.CurrentItem != null)
			{
				var currentPosition = visibleItems.IndexOf(shellSection.CurrentItem);
				_selecting = true;

				// Android doesn't really appreciate you calling SetCurrentItem inside a OnPageSelected callback.
				// It wont crash but the way its programmed doesn't really anticipate re-entrancy around that method
				// and it ends up going to the wrong location. Thus we must invoke.

				_viewPager.Post(() =>
				{
					if (currentPosition < _viewPager.ChildCount && _toolbarTracker != null)
					{
						_viewPager.SetCurrentItem(currentPosition, false);
						UpdateCurrentItem(shellSection.CurrentItem);
					}

					_selecting = false;
				});
			}
		}

		class ViewPagerPageChanged : ViewPager2.OnPageChangeCallback
		{
			private ShellSectionRenderer _shellSectionRenderer;

			public ViewPagerPageChanged(ShellSectionRenderer shellSectionRenderer)
			{
				_shellSectionRenderer = shellSectionRenderer;
			}

			public override void OnPageSelected(int position)
			{
				base.OnPageSelected(position);
				_shellSectionRenderer.OnPageSelected(position);
			}
		}
	}
}