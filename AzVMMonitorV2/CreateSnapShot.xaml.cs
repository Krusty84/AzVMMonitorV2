/*
CreateSnapShot.xaml.cs
28.12.2021 0:23:26
Alexey Sedoykin
*/

namespace AzVMMonitorV2
{
    using Microsoft.Azure.Management.Fluent;
    using System;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    /// Interaction logic for SnapShotManagement.xaml.
    /// </summary>
    public partial class CreateNewSnapshot : Window
    {
        //логгер NLog
        /// <summary>
        /// Defines the _logger.
        /// </summary>
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Defines the VMOsDiskID_, AzureTokenRESTAPI_, AzureSubscriptionID_, tempName.
        /// </summary>
        private string VMOsDiskID_, AzureTokenRESTAPI_, AzureSubscriptionID_, tempName;

        /// <summary>
        /// Defines the IsOkay.
        /// </summary>
        internal bool IsOkay = false;

        /// <summary>
        /// Defines the AzureCred_.
        /// </summary>
        private IAzure AzureCred_;

        //для задания мат.загадки перед созданием snapshot-а
        /// <summary>
        /// Defines the rndGenerator.
        /// </summary>
        private Random rndGenerator = new Random();

        /// <summary>
        /// Defines the a, b.
        /// </summary>
        private int a, b;

        //
        /// <summary>
        /// Defines the filterForSnapShotName.
        /// </summary>
        private readonly string filterForSnapShotName = "[^a-zA-Z_0-9]";

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateNewSnapshot"/> class.
        /// </summary>
        /// <param name="AzureTokenRESTAPI">The AzureTokenRESTAPI<see cref="string"/>.</param>
        /// <param name="AzureSubscriptionID">The AzureSubscriptionID<see cref="string"/>.</param>
        /// <param name="VMOsDiskID">The VMOsDiskID<see cref="string"/>.</param>
        /// <param name="azure">The azure<see cref="IAzure"/>.</param>
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

        /// <summary>
        /// The EqualField_TextChanged.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="TextChangedEventArgs"/>.</param>
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

        /// <summary>
        /// The ButtonCreateSnapshot_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private async void ButtonCreateSnapshot_Click(object sender, RoutedEventArgs e)
        {
            SnapShotNameField.Text = Regex.Replace(SnapShotNameField.Text, filterForSnapShotName, "");
            tempName = Regex.Replace(SnapShotNameField.Text, filterForSnapShotName, "");
            CreateNewSnapShot();
        }

        /// <summary>
        /// The EqualField_PreviewMouseLeftButtonUp.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="MouseButtonEventArgs"/>.</param>
        private void EqualField_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            EqualField.Text = "";
        }

        //обёртка для создания SnapShot-а и управление UI
        /// <summary>
        /// The CreateNewSnapShot.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
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
