using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample.Issues;

// Reproduces: tap gesture not recognized in the area where a child view is
// arranged *beyond* its direct parent's bounds (custom layout with IsClippedToBounds=false).
[Issue(IssueTracker.Github, 34148, "Gesture doesn't recognize in the view's spanned region on all the platforms", PlatformAffected.All)]
public class Issue34148 : ContentPage
{
	public Issue34148()
	{
		var statusLabel = new Label
		{
			Text = "Not Tapped",
			AutomationId = "StatusLabel",
			HorizontalOptions = LayoutOptions.Center
		};

		// CustomGrid mirrors the issue's reproduction: two rows of 100px each,
		// where the first row's right child (RowIndex=0, ColumnIndex=1) is arranged
		// with a height of 200px so it visually spills into the second row's area.
		var customGrid = new Issue34148CustomGrid(statusLabel);

		Content = new VerticalStackLayout
		{
			Children = { customGrid, statusLabel }
		};
	}
}

public class Issue34148CustomGrid : Issue34148ControlLayout
{
	public Issue34148CustomGrid(Label statusLabel)
	{
		IsClippedToBounds = false;
		this.Add(new Issue34148CustomRow(0, statusLabel));
		this.Add(new Issue34148CustomRow(1, null));
	}

	protected override ILayoutManager CreateLayoutManager() =>
		new Issue34148ControlLayoutManager(this);

	internal override Size ArrangeContent(Rect bounds)
	{
		int y = 0;
		foreach (var child in this.Children)
		{
			child.Arrange(new Rect(bounds.Left, y, 200, 100));
			y += 100;
		}
		return new Size(200, 200);
	}

	internal override Size MeasureContent(double widthConstraint, double heightConstraint)
	{
		foreach (var child in this.Children)
			child.Measure(200, 100);
		return new Size(200, 200);
	}
}

public class Issue34148CustomRow : Issue34148ControlLayout
{
	readonly int _rowIndex;

	public Issue34148CustomRow(int rowIndex, Label getStatusLabel)
	{
		_rowIndex = rowIndex;
		IsClippedToBounds = false;

		// Row 0: left box + right box (the right box spans into row 1)
		// Row 1: left box only (the right side is covered by row 0's right box)
		var leftBox = new Issue34148CustomBox(rowIndex, 0)
		{
			Background = rowIndex == 0 ? Colors.LightBlue : Colors.LightGreen,
			AutomationId = rowIndex == 0 ? "TopLeftCell" : "BottomLeftCell"
		};
		this.Add(leftBox);

		if (rowIndex == 0)
		{
			var rightBox = new Issue34148CustomBox(rowIndex, 1)
			{
				Background = Colors.LightCoral,
				AutomationId = "SpanningCell"
			};
			var tapGesture = new TapGestureRecognizer();
			tapGesture.Tapped += (s, e) =>
			{
				Console.WriteLine("ISSUE34148: Tap recognized in spanning cell");
				if (getStatusLabel != null)
					getStatusLabel.Text = "Tapped";
			};
			rightBox.GestureRecognizers.Add(tapGesture);
			this.Add(rightBox);
		}
	}

	protected override ILayoutManager CreateLayoutManager() =>
		new Issue34148ControlLayoutManager(this);

	internal override Size ArrangeContent(Rect bounds)
	{
		int x = 0;
		foreach (var child in this.Children)
		{
			if (child is Issue34148CustomBox box && box.RowIndex == 0 && box.ColumnIndex == 1)
				// Span into row 1: arrange with full 200px height
				child.Arrange(new Rect(x, bounds.Y, 100, 200));
			else
				child.Arrange(new Rect(x, bounds.Y, 100, 100));
			x += 100;
		}
		return new Size(200, 100);
	}

	internal override Size MeasureContent(double widthConstraint, double heightConstraint)
	{
		foreach (var child in this.Children)
		{
			if (child is Issue34148CustomBox box && box.RowIndex == 0 && box.ColumnIndex == 1)
				child.Measure(100, 200);
			else
				child.Measure(100, 100);
		}
		return new Size(200, 100);
	}
}

public class Issue34148CustomBox : Border
{
	public int RowIndex { get; }
	public int ColumnIndex { get; }

	public Issue34148CustomBox(int rowIndex, int columnIndex)
	{
		RowIndex = rowIndex;
		ColumnIndex = columnIndex;
		Stroke = Colors.Orange;
		StrokeThickness = 2;
	}
}

public abstract class Issue34148ControlLayout : Layout
{
	internal abstract Size ArrangeContent(Rect bounds);
	internal abstract Size MeasureContent(double widthConstraint, double heightConstraint);
}

internal class Issue34148ControlLayoutManager : LayoutManager
{
	readonly Issue34148ControlLayout _layout;

	internal Issue34148ControlLayoutManager(Issue34148ControlLayout layout) : base(layout)
	{
		_layout = layout;
	}

	public override Size ArrangeChildren(Rect bounds) => _layout.ArrangeContent(bounds);
	public override Size Measure(double widthConstraint, double heightConstraint) => _layout.MeasureContent(widthConstraint, heightConstraint);
}
