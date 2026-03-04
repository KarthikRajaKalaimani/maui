namespace Microsoft.Maui.IntegrationTests.Apple;

/// <summary>
/// Unit tests for <see cref="SimulatorVersionHelper"/>.
///
/// These tests validate the device-target substitution logic that was introduced to fix
/// https://github.com/dotnet/maui/issues/32735 — connectToDevice fails on CI machines
/// that have been upgraded to Xcode 26, because Xcode 26 only ships with iOS 26 runtimes.
/// </summary>
public class SimulatorVersionHelperTests
{
	// -------------------------------------------------------------------------
	// GetCompatibleDevice — Xcode 26 substitution
	// -------------------------------------------------------------------------

	[Theory]
	[InlineData("ios-simulator-64_18.4",  "ios-simulator-64_26.0")]
	[InlineData("ios-simulator-64_18.0",  "ios-simulator-64_26.0")]
	[InlineData("ios-simulator-64_17.5",  "ios-simulator-64_26.0")]
	[InlineData("ios-simulator-64_16.4",  "ios-simulator-64_26.0")]
	[InlineData("ios-simulator-64_25.9",  "ios-simulator-64_26.0")]
	public void GetCompatibleDevice_Xcode26_SubstitutesOlderIOS(string input, string expected)
	{
		var result = SimulatorVersionHelper.GetCompatibleDevice(input, xcodeMajorVersion: 26);
		Assert.Equal(expected, result);
	}

	[Theory]
	[InlineData("ios-simulator-64_26.0")]
	[InlineData("ios-simulator-64_26.1")]
	[InlineData("ios-simulator-64_27.0")]
	public void GetCompatibleDevice_Xcode26_PreservesCompatibleTarget(string input)
	{
		var result = SimulatorVersionHelper.GetCompatibleDevice(input, xcodeMajorVersion: 26);
		Assert.Equal(input, result);
	}

	// -------------------------------------------------------------------------
	// GetCompatibleDevice — Xcode ≤ 16 (iOS 18.x machines), no substitution
	// -------------------------------------------------------------------------

	[Theory]
	[InlineData("ios-simulator-64_18.4", 16)]
	[InlineData("ios-simulator-64_18.0", 16)]
	[InlineData("ios-simulator-64_17.5", 15)]
	public void GetCompatibleDevice_OlderXcode_NoSubstitution(string input, int xcodeMajor)
	{
		var result = SimulatorVersionHelper.GetCompatibleDevice(input, xcodeMajorVersion: xcodeMajor);
		Assert.Equal(input, result);
	}

	// -------------------------------------------------------------------------
	// GetCompatibleDevice — unknown / undetected Xcode version
	// -------------------------------------------------------------------------

	[Theory]
	[InlineData("ios-simulator-64_18.4")]
	[InlineData("ios-simulator-64_26.0")]
	public void GetCompatibleDevice_UnknownXcodeVersion_ReturnsOriginal(string input)
	{
		// xcodeMajorVersion = 0 means "could not detect"
		var result = SimulatorVersionHelper.GetCompatibleDevice(input, xcodeMajorVersion: 0);
		Assert.Equal(input, result);
	}

	[Theory]
	[InlineData("ios-simulator-64_18.4")]
	[InlineData("ios-simulator-64_26.0")]
	public void GetCompatibleDevice_NegativeXcodeVersion_ReturnsOriginal(string input)
	{
		var result = SimulatorVersionHelper.GetCompatibleDevice(input, xcodeMajorVersion: -1);
		Assert.Equal(input, result);
	}

	// -------------------------------------------------------------------------
	// GetCompatibleDevice — malformed / edge-case device strings
	// -------------------------------------------------------------------------

	[Theory]
	[InlineData("ios-simulator-64")]              // no version suffix
	[InlineData("ios-simulator-64_")]             // trailing underscore only
	[InlineData("ios-simulator-64_18")]           // no minor version
	[InlineData("")]                              // empty string
	[InlineData("just-a-name")]                   // arbitrary string
	public void GetCompatibleDevice_MalformedInput_ReturnsOriginal(string input)
	{
		var result = SimulatorVersionHelper.GetCompatibleDevice(input, xcodeMajorVersion: 26);
		Assert.Equal(input, result);
	}

	[Fact]
	public void GetCompatibleDevice_PreservesDeviceBaseName()
	{
		// The device base (everything before the version) must be preserved exactly.
		var result = SimulatorVersionHelper.GetCompatibleDevice(
			"ios-simulator-64_18.4", xcodeMajorVersion: 26);

		Assert.StartsWith("ios-simulator-64_", result, StringComparison.Ordinal);
	}

	// -------------------------------------------------------------------------
	// ParseIOSVersion
	// -------------------------------------------------------------------------

	[Theory]
	[InlineData("ios-simulator-64_18.4",  18, 4)]
	[InlineData("ios-simulator-64_26.0",  26, 0)]
	[InlineData("ios-simulator-64_17.5",  17, 5)]
	public void ParseIOSVersion_ValidTargets_ReturnsTuple(string input, int expectedMajor, int expectedMinor)
	{
		var result = SimulatorVersionHelper.ParseIOSVersion(input);

		Assert.NotNull(result);
		Assert.Equal(expectedMajor, result!.Value.Major);
		Assert.Equal(expectedMinor, result!.Value.Minor);
	}

	[Theory]
	[InlineData("ios-simulator-64")]
	[InlineData("ios-simulator-64_")]
	[InlineData("")]
	public void ParseIOSVersion_InvalidTargets_ReturnsNull(string input)
	{
		var result = SimulatorVersionHelper.ParseIOSVersion(input);
		Assert.Null(result);
	}

	// -------------------------------------------------------------------------
	// Constants
	// -------------------------------------------------------------------------

	[Fact]
	public void Xcode26MajorVersion_Is26()
	{
		Assert.Equal(26, SimulatorVersionHelper.Xcode26MajorVersion);
	}
}
