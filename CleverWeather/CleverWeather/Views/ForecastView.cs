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
            listView.SetBinding(ListView.IsVisibleProperty, "IsNotBusy"); //TODO: change this to templated call

            listView.ItemsSource = ViewModel.Items;

            var cell = new DataTemplate(typeof(ImageCell));

            cell.SetBinding(ImageCell.TextProperty, new Binding(".", converter: new ForecastValueConverter()));
            cell.SetBinding(ImageCell.DetailProperty, "Summary");
            cell.SetBinding(ImageCell.ImageSourceProperty, new Binding("IconCode", stringFormat:"cbc_white_{0:D2}.png"));

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

        private class ForecastValueConverter : IValueConverter
            {
            public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                if (value is string)
                    return value;

                var f = value as Forecast;
                if (f != null)
                    {
                    var builder = new System.Text.StringBuilder();
                    builder.Append(f.Name);

                    if (f.LowTemp.HasValue)
                        {
                        builder.Append(" ↓");
                        builder.Append(f.LowTemp);
                        }

                    if (f.HighTemp.HasValue)
                        {
                        builder.Append(" ↑");
                        builder.Append(f.HighTemp);
                        }

                    return builder.ToString();
                    }
                return value.ToString();
                }

            public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
                {
                throw new System.NotImplementedException();
                }
            }
        }
    }