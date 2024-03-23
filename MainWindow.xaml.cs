using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Devices.PointOfService;
using CTA_Tracker_Client;
using static System.Collections.Specialized.BitVector32;
using Windows.UI.Text;
using Microsoft.UI.Text;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CTA_StationDisplay
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        StationDisplayScreen displayScreen;

        public MainWindow()
        {
            this.InitializeComponent();
            _initLineColorPanel();
            _initApiKeyField();
        }

        private void _initLineColorPanel()
        {
            foreach (var color in Enum.GetValues(typeof(RouteColor)))
            {
                // Add a button for color
                Button button = new Button()
                {
                    Content = new TextBlock() { Text = color.ToString().ToUpper(), TextWrapping = TextWrapping.WrapWholeWords, FontSize = 18, FontWeight = FontWeights.Bold },
                    Width = 125,
                    Height = 75,
                    Tag = color,
                    Background = new SolidColorBrush(routeColor(color.ToString().ToUpper()))
                };

                button.Click += (s, e) => loadStations((RouteColor)button.Tag);
                linePanel.Children.Add(button);
            }
        }

        private void _initApiKeyField()
        {
            var key = (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["APIKey"] ?? String.Empty;
            txtApiKey.Text = key;
        }

        private void _initDisplayScreen()
        {
            displayScreen = new StationDisplayScreen();
            displayScreen.Closed += (s, e) => { this.displayScreen = null; };
            displayScreen.ApiErrorMessage += (s, e) => displayApiErrorMessage(e);
            displayScreen.Activate();
        }

        private void displayApiErrorMessage(string msg)
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                if (apiKeyPanel.Children.Where(x => x is TextBlock t && t.Name == "errorMessageText").SingleOrDefault() == null)
                {
                    TextBlock txt = new TextBlock()
                    {
                        Text = msg,
                        Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 0, 0)),
                        FontWeight = FontWeights.Bold,
                        Name = "errorMessageText"
                    };
                    apiKeyPanel.Children.Add(txt);
                }
            });
        }

        private void loadStations(RouteColor color)
        {
            var stationList = Route.ByColor(color).Stations;

            stationPanel.Children.Clear();

            foreach (var station in stationList.OrderBy(x => x.StationName))
            {
                var btn = new Button()
                {
                    Content = new TextBlock() { Text = station.StationName, TextWrapping = TextWrapping.WrapWholeWords, FontSize = 18 },
                    Width = 175,
                    Height = 100,
                    Tag = station,
                    Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 200, 200, 200)),
                    BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 32, 32, 32)),

                };

                stationPanel.Children.Add(btn);
                btn.Click += (s, e) => loadStationDisplay(station);
            }
        }

        private void loadStationDisplay(Station station)
        {
            if (displayScreen == null)
                _initDisplayScreen();

            displayScreen.Title = station.StationName;
            displayScreen.LoadStation(station);
        }

        private Windows.UI.Color routeColor(string name)
        {
            switch (name)
            {
                case "RED":
                    return Windows.UI.Color.FromArgb(255, 255, 0, 0);
                case "GREEN":
                    return Windows.UI.Color.FromArgb(255, 0, 255, 0);
                case "BLUE":
                    return Windows.UI.Color.FromArgb(255, 0, 125, 255);
                case "PURPLE":
                case "PURPLEEXP":
                    return Windows.UI.Color.FromArgb(255, 75, 0, 255);
                case "PINK":
                    return Windows.UI.Color.FromArgb(255, 255, 0, 255);
                case "BROWN":
                    return Windows.UI.Color.FromArgb(255, 100, 30, 10);
                case "YELLOW":
                    return Windows.UI.Color.FromArgb(255, 255, 220, 0);
                case "ORANGE":
                    return Windows.UI.Color.FromArgb(255, 255, 100, 0);
                default:
                    return Windows.UI.Color.FromArgb(255, 0, 0, 0);

            }
        }

        private void txtApiKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Remove error message if present
            var errTxt = apiKeyPanel.Children.Where(x => x is TextBlock t && t.Name == "errorMessageText").SingleOrDefault();
            if (errTxt is not null)
                apiKeyPanel.Children.Remove(errTxt);

            Windows.Storage.ApplicationData.Current.LocalSettings.Values["APIKey"] = txtApiKey.Text;
        }
    }
}
