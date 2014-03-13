using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using dBTutorial.Resources;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace dBTutorial
{
    public partial class MainPage : PhoneApplicationPage, INotifyPropertyChanged
    {
        // Data context for the local database
        private ToDoDataContext toDoDB;

        // Define an observable collection property that controls can bind to.
        private ObservableCollection<ToDoItem> _toDoItems;
        public ObservableCollection<ToDoItem> ToDoItems
        {
            get
            {
                return _toDoItems;
            }
            set
            {
                if (_toDoItems != value)
                {
                    _toDoItems = value;
                    NotifyPropertyChanged("ToDoItems");
                }
            }
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Connect to the database and instantiate data context.
            //To see how we create the database, take a look at App.xaml.cs
            toDoDB = new ToDoDataContext(ToDoDataContext.DBConnectionString);

            // Data context and observable collection are children of the main page.
            this.DataContext = this;
        }


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Define the query to gather all of the to-do items.
            var toDoItemsInDB = from ToDoItem todo in toDoDB.ToDoItems
                                select todo;

            // Execute the query and place the results into a collection.
            ToDoItems = new ObservableCollection<ToDoItem>(toDoItemsInDB);

            // Call the base method.
            base.OnNavigatedTo(e);
        }

        private void newToDoTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Clear the text box when it gets focus.
            newToDoTextBox.Text = String.Empty;
        }

        private void newToDoAddButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a new to-do item based on the text box.
            ToDoItem newToDo = new ToDoItem { ItemName = newToDoTextBox.Text };

            // Add a to-do item to the observable collection.
            ToDoItems.Add(newToDo);

            // Add a to-do item to the local database.
            toDoDB.ToDoItems.InsertOnSubmit(newToDo);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Call the base method.
            base.OnNavigatedFrom(e);

            // Save changes to the database.
            toDoDB.SubmitChanges();
        }

        private void deleteTaskButton_Click(object sender, RoutedEventArgs e)
        {
            // Cast parameter as a button.
            var button = sender as Button;

            if (button != null)
            {
                // Get a handle for the to-do item bound to the button.
                ToDoItem toDoForDelete = button.DataContext as ToDoItem;

                // Remove the to-do item from the observable collection.
                ToDoItems.Remove(toDoForDelete);

                // Remove the to-do item from the local database.
                toDoDB.ToDoItems.DeleteOnSubmit(toDoForDelete);

                // Save changes to the database.
                toDoDB.SubmitChanges();

                // Put the focus back to the main page.
                this.Focus();
            }
        }


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the app that a property has changed.
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
    /*In the MainPage.xaml.cs we create a Data Context. This more or less turns the 
     * database into an object that can be used as other ojects instead of needing to 
     * always interact with the database using querys like you would have to in javascript */
    //Calls Base Constructor and declares the database table named ToDoItems
    public class ToDoDataContext : DataContext
    {
        // Specify the connection string as a static, used in main page and app.xaml.
        public static string DBConnectionString = "Data Source=isostore:/ToDo.sdf";

        // Pass the connection string to the base class.
        public ToDoDataContext(string connectionString)
            : base(connectionString)
        { }

        // Specify a single table for the to-do items.
        public Table<ToDoItem> ToDoItems;
    }

    //This refers to the table. In this case it is called ToDoItem (it will store the ToDoItems)
    [Table]
    public class ToDoItem : INotifyPropertyChanged, INotifyPropertyChanging
    {
        // Define ID: private field, public property and database column.
        private int _toDoItemId;

        [Column(IsPrimaryKey = true, IsDbGenerated = true, DbType = "INT NOT NULL Identity", CanBeNull = false, AutoSync = AutoSync.OnInsert)]
        public int ToDoItemId
        {
            get
            {
                return _toDoItemId;
            }
            set
            {
                if (_toDoItemId != value)
                {
                    NotifyPropertyChanging("ToDoItemId");
                    _toDoItemId = value;
                    NotifyPropertyChanged("ToDoItemId");
                }
            }
        }

        //Item name Column. Private string will be accessed by the public column
        // Define item name: private field, public property and database column.
        private string _itemName;

        [Column]
        public string ItemName
        {
            get
            {
                return _itemName;
            }
            set
            {
                if (_itemName != value)
                {
                    NotifyPropertyChanging("ItemName");
                    _itemName = value;
                    NotifyPropertyChanged("ItemName");
                }
            }
        }

        //Column that defines whether or not a todo item is completed
        // Define completion value: private field, public property and database column.
        private bool _isComplete;

        [Column]
        public bool IsComplete
        {
            get
            {
                return _isComplete;
            }
            set
            {
                if (_isComplete != value)
                {
                    NotifyPropertyChanging("IsComplete");
                    _isComplete = value;
                    NotifyPropertyChanged("IsComplete");
                }
            }
        }

        //This will update performance using INotifyPropertyChanged
        //This will also help us with databinding (making the database into an object
        //that we then access like any other object as opposed to having to query)
        // Version column aids update performance.
        [Column(IsVersion = true)]
        private Binary _version;

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        // Used to notify the page that a data context property changed
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanging Members

        public event PropertyChangingEventHandler PropertyChanging;

        // Used to notify the data context that a data context property is about to change
        private void NotifyPropertyChanging(string propertyName)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        #endregion
    }

}