using CleverWeather.Shared.Models;
using CleverWeather.Shared.ViewModels;
using Xamarin.Forms;

namespace CleverWeather.Shared.Views
    {
    class CitiesView : BaseView
        {
        public CitiesView(bool onlyFavorites)
            {
            BindingContext = new CitiesViewModel(onlyFavorites);
            var stack = new StackLayout
                {
                Orientation = StackOrientation.Vertical,
                Padding = new Thickness(0, 8, 0, 8)
                };

            var activity = new ActivityIndicator
                {
                Color = Xamarin.Forms.Color.Blue, //Helpers.Color.DarkBlue.ToFormsColor(),
                IsEnabled = true
                };
            activity.SetBinding(ActivityIndicator.IsVisibleProperty, "IsBusy");
            activity.SetBinding(ActivityIndicator.IsRunningProperty, "IsBusy");
            stack.Children.Add(activity);

            var label = new Label()
                {
                    Text = "Loading...",
                    HorizontalOptions = LayoutOptions.Center
                };
            label.SetBinding(Label.IsVisibleProperty, "IsBusy");
            label.SetBinding(Label.TextProperty, "LoadingLabel");
            stack.Children.Add(label);

            var listView = new ListView();
            listView.SetBinding(ListView.IsVisibleProperty, "IsNotBusy");

            listView.ItemsSource = ViewModel.Items;

            var cell = new DataTemplate(typeof(TextCell));

            cell.SetBinding(TextCell.TextProperty, "NameEn");
            cell.SetBinding(TextCell.DetailProperty, "Province");

            listView.ItemTapped += (sender, args) =>
                {
                if (listView.SelectedItem == null)
                    return;
                this.Navigation.PushAsync(new ForecastView(listView.SelectedItem as City));
                listView.SelectedItem = null;
                };

            listView.ItemTemplate = cell;

            stack.Children.Add(listView);

            Content = stack;
            }

        private CitiesViewModel ViewModel
            {
            get
                {
                return BindingContext as CitiesViewModel;
                }
            }

        protected override void OnAppearing()
            {
            base.OnAppearing();
            if (ViewModel == null || !ViewModel.CanLoadMore || ViewModel.IsBusy || ViewModel.Items.Count > 0)
                return;

            ViewModel.LoadCommand.Execute(null);
            }
        }
    }