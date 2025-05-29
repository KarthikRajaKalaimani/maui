#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Maui.Controls.Sample
{
    public class MainPageViewModel : ObservableObject
    {
        private bool _isLoaded;
        public bool IsLoaded
        {
            get { return _isLoaded; }
            set
            {
                _isLoaded = value;
                OnPropertyChanged(nameof(IsLoaded));
            }
        }

        private ObservableCollectionFast<Category> categories;
        public ObservableCollectionFast<Category> Categories
        {
            get { return categories; }
            set
            {
                categories = value;
                OnPropertyChanged(nameof(Category));

            }
        }

        private bool enableCategories;
        public bool EnableCategories
        {
            get { return enableCategories; }
            set
            {
                enableCategories = value;
                OnPropertyChanged(nameof(EnableCategories));
            }
        }

        private Category selectedCategory;
        public Category SelectedCategory
        {
            get { return selectedCategory; }
            set
            {
                if (value != SelectedCategory)
                {
                    selectedCategory = value;
                    OnPropertyChanged(nameof(SelectedCategory));
                }
            }
        }



        private ObservableCollectionFast<QuestionGrouping> questionsGrouped;
        public ObservableCollectionFast<QuestionGrouping> QuestionsGrouped
        {
            get { return questionsGrouped; }
            set
            {
                questionsGrouped = value;
                OnPropertyChanged(nameof(QuestionsGrouped));
            }
        }

        private bool questionsVisible;
        public bool QuestionsVisible
        {
            get { return questionsVisible; }
            set
            {
                questionsVisible = value;
                OnPropertyChanged(nameof(QuestionsVisible));
            }
        }

        private Question selectedQuestion;
        public Question SelectedQuestion
        {
            get { return selectedQuestion; }
            set
            {
                if (value != SelectedQuestion)
                {
                    selectedQuestion = value;
                    OnPropertyChanged(nameof(SelectedQuestion));
                }
            }
        }

        public string SelectedCategoryPKey { get; set; }

        private ICommand _clickCommandButton;
        public ICommand ClickCommandButton
        {
            get { return _clickCommandButton ?? (_clickCommandButton = new Command(async (x) => await OnClicked(x))); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }

        ObservableCollection<Question> tempQuestions = new ObservableCollection<Question>();
        List<QuestionGrouping> tempQuestionsGrouped = new List<QuestionGrouping>();
        IEnumerable<QuestionGrouping> sortedQuestions;
        List<Category> tempCategories = new List<Category>();
        private string LastSelectedCategoryPKey = "";
        private bool ForceQuestionReload = false;
        private bool LoadingQuestions = false;


        public MainPageViewModel()
        {
            Categories = new ObservableCollectionFast<Category>();
            QuestionsGrouped = new ObservableCollectionFast<QuestionGrouping>();
            EnableCategories = false;
            QuestionsVisible = true;
        }

        public async void OnAppearing()
        {

            await Task.Run(async () =>
            {
                await LoadData();
            });

            Categories.InsertRange(0, tempCategories);

            SelectedCategory = Categories.FirstOrDefault();
            if (SelectedCategory != null)
            {
                SelectedCategoryPKey = SelectedCategory.CategoryPKey;
                tempQuestions = SelectedCategory.Questions;

                sortedQuestions = from item in tempQuestions
                                  orderby item.Sortorder
                                  group item by new { item.Location_id, item.Locationname, item.Sortorder } into questGroup
                                  select new QuestionGrouping(questGroup.Key.Locationname, questGroup);

                string lastLoc = "";
                int idx = 0;

                try
                {
                    foreach (QuestionGrouping q in sortedQuestions)
                    {
                        if (lastLoc != q.Key)
                        {
                            tempQuestionsGrouped.Add(q);
                            idx++;
                        }
                        else
                        {
                            tempQuestionsGrouped[idx - 1].InsertItems(q.GetItemList());
                        }
                        lastLoc = q.Key;
                    }

                    QuestionsGrouped.AddRange(tempQuestionsGrouped);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }

            }
            else
            {
                SelectedCategoryPKey = "0";
                ForceQuestionReload = true;
            }

            IsLoaded = true;
            EnableCategories = true;
        }


#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task LoadData()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            try
            {
                tempCategories.Clear();

                Category cat1 = new Category();
                cat1.CategoryPKey = "C1";
                cat1.Name = "Facts";
                ObservableCollection<Question> questList1 = new ObservableCollection<Question>();
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q1", Label = "Enter name of current month", Location_id = 1, Locationname = "Group 1", Sortorder = 1 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q2", Label = "Enter name of current weekday", Location_id = 1, Locationname = "Group 1", Sortorder = 2 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q3", Label = "Enter current year", Location_id = 1, Locationname = "Group 1", Sortorder = 3, IsNumeric = true });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q4", Label = "Enter current hour", Location_id = 1, Locationname = "Group 1", Sortorder = 4, IsNumeric = true });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q5", Label = "Enter current minute", Location_id = 1, Locationname = "Group 1", Sortorder = 5, IsNumeric = true });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q6", Label = "Enter day or night", Location_id = 2, Locationname = "Group 2", Sortorder = 6 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q7", Label = "Enter name of city", Location_id = 2, Locationname = "Group 2", Sortorder = 7 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q8", Label = "Enter name of state", Location_id = 2, Locationname = "Group 2", Sortorder = 8 });

                Question SwitchQuestionC1Q9 = new Question { CategoryPKey = "C1", Category = cat1, Controltype = "switch", QuestionPKey = "C1Q9", Label = "Facts Switch 1 Yes or No", Location_id = 2, Locationname = "Group 2", Sortorder = 9 };
                ObservableCollection<QuestionOption> questOptListC1Q9 = new ObservableCollection<QuestionOption>();
                QuestionOption YesOptionC1Q9 = new QuestionOption { Question = SwitchQuestionC1Q9, QuestionPKey = SwitchQuestionC1Q9.QuestionPKey, QuestionOptionPKey = "C1Q9YES", Text = "Yes", Value = "Yes" };
                QuestionOption NoOptionC1Q9 = new QuestionOption { Question = SwitchQuestionC1Q9, QuestionPKey = SwitchQuestionC1Q9.QuestionPKey, QuestionOptionPKey = "C1Q9NO", Text = "No", Value = "No" };
                questOptListC1Q9.Add(YesOptionC1Q9);
                questOptListC1Q9.Add(NoOptionC1Q9);
                SwitchQuestionC1Q9.Options = questOptListC1Q9;
                questList1.Add(SwitchQuestionC1Q9);

                Question SwitchQuestionC1Q10 = new Question { CategoryPKey = "C1", Category = cat1, Controltype = "switch", QuestionPKey = "C1Q10", Label = "Facts Switch 2 Yes or No", Location_id = 2, Locationname = "Group 2", Sortorder = 10 };
                ObservableCollection<QuestionOption> questOptListC1Q10 = new ObservableCollection<QuestionOption>();
                QuestionOption YesOptionC1Q10 = new QuestionOption { Question = SwitchQuestionC1Q10, QuestionPKey = SwitchQuestionC1Q10.QuestionPKey, QuestionOptionPKey = "C1Q10YES", Text = "Yes", Value = "Yes" };
                QuestionOption NoOptionC1Q10 = new QuestionOption { Question = SwitchQuestionC1Q10, QuestionPKey = SwitchQuestionC1Q10.QuestionPKey, QuestionOptionPKey = "C1Q10NO", Text = "No", Value = "No" };
                questOptListC1Q10.Add(YesOptionC1Q10);
                questOptListC1Q10.Add(NoOptionC1Q10);
                SwitchQuestionC1Q10.Options = questOptListC1Q10;
                questList1.Add(SwitchQuestionC1Q10);

                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q11", Label = "Facts Text Question 11 Really Long Text Facts Text Question 11 Really Long Text Facts Text Question 11 Really Long Text Facts Text Question 11 Test Test Test Test Test", Location_id = 2, Locationname = "Group 2", Sortorder = 11 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q12", Label = "Facts Text Question 12 Kind of Long Text Facts Text Question 12 Kind of Long Text Facts Text Question 12", Location_id = 2, Locationname = "Group 2", Sortorder = 12, IsNumeric = true });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q13", Label = "Facts Text Question 13", Location_id = 2, Locationname = "Group 2", Sortorder = 13 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q14", Label = "Facts Text Question 14", Location_id = 2, Locationname = "Group 2", Sortorder = 14 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q15", Label = "Facts Text Question 15", Location_id = 2, Locationname = "Group 2", Sortorder = 15 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q16", Label = "Facts Text Question 16", Location_id = 2, Locationname = "Group 2", Sortorder = 16 });
                questList1.Add(new Question { CategoryPKey = "C2", Category = cat1, Controltype = "text", QuestionPKey = "C1Q17", Label = "Facts Text Question 17", Location_id = 2, Locationname = "Group 2", Sortorder = 17 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q18", Label = "Facts Text Question 18", Location_id = 2, Locationname = "Group 2", Sortorder = 18 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q19", Label = "Facts Text Question 19", Location_id = 2, Locationname = "Group 2", Sortorder = 19 });

                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q20", Label = "Facts Text Question 20", Location_id = 2, Locationname = "Group 2", Sortorder = 20 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q21", Label = "Facts Text Question 21", Location_id = 2, Locationname = "Group 2", Sortorder = 21 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q22", Label = "Facts Text Question 22", Location_id = 2, Locationname = "Group 2", Sortorder = 22, IsNumeric = true });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q23", Label = "Facts Text Question 23", Location_id = 2, Locationname = "Group 2", Sortorder = 23 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q24", Label = "Facts Text Question 24", Location_id = 2, Locationname = "Group 2", Sortorder = 24 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q25", Label = "Facts Text Question 25", Location_id = 2, Locationname = "Group 2", Sortorder = 25 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q26", Label = "Facts Text Question 26", Location_id = 2, Locationname = "Group 2", Sortorder = 26 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q27", Label = "Facts Text Question 27", Location_id = 2, Locationname = "Group 2", Sortorder = 27 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q28", Label = "Facts Text Question 28", Location_id = 2, Locationname = "Group 2", Sortorder = 28 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q29", Label = "Facts Text Question 29", Location_id = 2, Locationname = "Group 2", Sortorder = 29 });

                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q30", Label = "Facts Text Question 30", Location_id = 2, Locationname = "Group 2", Sortorder = 30 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q31", Label = "Facts Text Question 31", Location_id = 2, Locationname = "Group 2", Sortorder = 31 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q32", Label = "Facts Text Question 32", Location_id = 2, Locationname = "Group 2", Sortorder = 32, IsNumeric = true });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q33", Label = "Facts Text Question 33", Location_id = 2, Locationname = "Group 2", Sortorder = 33 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q34", Label = "Facts Text Question 34", Location_id = 2, Locationname = "Group 2", Sortorder = 34 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q35", Label = "Facts Text Question 35", Location_id = 2, Locationname = "Group 2", Sortorder = 35 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q36", Label = "Facts Text Question 36", Location_id = 2, Locationname = "Group 2", Sortorder = 36 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q37", Label = "Facts Text Question 37", Location_id = 2, Locationname = "Group 2", Sortorder = 37 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q38", Label = "Facts Text Question 38", Location_id = 2, Locationname = "Group 2", Sortorder = 38 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q39", Label = "Facts Text Question 39", Location_id = 2, Locationname = "Group 2", Sortorder = 39 });

                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q40", Label = "Facts Text Question 40", Location_id = 2, Locationname = "Group 2", Sortorder = 40 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q41", Label = "Facts Text Question 41", Location_id = 2, Locationname = "Group 2", Sortorder = 41 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q42", Label = "Facts Text Question 42", Location_id = 2, Locationname = "Group 2", Sortorder = 42, IsNumeric = true });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q43", Label = "Facts Text Question 43", Location_id = 2, Locationname = "Group 2", Sortorder = 43 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q44", Label = "Facts Text Question 44", Location_id = 2, Locationname = "Group 2", Sortorder = 44 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q45", Label = "Facts Text Question 45", Location_id = 2, Locationname = "Group 2", Sortorder = 45 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q46", Label = "Facts Text Question 46", Location_id = 2, Locationname = "Group 2", Sortorder = 46 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q47", Label = "Facts Text Question 47", Location_id = 2, Locationname = "Group 2", Sortorder = 47 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q48", Label = "Facts Text Question 48", Location_id = 2, Locationname = "Group 2", Sortorder = 48 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q49", Label = "Facts Text Question 49", Location_id = 2, Locationname = "Group 2", Sortorder = 49 });

                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q50", Label = "Facts Text Question 50", Location_id = 2, Locationname = "Group 2", Sortorder = 50 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q51", Label = "Facts Text Question 51", Location_id = 2, Locationname = "Group 2", Sortorder = 51 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q52", Label = "Facts Text Question 52", Location_id = 2, Locationname = "Group 2", Sortorder = 52, IsNumeric = true });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q53", Label = "Facts Text Question 53", Location_id = 2, Locationname = "Group 2", Sortorder = 53 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q54", Label = "Facts Text Question 54", Location_id = 2, Locationname = "Group 2", Sortorder = 54 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q55", Label = "Facts Text Question 55", Location_id = 2, Locationname = "Group 2", Sortorder = 55 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q56", Label = "Facts Text Question 56", Location_id = 2, Locationname = "Group 2", Sortorder = 56 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q57", Label = "Facts Text Question 57", Location_id = 2, Locationname = "Group 2", Sortorder = 57 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q58", Label = "Facts Text Question 58", Location_id = 2, Locationname = "Group 2", Sortorder = 58 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q59", Label = "Facts Text Question 59", Location_id = 2, Locationname = "Group 2", Sortorder = 59 });

                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q60", Label = "Facts Text Question 60", Location_id = 2, Locationname = "Group 2", Sortorder = 60 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q61", Label = "Facts Text Question 61", Location_id = 2, Locationname = "Group 2", Sortorder = 61 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q62", Label = "Facts Text Question 62", Location_id = 2, Locationname = "Group 2", Sortorder = 62, IsNumeric = true });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q63", Label = "Facts Text Question 63", Location_id = 2, Locationname = "Group 2", Sortorder = 63 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q64", Label = "Facts Text Question 64", Location_id = 2, Locationname = "Group 2", Sortorder = 64 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q65", Label = "Facts Text Question 65", Location_id = 2, Locationname = "Group 2", Sortorder = 65 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q66", Label = "Facts Text Question 66", Location_id = 2, Locationname = "Group 2", Sortorder = 66 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q67", Label = "Facts Text Question 67", Location_id = 2, Locationname = "Group 2", Sortorder = 67 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q68", Label = "Facts Text Question 68", Location_id = 2, Locationname = "Group 2", Sortorder = 68 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q69", Label = "Facts Text Question 69", Location_id = 2, Locationname = "Group 2", Sortorder = 69 });

                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q70", Label = "Facts Text Question 70", Location_id = 2, Locationname = "Group 2", Sortorder = 70 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q71", Label = "Facts Text Question 71", Location_id = 2, Locationname = "Group 2", Sortorder = 71 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q72", Label = "Facts Text Question 72", Location_id = 2, Locationname = "Group 2", Sortorder = 72, IsNumeric = true });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q73", Label = "Facts Text Question 73", Location_id = 2, Locationname = "Group 2", Sortorder = 73 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q74", Label = "Facts Text Question 74", Location_id = 2, Locationname = "Group 2", Sortorder = 74 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q75", Label = "Facts Text Question 75", Location_id = 2, Locationname = "Group 2", Sortorder = 75 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q76", Label = "Facts Text Question 76", Location_id = 2, Locationname = "Group 2", Sortorder = 76 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q77", Label = "Facts Text Question 77", Location_id = 2, Locationname = "Group 2", Sortorder = 77 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q78", Label = "Facts Text Question 78", Location_id = 2, Locationname = "Group 2", Sortorder = 78 });
                questList1.Add(new Question { CategoryPKey = "C1", Category = cat1, Controltype = "text", QuestionPKey = "C1Q79", Label = "Facts Text Question 79", Location_id = 2, Locationname = "Group 2", Sortorder = 79 });

                cat1.Questions = questList1;
                tempCategories.Add(cat1);

                Category cat2 = new Category();
                cat2.CategoryPKey = "C2";
                cat2.Name = "Favorites";
                ObservableCollection<Question> questList2 = new ObservableCollection<Question>();
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q1", Label = "Enter favorite color", Location_id = 3, Locationname = "Group 3", Sortorder = 1 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q2", Label = "Enter favorite number", Location_id = 3, Locationname = "Group 3", Sortorder = 2, IsNumeric = true });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q3", Label = "Enter favorite car model", Location_id = 3, Locationname = "Group 3", Sortorder = 3 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q4", Label = "Enter favorite season", Location_id = 3, Locationname = "Group 3", Sortorder = 4 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q5", Label = "Enter favorite holiday", Location_id = 3, Locationname = "Group 3", Sortorder = 5 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q6", Label = "Enter favorite pet's name", Location_id = 4, Locationname = "Group 4", Sortorder = 6 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q7", Label = "Enter favorite song", Location_id = 4, Locationname = "Group 4", Sortorder = 7 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q8", Label = "Enter favorite cookie", Location_id = 4, Locationname = "Group 4", Sortorder = 8 });

                Question SwitchQuestionC2Q9 = new Question { CategoryPKey = "C2", Category = cat2, Controltype = "switch", QuestionPKey = "C2Q9", Label = "Favorites Switch 1 Yes or No", Location_id = 4, Locationname = "Group 4", Sortorder = 9 };
                ObservableCollection<QuestionOption> questOptListC2Q9 = new ObservableCollection<QuestionOption>();
                QuestionOption YesOptionC2Q9 = new QuestionOption { Question = SwitchQuestionC2Q9, QuestionPKey = SwitchQuestionC2Q9.QuestionPKey, QuestionOptionPKey = "C2Q9YES", Text = "Yes", Value = "Yes" };
                QuestionOption NoOptionC2Q9 = new QuestionOption { Question = SwitchQuestionC2Q9, QuestionPKey = SwitchQuestionC2Q9.QuestionPKey, QuestionOptionPKey = "C2Q9NO", Text = "No", Value = "No" };
                questOptListC2Q9.Add(YesOptionC2Q9);
                questOptListC2Q9.Add(NoOptionC2Q9);
                SwitchQuestionC2Q9.Options = questOptListC2Q9;
                questList2.Add(SwitchQuestionC2Q9);

                Question SwitchQuestionC2Q10 = new Question { CategoryPKey = "C2", Category = cat2, Controltype = "switch", QuestionPKey = "C2Q10", Label = "Favorites Switch 2 Yes or No", Location_id = 4, Locationname = "Group 4", Sortorder = 10, Ineligible = true };
                ObservableCollection<QuestionOption> questOptListC2Q10 = new ObservableCollection<QuestionOption>();
                QuestionOption YesOptionC2Q10 = new QuestionOption { Question = SwitchQuestionC2Q10, QuestionPKey = SwitchQuestionC2Q10.QuestionPKey, QuestionOptionPKey = "C2Q10YES", Text = "Yes", Value = "Yes" };
                QuestionOption NoOptionC2Q10 = new QuestionOption { Question = SwitchQuestionC2Q10, QuestionPKey = SwitchQuestionC2Q10.QuestionPKey, QuestionOptionPKey = "C2Q10NO", Text = "No", Value = "No" };
                questOptListC2Q10.Add(YesOptionC2Q10);
                questOptListC2Q10.Add(NoOptionC2Q10);
                SwitchQuestionC2Q10.Options = questOptListC2Q10;
                questList2.Add(SwitchQuestionC2Q10);

                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q11", Label = "Favorites Text Question 11", Location_id = 4, Locationname = "Group 4", Sortorder = 11 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q12", Label = "Favorites Text Question 12", Location_id = 4, Locationname = "Group 4", Sortorder = 12, IsNumeric = true });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q13", Label = "Favorites Text Question 13", Location_id = 4, Locationname = "Group 4", Sortorder = 13 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q14", Label = "Favorites Text Question 14", Location_id = 4, Locationname = "Group 4", Sortorder = 14 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q15", Label = "Favorites Text Question 15", Location_id = 4, Locationname = "Group 4", Sortorder = 15 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q16", Label = "Favorites Text Question 16", Location_id = 4, Locationname = "Group 4", Sortorder = 16 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q17", Label = "Favorites Text Question 17", Location_id = 4, Locationname = "Group 4", Sortorder = 17 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q18", Label = "Favorites Text Question 18", Location_id = 4, Locationname = "Group 4", Sortorder = 18 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q19", Label = "Favorites Text Question 19", Location_id = 4, Locationname = "Group 4", Sortorder = 19 });

                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q20", Label = "Favorites Text Question 20", Location_id = 4, Locationname = "Group 4", Sortorder = 20 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q21", Label = "Favorites Text Question 21", Location_id = 4, Locationname = "Group 4", Sortorder = 21 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q22", Label = "Favorites Text Question 22", Location_id = 4, Locationname = "Group 4", Sortorder = 22, IsNumeric = true });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q23", Label = "Favorites Text Question 23", Location_id = 4, Locationname = "Group 4", Sortorder = 23 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q24", Label = "Favorites Text Question 24", Location_id = 4, Locationname = "Group 4", Sortorder = 24 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q25", Label = "Favorites Text Question 25", Location_id = 4, Locationname = "Group 4", Sortorder = 25 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q26", Label = "Favorites Text Question 26", Location_id = 4, Locationname = "Group 4", Sortorder = 26 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q27", Label = "Favorites Text Question 27", Location_id = 4, Locationname = "Group 4", Sortorder = 27 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q28", Label = "Favorites Text Question 28", Location_id = 4, Locationname = "Group 4", Sortorder = 28 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q29", Label = "Favorites Text Question 29", Location_id = 4, Locationname = "Group 4", Sortorder = 29 });

                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q30", Label = "Favorites Text Question 30", Location_id = 4, Locationname = "Group 4", Sortorder = 30 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q31", Label = "Favorites Text Question 31", Location_id = 4, Locationname = "Group 4", Sortorder = 31 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q32", Label = "Favorites Text Question 32", Location_id = 4, Locationname = "Group 4", Sortorder = 32, IsNumeric = true });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q33", Label = "Favorites Text Question 33", Location_id = 4, Locationname = "Group 4", Sortorder = 33 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q34", Label = "Favorites Text Question 34", Location_id = 4, Locationname = "Group 4", Sortorder = 34 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q35", Label = "Favorites Text Question 35", Location_id = 4, Locationname = "Group 4", Sortorder = 35 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q36", Label = "Favorites Text Question 36", Location_id = 4, Locationname = "Group 4", Sortorder = 36 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q37", Label = "Favorites Text Question 37", Location_id = 4, Locationname = "Group 4", Sortorder = 37 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q38", Label = "Favorites Text Question 38", Location_id = 4, Locationname = "Group 4", Sortorder = 38 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q39", Label = "Favorites Text Question 39", Location_id = 4, Locationname = "Group 4", Sortorder = 39 });

                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q40", Label = "Favorites Text Question 40", Location_id = 4, Locationname = "Group 4", Sortorder = 40 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q41", Label = "Favorites Text Question 41", Location_id = 4, Locationname = "Group 4", Sortorder = 41 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q42", Label = "Favorites Text Question 42", Location_id = 4, Locationname = "Group 4", Sortorder = 42, IsNumeric = true });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q43", Label = "Favorites Text Question 43", Location_id = 4, Locationname = "Group 4", Sortorder = 43 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q44", Label = "Favorites Text Question 44", Location_id = 4, Locationname = "Group 4", Sortorder = 44 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q45", Label = "Favorites Text Question 45", Location_id = 4, Locationname = "Group 4", Sortorder = 45 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q46", Label = "Favorites Text Question 46", Location_id = 4, Locationname = "Group 4", Sortorder = 46 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q47", Label = "Favorites Text Question 47", Location_id = 4, Locationname = "Group 4", Sortorder = 47 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q48", Label = "Favorites Text Question 48", Location_id = 4, Locationname = "Group 4", Sortorder = 48 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q49", Label = "Favorites Text Question 49", Location_id = 4, Locationname = "Group 4", Sortorder = 49 });

                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q50", Label = "Favorites Text Question 50", Location_id = 4, Locationname = "Group 4", Sortorder = 50 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q51", Label = "Favorites Text Question 51", Location_id = 4, Locationname = "Group 4", Sortorder = 51 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q52", Label = "Favorites Text Question 52", Location_id = 4, Locationname = "Group 4", Sortorder = 52, IsNumeric = true });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q53", Label = "Favorites Text Question 53", Location_id = 4, Locationname = "Group 4", Sortorder = 53 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q54", Label = "Favorites Text Question 54", Location_id = 4, Locationname = "Group 4", Sortorder = 54 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q55", Label = "Favorites Text Question 55", Location_id = 4, Locationname = "Group 4", Sortorder = 55 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q56", Label = "Favorites Text Question 56", Location_id = 4, Locationname = "Group 4", Sortorder = 56 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q57", Label = "Favorites Text Question 57", Location_id = 4, Locationname = "Group 4", Sortorder = 57 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q58", Label = "Favorites Text Question 58", Location_id = 4, Locationname = "Group 4", Sortorder = 58 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q59", Label = "Favorites Text Question 59", Location_id = 4, Locationname = "Group 4", Sortorder = 59 });

                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q60", Label = "Favorites Text Question 60", Location_id = 4, Locationname = "Group 4", Sortorder = 60 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q61", Label = "Favorites Text Question 61", Location_id = 4, Locationname = "Group 4", Sortorder = 61 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q62", Label = "Favorites Text Question 62", Location_id = 4, Locationname = "Group 4", Sortorder = 62, IsNumeric = true });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q63", Label = "Favorites Text Question 63", Location_id = 4, Locationname = "Group 4", Sortorder = 63 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q64", Label = "Favorites Text Question 64", Location_id = 4, Locationname = "Group 4", Sortorder = 64 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q65", Label = "Favorites Text Question 65", Location_id = 4, Locationname = "Group 4", Sortorder = 65 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q66", Label = "Favorites Text Question 66", Location_id = 4, Locationname = "Group 4", Sortorder = 66 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q67", Label = "Favorites Text Question 67", Location_id = 4, Locationname = "Group 4", Sortorder = 67 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q68", Label = "Favorites Text Question 68", Location_id = 4, Locationname = "Group 4", Sortorder = 68 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q69", Label = "Favorites Text Question 69", Location_id = 4, Locationname = "Group 4", Sortorder = 69 });

                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q70", Label = "Favorites Text Question 70", Location_id = 4, Locationname = "Group 4", Sortorder = 70 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q71", Label = "Favorites Text Question 71", Location_id = 4, Locationname = "Group 4", Sortorder = 71 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q72", Label = "Favorites Text Question 72", Location_id = 4, Locationname = "Group 4", Sortorder = 72, IsNumeric = true });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q73", Label = "Favorites Text Question 73", Location_id = 4, Locationname = "Group 4", Sortorder = 73 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q74", Label = "Favorites Text Question 74", Location_id = 4, Locationname = "Group 4", Sortorder = 74 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q75", Label = "Favorites Text Question 75", Location_id = 4, Locationname = "Group 4", Sortorder = 75 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q76", Label = "Favorites Text Question 76", Location_id = 4, Locationname = "Group 4", Sortorder = 76 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q77", Label = "Favorites Text Question 77", Location_id = 4, Locationname = "Group 4", Sortorder = 77 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q78", Label = "Favorites Text Question 78", Location_id = 4, Locationname = "Group 4", Sortorder = 78 });
                questList2.Add(new Question { CategoryPKey = "C2", Category = cat2, Controltype = "text", QuestionPKey = "C2Q79", Label = "Favorites Text Question 79", Location_id = 4, Locationname = "Group 4", Sortorder = 79 });




                cat2.Questions = questList2;
                tempCategories.Add(cat2);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }


        public async Task OnItemTapped(object item)
        {
            try
            {

                EnableCategories = false;

                if (SelectedCategoryPKey == null || item == null)
                {
                    EnableCategories = true;
                    return;
                }

                var catItem = (Category)item;

                if (catItem.CategoryPKey != SelectedCategoryPKey || ForceQuestionReload)
                {
                    if (ForceQuestionReload)
                    {
                        ForceQuestionReload = false;
                    }

                    // Try up to 10 times in case method is currently busy
                    for (var x = 0; x < 10; x++)
                    {
                        var result = await LoadQuestions(catItem);
                        if (result == "busy")
                        {
                            Debug.WriteLine("-----> in OnItemTapped() BUSY, barcode = " + catItem.Name);
                            await Task.Delay(500);
                        }
                        else
                        {
                            break;
                        }
                    }
                }


                EnableCategories = true;
            }
            catch (Exception e)
            {
                EnableCategories = true;
                Debug.WriteLine(e.Message);
            }
        }


        public async Task<QuestionUpdateResult> OnQuestionChanged(Question theQuest, object newVal)
        {
            QuestionUpdateResult result = new QuestionUpdateResult();
            try
            {
                return await Task.Run(async () =>
                {
                    IsBusy = true;
                    var catData = theQuest.Category;

                    if (newVal == null)
                    {
                        newVal = string.Empty;
                    }

                    string strNewVal = newVal.ToString();
                    theQuest.Value = strNewVal;
                    theQuest.AnsweredByText = string.IsNullOrEmpty(strNewVal) ? "" : "Answered on " + DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff");


                    // Instead of saving the value to the database and retrieving everything, just update the ObservableCollections 
                    var catpair = Categories.Select((Value, Index) => new { Value, Index }).Single(p => p.Value.CategoryPKey == theQuest.Category.CategoryPKey);
                    if (catpair != null && catpair.Index != -1)
                    {
                        if (QuestionsGrouped.Count > 0)
                        {
                            int theIndex = -1;
                            QuestionGrouping theValue = null;
                            var groupPair = new { Value = theValue, Index = theIndex };

                            var groupPairList = QuestionsGrouped.Select((Value, Index) => new { Value, Index }).Where(z => z.Value.Key == theQuest.Locationname);
                            if (groupPairList != null && groupPairList.Count() > 1)
                            {
                                foreach (var x in groupPairList)
                                {
                                    if (x.Value.GetItemList().Any(qu => qu.QuestionPKey == theQuest.QuestionPKey))
                                    {
                                        theIndex = x.Index;
                                        theValue = x.Value;
                                        groupPair = new { Value = theValue, Index = theIndex };
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                groupPair = QuestionsGrouped.Select((Value, Index) => new { Value, Index }).SingleOrDefault(z => z.Value.Key == theQuest.Locationname);
                            }

                            if (groupPair != null && groupPair.Index != -1)
                            {
                                var qpair = groupPair.Value.Select((Value, Index) => new { Value, Index }).Single(q => q.Value.QuestionPKey == theQuest.QuestionPKey);
                                if (qpair.Index != -1)
                                {
                                    // update question
                                    Question source = theQuest;
                                    Question target = QuestionsGrouped[groupPair.Index][qpair.Index];
                                    var sourceType = typeof(Question);
                                    var targetType = typeof(Question);

                                    foreach (var sourceProperty in sourceType.GetProperties())
                                    {
                                        if (sourceProperty.Name == "Value")
                                        {
                                            var targetProperty = targetType.GetProperty(sourceProperty.Name);
                                            if (targetProperty != null)
                                            {
                                                if (targetProperty.GetSetMethod() != null)
                                                {
                                                    MainThread.BeginInvokeOnMainThread(() =>
                                                    {
                                                        targetProperty.SetValue(target, sourceProperty.GetValue(source, null), null);
                                                    });
                                                }
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        // update category
                        Category catSource = catData;
                        Category catTarget = Categories[catpair.Index];
                        var CatSourceType = typeof(Category);
                        var CatTargetType = typeof(Category);

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            foreach (var sourceProperty in CatSourceType.GetProperties())
                            {
                                if (sourceProperty.Name == "TotalAnsweredQuestions" || sourceProperty.Name == "TotalEligibleQuestions")
                                {
                                    var targetProperty = CatTargetType.GetProperty(sourceProperty.Name);
                                    if (targetProperty != null)
                                    {
                                        if (targetProperty.GetSetMethod() != null)
                                        {
                                            targetProperty.SetValue(catTarget, sourceProperty.GetValue(catSource, null), null);
                                        }
                                    }
                                }
                            }
                        });
                    }

                    if (theQuest.QuestionPKey == "C2Q9") // mimic analyze desc
                    {
                        Category theTempCat = Categories.Where(c => c.CategoryPKey == SelectedCategoryPKey).FirstOrDefault();
                        if (theTempCat != null)
                        {
                            Question theExtraQuestion = theTempCat.Questions.Where(q => q.QuestionPKey == "C2Q10").FirstOrDefault();
                            if (theExtraQuestion != null)
                            {
                                if (theQuest.Value == "Yes")
                                {
                                    theExtraQuestion.Ineligible = false;
                                }
                                else
                                {
                                    theExtraQuestion.Ineligible = true;
                                }

                                ForceQuestionReload = true;
                                await RefreshData();
                            }
                        }
                    }

                    result.QuestionObj = theQuest;
                    result.ReturnVal = "1";
                    IsBusy = false;

                    return result;
                });
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return result;
            }
        }



        public async Task<string> LoadQuestions(Category catItem)
        {
            try
            {
                if (LoadingQuestions)
                {
                    return "busy";
                }

                LoadingQuestions = true;
                tempQuestionsGrouped.Clear();
                QuestionsGrouped.Clear();

                await Task.Delay(50);

                SelectedCategoryPKey = catItem.CategoryPKey;

                sortedQuestions = from qitem in catItem.Questions
                                  where qitem.Visible
                                  orderby qitem.Sortorder
                                  group qitem by new { qitem.Location_id, qitem.Locationname, qitem.Sortorder } into questGroup
                                  select new QuestionGrouping(questGroup.Key.Locationname, questGroup);

                string lastLoc = "";
                int idx = 0;
                foreach (QuestionGrouping q in sortedQuestions)
                {
                    if (lastLoc != q.Key)
                    {
                        tempQuestionsGrouped.Add(q);
                        idx++;
                    }
                    else
                    {
                        tempQuestionsGrouped[idx - 1].InsertItems(q.GetItemList());
                    }
                    lastLoc = q.Key;
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    foreach (QuestionGrouping questgrp in tempQuestionsGrouped)
                    {
                        QuestionsGrouped.Add(questgrp);
                    }
                });


                LoadingQuestions = false;
                return "ok";
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return "ok";
            }
        }


        public async Task RefreshData(bool isReset = false)
        {

            try
            {

                // remember current SelectedCategory
                var theLastSelectedCategoryPKey = "";
                if (SelectedCategory == null)
                {
                    if (LastSelectedCategoryPKey != "")
                    {
                        theLastSelectedCategoryPKey = LastSelectedCategoryPKey;
                        LastSelectedCategoryPKey = "";
                    }
                }
                else
                {
                    theLastSelectedCategoryPKey = SelectedCategory.CategoryPKey;
                }

                if (isReset)
                {
                    await LoadData();
                }
                else
                {
                    tempCategories.Clear();
                    tempCategories = Categories.ToList();
                }

                EnableCategories = true;

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Categories.Clear();
                    foreach (Category b in tempCategories)
                    {
                        Categories.Add(b);
                    }

                    if (Categories.Count > 0)
                    {
                        if (string.IsNullOrEmpty(theLastSelectedCategoryPKey))
                        {
                            SelectedCategory = Categories.First();
                            if (SelectedCategory == null)
                            {
                                SelectedCategoryPKey = "0";
                            }
                        }
                        else
                        {
                            Category theSelectedCat = null;
                            theSelectedCat = Categories.Select(s => s).Where(x => x.CategoryPKey == theLastSelectedCategoryPKey).FirstOrDefault();
                            if (theSelectedCat == null)
                            {
                                SelectedCategory = Categories.FirstOrDefault();
                                if (SelectedCategory == null)
                                {
                                    SelectedCategoryPKey = "0";
                                }
                            }
                            else
                            {
                                SelectedCategory = null;
                                SelectedCategory = theSelectedCat;
                            }
                        }
                    }
                    else
                    {
                        QuestionsGrouped.Clear();
                    }

                });

            }
            catch (Exception e)
            {
                EnableCategories = true;
                Debug.WriteLine(e);
            }
        }


        public async Task OnClicked(object buttonText)
        {
            switch ((string)buttonText)
            {
                case "Reset":
                    ForceQuestionReload = true;
                    await RefreshData(true);
                    break;

                default:
                    break;
            }
        }




    }
}
