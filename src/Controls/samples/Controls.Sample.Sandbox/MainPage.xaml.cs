#nullable disable
namespace Maui.Controls.Sample
{
    public partial class MainPage : ContentPage
    {

        public MainPageViewModel vm;

        public MainPage()
        {
            InitializeComponent();
            Content.BindingContext = vm = new MainPageViewModel();
        }

        private async void CategoryListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (vm.IsLoaded)
            {
                await vm.OnItemTapped(e.SelectedItem);
            }
        }

        //protected override void OnAppearing()
        //{
        //    base.OnAppearing();
        //    vm.OnAppearing();
        //}

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            vm.OnAppearing();
        }

        private async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (vm.IsLoaded)
            {
                if (e.CurrentSelection.Count > 0)
                {
                    await vm.OnItemTapped(e.CurrentSelection.First());
                }
            }
        }

        private void CollectionView_BindingContextChanged(object sender, EventArgs e)
        {
        }
    }
}
