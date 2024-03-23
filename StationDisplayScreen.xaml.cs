using CTA_Tracker_Client;
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
using System.Threading.Tasks;
using System.Timers;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Cryptography;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CTA_StationDisplay
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StationDisplayScreen : Window
    {
        public event EventHandler<string>? ApiErrorMessage;
        private void OnErrorMEssage(string msg)
        {
            ApiErrorMessage?.Invoke(this, msg);
        }

        private string apiKey
        {
            get => (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values["APIKey"];
        }

        Client ctaClient { get; set; }

        public Station ActiveStation { get; private set; }

        Timer updateTimer { get; set; }

        public StationDisplayScreen()
        {
            this.InitializeComponent();
            _initClient();
            _initTimer();
        }

        private void _initClient()
        {
            ctaClient = new Client(apiKey);
        }

        private void _initTimer()
        {
            updateTimer = new Timer() { Interval = 1 };
            updateTimer.Elapsed += async (s, e) => await Update();
        }

        public void LoadStation(Station station)
        {
            stopUpdates();
            clear();

            this.ActiveStation = station;
            header.Text = $"    Next 'L' services at {station.StationName}";

            startUpdates();
        }

        private void startUpdates()
        {
            updateTimer.Start();
        }

        private void stopUpdates()
        {
            updateTimer.Stop();
            updateTimer.Interval = 1;
        }

        private async Task<Response> GetEstimate()
        {
            if (ctaClient.APIKey != apiKey)
                ctaClient.SetApiKey(apiKey);

            return await ctaClient.RequestUpdateByStationID(ActiveStation.MapID);
        }

        private void HandleError(Response response)
        {
            switch (response.ErrorCode)
            {
                case 101:
                    {
                        // Invalid API Key
                        OnErrorMEssage(response.ErrorMessage);
                    }
                    break;
                default:
                    break;
            }
        }

        private async Task Update()
        {
            updateTimer.Interval = 10000;

            var response = await GetEstimate();

            if (response.ErrorCode != 0)
            {
                stopUpdates();
                this.HandleError(response);
                return;
            }

            DispatcherQueue.TryEnqueue(() => clear());

            int line = 1;

            // Some stations are closed and return no estimates
            if (response.Estimates is null)
                return;

            foreach (var estimate in response.Estimates.Take(6))
            {
                DispatcherQueue.TryEnqueue(() => AddEntry(estimate, line++));
            }
        }

        private void clear()
        {
            foreach (var ctrl in displayGrid.Children)
            {
                if (ctrl is EstimateEntry ee)
                    displayGrid.Children.Remove(ctrl);
            }
        }

        private void AddEntry(Estimate estimate, int line)
        {
            var rec = new EstimateEntry();
            rec.SetDisplay(estimate);

            Grid.SetRow(rec, line);
            Grid.SetColumn(rec, 1);

            displayGrid.Children.Add(rec);
        }
    }
}
