using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Collections.ObjectModel;
using System.ComponentModel;
using NLog;

namespace ListCollectionViewAsynchronousTest
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        #region プロパティ

        //ログ用
        private static Logger logger = LogManager.GetCurrentClassLogger();

        [Serializable]
        public class MDB
        {
            public string OrderingValue { get; set; }
            public string Item1 { get; set; }
            public string Item2 { get; set; }
            public string Item3 { get; set; }
        }

        [Serializable]
        public class WCDB : MDB
        {
        }

        [Serializable]
        public class DCWCDB : WCDB
        {
        }

        [Serializable]
        public class JAMTD : DCWCDB
        {
            public string Item4 { get; set; }
            public string Item5 { get; set; }
        }

        private List<MDB> List = new List<MDB>();
        private List<MDB> List2 = new List<MDB>();

        public ObservableCollection<MDB> testList;

        public ObservableCollection<MDB> TestList
        {
            get
            {
                return testList;
            }
            set
            {
                if (value != this.testList)
                {
                    this.testList = value;
                }
            }
        }

        public ListCollectionView testViewList;

        public ListCollectionView TestViewList
        {
            get
            {
                return testViewList;
            }
            set
            {
                if (value != this.testViewList)
                {
                    this.testViewList = value;
                }
            }
        }

        public MDB selectedTestItem;

        public MDB SelectedTestItem
        {
            get
            {
                return selectedTestItem;
            }
            set
            {
                if (value != this.selectedTestItem)
                {
                    this.selectedTestItem = value;
                }
            }
        }

        DispatcherTimer dispatcherTimer;

        private readonly object SyncObj = new object();

        public int count = 0;
        public Boolean isRunning = false;

        #endregion

        #region メイン処理と実行停止処理

        public MainWindow()
        {
            InitializeComponent();

            //for (int i = 0; i < 500; i++)
            //{
            //    this.List.Add(new MDB() { OrderingValue = count++.ToString("000000000"), Item1 = "q", Item2 = "7", Item3 = "い" });
            //}

            this.TestList = new ObservableCollection<MDB>(List);

            var view = new ListCollectionView(this.TestList);

            view.SortDescriptions.Add(new SortDescription("OrderingValue", ListSortDirection.Descending));
            view.MoveCurrentTo(null);

            this.TestViewList = view;

            DataContext = this;

            dispatcherTimer = new DispatcherTimer(DispatcherPriority.Normal);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            dispatcherTimer.Start();
        }


        public void RunCMD(object sender, RoutedEventArgs e)
        {
            logger.Info("Runボタン押下");
            this.TestViewList.NewItemPlaceholderPosition = NewItemPlaceholderPosition.None;
            if (!isRunning)
            {
                isRunning = true;

                //並び替えをぶつける
                //dispatcherTimer.Tick += new EventHandler(ListSort);
                //dispatcherTimer.Tick += new EventHandler(ListSortDescending);

                //Model側の検証
                //dispatcherTimer.Tick += new EventHandler(ListAdd);
                //dispatcherTimer.Tick += new EventHandler(ListFindAllOrderBy);
                //dispatcherTimer.Tick += new EventHandler(ListOrderByToList);

                //ViewModel側の検証
                dispatcherTimer.Tick += new EventHandler(ObservalCollectionAdd);
                dispatcherTimer.Tick += new EventHandler(ListCollectionViewGetItemAt);
                dispatcherTimer.Tick += new EventHandler(ListCollectionViewRemoveAt);
                dispatcherTimer.Tick += new EventHandler(makeDisplayData);
                logger.Info("イベント実行完了");
            }
        }

        public void StopCMD(object sender, RoutedEventArgs e)
        {
            logger.Info("Stopボタン押下");
            if (isRunning)
            {
                isRunning = false;
                //並び替えをぶつける
                //dispatcherTimer.Tick -= new EventHandler(ListSort);
                //dispatcherTimer.Tick -= new EventHandler(ListSortDescending);

                //Model側の検証
                //dispatcherTimer.Tick -= new EventHandler(ListAdd);
                //dispatcherTimer.Tick -= new EventHandler(ListFindAllOrderBy);
                //dispatcherTimer.Tick -= new EventHandler(ListOrderByToList);

                //ViewModel側の検証
                dispatcherTimer.Tick -= new EventHandler(ObservalCollectionAdd);
                dispatcherTimer.Tick -= new EventHandler(ListCollectionViewGetItemAt);
                dispatcherTimer.Tick -= new EventHandler(ListCollectionViewRemoveAt);
                dispatcherTimer.Tick -= new EventHandler(makeDisplayData);
                logger.Info("イベント停止完了");
            }
        }

        #endregion

        #region Listのソートを二つぶつける
        //以下ソート2つを排他なしでぶつけるとNullReferenceExceptionの例外を吐く

        public void ListSort(object sender, EventArgs e)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                try
                {
                    this.List.Sort((x, y) => string.Compare(x.OrderingValue, y.OrderingValue));
                    logger.Info("Worked");
                }
                catch (Exception)
                {
                    logger.Error("{0}", e.ToString());
                }
            });
        }


        public void ListSortDescending(object sender, EventArgs e)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                try
                {
                    this.List.Sort((x, y) => string.Compare(y.OrderingValue, x.OrderingValue));
                    logger.Info("Worked");
                }
                catch (Exception)
                {
                    logger.Error("{0}", e.ToString());
                }
            });
        }

        #endregion

        #region Model側の検証

        public void ListAdd(object sender, EventArgs e)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                this.List.Add(new MDB() { OrderingValue = count++.ToString("000000000"), Item1 = "q", Item2 = "7", Item3 = "い" });
                logger.Info("Worked List.Add");
                this.List2.Add(new MDB() { OrderingValue = count++.ToString("000000000"), Item1 = "r", Item2 = "8", Item3 = "ろ" });
                logger.Info("Worked List2.Add");
            });
        }

        public void ListFindAllOrderBy(object sender, EventArgs e)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                try
                {
                    var list = this.List.FindAll(v => v.Item1 == "q").OrderBy(v => v.OrderingValue);
                    logger.Info("Worked");
                }
                catch (NullReferenceException)
                {
                    logger.Error("NullReferenceException");
                }
            });
        }

        public void ListOrderByToList(object sender, EventArgs e)
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                try
                {
                    this.List2 = this.List2.OrderBy(v => v.OrderingValue).ToList();
                    logger.Info("Worked");
                }
                catch (NullReferenceException)
                {
                    logger.Error("NullReferenceException");
                }
            });
        }



        #endregion

        #region ViewModel側の検証

        public void ObservalCollectionAdd(object sender, EventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.TestList.Add(new JAMTD() { OrderingValue = count++.ToString("000000000"), Item1 = "q", Item2 = "7", Item3 = "い", Item4 = "p", Item5 = "6"});
                logger.Info("Worked");

                if (count > 999999999) count = 0;

            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        public void ListCollectionViewGetItemAt(object sender, EventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.SelectedTestItem = (MDB)this.TestViewList.GetItemAt(this.TestViewList.Count - 1);
                logger.Info("Worked");
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        public void ListCollectionViewRemoveAt(object sender, EventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                while (this.TestViewList.Count > 50)
                {
                    this.TestViewList.RemoveAt(this.TestViewList.Count - 1);
                    logger.Info("Worked");
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        public void makeDisplayData(object sender, EventArgs e)
        {
            try
            {
                var list = this.GetTestList<DCWCDB>();
                logger.Info("Worked GetTestList");
                list.Sort((x, y) => string.Compare(x.OrderingValue, y.OrderingValue));
                logger.Info("Worked Sort");
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public List<T> GetTestList<T>()
        {
            if (this.testList == null)
            {
                logger.Info("testList was null");
                return new List<T>();
            }
            return this.testList.Cast<T>().ToList();
        }

        #endregion
    }
}
