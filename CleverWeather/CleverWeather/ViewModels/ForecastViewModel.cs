using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Xamarin.Forms;

using CleverWeather.Shared.Models;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CleverWeather.Shared.ViewModels
    {
    public class ForecastViewModel : BaseViewModel
        {
        private City m_city;
        private Command m_loadCommand;

        public ObservableCollection<Forecast> Items { get; set; }

        public ForecastViewModel(City city)
            {
            m_city = city;
            Title = city.NameEn;
            Items = new ObservableCollection<Forecast>();
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
                LoadingLabel = "Loading Forecast...";
                Items.Clear();

                if (App.Connection != null)
                    {
                    var query = App.Connection.Table<Forecast>().Where(f => f.CityCode == m_city.Code).OrderBy(f => f.Id);
                    var forecast = await query.FirstOrDefaultAsync();

                    //if forecast over an hour old, requery
                    if (forecast != null && forecast.UTCIssueTime.AddHours(1.0).CompareTo(DateTime.UtcNow) < 0)
                        forecast = null;

                    if (forecast == null)
                        {
                        var url = "http://dd.weatheroffice.ec.gc.ca/citypage_weather/xml/" + m_city.Province + "/" + m_city.Code + "_e.xml";
                        var xml = await SiteListUtils.GetResponse(url);
                        var items = await ParseForecasts(xml);

                        //TODO: rather than hard-code table name, we should use the mapping but the async connection doesn't have a function to get it
                        string sql = string.Format("delete from \"{0}\" where \"{1}\" = ?", "Forecast", "CityCode");
                        await App.Connection.ExecuteAsync(sql, m_city.Code);
                        await App.Connection.InsertAllAsync(items);
                        }

                    var list = await query.ToListAsync();
                    foreach (var item in list)
                        Items.Add(item);
                    }
                //TODO: show label when Forecasts is empty or not connected
                }
            catch (Exception ex)
                {
                var page = new ContentPage();
                var result = page.DisplayAlert("Error", "Unable to load Forecast, exception: " + ex.Message, "OK", null);
                }
            finally
                {
                IsBusy = false;
                }
            }

        private async Task<IEnumerable<Forecast>> ParseForecasts(string xml)
            {
            return await Task.Run(() =>
                {
                var element = XElement.Parse(xml);
                var location = element.Element("location").Element("name").Value;
                DateTime dt = DateTime.UtcNow;
                XElement dateTimeEl = element.Elements("dateTime").FirstOrDefault(e => e.Attribute("zone").Value == "UTC");
                if (dateTimeEl != null)
                    {
                    var timeStamp = dateTimeEl.Element("timeStamp").Value;
                    bool worked = DateTime.TryParseExact(timeStamp, "yyyyMMddHHmmss", 
                        System.Globalization.DateTimeFormatInfo.InvariantInfo, 
                        System.Globalization.DateTimeStyles.None, out dt);
                    }
                var query = element.Descendants("forecast").Select(item => new Forecast
                    {
                    CityCode = m_city.Code,
                    Name = (string)item.Element("period").Attribute("textForecastName"),
                    Summary = (string)item.Element("textSummary"),
                    UTCIssueTime = dt,
                    IconCode = (int) item.Element("abbreviatedForecast").Element("iconCode")
                    //TODO: HighTemp
                    //TODO: LowTemp
                    });
                return query;
                });
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

