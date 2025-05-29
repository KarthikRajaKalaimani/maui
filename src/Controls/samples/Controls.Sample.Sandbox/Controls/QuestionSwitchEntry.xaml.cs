#nullable disable
using System.Diagnostics;
using System.Reflection;

namespace Maui.Controls.Sample
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QuestionSwitchEntry : BaseQuestionEntryTemplate
    {

        public static readonly BindableProperty ValueProperty = BindableProperty.Create("Value", typeof(string), typeof(QuestionSwitchEntry), string.Empty, BindingMode.OneWay, propertyChanged: OnValuePropertyChanged);
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (newValue != oldValue)
            {
                var control = (QuestionSwitchEntry)bindable;
                control.Value = newValue == null ? "" : newValue.ToString();
            }
        }

        public bool ToggledIsBusy = false;


        public QuestionSwitchEntry()
        {
            InitializeComponent();
        }

        public string GetNewQuestionValue(bool toggledOn, string theOptionValue)
        {
            string result = "";

            if (toggledOn)
            {
                result = theOptionValue;
            }

            return result;
        }


        private async void OnSwitchToggled(object sender, ToggledEventArgs e)
        {

            // Known bug: XAML Switch Toggled event behaviour OnAppearing and Toggled not making sense. #9099
            // When a true switch is set to false, Toggled event is fired once.
            // When a false switch is set to true Toggled event is fired twice (once for the change and then once again for each item in the list as the form is redrawn).

            if (ToggledIsBusy || BindingContext is null || Parent is null || !(sender is Microsoft.Maui.Controls.Switch thisSwitch))
            {
                return;
            }
            if (!(thisSwitch.BindingContext is QuestionOption))
            {
                return;
            }

            try
            {
                var question = (Question)BindingContext;
                var questionOption = (QuestionOption)thisSwitch.BindingContext;
                var newVal = GetNewQuestionValue(e.Value, questionOption.Value);
#pragma warning disable IDE0029 // Use coalesce expression
                var oldVal = Value == null ? "" : Value;
#pragma warning restore IDE0029 // Use coalesce expression

                if (newVal != oldVal)
                {

                    // 6-26-20 anc
                    // Setting Value here instead of after getting result.ReturnVal due to bug #9099 (mentioned above)                
                    Value = newVal.ToString();

                    // If a switch was turned on (and a different switch is already turned on), turn off any other in the group.
                    if (e.Value && (oldVal != "" && oldVal != null))
                    {
                        bool stopLoop = false;
                        List<IView> GridList = SwitchGroup.Children.Where(x => x is Grid).ToList();
                        foreach (Grid gridObj in GridList.Cast<Grid>())
                        {
                            if (stopLoop)
                            {
                                break;
                            }
                            foreach (Microsoft.Maui.Controls.Switch switchObj in gridObj.Children.OfType<Microsoft.Maui.Controls.Switch>())
                            {
                                if (switchObj != thisSwitch && switchObj.IsToggled)
                                {
                                    ToggledIsBusy = true;
                                    //switchObj.Toggled -= OnSwitchToggled;
                                    switchObj.IsToggled = false;
                                    ToggledIsBusy = false;
                                    //switchObj.Toggled += OnSwitchToggled;

                                    stopLoop = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (ParentBindingContext != null)
                    {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                        PropertyInfo isBusyProperty = ParentBindingContext.GetType().GetProperty("IsBusy");
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                        if (isBusyProperty != null)
                        {
                            if (!(bool)isBusyProperty.GetValue(ParentBindingContext))
                            {
                                await SaveValue(question, newVal);
                                //Dispatcher.DispatchAsync(async () => await SaveValue(question, newVal));
                                //Task.Run(async() => await SaveValue(question, newVal));
                            }
                            else
                            {
                                //Utils.WriteErrLog(LogLevel.Error, null, "In OnSwitchToggled(), TaskPMPage is busy, q = " + question.Label, false);

                                Value = oldVal;
                                ToggledIsBusy = true;
                                //thisSwitch.Toggled -= OnSwitchToggled;
                                thisSwitch.IsToggled = !e.Value;
                                ToggledIsBusy = false;
                                //thisSwitch.Toggled += OnSwitchToggled;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public async Task SaveValue(object theQuestionObj, object newVal)
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
                    var task = (Task<QuestionUpdateResult>)theSaveMethod.Invoke(ParentBindingContext, new object[] { theQuestionObj, newVal });
                    QuestionUpdateResult result = await task;

                    // 3-3-25 anc
                    // Added some extra checks on the state of the awaited task.  
                    // I have never hit any of them, so these might not be useful but I will leave for now.
                    if (task.IsFaulted)
                    {
                        Debug.WriteLine("QuestionSwitchEntry task.IsFaulted");
                    }
                    else if (!task.IsCompleted)
                    {
                        Debug.WriteLine("QuestionSwitchEntry !task.IsCompleted");
                    }
                    else
                    {
                        if (result.ReturnVal == "1")
                        {
                            if (!string.IsNullOrEmpty(result.AlarmMsg))
                            {
                                MethodInfo theAlarmPromptMethod = theParentBindingContext.GetType().GetMethod("HandleAlarmPrompt");
                                theAlarmPromptMethod.Invoke(theParentBindingContext, new object[] { result });

                                // 2-17-25 anc
                                // Invoking HandleAlarmPrompt inside of a Task.Run() was causing the app to crash with the following error: **System.ArgumentException:** 'An item with the same key has already been added. Key: Microsoft.Maui.Controls.BindableProperty'
                                // It was happening if you kept answering Btn2State questions with a response in alarm. It was random in terms of how long it took to happen.  Sometimes after 3 questions, somtimes after 30 questions.
                                // Removing this and just invoking the method, like the other Question controls, stopped that from happening.
                                //await Task.Run(() => {
                                //    theAlarmPromptMethod.Invoke(ParentBindingContext, new object[] { result });
                                //});                            
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
                finally
                {
                    theParentBindingContext = null;
                }
            }
        }



    }


}