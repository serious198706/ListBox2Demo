using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Animation;
using System.Threading;
using System.Threading.Tasks;

namespace I_DO
{
    public partial class MyToast : UserControl
    {
        public MyToast()
        {
            InitializeComponent();

            gridDialog.Visibility = System.Windows.Visibility.Collapsed;

            // 把第一个实例赋值给全局静态对象
            if (_instance == null)
                _instance = this;
        }

        #region 自定义MessageBox
        //用来控制异步线程中 弹出框 结果返回的时机
        private static AutoResetEvent myResetEvent = new AutoResetEvent(false);

        static MessageBoxResult messageBoxResult;

        //用一个单一实例，使得应用中的所有页面使用同一个实例
        static MyToast _instance;
        static MyToast Instance
        {
            get
            {
                //if (_instance == null)
                //    _instance = new MyDialog();

                return _instance;
            }
        }

        // 用来控制当弹出框显示的时候，如果用户点击 Back 按键，则隐藏弹出框，
        // 在 App.xaml.cs 中的 RootFrame_Navigating 事件中调用
        public static bool DialogIsOpen
        {
            get
            {
                if (Instance != null && Instance.gridDialog.Visibility == Visibility.Visible)
                {
                    Instance.btnCancle_Click(null, null);

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 显示包含指定文本和“确定”按钮的消息框
        /// </summary>
        /// <param name="messageBoxText">要显示的消息</param>
        /// <returns> 在所有情况下均为 System.Windows.MessageBoxResult.OK</returns>
        public static Task<MessageBoxResult> Show(string messageBoxText)
        {
            return Task<MessageBoxResult>.Factory.StartNew(() =>
            {
                Instance.Dispatcher.BeginInvoke(delegate
                {
                    Instance.gridDialog.Visibility = Visibility.Visible;

                    Instance.contentContainer.Content = messageBoxText;

                    Instance.txtTitle.Text = "";

                    Instance.btnCancle.Visibility = Visibility.Collapsed;


                    Instance.ShowMessageBoxSB.Stop();
                    Instance.ShowMessageBoxSB.Begin();
                });

                myResetEvent.WaitOne();
                return messageBoxResult;
            });
        }

        /// <summary>
        /// 显示包含指定文本、标题栏标题和响应按钮的消息框
        /// </summary>
        /// <param name="messageBoxText">要显示的消息</param>
        /// <param name="caption">消息框的标题</param>
        /// <param name="button">一个值，用于指示要显示哪个按钮或哪些按钮</param>
        /// <returns>一个值，用于指示用户对消息的响应</returns>
        public static Task<MessageBoxResult> Show(string messageBoxText, string caption, MessageBoxButton button)
        {
            return Task<MessageBoxResult>.Factory.StartNew(() =>
            {
                Instance.Dispatcher.BeginInvoke(delegate
                {

                    Instance.gridDialog.Visibility = Visibility.Visible;

                    Instance.contentContainer.Content = messageBoxText;

                    Instance.txtTitle.Text = caption;

                    if (button == MessageBoxButton.OKCancel)
                    {
                        Instance.btnCancle.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Instance.btnCancle.Visibility = Visibility.Collapsed;
                    }

                    //Instance.UpdateLayout();


                    Instance.ShowMessageBoxSB.Stop();
                    Instance.ShowMessageBoxSB.Begin();
                });


                myResetEvent.WaitOne();

                return messageBoxResult;
            });
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            txtTitle.Text = "";
            contentContainer.Content = null;

            gridDialog.Visibility = System.Windows.Visibility.Collapsed;

            messageBoxResult = MessageBoxResult.OK;

            // 使异步线程的 Show() 方法继续执行
            myResetEvent.Set();
        }

        private void btnCancle_Click(object sender, RoutedEventArgs e)
        {
            txtTitle.Text = "";
            contentContainer.Content = null;

            gridDialog.Visibility = System.Windows.Visibility.Collapsed;

            messageBoxResult = MessageBoxResult.Cancel;

            myResetEvent.Set();
        }
        #endregion

        #region 自定义Toast
        //         const string myToast = @" <Storyboard xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
        //                 <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(CompositeTransform.TranslateY)"">
        //                     <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
        //                     <EasingDoubleKeyFrame KeyTime=""0:0:2"" Value=""70"">
        //                         <EasingDoubleKeyFrame.EasingFunction>
        //                             <ExponentialEase EasingMode=""EaseOut"" Exponent=""6""/>     
        //                         </EasingDoubleKeyFrame.EasingFunction>
        //                     </EasingDoubleKeyFrame>
        //                     <EasingDoubleKeyFrame KeyTime=""0:0:9"" Value=""-70""/>
        //                     <EasingDoubleKeyFrame KeyTime=""0:0:11"" Value=""0"">
        //                         <EasingDoubleKeyFrame.EasingFunction>
        //                            <ExponentialEase EasingMode=""EaseInOut"" Exponent=""6""/>
        //                         </EasingDoubleKeyFrame.EasingFunction>
        //                     </EasingDoubleKeyFrame>
        //                 </DoubleAnimationUsingKeyFrames>
        //                 <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Opacity)"">
        //                     <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
        //                     <EasingDoubleKeyFrame KeyTime=""0:0:2"" Value=""1""/>
        //                     <EasingDoubleKeyFrame KeyTime=""0:0:9"" Value=""1""/>
        //                     <EasingDoubleKeyFrame KeyTime=""0:0:11"" Value=""0"">
        //                         <EasingDoubleKeyFrame.EasingFunction>
        //                             <ExponentialEase EasingMode=""EaseIn"" Exponent=""6""/>
        //                         </EasingDoubleKeyFrame.EasingFunction>
        //                     </EasingDoubleKeyFrame>
        //                 </DoubleAnimationUsingKeyFrames>
        //             </Storyboard>";

        const string myToast = @" <Storyboard xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
                <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.RenderTransform).(CompositeTransform.TranslateY)"">
                    <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
                    <EasingDoubleKeyFrame KeyTime=""0:0:0.5"" Value=""70"">
                        <EasingDoubleKeyFrame.EasingFunction>
                            <ExponentialEase EasingMode=""EaseOut"" Exponent=""6""/>     
                        </EasingDoubleKeyFrame.EasingFunction>
                    </EasingDoubleKeyFrame>
                </DoubleAnimationUsingKeyFrames>
                <DoubleAnimationUsingKeyFrames Storyboard.TargetProperty=""(UIElement.Opacity)"">
                    <EasingDoubleKeyFrame KeyTime=""0"" Value=""0""/>
                    <EasingDoubleKeyFrame KeyTime=""0:0:0.5"" Value=""1""/>
                    <EasingDoubleKeyFrame KeyTime=""0:0:2"" Value=""1""/>
                    <EasingDoubleKeyFrame KeyTime=""0:0:2.5"" Value=""0"">
                        <EasingDoubleKeyFrame.EasingFunction>
                            <ExponentialEase EasingMode=""EaseIn"" Exponent=""6""/>
                        </EasingDoubleKeyFrame.EasingFunction>
                    </EasingDoubleKeyFrame>
                </DoubleAnimationUsingKeyFrames>
            </Storyboard>";


        // 把上面动画字符串转换成相应的 XAML 动画对象
        Storyboard StoryBoardToast = System.Windows.Markup.XamlReader.Load(myToast) as Storyboard;

        // 显示自定义 Toast 消息
        public static void ShowToastMessage(string message)
        {
            Instance.StoryBoardToast.Stop();

            Instance.txtToast.Text = message;

            foreach (var t in Instance.StoryBoardToast.Children)
                Storyboard.SetTarget(t, Instance.borderToast);

            Instance.StoryBoardToast.Begin();
        }
        #endregion
    }
}
