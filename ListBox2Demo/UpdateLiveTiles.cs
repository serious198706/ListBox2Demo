using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Shell;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO.IsolatedStorage;
using ListBox2Demo.Controls.LiveTileMedium;
using ListBox2Demo.Controls.LiveTileLarge;
using System.Collections.ObjectModel;
using Microsoft.Phone.Scheduler;
using System.Windows;
using ListBox2Demo.ViewModels;

namespace ListBox2Demo
{
    class UpdateLiveTiles
    {
        string mediumFrontFilename = "/Shared/ShellContent/BackgroundImage.jpg";
        string mediumBackFilename = "/Shared/ShellContent/BackBackgroundImage.jpg";
        string largeFrontFilename = "/Shared/ShellContent/WideBackgroundImage.jpg";
        string largeBackFilename = "/Shared/ShellContent/WideBackBackgroundImage.jpg";
        List<string> todayTileTextList = new List<string>() { "", "", "", "", "", "" };

        private void GetMeData(ObservableCollection<Todo> todos, List<string> tileTextList)
        {
            foreach (var item in todos)
            {
                int i = 0;
                if (!item.IsCompleted)
                {
                    tileTextList.Insert(i++, item.Name);
                }
            }

            if (tileTextList.Count() > 6)
            {
                tileTextList.Reverse();
                tileTextList.RemoveRange(0, 6);

                if (tileTextList.Count() < 6)
                {
                    int times = 6 - tileTextList.Count();
                    for (int i = 0; i < times; i++)
                    {
                        tileTextList.Add("");
                    }
                }
            }
        }


        public void UpdateTiles()
        {
            GetMeData((App.Current as App).todos_Today, todayTileTextList);

            TileData frontTileData = new TileData()
            {
                TextTitle = "即将",
                TextLine1 = todayTileTextList.ElementAt(0),
                TextLine2 = todayTileTextList.ElementAt(1),
                TextLine3 = todayTileTextList.ElementAt(2)
            };

            TileBackData backTileData = new TileBackData()
            {
                TextLine1 = todayTileTextList.ElementAt(3),
                TextLine2 = todayTileTextList.ElementAt(4),
                TextLine3 = todayTileTextList.ElementAt(5)
            };

            CreateMediumTiles(frontTileData, backTileData);
            CreateLargeTiles(frontTileData, backTileData);

            ShellTile tile = ShellTile.ActiveTiles.First();

            if ((App.Current as App).flip)
            {
                var data = new FlipTileData
                {
                    Count = 0,
                    Title = "I.DO",
                    BackTitle = "I.DO",
                    BackgroundImage = new Uri("isostore:" + mediumFrontFilename, UriKind.Absolute),
                    WideBackgroundImage = new Uri("isostore:" + largeFrontFilename, UriKind.Absolute),
                    BackBackgroundImage = new Uri("isostore:" + mediumBackFilename, UriKind.Absolute),
                    WideBackBackgroundImage = new Uri("isostore:" + largeBackFilename, UriKind.Absolute)
                };

                tile.Update(data);
            }
            else
            {
                var data = new FlipTileData
                {
                    Count = 0,
                    BackTitle = String.Empty,
                    BackContent = String.Empty,
                    BackgroundImage = new Uri("isostore:" + mediumFrontFilename, UriKind.Absolute),
                    WideBackgroundImage = new Uri("isostore:" + largeFrontFilename, UriKind.Absolute),
                    BackBackgroundImage = new Uri("aa", UriKind.Relative),
                    WideBackBackgroundImage = new Uri("aa", UriKind.Relative)
                };

                tile.Update(data);
            }
        }

        private void CreateMediumTiles(TileData frontTileData, TileBackData backTileData)
        {
            LiveTileMedium mediumFrontTile = new LiveTileMedium();

            mediumFrontTile.Measure(new Size(336, 336));
            mediumFrontTile.Arrange(new Rect(0, 0, 336, 336));

            // 准备磁贴正面图像
            mediumFrontTile.DataContext = frontTileData;
            var mediumFrontBmp = new WriteableBitmap(336, 336);
            mediumFrontBmp.Render(mediumFrontTile, null);
            mediumFrontBmp.Invalidate();

            // 准备磁贴背面图像
            mediumFrontTile.DataContext = backTileData;
            var mediumBackBmp = new WriteableBitmap(336, 336);
            mediumBackBmp.Render(mediumFrontTile, null);
            mediumBackBmp.Invalidate();

            var isf = IsolatedStorageFile.GetUserStoreForApplication();


            if (!isf.DirectoryExists("/Shared/ShellContent"))
            {
                isf.CreateDirectory("/Shared/ShellContent");
            }

            // 储存磁贴正面图像
            using (var stream = isf.OpenFile(mediumFrontFilename, System.IO.FileMode.OpenOrCreate))
            {
                mediumFrontBmp.SaveJpeg(stream, 336, 336, 0, 100);
                stream.Close();
            }

            // 储存磁贴背面图像
            using (var stream = isf.OpenFile(mediumBackFilename, System.IO.FileMode.OpenOrCreate))
            {
                mediumBackBmp.SaveJpeg(stream, 336, 336, 0, 100);
                stream.Close();
            }

            isf.Dispose();
        }

        private void CreateLargeTiles(TileData frontTileData, TileBackData backTileData)
        {
            LiveTileLarge largeFrontTile = new LiveTileLarge();

            largeFrontTile.Measure(new Size(691, 336));
            largeFrontTile.Arrange(new Rect(0, 0, 691, 336));

            // 准备磁贴正面图像
            largeFrontTile.DataContext = frontTileData;
            var largeFrontBmp = new WriteableBitmap(691, 336);
            largeFrontBmp.Render(largeFrontTile, null);
            largeFrontBmp.Invalidate();

            // 准备磁贴背面图像
            largeFrontTile.DataContext = backTileData;
            var largeBackBmp = new WriteableBitmap(691, 336);
            largeBackBmp.Render(largeFrontTile, null);
            largeBackBmp.Invalidate();

            var isf = IsolatedStorageFile.GetUserStoreForApplication();


            if (!isf.DirectoryExists("/Shared/ShellContent"))
            {
                isf.CreateDirectory("/Shared/ShellContent");
            }

            // 储存磁贴正面图像
            using (var stream = isf.OpenFile(largeFrontFilename, System.IO.FileMode.OpenOrCreate))
            {
                largeFrontBmp.SaveJpeg(stream, 691, 336, 0, 100);
                stream.Close();
            }

            // 储存磁贴背面图像
            using (var stream = isf.OpenFile(largeBackFilename, System.IO.FileMode.OpenOrCreate))
            {
                largeBackBmp.SaveJpeg(stream, 691, 336, 0, 100);
                stream.Close();
            }

            isf.Dispose();
        }
    }
}
