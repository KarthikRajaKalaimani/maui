using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Map = Microsoft.Maui.Controls.Maps.Map;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34910, "NullReferenceException is thrown when clicking map or pin info window on iOS and Mac", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue34910 : ContentPage
{
	public Issue34910()
	{
		var mapClickedLabel = new Label
		{
			Text = "MapClicked: No",
			AutomationId = "MapClickedLabel"
		};

		var markerClickedLabel = new Label
		{
			Text = "MarkerClicked: No",
			AutomationId = "MarkerClickedLabel"
		};

		var infoWindowClickedLabel = new Label
		{
			Text = "InfoWindowClicked: No",
			AutomationId = "InfoWindowClickedLabel"
		};

		var statusStack = new StackLayout
		{
			Padding = 10,
			BackgroundColor = Colors.LightGray,
			Children = { mapClickedLabel, markerClickedLabel, infoWindowClickedLabel }
		};

		var map = new Map
		{
			MapType = MapType.Street,
			AutomationId = "Map"
		};

		var pin = new Pin
		{
			Label = "Test Pin",
			Type = PinType.Place,
			Location = new Location(37.7749, -122.4194),
			Address = "San Francisco, CA"
		};

		pin.MarkerClicked += (s, e) =>
		{
			markerClickedLabel.Text = "MarkerClicked: Yes";
			e.HideInfoWindow = false;
		};

		pin.InfoWindowClicked += (s, e) =>
		{
			infoWindowClickedLabel.Text = "InfoWindowClicked: Yes";
			e.HideInfoWindow = true;
		};

		map.MapClicked += (s, e) =>
		{
			mapClickedLabel.Text = "MapClicked: Yes";
		};

		map.Pins.Add(pin);
		map.MoveToRegion(MapSpan.FromCenterAndRadius(pin.Location, Distance.FromKilometers(10)));

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
			}
		};

		grid.Add(statusStack, 0, 0);
		grid.Add(map, 0, 1);

		Content = grid;
	}
}
