using System;
using System.IO;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Xamarin.Forms.Platform.Android;

namespace CleverWeather.Droid
    {
    [Activity(Label = "Clever Weather", MainLauncher = true)]
    public class MainActivity : AndroidActivity
        {
        protected override void OnCreate(Bundle bundle)
            {
            base.OnCreate(bundle);

            Xamarin.Forms.Forms.Init(this, bundle);

            //copy database out of assembly resources if needed
            var sqliteFilename = "clever_weather.db3";
            string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // Documents folder
            var path = Path.Combine(documentsPath, sqliteFilename);
            if (!File.Exists(path))
                {
                using (Stream stream = CleverWeather.SiteListUtils.GetEmbeddedResourceStream(System.Reflection.Assembly.GetAssembly(typeof(CleverWeather.SiteListUtils)), sqliteFilename))
                using (FileStream writeStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                    ReadWriteStream(stream, writeStream);
                    }
                }

            //connect to database
            if (File.Exists(path))
                {
                var platform = new SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid();
                var connString = new SQLite.Net.SQLiteConnectionString(path, false);
                var conn = new SQLite.Net.SQLiteConnectionWithLock(platform, connString);
                App.Connection = new SQLite.Net.Async.SQLiteAsyncConnection(() => conn);
                }

            SetPage(App.GetMainPage());
            }

        /// <summary>
        /// Reads from one stream and write to another
        /// </summary>
        void ReadWriteStream(Stream readStream, Stream writeStream)
            {
            int length = 256;
            Byte[] buffer = new Byte[length];
            int bytesRead = readStream.Read(buffer, 0, length);
            // write the required bytes
            while (bytesRead > 0)
                {
                writeStream.Write(buffer, 0, bytesRead);
                bytesRead = readStream.Read(buffer, 0, length);
                }
            }
        }
    }

