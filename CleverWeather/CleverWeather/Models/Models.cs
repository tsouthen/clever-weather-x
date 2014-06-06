using System;
using System.Collections.Generic;
using System.Text;
using SQLite.Net.Attributes;

namespace CleverWeather.Shared.Models
    {
    public class City
        {
        [PrimaryKey]
        public string Code { get; set; }
        public string NameEn { get; set; }
        public string NameFr { get; set; }
        public string Province { get; set; }
        [Indexed]
        public double Latitude { get; set; }
        [Indexed]
        public double Longitude { get; set; }
        [Indexed]
        public bool IsFavorite { get; set; }
        }

    public class CurrentConditions
        {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string CityCode { get; set; }

        public DateTime UTCObservationTime { get; set; }
        public string Condition { get; set; }
        public int IconCode { get; set; }
        public double Temperature { get; set; }
        public double DewPoint { get; set; }
        public double WindChill { get; set; }
        public double Humidex { get; set; }
        public double Pressure { get; set; }
        public double Visibility { get; set; }
        public int RelativeHumidity { get; set; }
        public int WindSpeed { get; set; }
        public string WindDirection { get; set; }
        }

    public class Forecast
        {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public string CityCode { get; set; }
        public DateTime UTCIssueTime{ get; set; }

        public string Name { get; set; }
        public string Summary { get; set; }
        public int IconCode { get; set; }
        public int LowTemp { get; set; }
        public int HighTemp { get; set; }

        [Ignore]
        public string Icon
            {
            get 
                {
                return "cbc_white_" + IconCode.ToString("D2") + ".png";
                }
            set { }
            }
        }
    }
