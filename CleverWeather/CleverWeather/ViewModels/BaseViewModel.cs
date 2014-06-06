using System;
using System.ComponentModel;
using System.Collections.Generic;

namespace CleverWeather.Shared
    {
    public class BaseViewModel : INotifyPropertyChanged //, INotifyPropertyChanging
        {
        public BaseViewModel()
            {
            }

        private string m_title = string.Empty;
        public const string TitlePropertyName = "Title";

        /// <summary>
        /// Gets or sets the "Title" property
        /// </summary>
        /// <value>The title.</value>
        public string Title
            {
            get { return m_title; }
            set { SetProperty(ref m_title, value, TitlePropertyName); }
            }

        private string m_subTitle = string.Empty;
        /// <summary>
        /// Gets or sets the "Subtitle" property
        /// </summary>
        public const string SubtitlePropertyName = "Subtitle";
        public string Subtitle
            {
            get { return m_subTitle; }
            set { SetProperty(ref m_subTitle, value, SubtitlePropertyName); }
            }

        private string m_icon = null;
        /// <summary>
        /// Gets or sets the "Icon" of the viewmodel
        /// </summary>
        public const string IconPropertyName = "Icon";
        public string Icon
            {
            get { return m_icon; }
            set { SetProperty(ref m_icon, value, IconPropertyName); }
            }

        private bool m_isBusy;
        /// <summary>
        /// Gets or sets if the view is busy.
        /// </summary>
        public const string IsBusyPropertyName = "IsBusy";
        public bool IsBusy
            {
            get { return m_isBusy; }
            set { SetProperty(ref m_isBusy, value, IsBusyPropertyName); }
            }

        private bool m_canLoadMore = true;
        /// <summary>
        /// Gets or sets if we can load more.
        /// </summary>
        public const string CanLoadMorePropertyName = "CanLoadMore";
        public bool CanLoadMore
            {
            get { return m_canLoadMore; }
            set { SetProperty(ref m_canLoadMore, value, CanLoadMorePropertyName); }
            }

        protected void SetProperty<T>
            (
            ref T backingStore, T value,
            string propertyName,
            Action onChanged = null
            //Action<T> onChanging = null
            )
            {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return;

            //if (onChanging != null)
            //    onChanging(value);

            //OnPropertyChanging(propertyName);

            backingStore = value;

            if (onChanged != null)
                onChanged();

            OnPropertyChanged(propertyName);
            }

        //#region INotifyPropertyChanging implementation
        //public event PropertyChangingEventHandler PropertyChanging;
        //#endregion

        //public void OnPropertyChanging(string propertyName)
        //    {
        //    if (PropertyChanging == null)
        //        return;
        //
        //    PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        //    }


        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public void OnPropertyChanged(string propertyName)
            {
            if (PropertyChanged == null)
                return;

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

