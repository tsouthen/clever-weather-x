using CleverWeather.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CleverWeather.Shared.ViewModels
    {
    public class CitiesViewModel : ListViewModel<City>
        {
        private bool m_onlyFavorites;

        public CitiesViewModel(bool onlyFavorites)
            {
            m_onlyFavorites = onlyFavorites;
            Title = m_onlyFavorites ? "Favorites" : "All Cities";
            }

        protected async override Task<IEnumerable<City>> LoadItems()
            {
            if (App.Connection == null)
                return null;

            var query = App.Connection.Table<City>();
            if (m_onlyFavorites)
                query = query.Where(c => c.IsFavorite);
            query = query.OrderBy(c => c.NameEn);

            return await query.ToListAsync();
            }
        }
    }

