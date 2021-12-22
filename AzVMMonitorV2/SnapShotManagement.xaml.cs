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

        private string VMOsDiskID_, tempName;
        private IAzure AzureCred_;
        internal List<SnapShotHelper> ItemsSS = new List<SnapShotHelper>();

        //для задания мат.загадки перед созданием snapshot-а
        private Random rndGenerator = new Random();

        private int a, b;

        //
        private string filterForSnapShotName = "[^a-zA-Z_0-9]";

        public SnapShotManagement(string VMOsDiskID, IAzure azure)
        {
            InitializeComponent();
            //
            ProgressDataLoadPanel.Visibility = Visibility.Hidden;
            //
            a = rndGenerator.Next(0, 999);
            AField.Text = a.ToString();
            b = rndGenerator.Next(0, 999);
            BField.Text = b.ToString();
            ButtonCreateSnapshot.IsEnabled = false;
            //
            VMOsDiskID_ = VMOsDiskID;
            AzureCred_ = azure;
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

        private void TabItemExistSnapshot_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ItemsSS.Clear();
            this.Title = "The snapshot manager for the chosen VM";
            //
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
                        Console.WriteLine("snap OS Type: " + Snapshot.Inner.OsType.Value.ToString());
                        */
                        ItemsSS.Add(new SnapShotHelper(Snapshot));
                    }
                }
                ListOfVMSnapshots.ItemsSource = ItemsSS;
                _logger.Info("Retrieving Snapshots data was a success");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Trouble with retrive data about Snapshots");
            }
        }

        private async void ButtonCreateSnapshot_Click(object sender, RoutedEventArgs e)
        {
            SnapShotNameField.Text = Regex.Replace(SnapShotNameField.Text, filterForSnapShotName, "");
            tempName = Regex.Replace(SnapShotNameField.Text, filterForSnapShotName, "");
            CreateNewSnapShot();
        }

        private void ListOfVMSnapshots_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void ListOfVMSnapshots_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void TabItemNewSnapshot_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void ButtonDeleteSnapshot_Click(object sender, RoutedEventArgs e)
        {
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
            var task2CreateSS = SnapShotHelper.CreateSnapshotForVMDisk(VMOsDiskID_, SnapShotNameField.Text, AzureCred_);
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