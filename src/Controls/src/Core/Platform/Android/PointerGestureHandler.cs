#nullable disable
using System;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.Graphics;
using System.Runtime.Versioning;
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	internal class PointerGestureHandler : Java.Lang.Object, AView.IOnHoverListener
	{
		// Tracks the last button pressed so we can use it for subsequent Move/Up/Cancel
		ButtonsMask? _activeButton;
		float _touchStartX;
		float _touchStartY;
		bool _touchMoved;

		internal PointerGestureHandler(Func<View> getView, Func<AView> getControl)
		{
			GetView = getView;
			GetControl = getControl;
			SetupHandlerForPointer();
		}

		Func<View> GetView { get; }
		Func<AView> GetControl { get; }

		public bool OnHover(AView control, MotionEvent e)
		{
			var view = GetView();

			if (view == null)
				return false;

			var platformPointerArgs = new PlatformPointerEventArgs(control, e);

			foreach (var gesture in view.GetCompositeGestureRecognizers())
			{
				if (gesture is PointerGestureRecognizer)
				{
					var pgr = gesture as PointerGestureRecognizer;
					switch (e.Action)
					{
						case MotionEventActions.HoverEnter:
							pgr.SendPointerEntered(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs);
							break;
						case MotionEventActions.HoverMove:
							pgr.SendPointerMoved(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs);
							break;
						case MotionEventActions.HoverExit:
							pgr.SendPointerExited(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs);
							break;
					}
				}
			}

			return false;
		}

		// This method is called by InnerGestureListener to handle touch events for pointer gestures
		public bool OnTouch(MotionEvent e)
		{
			var view = GetView();

			if (view == null)
				return false;

			var control = GetControl();
			if (control == null)
				return false;

			var platformPointerArgs = new PlatformPointerEventArgs(control, e);
			bool handled = false;

			foreach (var gesture in view.GetCompositeGestureRecognizers())
			{
				if (gesture is PointerGestureRecognizer pgr)
				{
					handled = true;

					// Determine the button for this action. For Move/Up/Cancel prefer the active button, if any.
					ButtonsMask current = GetPressedButton(e);
					ButtonsMask effectiveButton = current;

					switch (e.Action)
					{
						case MotionEventActions.Down:
							// Primary button goes through Down/Up
							_activeButton = current;
							_touchStartX = e.RawX;
							_touchStartY = e.RawY;
							_touchMoved = false;
							effectiveButton = current;
							if (!CheckButtonMask(pgr, effectiveButton))
								continue;
							pgr.SendPointerPressed(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs, effectiveButton);
							break;
						case MotionEventActions.Move:
							// Keep reporting the button that initiated the press if one is active
							effectiveButton = _activeButton ?? current;
							if (!_touchMoved && HasExceededTouchSlop(control, e))
								_touchMoved = true;
							if (!CheckButtonMask(pgr, effectiveButton))
								continue;
							pgr.SendPointerMoved(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs, effectiveButton);
							break;
						case MotionEventActions.Up:
							// ACTION_UP does not carry ActionButton. Use the active one if available.
							effectiveButton = _activeButton ?? current;
							if (!CheckButtonMask(pgr, effectiveButton))
								continue;
							pgr.SendPointerReleased(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs, effectiveButton);
							TryDispatchAncestorClick(control, effectiveButton);
							// Clear active button after release
							_activeButton = null;
							_touchMoved = false;
							break;
						case MotionEventActions.Cancel:
							// Treat cancel similar to release for active button, then exit
							effectiveButton = _activeButton ?? current;
							if (!CheckButtonMask(pgr, effectiveButton))
								continue;
							pgr.SendPointerExited(view, (relativeTo) => e.CalculatePosition(GetView(), relativeTo), platformPointerArgs, effectiveButton);
							_activeButton = null;
							_touchMoved = false;
							break;
					}
				}
			}

			return handled;
		}

		ButtonsMask GetPressedButton(MotionEvent motionEvent)
		{
			if (motionEvent == null)
				return ButtonsMask.Primary;

			var action = motionEvent.Action;

			// For explicit button change events (mouse/pen), use ActionButton to determine which button changed
			if (OperatingSystem.IsAndroidVersionAtLeast(23) &&
				(action == MotionEventActions.ButtonPress || action == MotionEventActions.ButtonRelease))
			{
#pragma warning disable CA1416 // Validate platform compatibility
				var actionButton = motionEvent.ActionButton; // Which button changed for this event
				if ((actionButton & MotionEventButtonState.Secondary) == MotionEventButtonState.Secondary)
					return ButtonsMask.Secondary;
				if ((actionButton & MotionEventButtonState.Primary) == MotionEventButtonState.Primary)
					return ButtonsMask.Primary;
#pragma warning restore CA1416 // Validate platform compatibility
			}

			// Otherwise, infer from current ButtonState (covers Move/Down/Up and API < 23)
			var buttonState = motionEvent.ButtonState;

			// Check for secondary button (right mouse button)
			if ((buttonState & MotionEventButtonState.Secondary) == MotionEventButtonState.Secondary)
			{
				return ButtonsMask.Secondary;
			}

			// Check for stylus secondary button on API 23+
			if (OperatingSystem.IsAndroidVersionAtLeast(23))
			{
#pragma warning disable CA1416 // Validate platform compatibility
				if (CheckStylusSecondaryButton(buttonState))
#pragma warning restore CA1416 // Validate platform compatibility
				{
					return ButtonsMask.Secondary;
				}
			}

			// Default to primary button
			return ButtonsMask.Primary;
		}

		[SupportedOSPlatform("android23.0")]
		bool CheckStylusSecondaryButton(MotionEventButtonState buttonState)
		{
			return (buttonState & MotionEventButtonState.StylusSecondary) == MotionEventButtonState.StylusSecondary;
		}

		bool CheckButtonMask(PointerGestureRecognizer recognizer, ButtonsMask currentButton)
		{
			// If no buttons specified (enum backing value is 0), default to Primary only
			if ((int)recognizer.Buttons == 0)
				return currentButton == ButtonsMask.Primary;

			if (currentButton == ButtonsMask.Secondary)
			{
				return (recognizer.Buttons & ButtonsMask.Secondary) == ButtonsMask.Secondary;
			}

			return (recognizer.Buttons & ButtonsMask.Primary) == ButtonsMask.Primary;
		}

		public void SetupHandlerForPointer()
		{
			var view = GetView();
			if (view == null)
				return;

			var control = GetControl();
			if (control == null)
				return;

			if (HasAnyPointerGestures())
				control.SetOnHoverListener(this);
			else
				control.SetOnHoverListener(null);

			return;
		}

		public bool HasAnyPointerGestures()
		{
			var gestures = GetView().GetCompositeGestureRecognizers();
			if (gestures == null || gestures.Count == 0)
				return false;

			foreach (var gesture in gestures)
				if (gesture is PointerGestureRecognizer)
					return true;

			return false;
		}

		bool HasOnlyPointerGestures()
		{
			var gestures = GetView().GetCompositeGestureRecognizers();
			if (gestures == null || gestures.Count == 0)
				return false;

			bool hasPointerGesture = false;
			foreach (var gesture in gestures)
			{
				if (gesture is PointerGestureRecognizer)
				{
					hasPointerGesture = true;
					continue;
				}

				return false;
			}

			return hasPointerGesture;
		}

		bool HasExceededTouchSlop(AView control, MotionEvent e)
		{
			var touchSlop = ViewConfiguration.Get(control.Context)?.ScaledTouchSlop ?? 0;
			return Math.Abs(e.RawX - _touchStartX) > touchSlop || Math.Abs(e.RawY - _touchStartY) > touchSlop;
		}

		void TryDispatchAncestorClick(AView control, ButtonsMask button)
		{
			if (button != ButtonsMask.Primary || _touchMoved || !HasOnlyPointerGestures())
				return;

			for (AView descendant = control; descendant?.Parent is AView parent; descendant = parent)
			{
				if (parent is AdapterView adapterView)
				{
					var position = adapterView.GetPositionForView(descendant);
					if (position != AdapterView.InvalidPosition)
					{
						var id = adapterView.GetItemIdAtPosition(position);
						if (adapterView.PerformItemClick(descendant, position, id))
							return;
					}

					continue;
				}

				if (parent.CallOnClick() || parent.PerformClick())
					return;
			}
		}
	}
}
