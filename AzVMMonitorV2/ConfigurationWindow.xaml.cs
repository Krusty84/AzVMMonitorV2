/*
ConfigurationWindow.xaml.cs
15.12.2021 1:43:39
Alexey Sedoykin
*/

namespace AzVMMonitorV2
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
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
                PriorityRuleField.Text = xmlConfigFile.Descendants("securityRulePriority").First().Value;
                OpenPortsRuleField.Text = xmlConfigFile.Descendants("openPorts").First().Value;
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
            xmlConfigFile.Descendants("securityRulePriority").First().SetValue(PriorityRuleField.Text);
            xmlConfigFile.Descendants("openPorts").First().SetValue(OpenPortsRuleField.Text);
            xmlConfigFile.Save("configuration.xml");
            //DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// The PriorityRuleField_PreviewTextInput.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="System.Windows.Input.TextCompositionEventArgs"/>.</param>
        private void PriorityRuleField_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0)) e.Handled = true;
        }

        /// <summary>
        /// The PriorityRuleField_TextChanged.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="TextChangedEventArgs"/>.</param>
        private void PriorityRuleField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PriorityRuleField.Text == "")
                PriorityRuleField.Text = 100.ToString();
            int number = int.Parse(PriorityRuleField.Text);
            PriorityRuleField.Text = number.ToString();
            if (number <= 1000)
            {
                return;
            }
            PriorityRuleField.Text = PriorityRuleField.Text.Remove(2);
            PriorityRuleField.SelectionStart = PriorityRuleField.Text.Length;
        }

        /// <summary>
        /// The OpenPortsRuleField_PreviewTextInput.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="System.Windows.Input.TextCompositionEventArgs"/>.</param>
        private void OpenPortsRuleField_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[/^(?:\\d\\,)+\\d?$/]");
            e.Handled = !regex.IsMatch(e.Text);
        }

        /// <summary>
        /// The TabItemGeneralConf_PreviewMouseLeftButtonUp.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="System.Windows.Input.MouseButtonEventArgs"/>.</param>
        private void TabItemGeneralConf_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// The TabItemFinanceConf_PreviewMouseLeftButtonUpAsync.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="System.Windows.Input.MouseButtonEventArgs"/>.</param>
        private void TabItemFinanceConf_PreviewMouseLeftButtonUpAsync(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        /// <summary>
        /// The TabItemSecurityConf_PreviewMouseLeftButtonUpAsync.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="System.Windows.Input.MouseButtonEventArgs"/>.</param>
        private void TabItemSecurityConf_PreviewMouseLeftButtonUpAsync(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }
    }
}
