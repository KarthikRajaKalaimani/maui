using System.Globalization;
using System.Threading.Tasks;
using Android.Content.Res;
using Java.Util;
using Microsoft.Maui.ApplicationModel;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Application)]
	public class ApplicationTests
	{
		[Fact(DisplayName = "Verify Application.Current exists on IPlatformApplication")]
		public void PlatformApplicationCurrentExists()
		{
			var platform = Microsoft.Maui.IPlatformApplication.Current;
			Assert.NotNull(platform);
			Assert.NotNull(platform.Services);
			Assert.NotNull(platform.Application);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public Task ConfigurationChangeRefreshesSystemCultureWithoutOverridingAppCulture(bool setThreadCulture)
		{
			return MainThread.InvokeOnMainThreadAsync(() =>
			{
				using var originalLocale = Locale.Default;
				using var configuration = new Configuration(MauiApplication.Current.Resources.Configuration);
				var originalCulture = CultureInfo.CurrentCulture;
				var originalUICulture = CultureInfo.CurrentUICulture;
				var originalDefaultCulture = CultureInfo.DefaultThreadCurrentCulture;
				var originalDefaultUICulture = CultureInfo.DefaultThreadCurrentUICulture;
				var culture = new CultureInfo("de-DE");
				var uiCulture = new CultureInfo("ja-JP");
				var localeName = CultureInfo.InstalledUICulture.Name == "fr-FR" ? "en-US" : "fr-FR";
				using var locale = Locale.ForLanguageTag(localeName);

				try
				{
					if (setThreadCulture)
					{
						CultureInfo.CurrentCulture = culture;
						CultureInfo.CurrentUICulture = uiCulture;
					}
					else
					{
						CultureInfo.DefaultThreadCurrentCulture = culture;
						CultureInfo.DefaultThreadCurrentUICulture = uiCulture;
					}

					var expectedCulture = CultureInfo.CurrentCulture;
					var expectedUICulture = CultureInfo.CurrentUICulture;
					Locale.Default = locale;
					configuration.SetLocale(locale);
					MauiApplication.Current.OnConfigurationChanged(configuration);

					Assert.Equal(localeName, CultureInfo.InstalledUICulture.Name);
					Assert.Same(expectedCulture, CultureInfo.CurrentCulture);
					Assert.Same(expectedUICulture, CultureInfo.CurrentUICulture);
					Assert.Same(setThreadCulture ? originalDefaultCulture : culture, CultureInfo.DefaultThreadCurrentCulture);
					Assert.Same(setThreadCulture ? originalDefaultUICulture : uiCulture, CultureInfo.DefaultThreadCurrentUICulture);
				}
				finally
				{
					Locale.Default = originalLocale;
					CultureInfo.CurrentCulture = originalCulture;
					CultureInfo.CurrentUICulture = originalUICulture;
					CultureInfo.DefaultThreadCurrentCulture = originalDefaultCulture;
					CultureInfo.DefaultThreadCurrentUICulture = originalDefaultUICulture;
					CultureInfo.CurrentCulture.ClearCachedData();
				}
			});
		}
	}
}
