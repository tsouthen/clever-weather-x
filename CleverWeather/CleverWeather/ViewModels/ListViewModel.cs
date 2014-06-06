using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Xamarin.Forms;

using System.Threading.Tasks;

namespace CleverWeather.Shared.ViewModels
    {
    public abstract class ListViewModel<T> : BaseViewModel
        {
        private Command m_loadCommand;

        public ObservableCollection<T> Items { get; set; }

        public ListViewModel()
            {
            Items = new ObservableCollection<T>();
            }

        public Command LoadCommand
            {
            get
                {
                return m_loadCommand ?? (m_loadCommand = new Command(ExecuteLoadCommand));
                }
            }

        protected abstract Task<IEnumerable<T>> LoadItems();

        private async void ExecuteLoadCommand()
            {
            if (IsBusy)
                return;

            try
                {
                IsBusy = true;
                LoadingLabel = "Loading...";
                Items.Clear();

                var enumerable = await LoadItems();
                if (enumerable != null)
                    {
                    foreach (var item in enumerable)
                        Items.Add(item);
                    }
                }
            catch (Exception ex)
                {
                var page = new ContentPage();
                var result = page.DisplayAlert("Error", "Unable to load items, exception: " + ex.Message, "OK", null);
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