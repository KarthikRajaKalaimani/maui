#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Maui.Controls.Sample.Behaviors
{
    public class QuestionInputValidation : BehaviorBase<CustomEditor>
    {
        public const int DefaultMinimumTypingIntervalMiliseconds = 300;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public static readonly BindableProperty ValidateCommandProperty =
            BindableProperty.Create(nameof(ValidateCommand), typeof(ICommand), typeof(QuestionInputValidation),
                propertyChanged: ValidateCommandChanged);

        public static readonly BindableProperty MinimumTypingIntervalMilisecondsProperty =
            BindableProperty.Create(nameof(MinimumTypingIntervalMiliseconds), typeof(int),
                typeof(QuestionInputValidation), DefaultMinimumTypingIntervalMiliseconds);

        public ICommand ValidateCommand
        {
            get => (ICommand)GetValue(ValidateCommandProperty);
            set => SetValue(ValidateCommandProperty, value);
        }

        public int MinimumTypingIntervalMiliseconds
        {
            get => (int)GetValue(MinimumTypingIntervalMilisecondsProperty);
            set => SetValue(MinimumTypingIntervalMilisecondsProperty, value);
        }

        protected override void OnDetachingFrom(CustomEditor bindable)
        {
            base.OnDetachingFrom(bindable);
            if (AssociatedObject is null)
            {
                return;
            }
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            AssociatedObject.TextChanged -= Validate;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
        }

        private static void ValidateCommandChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var behavior = (QuestionInputValidation)bindable;
            behavior.ValidateCommandChanged(newValue);
        }

        private void ValidateCommandChanged(object newCommand)
        {
            if (newCommand is ICommand)
            {
                AssociatedObject.TextChanged += Validate;
            }
            else
            {
                AssociatedObject.TextChanged -= Validate;
            }
        }

        private void Validate(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            try
            {
                Interlocked.Exchange(ref _cancellationTokenSource, new CancellationTokenSource()).Cancel();
                Task.Delay(MinimumTypingIntervalMiliseconds, _cancellationTokenSource.Token)
                .ContinueWith(
                    delegate
                    { ExecuteValidate(); },
                    CancellationToken.None,
                    TaskContinuationOptions.OnlyOnRanToCompletion,
                    TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (OperationCanceledException)
            {
                // swallow
                //Debug.WriteLine("OperationCanceledException, message = " + ex.Message);
            }
        }

        private void ExecuteValidate()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {

                ValidateCommand?.Execute(null);

                if (!AssociatedObject.IsFocused)
                    AssociatedObject.Focus();
            });
        }
    }
}
