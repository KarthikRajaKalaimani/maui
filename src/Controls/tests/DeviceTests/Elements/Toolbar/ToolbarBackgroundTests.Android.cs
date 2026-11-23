using System.Threading.Tasks;
using Android.Graphics.Drawables;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Xunit;
using AColor = Android.Graphics.Color;
using AToolbar = AndroidX.AppCompat.Widget.Toolbar;
using Bitmap = Android.Graphics.Bitmap;
using Canvas = Android.Graphics.Canvas;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Toolbar)]
	public class ToolbarBackgroundTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task NativeGradientReplacesSolidBarColor()
		{
			if (RuntimeFeature.IsMaterial3Enabled && RuntimeFeature.UseMauiAndroidSystemBarBackgrounds)
				return;

			EnsureHandlerCreated();
			await InvokeOnMainThreadAsync(() =>
			{
				using var nativeToolbar = new AToolbar(MauiContext.Context);
				nativeToolbar.Background = new ColorDrawable(AColor.White);
				var toolbar = new Controls.Toolbar(new ContentPage())
				{
					BarBackground = new SolidColorBrush(Colors.Green)
				};
				nativeToolbar.UpdateBarBackground(toolbar);

				var gradient = new GradientDrawable();
				gradient.SetOrientation(GradientDrawable.Orientation.LeftRight);
				gradient.SetColors(new[] { AColor.Red.ToArgb(), AColor.Blue.ToArgb() });
				nativeToolbar.Background = gradient;

				var left = RenderPixel(nativeToolbar.Background, 1);
				var right = RenderPixel(nativeToolbar.Background, 98);
				Assert.True(left.R > 200 && left.B < 50);
				Assert.True(right.B > 200 && right.R < 50);
			});
		}

		[Fact]
		public async Task ClearingSolidBackgroundAfterGradientRemovesGradient()
		{
			if (RuntimeFeature.IsMaterial3Enabled && RuntimeFeature.UseMauiAndroidSystemBarBackgrounds)
				return;

			EnsureHandlerCreated();
			await InvokeOnMainThreadAsync(() =>
			{
				using var nativeToolbar = new AToolbar(MauiContext.Context);
				nativeToolbar.Background = new ColorDrawable(AColor.White);
				var toolbar = new Controls.Toolbar(new ContentPage())
				{
					BarBackground = new LinearGradientBrush(
						new GradientStopCollection
						{
							new GradientStop(Colors.Red, 0),
							new GradientStop(Colors.Blue, 1)
						},
						new Point(0, 0),
						new Point(1, 0))
				};
				nativeToolbar.UpdateBarBackground(toolbar);
				Assert.IsType<GradientStrokeDrawable>(nativeToolbar.Background);

				toolbar.BarBackground = new SolidColorBrush(Colors.Green);
				nativeToolbar.UpdateBarBackground(toolbar);
				Assert.Equal(Colors.Green.ToPlatform().ToArgb(), RenderPixel(nativeToolbar.Background, 50).ToArgb());

				toolbar.BarBackground = null;
				nativeToolbar.UpdateBarBackground(toolbar);
				Assert.Null(nativeToolbar.Background);
			});
		}

		[Fact]
		public async Task SolidBarColorPreservesNativeDrawableLevel()
		{
			if (RuntimeFeature.IsMaterial3Enabled && RuntimeFeature.UseMauiAndroidSystemBarBackgrounds)
				return;

			EnsureHandlerCreated();
			await InvokeOnMainThreadAsync(() =>
			{
				using var nativeToolbar = new AToolbar(MauiContext.Context);
				var background = new LevelListDrawable();
				background.AddLevel(1, 10000, new ColorDrawable(AColor.White));
				background.SetLevel(5000);
				nativeToolbar.Background = background;
				Assert.Equal(5000, nativeToolbar.Background.Level);

				var toolbar = new Controls.Toolbar(new ContentPage())
				{
					BarBackground = new SolidColorBrush(Colors.Red)
				};
				nativeToolbar.UpdateBarBackground(toolbar);
				Assert.Equal(5000, nativeToolbar.Background.Level);
				Assert.Equal(AColor.Red.ToArgb(), RenderPixel(nativeToolbar.Background, 50).ToArgb());

				toolbar.BarBackground = null;
				nativeToolbar.UpdateBarBackground(toolbar);
				Assert.Equal(5000, nativeToolbar.Background.Level);
			});
		}

		[Fact]
		public async Task ClearingSolidBarColorRestoresNativeDrawableTint()
		{
			if (RuntimeFeature.IsMaterial3Enabled && RuntimeFeature.UseMauiAndroidSystemBarBackgrounds)
				return;

			EnsureHandlerCreated();
			await InvokeOnMainThreadAsync(() =>
			{
				using var nativeToolbar = new AToolbar(MauiContext.Context);
				var background = new ColorDrawable(AColor.White);
				background.SetTint(AColor.Green);
				nativeToolbar.Background = background;
				Assert.Equal(AColor.Green.ToArgb(), RenderPixel(nativeToolbar.Background, 50).ToArgb());

				var toolbar = new Controls.Toolbar(new ContentPage())
				{
					BarBackground = new SolidColorBrush(Colors.Red)
				};
				nativeToolbar.UpdateBarBackground(toolbar);
				Assert.Equal(AColor.Red.ToArgb(), RenderPixel(nativeToolbar.Background, 50).ToArgb());

				toolbar.BarBackground = null;
				nativeToolbar.UpdateBarBackground(toolbar);
				Assert.Equal(AColor.Green.ToArgb(), RenderPixel(nativeToolbar.Background, 50).ToArgb());
			});
		}

		static AColor RenderPixel(Drawable drawable, int x)
		{
			drawable.SetBounds(0, 0, 100, 20);
			using var bitmap = Bitmap.CreateBitmap(100, 20, Bitmap.Config.Argb8888);
			using var canvas = new Canvas(bitmap);
			drawable.Draw(canvas);
			return new AColor(bitmap.GetPixel(x, 10));
		}
	}
}
