#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Maui.Controls.Sample
{
    public class PopupTextInputViewModel : ObservableObject
    {

        private string textValue;
        public string TextValue
        {
            get { return textValue; }
            set
            {
                //if (textValue == value) return;
                textValue = value;
                OnPropertyChanged(nameof(TextValue));
            }
        }

        private int _inputHeight;
        public int InputHeight
        {
            get { return _inputHeight; }
            set
            {
                _inputHeight = value;
                OnPropertyChanged(nameof(InputHeight));
            }
        }

        private string labelText;
        public string LabelText
        {
            get { return labelText; }
            set
            {
                labelText = value;
                OnPropertyChanged(nameof(LabelText));
            }
        }

        private string keyboardType;
        public string KeyboardType
        {
            get { return keyboardType; }
            set
            {
                keyboardType = value;
                OnPropertyChanged(nameof(KeyboardType));
            }
        }

        private string errorMsgText;
        public string ErrorMsgText
        {
            get { return errorMsgText; }
            set
            {
                errorMsgText = value;
                OnPropertyChanged(nameof(ErrorMsgText));

                if (string.IsNullOrEmpty(ErrorMsgText))
                {
                    EnableDoneButton = true;
                }
                else
                {
                    EnableDoneButton = false;
                }

            }
        }


        private bool enableDoneButton;
        public bool EnableDoneButton
        {
            get { return enableDoneButton; }
            set
            {
                enableDoneButton = value;
                OnPropertyChanged(nameof(EnableDoneButton));
            }
        }

        private object inputParentBindingContext;
        public object InputParentBindingContext
        {
            get { return inputParentBindingContext; }
            set
            {
                inputParentBindingContext = value;
                OnPropertyChanged(nameof(InputParentBindingContext));
            }
        }

        private Func<object, bool> allowCompleteOnEnter;
        public Func<object, bool> AllowCompleteOnEnter
        {
            get { return allowCompleteOnEnter; }
            set
            {
                allowCompleteOnEnter = value;
                OnPropertyChanged(nameof(AllowCompleteOnEnter));
            }
        }

        private DelegateCommand doCompleteOnEnter;
        public DelegateCommand DoCompleteOnEnter
        {
            get { return doCompleteOnEnter; }
            set
            {
                doCompleteOnEnter = value;
                OnPropertyChanged(nameof(DoCompleteOnEnter));
            }
        }

        private bool isNumeric;
        public bool IsNumeric
        {
            get { return isNumeric; }
            set
            {
                isNumeric = value;
                OnPropertyChanged(nameof(IsNumeric));
            }
        }

        private string editorHeight;
        public string EditorHeight
        {
            get { return editorHeight; }
            set
            {
                editorHeight = value;
                OnPropertyChanged(nameof(EditorHeight));
            }
        }

        public PopupTextInputViewModel(string TheLabelText, string TheTextValue, object TheDataObject, Action<string> OnClickDone)
        {
            LabelText = TheLabelText;
            TextValue = TheTextValue;
            AllowCompleteOnEnter = (x) => AllowSaveResponse(null);
            DoCompleteOnEnter = new DelegateCommand(ClickDoneBtn);
            KeyboardType = "";

            ClickDoneBtnEvent += (object sender, EventArgs e) =>
            {
                try
                {
                    OnClickDone(TextValue);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            };

            if (TheDataObject is Question q)
            {
                KeyboardType = q.ValidationRule;
                IsNumeric = q.IsNumeric;
                EditorHeight = q.TextBoxHeight;
            }


            //int inputHeight = 200;
            //if (TheDataObject is Question q)
            //{
            //    if (q.IsNumeric)
            //    {
            //        inputHeight = 40;
            //    }
            //}

            //InputHeight = inputHeight;
            InputParentBindingContext = TheDataObject;

            //--Task.Run(async() => { await ValidateData(); });
            ValidateData();
        }




        private ICommand validationCommand;
        public ICommand ValidationCommand
        {
            get { return validationCommand ?? (validationCommand = new Command((x) => ValidateData())); }
        }

        private ICommand clickDoneBtnCommand;
        public ICommand ClickDoneBtnCommand
        {
            get { return clickDoneBtnCommand ?? (clickDoneBtnCommand = new Command(async (x) => await ClickDoneBtn(), AllowSaveResponse)); }
        }

        public event EventHandler<EventArgs> ClickDoneBtnEvent;

        public async Task ClickDoneBtn()
        {
            try
            {
                if (EnableDoneButton)
                {
                    // 1-29-25 anc
                    // Extra validation on numeric input fields that allow negative and/or decimal to make sure it also contains at least 1 number.
                    if (IsNumeric)
                    {
                        if (TextValue == "-" || TextValue == "." || TextValue == "-.")
                        {
                            ErrorMsgText = "Please enter a valid integer.";
                            return;
                        }
                    }

                    // create a continuation point so every following statement will get executed as ContinueWith
                    await Task.FromResult(0);

                    // allow validation to finish
                    //--await Task.Delay(200);  

                    // 2-28-25 anc 
                    // TODO:  Check if we still need this for Windows
                    //if (Microsoft.Maui.Devices.DeviceInfo.Platform == DevicePlatform.WinUI)
                    //{
                    //    await Task.Delay(200);
                    //}

                    ClickDoneBtnEvent(this, new EventArgs());

                    //await MainThread.InvokeOnMainThreadAsync(() => ClickDoneBtnEvent(this, new EventArgs()));

                    // close popup
                    // if (Mopups.Services.MopupService.Instance.PopupStack.Count > 0 && (Mopups.Services.MopupService.Instance.PopupStack[(Mopups.Services.MopupService.Instance.PopupStack.Count - 1)].GetType() == typeof(PopupTextInputPage)))
                    // {
                    //     await Mopups.Services.MopupService.Instance.PopAsync(false);
                    // }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        protected void RefreshCommandCanExecutes()
        {
            ((Command)ClickDoneBtnCommand).ChangeCanExecute();
        }

        public Func<object, bool> AllowSaveResponse => ShouldAllowSaveResponse;

        bool ShouldAllowSaveResponse(object arg)
        {
            // Add logic to determine if the UI should be disabled 
            // while executing a task that is observing this action.  
            return EnableDoneButton;
        }

        public void ValidateData()
        {
            if (string.IsNullOrWhiteSpace(TextValue))
            {
                ErrorMsgText = "";
            }
            else
            {
                if (InputParentBindingContext is Question q)
                {
                    Utils.TextValidationResult res = Utils.ValidateQuestionTextInput(TextValue, q);
                    TextValue = res.NewTextValue;
                    ErrorMsgText = res.ErrorMessage;
                }
            }
        }

    }
}
