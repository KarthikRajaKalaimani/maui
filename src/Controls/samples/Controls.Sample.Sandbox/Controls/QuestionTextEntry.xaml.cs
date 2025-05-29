#nullable disable
using System.Diagnostics;
using System.Windows.Input;
using System.Reflection;

namespace Maui.Controls.Sample
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QuestionTextEntry : BaseQuestionEntryTemplate
    {
        private bool isLoaded = false;


        // Value
        public static readonly BindableProperty ValueProperty = BindableProperty.Create("Value", typeof(string), typeof(QuestionTextEntry), string.Empty, BindingMode.OneWay, propertyChanged: OnValuePropertyChanged);
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {

            if (oldValue != newValue)
            {
                if (newValue is null)
                {
                    newValue = string.Empty;
                }
                var control = (QuestionTextEntry)bindable;
                control.Value = newValue.ToString();
                if (!control.isLoaded)
                {
                    control.isLoaded = true;
                }
            }

        }

        // In Error
        public static readonly BindableProperty InErrorProperty = BindableProperty.Create("InError", typeof(bool), typeof(QuestionTextEntry), false, BindingMode.Default, propertyChanged: OnInErrorPropertyChanged);
        public bool InError
        {
            get { return (bool)GetValue(InErrorProperty); }
            set { SetValue(InErrorProperty, value); }
        }

        private static void OnInErrorPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (QuestionTextEntry)bindable;
            control.InError = (bool)newValue;
        }

        // Error Message
        public static readonly BindableProperty ErrorMessageProperty = BindableProperty.Create("ErrorMessage", typeof(string), typeof(QuestionTextEntry), string.Empty, BindingMode.Default, propertyChanged: OnErrorMessagePropertyChanged);
        public string ErrorMessage
        {
            get { return (string)GetValue(ErrorMessageProperty); }
            set { SetValue(ErrorMessageProperty, value); }
        }

        private static void OnErrorMessagePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (QuestionTextEntry)bindable;
            control.ErrorMessage = newValue.ToString();
        }

        public static readonly BindableProperty TapCommandProperty = BindableProperty.Create("TapCommand", typeof(ICommand), typeof(QuestionTextEntry), null, BindingMode.OneTime, propertyChanged: OnTapCommandPropertyChanged);
        public ICommand TapCommand
        {
            get { return (ICommand)GetValue(TapCommandProperty); }
            set { SetValue(TapCommandProperty, value); }
        }

        private static void OnTapCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (QuestionTextEntry)bindable;
            control.TapCommand = (ICommand)newValue;
        }

        public static readonly BindableProperty LastValueTapCommandProperty = BindableProperty.Create("LastValueTapCommand", typeof(ICommand), typeof(QuestionTextEntry), null, BindingMode.OneTime, propertyChanged: OnLastValueTapCommandPropertyChanged);
        public ICommand LastValueTapCommand
        {
            get { return (ICommand)GetValue(LastValueTapCommandProperty); }
            set { SetValue(LastValueTapCommandProperty, value); }
        }

        private static void OnLastValueTapCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != oldValue)
            {
                var control = (QuestionTextEntry)bindable;
                control.LastValueTapCommand = (ICommand)newValue;
            }
        }

        public bool CanClick = true;

        public Point LastTouchPoint { get; set; }

        public bool TextChangedIsBusy = false;



        public QuestionTextEntry()
        {
            ErrorMessage = "";
            InitializeComponent();
            TapCommand = new Command((x) => OnEntryTappedCommand(x));

        }

        public string GetNewQuestionValue(string entryText)
        {
            var result = entryText;

            if (entryText.Trim() == "")
            {
                result = "";
            }
            return result;
        }

        public async Task SaveValue(CustomEditor TheEntry, object newVal)
        {

            MethodInfo theSaveMethod = ParentBindingContext.GetType().GetMethod("OnQuestionChanged");
            if (theSaveMethod != null)
            {
                // 2-18-25 anc
                // Need to save ParentBindingContext to a local variable in case a question has descendants AND is in alarm.
                // After the response is saved, AnalyzingDescendants will run which will call TearDownBehavior on the question list which removes the ParentBindingContext.
                // So, by the time we get back here to call HandleAlarmPrompt(), ParentBindingContext will be null.
                object theParentBindingContext = ParentBindingContext;
                try
                {
                    TextChangedIsBusy = true;
                    object saveVal = GetNewQuestionValue(newVal.ToString());
                    var task = (Task<QuestionUpdateResult>)theSaveMethod.Invoke(ParentBindingContext, new object[] { BindingContext, saveVal });
                    QuestionUpdateResult result = await task;

                    if (result.ReturnVal == "1")
                    {
                        Value = newVal.ToString();
                        if (result.QuestionObj.ResponseValid)
                        {
                            InError = false;
                            ErrorMessage = "";
                        }
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                finally
                {
                    TextChangedIsBusy = false;
                    theParentBindingContext = null;
                }
            }

        }


        public void OnEntryTappedCommand(object sender)
        {
            try
            {
                if (CanClick)
                {
                    CanClick = false;

                    var TheEntry = (CustomEditor)sender;

                    if (TheEntry.BindingContext is Question q)
                    {
                        Utils.ShowTextInput(q.Label, Value, q, async (changedVal) =>
                        {
                            string theNewValue = changedVal.Trim();
                            if (Value != theNewValue)
                            {
                                await SaveValue(TheEntry, theNewValue);
                            }

                            CanClick = true;
                        });
                    }
                    else
                    {
                        CanClick = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }


        // Not used
        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextChangedIsBusy)
            {
                return;
            }

            if (this.BindingContext is Question q)
            {
                if (e.OldTextValue == null && e.NewTextValue == null)
                {
                    return;
                }
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
#pragma warning disable CA1311 // Specify a culture or use an invariant version
                if (e.OldTextValue == null || e.NewTextValue == null || e.OldTextValue.ToLower() != e.NewTextValue.ToLower())
                {
                    // do validation
                    Utils.TextValidationResult res = Utils.ValidateQuestionTextInput(e.NewTextValue, q);
                    InError = res.InError;
                    Value = res.NewTextValue;
                    ErrorMessage = res.ErrorMessage;
                }
#pragma warning restore CA1311 // Specify a culture or use an invariant version
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
            }

        }






    }
}
