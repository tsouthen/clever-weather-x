using CleverWeather.Shared.Models;
using CleverWeather.Shared.ViewModels;
using Xamarin.Forms;

namespace CleverWeather.Shared.Views
    {
    class ForecastView : BaseView
        {
        public ForecastView(City city)
            {
            BindingContext = new ForecastViewModel(city);
            var stack = new StackLayout
                {
                Orientation = StackOrientation.Vertical,
                Padding = new Thickness(0, 8, 0, 8)
                };

            var activity = new ActivityIndicator
                {
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

            var cell = new DataTemplate(typeof(ImageCell));

            cell.SetBinding(ImageCell.TextProperty, "Name");
            cell.SetBinding(ImageCell.DetailProperty, "Summary");
            cell.SetBinding(ImageCell.ImageSourceProperty, "Icon");

            listView.ItemTapped += (sender, args) =>
                {
                if (listView.SelectedItem == null)
                    return;
                //this.Navigation.PushAsync(new BlogDetailsView(listView.SelectedItem as FeedItem));
                listView.SelectedItem = null;
                };

            listView.ItemTemplate = cell;

            stack.Children.Add(listView);

            Content = stack;
            }

        private ForecastViewModel ViewModel
            {
            get
                {
                return BindingContext as ForecastViewModel;
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