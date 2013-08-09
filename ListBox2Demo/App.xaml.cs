using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Resources;
using System.Xml;
using System.Xml.Linq;
using ListBox2Demo;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Scheduler;
using ListBox2Demo.ViewModels;


namespace ListBox2Demo
{
    class AssociationUriMapper : UriMapperBase
    {
        private string tempUri;

        public override Uri MapUri(Uri uri)
        {
            tempUri = System.Net.HttpUtility.UrlDecode(uri.ToString());
            if (tempUri.Contains("ido:"))
            {
                return new Uri("/MainPage.xaml", UriKind.Relative);
            }

            // Otherwise perform normal launch.
            return uri;
        }
    }

    public partial class App : Application
    {
        public Todo todo_edit { get; set; }
        public Todo todo_delete { get; set; }
        public ObservableCollection<Todo> todos_Today = new ObservableCollection<Todo>();
        public bool autoclean { get; set; }
        public bool flip { get; set; }
        UpdateLiveTiles update = new UpdateLiveTiles();
        
        /// <summary>
        ///提供对电话应用程序的根框架的轻松访问。
        /// </summary>
        /// <returns>电话应用程序的根框架。</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Application 对象的构造函数。
        /// </summary>
        public App()
        {
            // 未捕获的异常的全局处理程序。 
            UnhandledException += Application_UnhandledException;

            // 标准 Silverlight 初始化
            InitializeComponent();

            // 特定于电话的初始化
            InitializePhoneApplication();

            RootFrame.Style = App.Current.Resources["MyPhoneApplicationFrameStyle"] as Style;

            // 调试时显示图形分析信息。
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // 显示当前帧速率计数器。
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // 显示在每个帧中重绘的应用程序区域。
                //Application.Current.Host.Settings.EnableRedrawRegions = true；

                // 启用非生产分析可视化模式， 
                // 该模式显示递交给 GPU 的包含彩色重叠区的页面区域。
                //Application.Current.Host.Settings.EnableCacheVisualization = true；

                // 通过将应用程序的 PhoneApplicationService 对象的 UserIdleDetectionMode 属性
                // 设置为 Disabled 来禁用应用程序空闲检测。
                //  注意: 仅在调试模式下使用此设置。禁用用户空闲检测的应用程序在用户不使用电话时将继续运行
                // 并且消耗电池电量。
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }
        }

        private bool to_bool(string s)
        {
            return s != "False";
        }


        // 应用程序启动(例如，从“开始”菜单启动)时执行的代码
        // 此代码在重新激活应用程序时不执行
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
        }

        // 激活应用程序(置于前台)时执行的代码
        // 此代码在首次启动应用程序时不执行
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
        }

        // 停用应用程序(发送到后台)时执行的代码
        // 此代码在应用程序关闭时不执行
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            SaveData();
            SaveSettings();
        }

        // 应用程序关闭(例如，用户点击“后退”)时执行的代码
        // 此代码在停用应用程序时不执行
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            SaveData();
            SaveSettings();
            update.UpdateTiles();
        }

        // 导航失败时执行的代码
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // 导航已失败；强行进入调试器
                System.Diagnostics.Debugger.Break();
            }
        }

        // 出现未处理的异常时执行的代码
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // 出现未处理的异常；强行进入调试器
                System.Diagnostics.Debugger.Break();
            }
        }

        #region 首次运行
        /// <summary>
        /// 检查是否为首运行
        /// </summary>
        private void BuildDefaultSettings()
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            bool? firstrun;
            settings.TryGetValue("FirstRun", out firstrun);
            if (firstrun ?? true)
            {
                // 是首次运行
                todos_Today = BuildFirstRunData();
                SaveSettings(true);
                LoadSettings();

                firstrun = false;
                settings["FirstRun"] = firstrun;
            }
            else
            {
                LoadData();
                LoadSettings();
            }
        }

        /// <summary>
        /// 构建初次运行的数据
        /// </summary>
        /// <returns>ObservableCollection<Todo></returns>
        private ObservableCollection<Todo> BuildFirstRunData()
        {
            ObservableCollection<Todo> firstRunTodos = new ObservableCollection<Todo>();

            for (int i = 0; i < 5; i++)
            {
                Todo todo = new Todo();

                todo.Options = new List<Thing>()
                {
                    new Thing()
                };

                todo.IsCompleted = false;
                todo.Options.First().IsCompleted = false;
                todo.Importance = false;
                todo.HasAlarm = false;
                todo.Options.First().HasAlarm = false;
                todo.ReminderName = System.Guid.NewGuid().ToString();
                firstRunTodos.Add(todo);
            }

            firstRunTodos.ElementAt(0).Name = "向右滑以标记为完成";
            firstRunTodos.ElementAt(1).Name = "向左滑以标记为未完成";
            firstRunTodos.ElementAt(1).IsCompleted = true;
            firstRunTodos.ElementAt(1).Options.First().IsCompleted = true;
            firstRunTodos.ElementAt(2).Name = "长按以删除";
            firstRunTodos.ElementAt(3).Name = "点击条目并添加闹钟";
            firstRunTodos.ElementAt(4).Name = "点击条目并进行编辑";

            return firstRunTodos;
        }
        #endregion

        #region 初始化（读取设置、读取数据、注册闹钟）
        /// <summary>
        /// 读取设置
        /// </summary>
        private void LoadSettings()
        {
            XElement xml = null;

            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // 首次运行，要先检查Content.xml是否存在
                if (!storage.FileExists("Settings.xml"))
                {
                    return;
                }
                else
                {
                    using (IsolatedStorageFileStream stream = storage.OpenFile("Settings.xml", System.IO.FileMode.OpenOrCreate))
                    {
                        xml = XElement.Load(stream);
                        stream.Close();
                        storage.Dispose();
                    }
                }
            }

            if (xml == null)
            {
                return;
            }

            XElement general = xml.Element("General");
            XElement livetiles = xml.Element("LiveTiles");

            IEnumerable<XElement> collect = general.Elements();
            GetGeneralSettingsFromXml(collect);

            collect = livetiles.Elements();
            GetLiveTileSettingsFromXml(collect);

            return;
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        private void LoadData()
        {
            XElement xml = null;

            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // 先检查Content.xml是否存在
                if (!storage.FileExists("Content.xml"))
                {
                    return;
                }
                else
                {
                    using (IsolatedStorageFileStream stream = storage.OpenFile("Content.xml", System.IO.FileMode.OpenOrCreate))
                    {
                        xml = XElement.Load(stream);
                        stream.Close();
                        storage.Dispose();
                    }
                }
            }

            if (xml == null)
            {
                return;
            }

            XElement today = xml.Element("Today");

            IEnumerable<XElement> collect = today.Elements();
            GetItemsFromXml(collect, todos_Today);

            RegisterAlarms(todos_Today);

            return;
        }

        /// <summary>
        /// 注册闹钟
        /// </summary>
        /// <param name="todos"></param>
        private void RegisterAlarms(ObservableCollection<Todo> todos)
        {
            foreach (object item in todos)
            {
                Todo todo = item as Todo;

                if (todo.HasAlarm && (ScheduledActionService.Find(todo.ReminderName) == null))
                {
                    Reminder reminder = new Reminder(todo.ReminderName);
                    reminder.Title = "I.do 提醒~";
                    reminder.Content = todo.Name;
                    reminder.BeginTime = todo.AlarmTime;
                    //reminder.ExpirationTime = todo.AlarmTime + new DateTime(0, 0, 0, 0, 3, 0);
                    //reminder.RecurrenceType = recurrence;

                    // Register the reminder with the system.
                    ScheduledActionService.Add(reminder);
                }
            }
        }


        /// <summary>
        /// 将从XML文件获取的列表读取到内存中
        /// </summary>
        /// <param name="collect"></param>
        /// <param name="todos"></param>
        private void GetItemsFromXml(IEnumerable<XElement> collect,
            ObservableCollection<Todo> todos)
        {
            foreach (var item in collect)
            {
                Todo todo = new Todo();
                todo.Options = new List<Thing>
	            {
		            new Thing()
	            };

                // 根据文件内容进行变动
                todo.Name = item.Element("Name").Value;
                todo.HasAlarm = todo.Options.First().HasAlarm = to_bool(item.Element("HasAlarm").Value);
                todo.IsCompleted = todo.Options.First().IsCompleted = to_bool(item.Element("Completed").Value);
                todo.AlarmTime = DateTime.Parse(item.Element("AlarmTime").Value);

                // 闹钟时间早于当前时间了，就意味着这个闹钟已经被消除了
                if (todo.HasAlarm && todo.AlarmTime < DateTime.Now)
                {
                    todo.HasAlarm = todo.Options.First().HasAlarm = false;
                    todo.IsCompleted = true;
                    todo.Options.First().IsCompleted = true;
                }

                todo.Importance = to_bool(item.Element("Importance").Value);
                todo.IsExpanded = false;
                todo.ReminderName = item.Element("ReminderName").Value;

                todos.Add(todo);
            }
        }

        /// <summary>
        /// 获取常规设置
        /// </summary>
        /// <param name="collect"></param>
        private void GetGeneralSettingsFromXml(IEnumerable<XElement> collect)
        {
            autoclean = to_bool(collect.ElementAt(0).Value);
        }

        /// <summary>
        /// 获取Live Tiles设置
        /// </summary>
        /// <param name="collect"></param>
        private void GetLiveTileSettingsFromXml(IEnumerable<XElement> collect)
        {
            flip = to_bool(collect.First().Value);
        }
        
        #endregion

        #region 退出时的操作（保存设置、保存数据、清除已完成）
        /// <summary>
        /// 保存设置
        /// </summary>
        /// <param name="firstrun"></param>
        private void SaveSettings(bool firstrun = false)
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                XDocument _doc = new XDocument();

                // root
                XElement _root = new XElement("Settings");

                // general and livetiles
                XElement _general = new XElement("General");
                XElement _livetiles = new XElement("LiveTiles");

                // general items
                XElement _autoclean;

                // live tiles items
                XElement _flip;

                if (firstrun)
                {
                    _autoclean = new XElement("AutoClean", "False");
                    _flip = new XElement("Flip", "False");
                }
                else
                {
                    _autoclean = new XElement("AutoClean", autoclean.ToString());
                    _flip = new XElement("Flip", flip.ToString());
                }


                _general.Add(_autoclean/*, _theme*/);
                _livetiles.Add(_flip);

                _root.Add(_general, _livetiles);

                _doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), _root);

                IsolatedStorageFileStream location = new IsolatedStorageFileStream("Settings.xml",
                    System.IO.FileMode.Create,
                    System.IO.FileAccess.ReadWrite,
                    System.IO.FileShare.Read,
                    storage);

                System.IO.StreamWriter file = new System.IO.StreamWriter(location);

                _doc.Save(file);
                file.Dispose();
                location.Dispose();
            }
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        private void SaveData()
        {
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                XDocument _doc = new XDocument();

                // root
                XElement _root = new XElement("Ido");

                // today and future
                XElement _today = new XElement("Today");

                AddItems(_today);

                _root.Add(_today);

                _doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), _root);

                IsolatedStorageFileStream location = new IsolatedStorageFileStream("Content.xml",
                    System.IO.FileMode.Create,
                    System.IO.FileAccess.ReadWrite,
                    System.IO.FileShare.Read,
                    storage);

                System.IO.StreamWriter file = new System.IO.StreamWriter(location);

                _doc.Save(file);
                file.Dispose();
                location.Dispose();
            }
        }

        /// <summary>
        /// 自动清除已完成项目
        /// </summary>
        /// <param name="_todos"></param>
        private void AutoCleanItems(bool autoclean)
        {
            if (!autoclean)
            {
                return;
            }

            ObservableCollection<Todo> toDelete = new ObservableCollection<Todo>();

            foreach (object item in todos_Today)
            {
                Todo todo = item as Todo;
                if (todo.IsCompleted)
                {
                    toDelete.Add(todo);
                }
            }

            while (toDelete.Count > 0)
            {
                // 删除时，注销提醒事件，再删除todo
                if (ScheduledActionService.Find(toDelete[0].ReminderName) != null)
                    ScheduledActionService.Remove(toDelete[0].ReminderName);
                todos_Today.Remove(toDelete[0]);
                toDelete.RemoveAt(0);
            }
        }

        /// <summary>
        /// 将内存中的数据保存到文件
        /// </summary>
        /// <param name="_todos"></param>
        /// <param name="_day"></param>
        private void AddItems(XElement _day)
        {
            XElement _todo = null;
            XElement _name = null;
            XElement _hasalarm = null;
            XElement _alarmtime = null;
            XElement _importance = null;
            XElement _completed = null;
            XElement _remindername = null;

            // 如果选中了“自动清除已完成”
            AutoCleanItems(autoclean);

            foreach (object item in todos_Today)
            {
                Todo todo = item as Todo;

                _todo = new XElement("Todo");
                _name = new XElement("Name", todo.Name);
                _hasalarm = new XElement("HasAlarm", todo.HasAlarm.ToString());
                _alarmtime = new XElement("AlarmTime", todo.AlarmTime.ToString("yyyy-MM-dd HH:mm:ss"));
                _importance = new XElement("Importance", todo.Importance.ToString());
                _completed = new XElement("Completed", todo.IsCompleted.ToString());
                _remindername = new XElement("ReminderName", todo.ReminderName);

                _todo.Add(_name, _hasalarm, _alarmtime, _importance, _completed, _remindername);
                _day.Add(_todo);
            }
        }
        #endregion

        #region 电话应用程序初始化

        // 避免双重初始化
        private bool phoneApplicationInitialized = false;

        // 请勿向此方法中添加任何其他代码
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // 创建框架但先不将它设置为 RootVisual；这允许初始
            // 屏幕保持活动状态，直到准备呈现应用程序时。
            RootFrame = new TransitionFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;
            RootFrame.Navigating += RootFrame_Navigating;
            RootFrame.Dispatcher.BeginInvoke(BuildDefaultSettings);
            // Assign the URI-mapper class to the application frame.
            RootFrame.UriMapper = new AssociationUriMapper();


            // 处理导航故障
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;
            
            // 确保我们未再次初始化
            phoneApplicationInitialized = true;
        }

        // 请勿向此方法中添加任何其他代码
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // 设置根视觉效果以允许应用程序呈现
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // 删除此处理程序，因为不再需要它
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            // 如果自定义弹出框在显示，则隐藏它，并且取消导航
            if (MyToast.DialogIsOpen && e.NavigationMode == NavigationMode.Back)
            {
                e.Cancel = true;
            }
        }
        #endregion
    }
}