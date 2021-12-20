using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AzVMMonitorV2
{

    /// <summary>
    /// Interaction logic for SnapShotManagement.xaml
    /// </summary>
    public partial class SnapShotManagement : Window
    {
        private string VMOsDiskID_;
        IAzure AzureCred_;

        Random rndGenerator = new Random();
        int a,b;

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

        private void TabItemNewSnapshot_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void TabItemExistSnapshot_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void ButtonCreateSnapshot_Click(object sender, RoutedEventArgs e)
        {
            ProgressDataLoadPanel.Visibility = Visibility.Visible;
            IDisk disk = AzureCred_.Disks.GetById(VMOsDiskID_);
            AzureCred_.Snapshots.Define(SnapShotNameField.Text.ToString()).WithRegion(disk.RegionName).WithExistingResourceGroup(disk.ResourceGroupName).WithDataFromDisk(disk).WithIncremental(true).Create();
            ProgressDataLoadPanel.Visibility = Visibility.Hidden;
        }

        private void EqualField_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            EqualField.Text = "";
        }
    }
}
