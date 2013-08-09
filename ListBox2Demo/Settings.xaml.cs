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
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO.IsolatedStorage;
using ListBox2Demo.Controls.LiveTileMedium;
using ListBox2Demo.Controls.LiveTileLarge;
using System.Collections.ObjectModel;
using Microsoft.Phone.Scheduler;
using ListBox2Demo;
using ListBox2Demo.ViewModels;

namespace ListBox2Demo
{
    public partial class Settings : PhoneApplicationPage 
    {
        public Settings()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            AutoCleanToggle.IsChecked = (App.Current as App).autoclean;
            FlipToggle.IsChecked = (App.Current as App).flip;

//             if ((App.Current as App).theme == "Black")
//             {
//                 ThemePicker.SelectedIndex = 0;
//             }
//             else
//             {
//                 ThemePicker.SelectedIndex = 1;
//             }

            base.OnNavigatedTo(e);
        }

        private void on_clear(object sender, EventArgs e)
        {
            if (MessageBox.Show("真的要清除所有数据吗？", "友情提示", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                foreach (object item in (App.Current as App).todos_Today)
                {
                    Todo todo = item as Todo;

                    // 删除时，注销提醒事件，再删除todo
                    if (ScheduledActionService.Find(todo.ReminderName) != null)
                        ScheduledActionService.Remove(todo.ReminderName);
                }

                // 再删除数据
                (App.Current as App).todos_Today.Clear();
            }
        }

        private void on_save_settings(object sender, EventArgs e)
        {
            (App.Current as App).autoclean = (bool)AutoCleanToggle.IsChecked;
            (App.Current as App).flip = (bool)FlipToggle.IsChecked;

            // 根据设置把不同的磁贴都特么更新一遍
            UpdateLiveTiles update = new UpdateLiveTiles();
            update.UpdateTiles();

            NavigationService.GoBack();
        }
    }
}