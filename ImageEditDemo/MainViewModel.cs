using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Img2DocxSlvrt;

namespace ImageEditDemo
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion // INotifyPropertyChanged Members


        private ObservableCollection<Item> _items = new ObservableCollection<Item>();
        public ObservableCollection<Item> Items
        {
            get { return _items; }
        }

        private Item _selectedItem;
        public Item SelectedItem
        {
            get { return _selectedItem; }
            set { _selectedItem = value; }
        }


        public MainViewModel()
        {
            //_items.Add(new Item { Filename = "picture1.jpg"});
            //_items.Add(new Item { Filename = "picture2.jpg" });
            //_items.Add(new Item { Filename = "picture3.jpg" });
            //_items.Add(new Item { Filename = "picture4.jpg" });
            //_items.Add(new Item { Filename = "picture5.jpg" });
        }
        //public void AddItem(Item item)
        ///{
          //  _items.Add(item);
            //_items.Add(new Item{Filename = itemName });
            //OnPropertyChanged("Items");
        //}
        public void Delete()
        {
            if (SelectedItem != null)
                Items.Remove(SelectedItem);
        }
    }
}
