using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AzVMMonitorV2
{
    /// <summary>
    /// Interaction logic for SnapShotManagement.xaml
    /// </summary>
    public partial class CreateNewSnapshot : Window
    {
        //логгер NLog
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private string VMOsDiskID_, AzureTokenRESTAPI_, AzureSubscriptionID_, tempName;
        internal bool IsOkay = false;
        private IAzure AzureCred_;


        //для задания мат.загадки перед созданием snapshot-а
        private Random rndGenerator = new Random();

        private int a, b;

        //
        private readonly string filterForSnapShotName = "[^a-zA-Z_0-9]";

        public CreateNewSnapshot(string AzureTokenRESTAPI, string AzureSubscriptionID, string VMOsDiskID, IAzure azure)
        {
            InitializeComponent();
            //
            ProgressDataLoadPanel.Visibility = Visibility.Hidden;
            ButtonCreateSnapshot.IsEnabled = false;
            //
            a = rndGenerator.Next(0, 999);
            AField.Text = a.ToString();
            b = rndGenerator.Next(0, 999);
            BField.Text = b.ToString();
            //
            VMOsDiskID_ = VMOsDiskID;
            AzureCred_ = azure;
            AzureTokenRESTAPI_ = AzureTokenRESTAPI;
            AzureSubscriptionID_ = AzureSubscriptionID;
        }

        private void EqualField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (EqualField.Text.Length > 0)
            {
                if (SnapShotNameField.Text.Length > 10)
                {
                    if (Int64.Parse(EqualField.Text) == a + b)
                    {
                        ButtonCreateSnapshot.IsEnabled = true;
                    }
                }
            }
        }


        private async void ButtonCreateSnapshot_Click(object sender, RoutedEventArgs e)
        {
            SnapShotNameField.Text = Regex.Replace(SnapShotNameField.Text, filterForSnapShotName, "");
            tempName = Regex.Replace(SnapShotNameField.Text, filterForSnapShotName, "");
            CreateNewSnapShot();
        }


        private void EqualField_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            EqualField.Text = "";
        }

        //обёртка для создания SnapShot-а и управление UI
        private async Task CreateNewSnapShot()
        {
            this.Title = "Creating the snapshot...";
            ProgressDataLoadPanel.Visibility = Visibility.Visible;
            SnapShotNameField.IsEnabled = false;
            AField.IsEnabled = false;
            BField.IsEnabled = false;
            EqualField.IsEnabled = false;
            ButtonCreateSnapshot.IsEnabled = false;
            //
            var task2CreateSS = SSHelper.CreateSnapshotForVMDisk(VMOsDiskID_, SnapShotNameField.Text, AzureCred_);
            await task2CreateSS;
            //
            SnapShotNameField.Clear();
            ProgressDataLoadPanel.Visibility = Visibility.Hidden;
            SnapShotNameField.IsEnabled = true;
            AField.IsEnabled = true;
            BField.IsEnabled = true;
            EqualField.IsEnabled = true;
            ButtonCreateSnapshot.IsEnabled = true;
            this.Title = "The snapshot: " + tempName + " was created";
        }


    }
}