using SQLite.Net.Async;
using Xamarin.Forms;

namespace CleverWeather
    {
    public class App
        {
        private static SQLiteAsyncConnection m_connection;

        public static SQLiteAsyncConnection Connection
            {
            get { return App.m_connection; }
            set { App.m_connection = value; }
            }

        public static Page GetMainPage()
            {
            var tabbedPage = new TabbedPage();
            tabbedPage.Children.Add(new Page() { Title = "Search" });
            var favs = new CleverWeather.Shared.Views.CitiesView(true);
            tabbedPage.Children.Add(favs);
            tabbedPage.Children.Add(new CleverWeather.Shared.Views.CitiesView(false));
            tabbedPage.Children.Add(new Page() { Title = "Location" });
            tabbedPage.Children.Add(new Page() { Title = "Browse" });
            tabbedPage.Title = "Clever Weather";
            tabbedPage.SelectedItem = favs;
            return new NavigationPage(tabbedPage);
            }
        }
    }
