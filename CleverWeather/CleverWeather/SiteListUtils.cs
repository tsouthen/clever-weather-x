using CleverWeather.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CleverWeather
    {
    public class SiteListUtils
        {
        public static void ForecastTest(TextWriter console)
            {
            try
                {
                var xml = GetResponse("http://dd.weatheroffice.ec.gc.ca/citypage_weather/xml/BC/s0000656_e.xml").Result;
                var siteData = XElement.Parse(xml);
                var location = siteData.Element("location").Element("name").Value;

                var query = siteData.Descendants("forecast").Select(item => new Forecast
                            {
                                Name = (string)item.Element("period").Attribute("textForecastName"),
                                Summary = (string)item.Element("textSummary")
                            });
#if QUERYSYNTAX
                var query = from item in siteData.Descendants("forecast")
                            select new Forecast
                            {
                                Name = (string)item.Element("period").Attribute("textForecastName"),
                                Summary = (string)item.Element("textSummary")
                            };
#endif
                console.WriteLine("Forecast for {0}:", location);
                foreach (Forecast forecast in query)
                    {
                    console.WriteLine("{0}: {1}", forecast.Name, forecast.Summary);
                    }
                }
            catch (Exception ex)
                {
                console.WriteLine("Exception: {0}", ex.Message);
                }
            }

        public static async Task<string> GetResponse(string url)
            {
            var httpClientHandler = new System.Net.Http.HttpClientHandler();
            httpClientHandler.AutomaticDecompression = System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.GZip;

            var httpClient = new System.Net.Http.HttpClient(httpClientHandler);
            var response = await httpClient.GetAsync(new Uri(url));

            response.EnsureSuccessStatusCode();
            using (var responseStream = await response.Content.ReadAsStreamAsync())
            using (var streamReader = new StreamReader(responseStream, Encoding.GetEncoding("iso-8859-1")))
                {
                return streamReader.ReadToEnd();
                }
            }

        public static IEnumerable<City> GetCities()
            {
            var url = "http://dd.weatheroffice.ec.gc.ca/citypage_weather/xml/siteList.xml";
            var xml = GetResponse(url).Result;
            var siteData = XElement.Parse(xml);
            if (siteData != null)
                {
                var query = from item in siteData.Descendants("site")
                            //where (string)item.Attribute("code") == "s0000186"
                            select new City
                            {
                                Code = (string)item.Attribute("code"),
                                NameEn = (string)item.Element("nameEn"),
                                NameFr = (string)item.Element("nameFr"),
                                Province = (string)item.Element("provinceCode")
                            };
                return query;
                }
            return new List<City>(); //an empty list
            }

        public static void AddLatLon(City city, TextWriter console)
            {
            try
                {
                var httpClient = new System.Net.Http.HttpClient();
                var baseUrl = "http://dd.weatheroffice.ec.gc.ca/citypage_weather/xml/";
                var url = string.Format("{0}{1}/{2}_e.xml", baseUrl, city.Province, city.Code);
                var response = httpClient.GetAsync(url).Result;
                if (response.IsSuccessStatusCode)
                    {
                    var xml = response.Content.ReadAsStringAsync().Result;
                    var siteData = XElement.Parse(xml);
                    var nameEl = siteData.Element("location").Element("name");
                    if (nameEl != null)
                        {
                        var lat = (string)nameEl.Attribute("lat");
                        var lon = (string)nameEl.Attribute("lon");
                        if (lat != null && lon != null)
                            {
                            city.Latitude = Double.Parse(lat.Substring(0, lat.Length - 1));
                            city.Longitude = Double.Parse(lon.Substring(0, lon.Length - 1));
                            }
                        }
                    }
                }
            catch (Exception ex)
                {
                console.WriteLine("AddLatLon Exception: {0}", ex.Message);
                }
            }

        public static void GenerateCityList(TextWriter outFile, TextWriter console)
            {
            console.WriteLine("Getting Cities");
            List<City> cities = GetCities().ToList();

            console.WriteLine("Serializing JSON");
            Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
            foreach (var city in cities)
                {
                serializer.Serialize(outFile, city);
                outFile.WriteLine();
                }
            console.WriteLine("Done.");
            }

        public static IEnumerable<string> ReadLines(TextReader reader)
            {
            string line;
            while ((line = reader.ReadLine()) != null)
                yield return line;
            }

        public static IEnumerable<City> ReadCities(TextReader reader, TextWriter console)
            {
            foreach (string json in ReadLines(reader))
                {
                var city = Newtonsoft.Json.JsonConvert.DeserializeObject<City>(json);
                if (console != null)
                    console.WriteLine("{0} ({1})", city.NameEn, city.Province);
                yield return city;
                }
            }

        public static void AddLatLon(TextReader inFile, TextWriter outFile, TextWriter console)
            {
            try
                {
                Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
                console.WriteLine("Adding Lat/Lon");
                foreach (var json in ReadLines(inFile))
                    {
                    var city = Newtonsoft.Json.JsonConvert.DeserializeObject<City>(json);
                    if (city.Latitude == 0)
                        {
                        console.WriteLine("  Processing: {0} ({1})", city.NameEn, city.Province);
                        AddLatLon(city, console);
                        }

                    serializer.Serialize(outFile, city);
                    outFile.WriteLine();
                    }
                }
            catch (Exception ex)
                {
                console.WriteLine("AddLatLon Exception: {0}", ex.Message);
                }
            }

        public static Stream GetEmbeddedResourceStream(Assembly assembly, string resourceFileName)
            {
            var resourcePaths = assembly.GetManifestResourceNames()
                .Where(x => x.EndsWith(resourceFileName, StringComparison.CurrentCultureIgnoreCase))
                .ToArray();

            if (!resourcePaths.Any())
                throw new Exception(string.Format("Resource ending with {0} not found.", resourceFileName));

            if (resourcePaths.Count() > 1)
                throw new Exception(string.Format("Multiple resources ending with {0} found: {1}{2}", resourceFileName, Environment.NewLine, string.Join(Environment.NewLine, resourcePaths)));

            return assembly.GetManifestResourceStream(resourcePaths.Single());
            }

        public static IEnumerable<City> GetEmbeddedCitiesResource(Assembly assembly)
            {
            using (Stream stream = GetEmbeddedResourceStream(assembly, "cities.json"))
            using (var reader = new StreamReader(stream))
                {
                foreach (string json in ReadLines(reader))
                    {
                    var city = Newtonsoft.Json.JsonConvert.DeserializeObject<City>(json);
                    yield return city;
                    }
                }
            }
        }
    }
