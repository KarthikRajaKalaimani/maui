#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.Controls.Sample
{
    public class QuestionOption : ObservableObject
    {
        public string QuestionOptionPKey { get; set; }

        public Question Question { get; set; }

        public string QuestionPKey { get; set; }

        public string Text { get; set; }

        public string Value { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get
            {
                bool isSel = false;
                string val = null;
                string controlType = "";

                if (Question != null)
                {
                    val = Question.Value;
                    controlType = Question.Controltype;
                }
                if (val != null)
                {
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
#pragma warning disable CA1311 // Specify a culture or use an invariant version
                    if (val.ToLower() == Value.ToLower())
                    {
                        isSel = true;
                    }
#pragma warning restore CA1311 // Specify a culture or use an invariant version
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
                }
                return isSel;
            }

            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }

        }
    }
}
