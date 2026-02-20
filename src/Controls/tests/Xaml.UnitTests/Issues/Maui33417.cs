using System;
using System.Linq;
using Xunit;
using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Maui33417
{
	const string InvalidDataTypeXaml = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
					 xmlns:local="clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests"
		             x:Class="Microsoft.Maui.Controls.Xaml.UnitTests.Maui33417_InvalidDataType"
		             x:DataType="local:Foo">
		    <VerticalStackLayout>
		        <Label Text="{Binding NonExistentProperty}" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	const string InvalidBindingPropertyXaml = """
		<?xml version="1.0" encoding="utf-8" ?>
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             x:Class="Microsoft.Maui.Controls.Xaml.UnitTests.Maui33417_InvalidBindingProperty"
		             x:DataType="ContentPage">
		    <VerticalStackLayout>
		        <Label Text="{Binding Foo}" />
		    </VerticalStackLayout>
		</ContentPage>
		""";

	[Fact]
	public void InvalidDataType_ShouldReportTypeResolutionError()
	{
		var result = CreateMauiCompilation()
			.RunMauiSourceGenerator(new AdditionalXamlFile("Maui33417_InvalidDataType.xaml", InvalidDataTypeXaml));
		Assert.NotEmpty(result.Diagnostics);
		var hasTypeError = result.Diagnostics.Any(d =>
			d.Id == "MAUIX2000" && d.GetMessage().Contains("Foo", StringComparison.Ordinal));
		Assert.True(hasTypeError,
			$"Should report type resolution error (MAUIX2000) for 'local:Foo'. Found diagnostics: {string.Join(", ", result.Diagnostics.Select(d => $"{d.Id}: {d.GetMessage()}"))}");
	}

	[Fact]
	public void InvalidBindingProperty_ShouldReportPropertyNotFound()
	{
		var result = CreateMauiCompilation()
			.RunMauiSourceGenerator(new AdditionalXamlFile("Maui33417_InvalidBindingProperty.xaml", InvalidBindingPropertyXaml));
		Assert.NotEmpty(result.Diagnostics);
		var hasPropertyError = result.Diagnostics.Any(d =>
			d.Id == "MAUIG2045" && d.GetMessage().Contains("Foo", StringComparison.Ordinal));
		Assert.True(hasPropertyError,
			$"Should report binding property not found warning (MAUIG2045) for 'Foo'. Found diagnostics: {string.Join(", ", result.Diagnostics.Select(d => $"{d.Id}: {d.GetMessage()}"))}");
	}
}
