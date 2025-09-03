
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Maui.Controls.Sample.Issues;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 20654, "Improve the CollectionView FindEstimatedSizeUsingWidth to handle grouping and testing", PlatformAffected.iOS)]
public class Issue20654 : TestContentPage
{
	CollectionView collectionView;
	protected override void Init()
	{
		BindingContext = new Issue20654MainPageViewModel();
		var descriptionLabel = new Label
		{
			Text = "Grouped: Testing FindEstimatedSizeUsingWidth with Groups (scroll to see Group 3 items)"
		};
		var scrollButton = new Button
		{
			Text = "Click to scroll last index",
			AutomationId = "ScrollButton"
		};
		scrollButton.Clicked += Button_Clicked;
		collectionView = new CollectionView
		{
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal),
			ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems,
			IsGrouped = true,
			HeightRequest = 180,
			ItemTemplate = new DataTemplate(() =>
			{
				var grid = new Grid
				{
					HeightRequest = 180
				};

				grid.SetBinding(Grid.BackgroundColorProperty, "Color");
				grid.SetBinding(Grid.WidthRequestProperty, "Width");

				var stackLayout = new StackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					BackgroundColor = Colors.Black
				};

				var groupNameLabel = new Label
				{
					TextColor = Colors.White,
					HorizontalOptions = LayoutOptions.Center
				};
				groupNameLabel.SetBinding(Label.TextProperty, "GroupName");

				var indexLabel = new Label
				{
					TextColor = Colors.White,
					HorizontalOptions = LayoutOptions.Center
				};
				indexLabel.SetBinding(Label.TextProperty, "Index");

				stackLayout.Children.Add(groupNameLabel);
				stackLayout.Children.Add(indexLabel);
				grid.Children.Add(stackLayout);

				return grid;
			}),
			GroupHeaderTemplate = new DataTemplate(() =>
			{
				var grid = new Grid
				{
					BackgroundColor = Colors.Gray,
					WidthRequest = 80,
					HeightRequest = 180
				};

				var label = new Label
				{
					TextColor = Colors.White,
					FontAttributes = FontAttributes.Bold,
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center
				};
				label.SetBinding(Label.TextProperty, "GroupName");

				grid.Children.Add(label);
				return grid;
			})
		};

		collectionView.SetBinding(ItemsView.ItemsSourceProperty, "GroupedItems");

		// Layout
		var stackLayoutMain = new StackLayout
		{
			Children =
			{
				descriptionLabel,
				scrollButton,
				collectionView
			}
		};
		Content = stackLayoutMain;
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		var viewModel = BindingContext as Issue20654MainPageViewModel;
		if (viewModel?.GroupedItems != null && viewModel.GroupedItems.Count > 0)
		{
			var lastGroup = viewModel.GroupedItems.Last();
			if (lastGroup.Count > 0)
			{
				var lastItem = lastGroup.Last();
				collectionView.ScrollTo(lastItem, lastGroup, ScrollToPosition.End, false);
			}
		}
	}
}

public partial class Issue20654ItemViewModel
{
	public int Width { get; set; }
	public int Index { get; set; }
	public string GroupName { get; set; } = string.Empty;
	public Color Color { get; set; }
}

public partial class GroupedItemViewModel : ObservableCollection<Issue20654ItemViewModel>
{
	public string GroupName { get; set; } = string.Empty;

	public GroupedItemViewModel(string groupName, IEnumerable<Issue20654ItemViewModel> items) : base(items)
	{
		GroupName = groupName;
		foreach (var item in this)
		{
			item.GroupName = groupName;
		}
	}
}

public partial class Issue20654MainPageViewModel : INotifyPropertyChanged
{
    private ObservableCollection<GroupedItemViewModel> _groupedItems;

    public event PropertyChangedEventHandler PropertyChanged;

    public ObservableCollection<GroupedItemViewModel> GroupedItems
    {
        get => _groupedItems;
        set
        {
            if (_groupedItems != value)
            {
                _groupedItems = value;
                OnPropertyChanged(nameof(GroupedItems));
            }
        }
    }

    public Issue20654MainPageViewModel()
    {
        try
        {
            _groupedItems = new ObservableCollection<GroupedItemViewModel>();
            
            var group1Items = new List<Issue20654ItemViewModel>();
            for (int i = 0; i < 6; i++)
            {
                group1Items.Add(new Issue20654ItemViewModel { Index = i, Width = 60, Color = GetColor(i) });
            }
            _groupedItems.Add(new GroupedItemViewModel("Group 1", group1Items));

            var group2Items = new List<Issue20654ItemViewModel>();
            for (int i = 0; i < 8; i++)
            {
                int width = i % 2 == 0 ? 50 : 120;
                group2Items.Add(new Issue20654ItemViewModel { Index = i + 6, Width = width, Color = GetColor(i + 6) });
            }
            _groupedItems.Add(new GroupedItemViewModel("Group 2", group2Items));

            var group3Items = new List<Issue20654ItemViewModel>();
            for (int i = 0; i < 8; i++)
            {
                int width = i < 3 ? 150 : 80;
                group3Items.Add(new Issue20654ItemViewModel { Index = i + 14, Width = width, Color = GetColor(i + 14) });
            }
            _groupedItems.Add(new GroupedItemViewModel("Group 3", group3Items));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in Issue20654MainPageViewModel constructor: {ex.Message}");
            _groupedItems = new ObservableCollection<GroupedItemViewModel>();
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private Color GetColor(int i)
    {
        switch (i % 4)
        {
            case 0: return Colors.Red;
            case 1: return Colors.Green;
            case 2: return Colors.Blue;
            case 3: return Colors.Yellow;
            default: return Colors.Black;
        }
    }
}
