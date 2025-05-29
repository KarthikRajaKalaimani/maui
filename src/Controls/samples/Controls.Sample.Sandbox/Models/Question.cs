using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Maui.Controls.Sample
{
    public class Question : ObservableObject
    {

        public string QuestionPKey { get; set; }

        public Category Category { get; set; }

        public string CategoryPKey { get; set; }

        public int Category_id { get; set; }

        public int Question_id { get; set; }

        public int Location_id { get; set; }

        public string Locationname { get; set; }

        public bool Hidden { get; set; }

        public bool Ineligible { get; set; }

        public bool Required { get; set; }

        public string Label { get; set; }

        public string UnitsText { get; set; }

        public int Sortorder { get; set; }

        public string Controltype { get; set; }

        public string ValidationRule { get; set; }

        public int TotalAlarms { get; set; }

        private bool _alarmed;
        public bool Alarmed
        {
            get { return _alarmed; }
            set { _alarmed = value; OnPropertyChanged(nameof(Alarmed)); }
        }

        public int Rows { get; set; }

        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }

        private bool _visible;
        public bool Visible
        {
            get
            {
                return !Ineligible;
            }
            set { _visible = value; OnPropertyChanged(nameof(Visible)); }
        }

        private bool _isNumeric;
        public bool IsNumeric
        {
            get
            {
                return _isNumeric;
            }
            set { _isNumeric = value; OnPropertyChanged(nameof(IsNumeric)); }
        }

        public string TextBoxHeight
        {
            get
            {
                Double heightVal = 40;
                Double fontHeightVal = 40;

                if (Rows > 1)
                {
                    heightVal = Rows * fontHeightVal;
                }
                return heightVal.ToString();
            }
        }


        public bool ResponseValid
        {
            get
            {
                if (Ineligible || !Visible)
                {
                    return true;
                }

                if (Required && string.IsNullOrEmpty(Value))
                {
                    return false;
                }
                return true;
            }
        }

        private string _answeredByText;
        public virtual string AnsweredByText
        {
            get { return _answeredByText; }
            set { _answeredByText = value; OnPropertyChanged(nameof(AnsweredByText)); OnPropertyChanged(nameof(AnsweredByHeight)); }
        }

        private int _answeredByHeight;
        public int AnsweredByHeight
        {
            get { return Visible && !string.IsNullOrEmpty(AnsweredByText) ? 40 : 0; }
            set { _answeredByHeight = value; OnPropertyChanged(nameof(AnsweredByHeight)); }
        }

        public ObservableCollection<QuestionOption> Options { get; set; }


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Question()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {

        }
    }


    public class Grouping<K, T> : ObservableCollection<T>
    {
        public K Key { get; set; }

        public Grouping(K key, IEnumerable<T> items)
        {
            Key = key;
            foreach (var item in items)
                this.Items.Add(item);
        }

        public IEnumerable<T> GetItems()
        {
            return this.Items;
        }
        public List<T> GetItemList()
        {
            return this.ToList();
        }
        public void InsertItems(IEnumerable<T> newitems)
        {
            foreach (var newitem in newitems)
                this.Items.Add(newitem);
        }

    }


    public class QuestionGrouping : Grouping<string, Question>
    {
        public QuestionGrouping(string a, IEnumerable<Question> b) : base(a, b)
        {

        }
    }


    public class QuestionUpdateResult
    {

        public string ReturnVal { get; set; }
        public string AlarmMsg { get; set; }
        public Question? QuestionObj { get; set; }
        public List<Question> NewAlarmedQuestions { get; set; }
        public List<Question> ClearAlarmedQuestions { get; set; }
        public bool InvalidResponse { get; set; }
        public bool ChangedDescAlarms { get; set; }
        public List<Question> ChangedDescAlarmQuestions { get; set; }

        public QuestionUpdateResult()
        {
            ReturnVal = "0";
            AlarmMsg = "";
            QuestionObj = null;
            NewAlarmedQuestions = new List<Question>();
            ClearAlarmedQuestions = new List<Question>();
            InvalidResponse = false;
            ChangedDescAlarms = false;
            ChangedDescAlarmQuestions = new List<Question>();
        }

    }

}
