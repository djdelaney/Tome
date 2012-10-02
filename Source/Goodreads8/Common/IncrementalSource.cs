using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Goodreads8.Common
{
    public class IncrementalSource<T, K> : ObservableCollection<K>, ISupportIncrementalLoading
        where T : IPagedSource<K>, new()
    {
        private int VirtualTotal { get; set; }
        private int CurrentTotal { get; set; }
        private int CurrentPage { get; set; }
        private IPagedSource<K> Source { get; set; }
        private bool isLoading = false;

        public void UpdateArgument(object argument)
        {
            this.CurrentTotal = 0;
            this.CurrentPage = 0;
            CheckReentrancy();
            this.Items.Clear();
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            NotifyCollectionChangedEventArgs e =
            new NotifyCollectionChangedEventArgs
            (
                NotifyCollectionChangedAction.Reset
            );


            OnCollectionChanged(e);

            /*this.CurrentTotal = 0;
            this.CurrentPage = 0;
            this.Source.SetArguments(argument);
            //this.Clear();
            this.ClearItems();

            this.OnCollectionChanged

            this.OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Count"));
            this.OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("Item[]"));*/
            //this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            //this.OnPropertyChanged("Count");
        }


        public IncrementalSource(object argument)
        {
            this.Source = new T();
            this.Source.SetArguments(argument);
            this.VirtualTotal = int.MaxValue;
            this.CurrentTotal = 0;
            this.CurrentPage = 0;
        }

        #region ISupportIncrementalLoading

        public bool HasMoreItems
        {
            get { return this.CurrentTotal < this.VirtualTotal; }
        }

        // A delegate type for hooking up change notifications.
        public delegate void LoadingEventHandler();

        // An event that clients can use to be notified whenever the
        // elements of the list change.
        public event LoadingEventHandler BeginLoad;
        public event LoadingEventHandler EndLoad;

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            if (BeginLoad != null)
                BeginLoad();

            CoreDispatcher dispatcher = Window.Current.Dispatcher;

            return Task.Run<LoadMoreItemsResult>(
                async () =>
                {
                    lock (this)
                    {
                        if (isLoading)
                            return new LoadMoreItemsResult() { Count = 0 };

                        isLoading = true;
                    }

                    IPagedResponse<K> result = await this.Source.GetPage(++this.CurrentPage);

                    this.VirtualTotal = result.VirtualTotal;
                    this.CurrentTotal = result.CurrentTotal;

                    await dispatcher.RunAsync(
                        CoreDispatcherPriority.Normal,
                        () =>
                        {

                            if (EndLoad != null)
                                EndLoad();

                            foreach (K item in result.Items)
                                this.Add(item);

                            lock (this)
                            {
                                isLoading = false;
                            }
                        });

                    return new LoadMoreItemsResult() { Count = (uint)result.Items.Count() };

                }).AsAsyncOperation<LoadMoreItemsResult>();
        }

        #endregion
    }
}
