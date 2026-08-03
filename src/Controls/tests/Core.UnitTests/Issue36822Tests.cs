using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class Issue36822Tests : BaseTestFixture
	{
		public Issue36822Tests()
		{
			Application.Current = new MockApplication();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Application.Current = null;
			}

			base.Dispose(disposing);
		}

		class BadgeView : ContentView
		{
			internal readonly Label _label;

			public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
				nameof(TextColor), typeof(Color), typeof(BadgeView), Colors.Black,
				propertyChanged: (b, _, v) => ((BadgeView)b)._label.TextColor = (Color)v);

			public Color TextColor
			{
				get => (Color)GetValue(TextColorProperty);
				set => SetValue(TextColorProperty, value);
			}

			public BadgeView()
			{
				_label = new Label { TextColor = TextColor };
				Content = _label;
			}
		}

		[Fact]
		public void ImplicitStyleDoesNotApplyInsideBaseConstructor()
		{
			var style = new Style(typeof(BadgeView));
			style.Setters.Add(new Setter { Property = BadgeView.TextColorProperty, Value = Colors.Red });
			Application.Current.Resources.Add(style);

			// Should not throw NullReferenceException from the StyleableElement base ctor
			var badge = new BadgeView();

			Assert.NotNull(badge._label);
		}
	}
}
