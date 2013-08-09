using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System.IO; 
 

namespace ListBox2Demo
{
    public partial class About : PhoneApplicationPage
    {
        public About()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Uri manifest = new Uri("WMAppManifest.xml", UriKind.Relative);
            var si = Application.GetResourceStream(manifest);
            if (si != null)
            {
                using (StreamReader sr = new StreamReader(si.Stream))
                {
                    bool haveApp = false;
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (!haveApp)
                        {
                            int i = line.IndexOf("AppPlatformVersion=\"", StringComparison.InvariantCulture);
                            if (i >= 0)
                            {
                                haveApp = true;
                                line = line.Substring(i + 20);

                                int z = line.IndexOf("\"");
                                if (z >= 0)
                                {
                                    // if you're interested in the app plat version at all
                                    // AppPlatformVersion = line.Substring(0, z);
                                }
                            }
                        }

                        int y = line.IndexOf("Version=\"", StringComparison.InvariantCulture);
                        if (y >= 0)
                        {
                            int z = line.IndexOf("\"", y + 9, StringComparison.InvariantCulture);
                            if (z >= 0)
                            {
                                // We have the version, no need to read on.
                                _versionText.Text = line.Substring(y + 9, z - y - 9);
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                _versionText.Text = "Unknown";
            }
            base.OnNavigatedTo(e);
        }

        private void on_send_email(object sender, RoutedEventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();

            emailComposeTask.Subject = "问题反馈";
            emailComposeTask.Body = "";
            emailComposeTask.To = "serious198706@gmail.com";
            emailComposeTask.Body = "\n\n\n来自I.DO";

            emailComposeTask.Show();
        }

        private void on_comment(object sender, RoutedEventArgs e)
        {
            MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();

            marketplaceReviewTask.Show();
        }
    }
}