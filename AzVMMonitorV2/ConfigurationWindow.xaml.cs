/*
ConfigurationWindow.xaml.cs
06.10.2021 22:22:54
Alexey Sedoykin
*/

namespace AzVMMonitorV2
{
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Xml.Linq;

    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml.
    /// </summary>
    public partial class ConfigurationWindow : Window
    {
        /// <summary>
        /// Defines the xmlConfigFile.
        /// </summary>
        private XDocument xmlConfigFile = XDocument.Load("configuration.xml");

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationWindow"/> class.
        /// </summary>
        public ConfigurationWindow()
        {
            InitializeComponent();
            //
            try
            {
                ClientIDField.Text = xmlConfigFile.Descendants("clientID_appID").First().Value;
                ClientSecretField.Text = xmlConfigFile.Descendants("clietnSecret_appSecret").First().Value;
                ClientTenantIDField.Text = xmlConfigFile.Descendants("clientTenantId").First().Value;
                ClientSubscriptionIDField.Text = xmlConfigFile.Descendants("azureSubscriptionID").First().Value;

                comboBoxCurrency.Text = xmlConfigFile.Descendants("currency").First().Value;
                comboBoxTypeTimeFrame.Text = xmlConfigFile.Descendants("typeTimeFrame").First().Value;
            }
            catch (FileNotFoundException)
            {
                //label_WarningErrorNotes.Text = "Configuration.xml is missing";
                // label_WarningErrorNotes.Visible = true;
            }
        }

        /// <summary>
        /// The Window_Loaded.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// The btn_saveConfiguration_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void ButtonSaveConfiguration_Click(object sender, RoutedEventArgs e)
        {
            xmlConfigFile.Descendants("clientID_appID").First().SetValue(ClientIDField.Text);
            xmlConfigFile.Descendants("clietnSecret_appSecret").First().SetValue(ClientSecretField.Text);
            xmlConfigFile.Descendants("clientTenantId").First().SetValue(ClientTenantIDField.Text);
            xmlConfigFile.Descendants("azureSubscriptionID").First().SetValue(ClientSubscriptionIDField.Text);
            xmlConfigFile.Descendants("currency").First().SetValue(comboBoxCurrency.Text);
            xmlConfigFile.Descendants("typeTimeFrame").First().SetValue(comboBoxTypeTimeFrame.Text);
            xmlConfigFile.Save("configuration.xml");
            //DialogResult = true;
            this.Close();
        }
    }
}
