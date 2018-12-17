﻿using QRCoder;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Conduit
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            Logo.Source = Imaging.CreateBitmapSourceFromHIcon(Properties.Resources.mimic.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            StartOnStartupCheckbox.IsChecked = Persistence.LaunchesAtStartup();

            if (Persistence.GetHubCode() != null)
            {
                RenderCode();
            }
            Persistence.OnHubCodeChanged += RenderCode;
        }

        /**
         * Renders the current hub code. This assumes that a hub token exists and doesn't check for null.
         */
        private void RenderCode()
        {
            Dispatcher.Invoke(() =>
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode("https://remote.mimic.lol/" + Persistence.GetHubCode(), QRCodeGenerator.ECCLevel.Q);
                XamlQRCode qrCode = new XamlQRCode(qrCodeData);

                ConnectionQR.Source = qrCode.GetGraphic(20);
                ConnectionQR.Visibility = Visibility.Visible;

                CodeLabel.Content = Persistence.GetHubCode();
                CodeLabel.Visibility = Visibility.Visible;

                ConnectionSteps.Visibility = Visibility.Visible;
                NoCodeText.Visibility = Visibility.Hidden;
            });
        }

        /**
         * Opens the project github link in the default browser.
         */
        public void OpenGithub(object sender, EventArgs args)
        {
            Process.Start("https://github.com/molenzwiebel/mimic");
        }

        /**
         * Opens the project discord in the default browser.
         */
        public void OpenDiscord(object sender, EventArgs args)
        {
            Process.Start("https://discord.gg/bfxdsRC");
        }

        /**
         * (Attempts to) uninstall sentinel.
         */
        public void Uninstall(object sender, EventArgs args)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to uninstall Sentinel? All files will be deleted.", "Sentinel", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No) return;

            // Step 1: Delete AppData.
            try { Directory.Delete(Persistence.DATA_DIRECTORY, true); } catch { /* ignored */ }

            // Step 2: Delete Executable.
            Process.Start(new ProcessStartInfo
            {
                Arguments = "/C choice /C Y /N /D Y /T 3 & Del " + System.Reflection.Assembly.GetExecutingAssembly().Location,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            });

            // Step 3: Stop Program.
            Application.Current.Shutdown();
        }

        /**
         * Invoked when window closes, unregisters from persistence listeners.
         */
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Persistence.OnHubCodeChanged -= RenderCode;
        }

        private void HandleStartupChange(object sender, EventArgs e)
        {
            Persistence.ToggleLaunchAtStartup();
        }
    }
}
