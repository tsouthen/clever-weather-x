using CleverWeather.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CleverWeather.Shared.ViewModels
    {
    public class ForecastViewModel : ListViewModel<Forecast>
        {
        private City m_city;

        public ForecastViewModel(City city)
            {
            m_city = city;
            Title = city.NameEn;
            }

        protected async override Task<IEnumerable<Forecast>> LoadItems()
            {
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

                return await query.ToListAsync();
                }
            return null;
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
                var query = element.Descendants("forecast").Select(delegate(XElement item)
                    {
                    int? high = null;
                    int? low = null;

                    var temps = item.Element("temperatures");
                    if (temps != null)
                        {
                        foreach (var temp in temps.Descendants("temperature"))
                            {
                            if ("high" == (string) temp.Attribute("class"))
                                high = (int)temp;
                            else if ("low" == (string)temp.Attribute("class"))
                                low = (int)temp;
                            }
                        }
                    return new Forecast
                        {
                        CityCode = m_city.Code,
                        Name = (string)item.Element("period").Attribute("textForecastName"),
                        Summary = (string)item.Element("textSummary"),
                        UTCIssueTime = dt,
                        IconCode = (int) item.Element("abbreviatedForecast").Element("iconCode"),
                        HighTemp = high,
                        LowTemp = low
                        };
                    });
                return query;
                });
            }
        }
    }

