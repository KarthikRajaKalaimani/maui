using System.Text.RegularExpressions;

namespace Microsoft.Maui.IntegrationTests.Apple;

/// <summary>
/// Pure logic extracted from <c>eng/devices/ios.cake</c> helpers
/// <c>GetCompatibleSimulatorDevice</c> and <c>GetXcodeMajorVersion</c>.
///
/// Xcode 26+ (released 2025) ships only with iOS 26+ simulator runtimes.
/// When a CI job requests an older simulator (e.g. iOS 18.4) on an Xcode 26 machine,
/// this helper substitutes a compatible target so the job does not fail with a
/// <see cref="System.Collections.Generic.KeyNotFoundException"/> inside XHarness.
/// </summary>
public static class SimulatorVersionHelper
{
	// Xcode 26 was the first release where Apple aligned the OS and Xcode major versions.
	// From Xcode 26 onwards only iOS 26+ runtimes ship with the product.
	internal const int Xcode26MajorVersion = 26;

	/// <summary>
	/// Returns a simulator device target that is compatible with the installed Xcode.
	/// </summary>
	/// <param name="requestedDevice">
	/// XHarness device target string, e.g. <c>ios-simulator-64_18.4</c>.
	/// </param>
	/// <param name="xcodeMajorVersion">
	/// The installed Xcode major version (0 = unknown / could not detect).
	/// </param>
	/// <returns>
	/// The original <paramref name="requestedDevice"/> when no substitution is needed,
	/// or a replacement target when the requested iOS version is incompatible with the
	/// installed Xcode (e.g. <c>ios-simulator-64_26.0</c> when Xcode 26 is detected).
	/// </returns>
	public static string GetCompatibleDevice(string requestedDevice, int xcodeMajorVersion)
	{
		if (xcodeMajorVersion <= 0)
			return requestedDevice;

		// Parse "ios-simulator-64_18.4"  →  base="ios-simulator-64", major=18, minor="4"
		var match = Regex.Match(requestedDevice, @"^(.+)_(\d+)\.(\d+)$");
		if (!match.Success)
			return requestedDevice;

		var deviceBase     = match.Groups[1].Value;
		var requestedMajor = int.Parse(match.Groups[2].Value);

		// Xcode 26+ only ships with iOS 26+ runtimes.
		if (xcodeMajorVersion >= Xcode26MajorVersion && requestedMajor < Xcode26MajorVersion)
			return $"{deviceBase}_26.0";

		return requestedDevice;
	}

	/// <summary>
	/// Given a device target string returns the iOS major/minor version, or
	/// <see langword="null"/> if the string does not match the expected format.
	/// </summary>
	public static (int Major, int Minor)? ParseIOSVersion(string deviceTarget)
	{
		var match = Regex.Match(deviceTarget, @"_(\d+)\.(\d+)$");
		if (!match.Success)
			return null;

		return (int.Parse(match.Groups[1].Value), int.Parse(match.Groups[2].Value));
	}
}
