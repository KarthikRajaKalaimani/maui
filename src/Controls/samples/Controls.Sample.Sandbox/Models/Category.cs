using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.Controls.Sample
{
    public class Category : ObservableObject
    {

        public string CategoryPKey { get; set; }

        public string Name { get; set; }

        public ObservableCollection<Question> Questions { get; set; }


        private int _totalEligibleQuestions;
        public int TotalEligibleQuestions
        {
            get
            {
                if (Questions != null)
                {
                    //return Questions.Where(q => !q.Ineligible && !q.Hidden && q.Controltype != "labelonly").Count();
                    return Questions.Where(q => !q.Ineligible && q.Controltype != "labelonly" && q.Controltype != "postcalc").Count();
                }
                else
                {
                    return 0;
                }
            }
            set { _totalEligibleQuestions = value; OnPropertyChanged(nameof(TotalEligibleQuestions)); }

        }




        private int _totalAnsweredQuestions;
        public int TotalAnsweredQuestions
        {
            get
            {
                if (Questions != null)
                {
                    //return Questions.Count(q => q.Value != null && q.Value != "" && !q.Ineligible && !q.Hidden && q.Controltype != "labelonly");
                    return Questions.Count(q => q.Value != null && q.Value != "" && !q.Ineligible && q.Controltype != "labelonly" && q.Controltype != "postcalc");
                }
                else
                {
                    return 0;
                }
            }
            set { _totalAnsweredQuestions = value; OnPropertyChanged(nameof(TotalAnsweredQuestions)); }
        }



#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Category()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {

        }
    }
}
