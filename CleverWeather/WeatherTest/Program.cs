using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CleverWeather.Shared.Models;

namespace WeatherTest
    {
    static class Program
        {
        static void Main(string[] args)
            {
            try
                {
                CreateDatabaseFromJson(@"c:\temp\clever_weather.db3");
                }
            catch (Exception ex)
                {
                System.Console.WriteLine("Exception: {0}", ex.Message);
                }
            finally
                {
                WaitForKey();
                }
            }

        private static void CreateDatabaseFromJson(string dbFileName)
            {
            //CleverWeather.SiteListUtils.ForecastTest(System.Console.Out);
            var connection = new SQLite.Net.SQLiteConnection(new SQLite.Net.Platform.Win32.SQLitePlatformWin32(), dbFileName);

            //Create tables and populate city table if needed
            if (connection.RowCount<City>() == 0)
                {
                //Create tables
                connection.CreateTable<City>();
                connection.CreateTable<CurrentConditions>();
                connection.CreateTable<Forecast>();

                var assembly = System.Reflection.Assembly.GetAssembly(typeof(CleverWeather.SiteListUtils));
                connection.InsertAll(CleverWeather.SiteListUtils.GetEmbeddedCitiesResource(assembly), typeof(City));
                }
            }

        private static int RowCount<T>(this SQLite.Net.SQLiteConnection connection)
            {
            //this could potentially create a new mapping if it doesn't exist in the connection already, alternative is to search
            //the TableMappings enumerable for an existing mapping but that would be slower
            return connection.ExecuteScalar<int>(string.Format("select count(*) from {0}", connection.GetMapping<T>().TableName));
            }

        private static void WaitForKey()
            {
            System.Console.WriteLine();
            System.Console.WriteLine("Press any key to exit");
            System.Console.ReadKey();
            }
        }
    }
