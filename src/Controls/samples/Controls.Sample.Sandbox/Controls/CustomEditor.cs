#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if __IOS__
using UIKit;
using Foundation;
#endif

namespace Maui.Controls.Sample
{
    public class CustomEditor : Editor
    {

#if __IOS__
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public static UIKit.UILabel _placeholder;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public static bool lastCharEnteredWasTab = false;
#endif

        public static readonly BindableProperty RequiredProperty = BindableProperty.Create("Required", typeof(bool), typeof(CustomEditor), false, propertyChanged: OnRequiredPropertyChanged);
        public bool Required
        {
            get { return (bool)GetValue(RequiredProperty); }
            set { SetValue(RequiredProperty, value); }
        }

        private static void OnRequiredPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (CustomEditor)bindable;
            control.Required = (bool)newValue;
        }

        public static readonly BindableProperty FocusOnDisplayProperty = BindableProperty.Create("FocusOnDisplay", typeof(bool), typeof(CustomEditor), false);
        public bool FocusOnDisplay
        {
            get { return (bool)GetValue(FocusOnDisplayProperty); }
            set { SetValue(FocusOnDisplayProperty, value); }
        }

        public static readonly BindableProperty CompleteOnEnterProperty = BindableProperty.Create("CompleteOnEnter", typeof(bool), typeof(CustomEditor), false);
        public bool CompleteOnEnter
        {
            get { return (bool)GetValue(CompleteOnEnterProperty); }
            set { SetValue(CompleteOnEnterProperty, value); }
        }

        public static readonly BindableProperty AllowCompleteOnEnterProperty = BindableProperty.Create(nameof(AllowCompleteOnEnter), typeof(Func<object, bool>), typeof(CustomEditor), null);
        public Func<object, bool> AllowCompleteOnEnter
        {
            get
            {
                return (Func<object, bool>)GetValue(AllowCompleteOnEnterProperty);
            }
            set { SetValue(AllowCompleteOnEnterProperty, value); }
        }

        public static readonly BindableProperty DoCompleteOnEnterProperty = BindableProperty.Create(nameof(DoCompleteOnEnter), typeof(DelegateCommand), typeof(CustomEditor), null);
        public DelegateCommand DoCompleteOnEnter
        {
            get
            {
                return (DelegateCommand)GetValue(DoCompleteOnEnterProperty);
            }
            set { SetValue(DoCompleteOnEnterProperty, value); }
        }

        public static readonly BindableProperty IsForceScanProperty = BindableProperty.Create("IsForceScan", typeof(bool), typeof(CustomEditor), false);
        public bool IsForceScan
        {
            get { return (bool)GetValue(IsForceScanProperty); }
            set { SetValue(IsForceScanProperty, value); }
        }

        public static readonly BindableProperty IsQuestionProperty = BindableProperty.Create("IsQuestion", typeof(bool), typeof(CustomEditor), false, propertyChanged: OnIsQuestionPropertyChanged);
        public bool IsQuestion
        {
            get { return (bool)GetValue(IsQuestionProperty); }
            set { SetValue(IsQuestionProperty, value); }
        }

        private static void OnIsQuestionPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (CustomEditor)bindable;
            control.IsQuestion = (bool)newValue;
        }

        public CustomEditor() : base()
        {
            IsTextPredictionEnabled = true;
        }


        // 12-20-23 anc
        // Tab key actually enters a tab char in the editor.
        // Related to issue?: Tab key behaves differently if Entry or Editor is last interactable control on a page on Windows #7746
        public static void Setup()
        {
            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping(nameof(CustomEditor), (handler, view) =>
            {
                if (view is CustomEditor theEditor)
                {
#if __IOS__

                    //UpdatePlaceholder(handler); // still need to do this in .net 8?
                    handler.PlatformView.Layer.BorderColor = UIKit.UIColor.LightGray.CGColor;
                    handler.PlatformView.Layer.BorderWidth = new nfloat(0.5);
                    handler.PlatformView.Layer.CornerRadius = 5;
                    //handler.PlatformView.KeyCommands


                    //UIKit.UIElement? nativeView = handler?.PlatformView.InputView as UIKit.UIElement;
                    //if (nativeView != null)
                    //{

                    //    nativeView.KeyDown += this.PlatformView_KeyDown;
                    //    nativeView.KeyUp += this.PlatformView_KeyUp;
                    //    nativeView.PreviewKeyDown += this.PlatformView_PreviewKeyDown;
                    //}

                    if (theEditor.Keyboard == Keyboard.Numeric)
                    {
                        handler.PlatformView.KeyboardType = UIKit.UIKeyboardType.NumbersAndPunctuation;
                    }

                    if (theEditor.FocusOnDisplay && theEditor.IsVisible && theEditor.IsEnabled && !theEditor.IsQuestion)
                    {

                        // 10-22-24 anc
                        // Scroll to end of text and put cursor at end of text.
                        NSRange range = new NSRange(0, theEditor.Text.Length);
                        // Grab the underlying UITextView object
                        var uiTextView = theEditor.Handler.PlatformView as UIKit.UITextView;
                        uiTextView.ScrollEnabled = true;
                        uiTextView.ScrollRangeToVisible(range);
                        theEditor.CursorPosition = theEditor.Text.Length;

                        // Maybe this will be fixed in a future version of .net maui?  Cursor position is at the beginning of entry/editor #14707
                        handler.PlatformView.BecomeFirstResponder();
                    }
                    if (!theEditor.IsSpellCheckEnabled)
                    {
                        theEditor.Keyboard = Keyboard.Create(KeyboardFlags.None);
                    }

                    // work around for: [iOS] Issue when using BindableLayout and Entry/Editor #15937
                    // also: Issue when using BindableLayout and Entry/Editor with special characters #19954
                    // 3-6-25 anc
                    // Need to revisit this.  Do we still need it?
                    //handler.PlatformView.TextSetOrChanged += (object sender, EventArgs e) =>
                    //{
                    //    //var textEntered = handler.PlatformView.Text;                                                
                    //    //Regex RegexTab = new Regex(@"[^\t]"); //new Regex(@"[^\u0009]");
                    //    //bool found = Regex.IsMatch(textEntered, @"[^\u0009]");
                    //    //if (RegexTab.IsMatch(textEntered))
                    //    //{
                    //    //    lastCharEnteredWasTab = true;
                    //    //}

                    //    var clean = Regex.Replace(handler.PlatformView.Text, @"[^\u0009\u000A\u000D\u0020-\u007E]", string.Empty);
                    //    var maxLength = -1;
                    //    if (handler.VirtualView is Editor editor && editor.MaxLength > 0)
                    //    {                            
                    //        maxLength = editor.MaxLength;
                    //    }
                    //    if (handler.VirtualView is Entry entry && entry.MaxLength > 0)
                    //    {
                    //        maxLength = entry.MaxLength;
                    //    }
                    //    if (maxLength > 0 && clean.Length > maxLength)
                    //    {
                    //        clean = clean.Substring(0, maxLength);
                    //        System.Diagnostics.Debug.WriteLine($"Reduced cleaned string to {maxLength} to prevent crash after sanitized string");
                    //    }
                    //    if (clean != handler.PlatformView.Text)
                    //    {
                    //        handler.PlatformView.Text = clean;
                    //    }
                    //};

                    if (theEditor.CompleteOnEnter)
                    {
                        handler.PlatformView.ReturnKeyType = UIKit.UIReturnKeyType.Done;
                        handler.PlatformView.ShouldChangeText = (text, range, replacementString) =>
                        {
#pragma warning disable CA1307 // Specify StringComparison for clarity
#pragma warning disable CA1309 // Use ordinal string comparison
                            if (replacementString.Equals("\t"))
                            {
                                lastCharEnteredWasTab = true;
                            }
#pragma warning restore CA1309 // Use ordinal string comparison
#pragma warning restore CA1307 // Specify StringComparison for clarity

                            // if Return Key was pressed
#pragma warning disable CA1307 // Specify StringComparison for clarity
#pragma warning disable CA1309 // Use ordinal string comparison
                            if (replacementString.Equals("\n"))
                            {
                                //hide keyboard here
                                //handler.PlatformView.EndEditing(true);

                                if (theEditor.DoCompleteOnEnter != null)
                                {
                                    //perform some action here
                                    if (theEditor.IsForceScan)
                                    {
                                        if (lastCharEnteredWasTab)
                                        {
                                            lastCharEnteredWasTab = false;
                                            theEditor.DoCompleteOnEnter.Execute(null);
                                            return false;
                                        }
                                        else
                                        {
                                            lastCharEnteredWasTab = false;
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        theEditor.DoCompleteOnEnter.Execute(null);
                                        return false;
                                    }
                                }
                                else
                                {
                                    return true; // add carriage return
                                }
                            }
                            else
                            {
                                return true; // add carriage return
                            }
#pragma warning restore CA1309 // Use ordinal string comparison
#pragma warning restore CA1307 // Specify StringComparison for clarity
                        };
                    }
#endif


                }
            });
        }










    }
}
