using System.Windows;

namespace AzVMMonitorV2
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void EMailHyperLink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:sedoykin@gmail.com");
        }

        private void GitHubHyperLink_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Krusty84/AzVMMonitorV2");
        }

        private void openLogFile_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("AzVMMonitorV2.syslog.txt");
        }
    }
}