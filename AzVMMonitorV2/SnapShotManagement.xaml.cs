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
    public partial class SnapShotManagement : Window
    {
        //логгер NLog
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private string VMOsDiskID_, AzureTokenRESTAPI_, AzureSubscriptionID_, tempName;
        internal bool IsOkay = false;
        private IAzure AzureCred_;
        internal List<SSHelper> ItemsSS = new List<SSHelper>();
        internal SSHelper SelectedSS;

        //для задания мат.загадки перед созданием snapshot-а
        private Random rndGenerator = new Random();

        private int a, b;

        //
        private string filterForSnapShotName = "[^a-zA-Z_0-9]";

        public SnapShotManagement(string AzureTokenRESTAPI, string AzureSubscriptionID, string VMOsDiskID, IAzure azure)
        {
            InitializeComponent();
            //
            ProgressDataLoadPanel.Visibility = Visibility.Hidden;
            ProgressDataLoadPanel2.Visibility = Visibility.Hidden;
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

        private async void TabItemExistSnapshot_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var taskRefreshSSData = GetSSData();
            await taskRefreshSSData;
        }

        private async void TabItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var taskRefreshSSData = GetSSData();
            await taskRefreshSSData;
        }

        private async void ButtonCreateSnapshot_Click(object sender, RoutedEventArgs e)
        {
            SnapShotNameField.Text = Regex.Replace(SnapShotNameField.Text, filterForSnapShotName, "");
            tempName = Regex.Replace(SnapShotNameField.Text, filterForSnapShotName, "");
            CreateNewSnapShot();
        }

        private async void ButtonDeleteSnapshot_Click(object sender, RoutedEventArgs e)
        {
            SelectedSS = (SSHelper)this.ListOfVMSnapshots.SelectedItem;
            var taskDeleteSnapshot = DeleteChosenSnapshot();
            await taskDeleteSnapshot;
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

        private async Task DeleteChosenSnapshot()
        {
            this.Title = "Deleting the snapshot...";
            ProgressDataLoadPanel2.Visibility = Visibility.Visible;
            var task2DeleteSS = SSHelper.DeleteSnapshotForVMDisk(AzureTokenRESTAPI_, AzureSubscriptionID_, SelectedSS.SSGroupName, SelectedSS.SSName);
            await task2DeleteSS;
            var taskGetSSData = GetSSData();
            await taskGetSSData;
            ListOfVMSnapshots.Items.Refresh();
            ProgressDataLoadPanel2.Visibility = Visibility.Hidden;
            this.Title = "The snapshot: " + SelectedSS.SSName + " was deleted";
        }

        private async Task RefreshSSData()
        {
            ProgressDataLoadPanel2.Visibility = Visibility.Visible;
            var taskGetSSData = GetSSData();
            await taskGetSSData;
            ListOfVMSnapshots.Items.Refresh();
            ProgressDataLoadPanel2.Visibility = Visibility.Hidden;
        }

        private async Task GetSSData()
        {
            ItemsSS.Clear();
            _ = Task.Delay(10000);
            try
            {
                IDisk vmDisk = AzureCred_.Disks.GetById(VMOsDiskID_);
                var snapshotsResourceGroup = AzureCred_.Snapshots.ListByResourceGroup(vmDisk.ResourceGroupName);
                foreach (var Snapshot in snapshotsResourceGroup.ToList())
                {
                    //отбираем снэпшоты связанные с кокретным диском
                    if (Snapshot.Incremental == true && Snapshot.Inner.CreationData.SourceResourceId == vmDisk.Id && Snapshot.Inner.CreationData.SourceUniqueId == vmDisk.Inner.UniqueId)
                    {
                        /*Console.WriteLine("snap name: " + Snapshot.Name.ToString());
                        Console.WriteLine("snap created: " + Snapshot.Inner.TimeCreated.ToString());
                        Console.WriteLine("snap size GB: " + Snapshot.Inner.DiskSizeGB.ToString());
                        Console.WriteLine("disk id______: "+ Snapshot.Inner.CreationData.SourceResourceId.ToString());
                        */
                        ItemsSS.Add(new SSHelper(Snapshot));
                    }
                }
                ListOfVMSnapshots.ItemsSource = ItemsSS;
                ProgressDataLoadPanel2.Visibility = Visibility.Hidden;
                _logger.Info("Retrieving Snapshots data was a success");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Trouble with retrive data about Snapshots");
            }
        }
    }
}