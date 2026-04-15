// Test fails on Windows because the Maps control is not supported on this platform.
// Test fails on Android because a valid API key is required to render the map.
#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34910 : _IssuesUITest
{
	const int InfoWindowOffsetFromCenter = 30;

	public Issue34910(TestDevice device) : base(device) { }

	public override string Issue => "NullReferenceException is thrown when clicking map or pin info window on iOS and Mac";

	[Test]
	[Category(UITestCategories.Maps)]
	public void MapClickAndPinInfoWindowShouldNotThrowNullReferenceException()
	{
		App.WaitForElement("Map");

		// Tap on the pin marker — verifies no NRE in MkMapViewOnAnnotationViewSelected → GetPinForAnnotation
		TapMapPin();
		App.WaitForElement("MarkerClicked: Yes");

		// Tap on the info window — verifies no NRE in OnCalloutClicked → GetPinForAnnotation
		TapInfoWindow();
		App.WaitForElement("InfoWindowClicked: Yes");

		// Tap on an empty area of the map — verifies no NRE in OnMapClicked
		var mapElement = App.FindElement("Map");
		var mapRect = mapElement.GetRect();
		App.TapCoordinates(mapRect.X + 10, mapRect.Y + 10);
		App.WaitForElement("MapClicked: Yes");
	}

	void TapMapPin()
	{
		var mapElement = App.FindElement("Map");
		var rect = mapElement.GetRect();
		var centerX = rect.X + rect.Width / 2;
		var centerY = rect.Y + rect.Height / 2;
		App.TapCoordinates(centerX, centerY);
	}

	void TapInfoWindow()
	{
		var mapElement = App.FindElement("Map");
		var rect = mapElement.GetRect();
#if IOS
		var centerX = rect.X + rect.Width / 2;
		var infoWindowY = rect.Y + rect.Height / 2 - InfoWindowOffsetFromCenter;
		App.TapCoordinates(centerX, infoWindowY);
#elif MACCATALYST
		var centerX = rect.X + rect.Width / 2 + 20;
		var infoWindowY = rect.Y + rect.Height / 2;
		App.TapCoordinates(centerX, infoWindowY);
#endif
	}
}
#endif
