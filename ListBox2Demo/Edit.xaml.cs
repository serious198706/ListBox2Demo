using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Data;
using System.ComponentModel;
using Microsoft.Phone.Scheduler;
using ListBox2Demo.ViewModels;
using System.Windows.Media;

namespace ListBox2Demo
{
    public partial class Page1 : PhoneApplicationPage
    {
        public bool inPage;
        string msg;
        Todo todo_edit;

        Dictionary<string, double> ForwardListPickerDictionary;

        public Page1()
        {
            InitializeComponent();
            inPage = false;

            ForwardListPickerDictionary = new Dictionary<string, double>();

            ForwardListPickerDictionary.Add("5分钟", 5);
            ForwardListPickerDictionary.Add("10分钟", 10);
            ForwardListPickerDictionary.Add("30分钟", 30);
            ForwardListPickerDictionary.Add("1小时", 60);
            ForwardListPickerDictionary.Add("2小时", 120);

            ForwardListPicker.ItemsSource = ForwardListPickerDictionary.Keys;

            this.Loaded += Page1_Loaded;
        }

        void Page1_Loaded(object sender, RoutedEventArgs e)
        {
            switch (editPivot.SelectedIndex)
            {
                case 0:
                    contentBox.Focus();
                    break;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // 居然还要判断是不是从当前页面的子页面导航回来的
            // 因为在选择日期时，确定后会再次触发OnNavigatedTo事件，会再进行一次赋值
            // 为了避免这种情况发生，要设一个标志位。。。
            if (inPage)
            {
                base.OnNavigatedTo(e);
                return;
            }

            NavigationContext.QueryString.TryGetValue("action", out msg);


            // 如果导航来时有edit参数，则代表要修改一个项目
            if (msg == "edit")
            {
                // 获取要修改的todo
                todo_edit = (App.Current as App).todo_edit;

                contentBox.Text = todo_edit.Name;
                AlarmToggle.IsChecked = todo_edit.HasAlarm;
                
                DatePicker.Value = todo_edit.AlarmTime;
                TimePicker.Value = todo_edit.AlarmTime;
                ImportanceToggle.IsChecked = todo_edit.Importance;
                ForwardListPicker.SelectedIndex = 1;

                string page = null;
                if(NavigationContext.QueryString.TryGetValue("page", out page))
                {
                    if (page == "1")
                    {
                        AlarmToggle.IsChecked = true;
                        editPivot.SelectedIndex = 1;
                    }
                }
            }

            // 如果导航来时有addnew参数，则代表要添加一个新的项目
            else if (msg == "addnew")
            {
                ImportanceToggle.IsChecked = false;
                AlarmToggle.IsChecked = false;
                DatePicker.Value = DateTime.Now;
                TimePicker.Value = DateTime.Now;

                ForwardListPicker.SelectedIndex = 1;
            }

            inPage = true;
            base.OnNavigatedTo(e);
        }

        private void save(object sender, EventArgs e)
        {
            if (contentBox.Text == "")
            {
                MessageBox.Show("您还没有填写内容哦~", "友情提示", MessageBoxButton.OK);
                editPivot.SelectedIndex = 0;
                return;
            }

            if (msg == "edit")
            {
                // 存储修改项的内容
                todo_edit.Name = contentBox.Text;
                
                // 时间设置有误
                if ((todo_edit.AlarmTime = GetMeAlarmTime()) == DateTime.MinValue)
                {
                    MessageBox.Show("您设置的闹钟时间比当前时间早哟~", "友情提示", MessageBoxButton.OK);
                    return;
                }

                // 既然进行了编辑，就意味着任务未完成
                todo_edit.IsCompleted = todo_edit.Options.First().IsCompleted = false;
                todo_edit.HasAlarm = todo_edit.Options.First().HasAlarm =  (bool)AlarmToggle.IsChecked;
                todo_edit.Importance = (bool)ImportanceToggle.IsChecked;

                if (todo_edit.HasAlarm)
                {
                    Reminder reminder = new Reminder(todo_edit.ReminderName);
                    reminder.Title = "提醒";
                    reminder.Content = todo_edit.Name;
                    reminder.BeginTime = todo_edit.AlarmTime;

                    if (ScheduledActionService.Find(reminder.Name) != null)
                    {
                        ScheduledActionService.Remove(reminder.Name);
                        ScheduledActionService.Add(reminder);
                    }
                    else
                    {
                        ScheduledActionService.Add(reminder);
                    }
                }
            }
            else if (msg == "addnew")
            {
                string date = DatePicker.Value.Value.ToShortDateString();
                string time = TimePicker.Value.Value.ToShortTimeString();

                Todo todo = new Todo();
                Thing thing = new Thing();

                todo.Name = contentBox.Text;

                // 时间设置有误
                if ((todo.AlarmTime = GetMeAlarmTime()) == DateTime.MinValue)
                {
                    MessageBox.Show("您设置的闹钟时间比当前时间早哟~", "友情提示", MessageBoxButton.OK);
                    return;
                }

                todo.IsCompleted = thing.IsCompleted = false;
                todo.HasAlarm = thing.HasAlarm = (bool)AlarmToggle.IsChecked;
                todo.Importance = (bool)ImportanceToggle.IsChecked;

                // 使用GUID作为每一个todo的ReminderName
                todo.ReminderName = System.Guid.NewGuid().ToString();
                todo.IsExpanded = false;
                todo.Options = new List<Thing>();
                todo.Options.Add(thing);

                (App.Current as App).todos_Today.Add(todo);

                if (todo.HasAlarm)
                {
                    Reminder reminder = new Reminder(todo.ReminderName);
                    reminder.Title = "提醒";
                    reminder.Content = todo.Name;
                    reminder.BeginTime = todo.AlarmTime;

                    ScheduledActionService.Add(reminder);
                }
            }
            
            inPage = false;
            NavigationService.GoBack();
        }

        private void cancel(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }


        private void on_checked(object sender, RoutedEventArgs e)
        {
            showDateAndTime.Begin();
        }

        private void on_unchecked(object sender, RoutedEventArgs e)
        {
            hideDateAndTime.Begin();
        }

        private DateTime GetMeAlarmTime()
        {
            DateTime alarmTime = new DateTime();

            switch (TimeSetTypePivot.SelectedIndex)
            {
                // 延迟方式
                case 0:
                    alarmTime = DateTime.Now.AddMinutes(ForwardListPickerDictionary[(ForwardListPicker.SelectedItem as string)]);
                    break;
                // 自定义方式
                case 1:
                    {
                        string date = DatePicker.Value.Value.ToShortDateString();
                        string time = TimePicker.Value.Value.ToShortTimeString();

                        // 如果时间设置得比当前时间要“早”，并且闹钟为开启
                        if ((DateTime.Parse(date + " " + time) < DateTime.Now) && (bool)AlarmToggle.IsChecked)
                        {
                            return DateTime.MinValue;
                        }

                        alarmTime = DateTime.Parse(date + " " + time);
                    }

                    break;
            }

            return alarmTime;
        }

        private void PhoneTextBox_ActionIconTapped(object sender, EventArgs e)
        {
            contentBox.Text = "";
            contentBox.Focus();
        }

        private void contentBox_GotFocus(object sender, RoutedEventArgs e)
        {
            (sender as PhoneTextBox).Background = new SolidColorBrush(Colors.Transparent);
        }
    }
}