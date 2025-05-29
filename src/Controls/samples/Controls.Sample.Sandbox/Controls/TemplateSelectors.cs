#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.ComponentModel;
using System.Reflection;

namespace Maui.Controls.Sample
{

    public class BaseQuestionEntryTemplate : StackLayout, IDisposable
    {
        public static BindableProperty ParentBindingContextProperty = BindableProperty.Create(nameof(ParentBindingContext), typeof(object), typeof(BaseQuestionEntryTemplate), null);

        public object ParentBindingContext
        {
            get { return GetValue(ParentBindingContextProperty); }
            set { SetValue(ParentBindingContextProperty, value); }
        }

        public static readonly BindableProperty ContextMenuTapCommandProperty = BindableProperty.Create("ContextMenuTapCommand", typeof(ICommand), typeof(BaseQuestionEntryTemplate), null);

        public ICommand ContextMenuTapCommand
        {
            get { return (ICommand)GetValue(ContextMenuTapCommandProperty); }
            set { SetValue(ContextMenuTapCommandProperty, value); }
        }

        public Func<object, bool> AllowPopup => ShouldAllowPopup;

        public bool ShouldAllowPopup(object arg)
        {
            // Add logic to determine if the UI should be disabled 
            // while executing a task that is observing this action.  
            return CanAllowPopup;
        }

        private bool canAllowPopup = true;
        public bool CanAllowPopup
        {
            get { return canAllowPopup; }
            set
            {
                canAllowPopup = value;
                OnPropertyChanged(nameof(CanAllowPopup));
                RefreshCommandCanExecutes();
            }
        }

        protected virtual void RefreshCommandCanExecutes()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ((Command)ContextMenuTapCommand).ChangeCanExecute();
            });
        }

        protected async Task OnContextMenuTapCommand(object sender)
        {
            if (!CanAllowPopup)
            {
                return;
            }

            CanAllowPopup = false;

            await OpenContextMenu(sender);

            CanAllowPopup = true;
        }

        protected virtual async Task OpenContextMenu(object sender)
        {
            await Task.Delay(0);
        }

        public BaseQuestionEntryTemplate() : base()
        {
            ContextMenuTapCommand = new Command(async (x) => await OnContextMenuTapCommand(x), ShouldAllowPopup);

        }



        public void Dispose()
        {
            ContextMenuTapCommand = null;
            ParentBindingContext = null;

            if (this is QuestionTextEntry questTextEntry)
            {
                questTextEntry.LastValueTapCommand = null;
            }
        }
    }

    public class QuestionEntryTemplateSelector : DataTemplateSelector
    {

        public DataTemplate QuestionTextTemplate { get; set; }
        public DataTemplate QuestionSwitchTemplate { get; set; }
        public DataTemplate BlankTemplate { get; set; }


        public QuestionEntryTemplateSelector()
        {
            //Retain instances!
            QuestionTextTemplate = new DataTemplate(typeof(QuestionTextEntry));
            QuestionSwitchTemplate = new DataTemplate(typeof(QuestionSwitchEntry));
            BlankTemplate = new DataTemplate(typeof(QuestionHiddenEntry));
        }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {

            if (item == null)
            {
                return BlankTemplate;
            }

            Type theItemType = item.GetType();
            PropertyInfo ControltypeProperty = theItemType.GetProperty("Controltype");
            if (ControltypeProperty is null)
            {
                return BlankTemplate;
            }
            PropertyInfo VisibleProperty = theItemType.GetProperty("Visible");
            if (VisibleProperty != null)
            {
                if (VisibleProperty.GetValue(item) is bool visibleValue)
                {
                    if (!visibleValue)
                    {
                        return BlankTemplate;
                    }
                }
            }


            switch (ControltypeProperty.GetValue(item))
            {
                case "text":
                case "textbox":
                    QuestionTextTemplate.SetValue(BaseQuestionEntryTemplate.ParentBindingContextProperty, container.BindingContext);
                    return QuestionTextTemplate;
                case "switch":
                    QuestionSwitchTemplate.SetValue(BaseQuestionEntryTemplate.ParentBindingContextProperty, container.BindingContext);
                    return QuestionSwitchTemplate;
                default:
                    // text                    
                    QuestionTextTemplate.SetValue(BaseQuestionEntryTemplate.ParentBindingContextProperty, container.BindingContext);
                    return QuestionTextTemplate;
            }





        }

    }
}

