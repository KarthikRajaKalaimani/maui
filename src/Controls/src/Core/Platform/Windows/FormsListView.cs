#nullable disable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using UwpApp = Microsoft.UI.Xaml.Application;
using UwpControlTemplate = Microsoft.UI.Xaml.Controls.ControlTemplate;
using UwpListViewHeaderItem = Microsoft.UI.Xaml.Controls.ListViewHeaderItem;
using UwpScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollBarVisibility;
using UwpThickness = Microsoft.UI.Xaml.Thickness;
using WVisibility = Microsoft.UI.Xaml.Visibility;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class FormsListView : Microsoft.UI.Xaml.Controls.ListView, IEmptyView
	{
		ContentControl _emptyViewContentControl;
		FrameworkElement _headerElement;
		FrameworkElement _emptyView;
		View _formsEmptyView;

		public FormsListView()
		{
			Template = (UwpControlTemplate)UwpApp.Current.Resources["FormsListViewTemplate"];

			ScrollViewer.SetHorizontalScrollBarVisibility(this, UwpScrollBarVisibility.Disabled);
			ScrollViewer.SetVerticalScrollBarVisibility(this, UwpScrollBarVisibility.Auto);
		}

		public static readonly DependencyProperty EmptyViewVisibilityProperty =
			DependencyProperty.Register(nameof(EmptyViewVisibility), typeof(Visibility),
				typeof(FormsListView), new PropertyMetadata(WVisibility.Collapsed, EmptyViewVisibilityChanged));

		static void EmptyViewVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is FormsListView listView)
			{
				// Update this manually; normally we'd just bind this, but TemplateBinding doesn't seem to work
				// for WASDK right now.
				listView.UpdateEmptyViewVisibility((WVisibility)e.NewValue);
			}
		}

		public WVisibility EmptyViewVisibility
		{
			get
			{
				return (WVisibility)GetValue(EmptyViewVisibilityProperty);
			}
			set
			{
				SetValue(EmptyViewVisibilityProperty, value);
			}
		}

		public void SetEmptyView(FrameworkElement emptyView, View formsEmptyView)
		{
			_emptyView = emptyView;
			_formsEmptyView = formsEmptyView;

			if (_emptyViewContentControl != null)
			{
				_emptyViewContentControl.Content = emptyView;
				UpdateEmptyViewVisibility(EmptyViewVisibility);
			}
		}

		public void UpdateHeaderMargin() => UpdateEmptyViewVisibility(EmptyViewVisibility);

		public void SetHeader(FrameworkElement headerElement)
		{
			_headerElement = headerElement;
		}

		protected override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_emptyViewContentControl = GetTemplateChild("EmptyViewContentControl") as ContentControl;

			if (_emptyView != null)
			{
				_emptyViewContentControl.Content = _emptyView;
				UpdateEmptyViewVisibility(EmptyViewVisibility);
			}
		}

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			double headerHeight = GetHeaderHeight();
			double emptyViewHeight = Math.Max(0, finalSize.Height - headerHeight);

			_formsEmptyView?.Arrange(new Rect(0, 0, finalSize.Width, emptyViewHeight));

			var result = base.ArrangeOverride(finalSize);
			UpdateEmptyViewVisibility(EmptyViewVisibility);
			return result;
		}

		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			GroupFooterItemTemplateContext.EnsureSelectionDisabled(element, item);
			base.PrepareContainerForItemOverride(element, item);
		}

		void UpdateEmptyViewVisibility(WVisibility visibility)
		{
			if (_emptyViewContentControl is null)
			{
				return;
			}

			bool isVisible = visibility == WVisibility.Visible;
			bool isInteractiveEmptyView = _formsEmptyView?.InputTransparent == false;
			double headerHeight = GetHeaderHeight();

			_emptyViewContentControl.Margin = new UwpThickness(0, headerHeight, 0, 0);
			_emptyViewContentControl.IsHitTestVisible = isVisible && isInteractiveEmptyView;

			_emptyViewContentControl.Visibility = visibility;
		}

		double GetHeaderHeight()
		{
			var headerItem = this.GetFirstDescendant<UwpListViewHeaderItem>();
			if (headerItem is null && _headerElement is null)
			{
				return 0;
			}

			double headerItemHeight = headerItem?.ActualHeight ?? 0;
			double headerElementHeight = _headerElement?.ActualHeight ?? 0;
			double desiredHeaderHeight = _headerElement?.DesiredSize.Height ?? 0;
			double fallbackHeight = GetThemeHeaderMinHeight();
			double headerHeight = Math.Max(headerItemHeight, Math.Max(headerElementHeight, Math.Max(desiredHeaderHeight, fallbackHeight)));

			return headerHeight;
		}

		double GetThemeHeaderMinHeight() =>
			TryGetDoubleResource("ListViewHeaderItemMinHeight");

		double TryGetDoubleResource(string resourceKey)
		{
			if (UwpApp.Current.Resources.TryGetValue(resourceKey, out var value) && value is double dimension)
			{
				return dimension;
			}

			return 0;
		}
	}
}
