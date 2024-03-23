using ABI.Windows.UI;
using CTA_Tracker_Client;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CTA_StationDisplay
{
    public sealed partial class EstimateEntry : UserControl
    {
        public EstimateEntry()
        {
            this.InitializeComponent();
        }

        public void SetDisplay(Estimate estimate)
        {
            recEntryLeft.Background = recEntryRight.Background = new SolidColorBrush(routeColor(estimate.RouteName));

            if (estimate.RouteName == "Yellow")
            {
                var b = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 0, 0));
                topLine.Foreground = b;
                bottomLine.Foreground = b;
                timeEstimate.Foreground = b;
                rightLineMin.Foreground = b;
            }

            topLine.Text = $"{estimate.RouteName} Line #{estimate.RunNumber} to";
            bottomLine.Text = $"{estimate.Destination}";

            var arrivalMins = minutesToArrive(estimate);

            if (arrivalMins > 0)
                timeEstimate.Text = $"{arrivalMins}";
            else if (arrivalMins == 0)
            {
                timeEstimate.Text = "Due";
                timeEstimate.Width = 200;
                timeEstimate.TextAlignment = TextAlignment.Left;
                Canvas.SetLeft(timeEstimate, 275);
                rightLineMin.Text = "";
            }
            else
            {
                timeEstimate.Text = "Delayed";
                timeEstimate.Width = 400;
                Canvas.SetLeft(timeEstimate, 60);
                rightLineMin.Text = "";
            }
        }

        private int minutesToArrive(Estimate estimate)
        {
            var arrivalTime = DateTime.ParseExact(estimate.ArriveDepartTimestamp, "yyyyMMdd HH:mm:ss", null);
            return (int)(arrivalTime - DateTime.Now).TotalMinutes;
        }

        private Windows.UI.Color routeColor(string name)
        {
            switch (name)
            {
                case "Red":
                    return Windows.UI.Color.FromArgb(255, 255, 0, 0);
                case "Green":
                    return Windows.UI.Color.FromArgb(255, 0, 255, 0);
                case "Blue":
                    return Windows.UI.Color.FromArgb(255, 0, 125, 255);
                case "Purple":
                    return Windows.UI.Color.FromArgb(255, 75, 0, 255);
                case "Pink":
                    return Windows.UI.Color.FromArgb(255, 255, 0, 255);
                case "Brown":
                    return Windows.UI.Color.FromArgb(255, 100, 30, 10);
                case "Yellow":
                    return Windows.UI.Color.FromArgb(255, 255, 220, 0);
                case "Orange":
                    return Windows.UI.Color.FromArgb(255, 255, 100, 0);
                default:
                    return Windows.UI.Color.FromArgb(255, 0, 0, 0);

            }
        }
    }
}
