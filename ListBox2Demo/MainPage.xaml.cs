/*
 * Windows Phone ListBox 实现下拉刷新，上拉加载更多。
 * Powered By 博客园 Kangzubin
 * 2013-03-11
 */

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
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using ListBox2Demo.Controls;
using System.Diagnostics;
using System.Windows.Data;
using ListBox2Demo;
using Microsoft.Phone.Scheduler;
using System.Collections;
using ListBox2Demo.ViewModels;


namespace ListBox2Demo
{
    #region Converters

    #endregion

    public partial class MainPage : PhoneApplicationPage
    {
        bool isFromExpandedItem = false;

        private Todo lastTodo;
        private Todo todo = null;

        // 构造函数
        public MainPage()
        {
            InitializeComponent();

            this.ListBox_Today.ItemsSource = (App.Current as App).todos_Today;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if ((App.Current as App).todo_edit != null)
            {
                // 从编辑页面回来之后，要更新当前的todo
                //todo = (App.Current as App).todo_edit;
            }

            base.OnNavigatedTo(e);
        }

        private void expandes_click(object sender, RoutedEventArgs e)
        {
            // 因为点击子项也属于点击该Item，所以要判断上次的点击
            // 是否来自子项，如果是，则不进行Expand的处理
            if (isFromExpandedItem)
            {
                isFromExpandedItem = false;
                return;
            }

            // 获取被点击的todo
            todo = ListBox_Today.SelectedItem as Todo;
            
            if ((lastTodo != null) & (lastTodo != todo))
            {
                lastTodo.IsExpanded = false;
            }

            isFromExpandedItem = false;
            lastTodo = todo;
        }

        private void on_set_important(object sender, RoutedEventArgs e)
        {
            if (todo != null)
            {
                todo.Importance = !todo.Importance;
            }

            isFromExpandedItem = true;
        }

        private void on_set_alarm(object sender, RoutedEventArgs e)
        {
            if (todo != null)
            {
                Thing thing = todo.Options.First();

                // 判断之前是否有闹钟，如果没有，则直接进入设置界面
                if (!todo.HasAlarm)
                {
                    (App.Current as App).todo_edit = todo;

                    NavigationService.Navigate(new Uri("/Edit.xaml?action=edit&page=1", UriKind.Relative));
                }
                else
                {
                    thing.HasAlarm = !thing.HasAlarm;
                    todo.HasAlarm = !todo.HasAlarm;

                    // 如果设置成“无闹钟”，则要把已添加的Schedule取消掉
                    if (ScheduledActionService.Find(todo.ReminderName) != null)
                        ScheduledActionService.Remove(todo.ReminderName);
                }
            }

            isFromExpandedItem = true;
        }

        private void on_set_done(object sender, RoutedEventArgs e)
        {
            if (todo != null)
            {
                Thing thing = todo.Options.First();

                thing.IsCompleted = !thing.IsCompleted;
                todo.IsCompleted = !todo.IsCompleted;
            }
            
            isFromExpandedItem = true;
        }

        private void on_edit(object sender, RoutedEventArgs e)
        {
            if (todo != null)
            {
                (App.Current as App).todo_edit = todo;

                NavigationService.Navigate(new Uri("/Edit.xaml?action=edit", UriKind.Relative));
            }

            isFromExpandedItem = true;
        }

        private void on_add_new(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Edit.xaml?action=addnew", UriKind.Relative));
        }

        private void ListItem_Hold(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            var holdItem = (sender as StackPanel).DataContext;

            Todo todo = holdItem as Todo;

            #region SilverLight BUG
            // 居然有BUG，次奥。是SilverLight的问题吗？因为toolkit来自SilverLight
            // 在某个item上长按时，会触发两次事件，这样就需要判断一下
            // 如果第二次的跟第一次的不一样，就无视之
            if ((todo != (App.Current as App).todo_delete) && ((App.Current as App).todo_delete != null))
            {
                return;
            }
            #endregion
            

            (App.Current as App).todo_delete = todo;
        }

        private void delete_one(ObservableCollection<Todo> todos)
        {
            if (ScheduledActionService.Find((App.Current as App).todo_delete.ReminderName) != null)
                ScheduledActionService.Remove((App.Current as App).todo_delete.ReminderName);
            todos.Remove((App.Current as App).todo_delete);
        }


        private void delete_all(ObservableCollection<Todo> todos)
        {
            if (MessageBox.Show("您确定要清除所有已完成项吗？", "友情提示", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
            {
                return;
            }

            // 如果边遍历边删除的话，就会崩溃
            // 所以要把所有需要删除的todo都收集起来，再挨个删除
            List<Todo> toDelete = new List<Todo>();

            foreach (object item in todos)
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

                todos.Remove(toDelete[0]);
                toDelete.RemoveAt(0);
            }
        }

        private void on_delete_one(object sender, EventArgs e)
        {
            if ((App.Current as App).todo_delete != null)
            {
                delete_one((App.Current as App).todos_Today);
            }
            
            (App.Current as App).todo_delete = null;
        }


        private void on_delete_all(object sender, EventArgs e)
        {
            delete_all((App.Current as App).todos_Today);
        }

        private void on_help(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Help.xaml", UriKind.Relative));
        }

        private void on_about(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        private void on_settings(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Settings.xaml", UriKind.Relative));
        }

        #region Exit Confirm
        DateTime lastTime = DateTime.MinValue;
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            DateTime now = DateTime.Now;
            if ((now - lastTime) >= new TimeSpan(0, 0, 0, 0, 2000))
            {
                lastTime = now;
                MyToast.ShowToastMessage("再按一次返回键退出程序");
                e.Cancel = true;
            }
            else
            {
                base.OnBackKeyPress(e);
            }
        }
        #endregion

        private void ExpanderView_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            ExpanderView ev = sender as ExpanderView;
            Todo todo = (Todo)ev.Expander;

            // 向右滑动，设为完成
            if (e.TotalManipulation.Translation.X >= 50)
            {
                todo.IsCompleted = true;
                todo.Options.First().IsCompleted = true;
                todo.HasAlarm = todo.Options.First().HasAlarm = false;

                if (ScheduledActionService.Find(todo.ReminderName) != null)
                    ScheduledActionService.Remove(todo.ReminderName);
            }

            // 向左滑动，设为未完成
            if (e.TotalManipulation.Translation.X <= -50)
            {
                todo.IsCompleted = false;
                todo.Options.First().IsCompleted = false;

                if (todo.HasAlarm && (ScheduledActionService.Find(todo.ReminderName) == null))
                {
                    Reminder reminder = new Reminder(todo.ReminderName);
                    reminder.Title = "I.do 提醒~";
                    reminder.Content = todo.Name;
                    reminder.BeginTime = todo.AlarmTime;

                    ScheduledActionService.Add(reminder);
                }
            }
        }
    }
}