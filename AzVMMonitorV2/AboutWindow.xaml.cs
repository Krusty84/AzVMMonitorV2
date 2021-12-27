/*
AboutWindow.xaml.cs
28.12.2021 0:23:16
Alexey Sedoykin
*/

namespace AzVMMonitorV2
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for AboutWindow.xaml.
    /// </summary>
    public partial class AboutWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AboutWindow"/> class.
        /// </summary>
        public AboutWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The EMailHyperLink_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void EMailHyperLink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:sedoykin@gmail.com");
        }

        /// <summary>
        /// The GitHubHyperLink_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void GitHubHyperLink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Krusty84/AzVMMonitorV2");
        }

        /// <summary>
        /// The openLogFile_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void openLogFile_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("AzVMMonitorV2.syslog.txt");
        }
    }
}
