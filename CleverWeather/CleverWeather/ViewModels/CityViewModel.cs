using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Xamarin.Forms;

using CleverWeather.Shared.Models;

namespace CleverWeather.Shared.ViewModels
    {
    public class CitiesViewModel : BaseViewModel
        {
        private Command m_loadCommand;
        private bool m_onlyFavorites;

        public ObservableCollection<City> Items { get; set; }

        public CitiesViewModel(bool onlyFavorites)
            {
            m_onlyFavorites = onlyFavorites;
            Title = m_onlyFavorites ? "Favorites" : "All Cities";
            Items = new ObservableCollection<City>();
            }

        public Command LoadCommand
            {
            get
                {
                return m_loadCommand ?? (m_loadCommand = new Command(ExecuteLoadCommand));
                }
            }

        private async void ExecuteLoadCommand()
            {
            if (IsBusy)
                return;

            try
                {
                IsBusy = true;
                LoadingLabel = "Loading Cities...";
                Items.Clear();

                IEnumerable<City> cities = null;
                if (App.Connection != null)
                    {
                    var query = App.Connection.Table<City>();
                    if (m_onlyFavorites)
                        query = query.Where(c => c.IsFavorite);
                    query = query.OrderBy(c => c.NameEn);
                    cities = await query.ToListAsync();

                    foreach (var city in cities)
                        Items.Add(city);
                    }
                //TODO: show label when Cities is empty or not connected
                }
            catch (Exception ex)
                {
                var page = new ContentPage();
                var result = page.DisplayAlert("Error", "Unable to load Cities, exception: " + ex.Message, "OK", null);
                }
            finally
                {
                IsBusy = false;
                }
            }

        public const string IsNotBusyPropertyName = "IsNotBusy";
        public bool IsNotBusy
            {
            get 
                { 
                return !IsBusy; 
                }
            set 
                {
                IsBusy = !value;
                OnPropertyChanged(IsNotBusyPropertyName);
                }
            }

        public const string LoadingLabelPropertyName = "LoadingLabel";
        private string m_loadingLabel;
        public string LoadingLabel
            {
            get
                {
                return m_loadingLabel;
                }
            set
                {
                SetProperty(ref m_loadingLabel, value, LoadingLabelPropertyName); 
                }
            }
        }
    }

