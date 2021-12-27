/*
SetNetworkRuleWindow.xaml.cs
20.12.2021 17:11:46
Alexey Sedoykin
*/

namespace AzVMMonitorV2
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Xml.Linq;

    /// <summary>
    /// Interaction logic for SetNetworkRuleWindow.xaml.
    /// </summary>
    public partial class YourCurrentIP
    {
        /// <summary>
        /// Gets or sets the Ip.
        /// </summary>
        [JsonProperty("ip")]
        public string Ip { get; set; }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        [JsonProperty("country")]
        public string country { get; set; }
    }

    /// <summary>
    /// Defines the <see cref="SetNetworkRuleWindow" />.
    /// </summary>
    public partial class SetNetworkRuleWindow : Window
    {
        //логгер NLog
        /// <summary>
        /// Defines the _logger.
        /// </summary>
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Defines the AzureTokenRESTAPI_, AzureSubscriptionID_, CurrentGroup_, strCurIP, Nsg_, PortsList_, securityRuleName, securityRulePayload..
        /// </summary>
        private string AzureTokenRESTAPI_, AzureSubscriptionID_, CurrentGroup_, strCurIP, Nsg_, PortsList_, securityRuleName, securityRulePayload;

        /// <summary>
        /// Defines the securityRulePriority_, futureSecurityRulePriority..
        /// </summary>
        private int securityRulePriority_, futureSecurityRulePriority;

        /// <summary>
        /// Defines the xmlConfigFile.
        /// </summary>
        private XDocument xmlConfigFile = XDocument.Load("configuration.xml");

        /// <summary>
        /// Initializes a new instance of the <see cref="SetNetworkRuleWindow"/> class.
        /// </summary>
        /// <param name="AzureTokenRESTAPI">The AzureTokenRESTAPI<see cref="string"/>.</param>
        /// <param name="AzureSubscriptionID">The AzureSubscriptionID<see cref="string"/>.</param>
        /// <param name="CurrentGroup">The CurrentGroup<see cref="string"/>.</param>
        /// <param name="Nsg">The Nsg<see cref="string"/>.</param>
        /// <param name="PortsList">The PortsList<see cref="string"/>.</param>
        public SetNetworkRuleWindow(string AzureTokenRESTAPI, string AzureSubscriptionID, string CurrentGroup, string Nsg, string PortsList)
        {
            InitializeComponent();
            //
            AzureTokenRESTAPI_ = AzureTokenRESTAPI;
            AzureSubscriptionID_ = AzureSubscriptionID;
            CurrentGroup_ = CurrentGroup;
            Nsg_ = Nsg;
            PortsList_ = PortsList;
            //имя будущего правила доступа, Access_+имя компьютера
            securityRuleName = "Access_" + Environment.MachineName.ToString();
            LabelSecurityRuleName.Text = securityRuleName;
            //
            ProgressDataLoadPanel.Visibility = Visibility.Hidden;
            DataPanel.IsEnabled = true;
            //
            try
            {
                LabelOpenPorts.Text = xmlConfigFile.Descendants("openPorts").First().Value;
                _logger.Info("Read Config.xml data successfully");
            }
            catch (FileNotFoundException ex)
            {
                _logger.Error(ex, "Configuration.xml is missing");
            }
            try
            {
                WebRequest requestMyIP = WebRequest.Create("https://api.myip.com");
                requestMyIP.Credentials = System.Net.CredentialCache.DefaultCredentials;
                WebResponse responseMyIP = requestMyIP.GetResponse();
                using (System.IO.Stream dataStreamMyIP = responseMyIP.GetResponseStream())
                {
                    StreamReader readerMyIP = new StreamReader(dataStreamMyIP);
                    string responseFromServerIP = readerMyIP.ReadToEnd();
                    var myCurrentIP = JsonConvert.DeserializeObject<YourCurrentIP>(responseFromServerIP);
                    strCurIP = myCurrentIP.Ip;
                    LabelCurrentIP.Text = myCurrentIP.Ip;
                    LabelCurrentCountry.Text = myCurrentIP.country;
                }
                responseMyIP.Close();
                _logger.Info("Retrieving current IP address was a success");
            }
            catch (WebException ex)
            {
                _logger.Error(ex, "Retrieving current IP address was not a success");
            }
        }

        /// <summary>
        /// The ButtonCloseWindow_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private void ButtonCloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// The ButtonProvideAccess_Click.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="RoutedEventArgs"/>.</param>
        private async void ButtonProvideAccess_Click(object sender, RoutedEventArgs e)
        {
            CreateNewSecurityRuleForCurrentWorkstation();
        }

        /// <summary>
        /// The CreateNewSecurityRuleForCurrentWorkstation.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        private async Task CreateNewSecurityRuleForCurrentWorkstation()
        {
            //заполянем структуру будущего правила доступа, где IP - ткекущий IP компьютера
            List<string> PortRanges = PortsList_.Split(',').ToList();
            AzVMMonitorNetworkSecurity.NetworkSecurity nsrule = new AzVMMonitorNetworkSecurity.NetworkSecurity();
            nsrule.Properties.sourceAddressPrefix = strCurIP;
            nsrule.Properties.access = "Allow";
            nsrule.Properties.destinationPortRanges = PortRanges;
            nsrule.Properties.destinationAddressPrefix = "*";
            nsrule.Properties.direction = "Inbound";
            //
            ProgressDataLoadPanel.Visibility = Visibility.Visible;
            DataPanel.IsEnabled = false;
            this.Title = "Creating the security rule...";
            //
            var task2GetNetworkRuleVM = AzNetworkRESTHelper.GetExistSecurityRule(AzureTokenRESTAPI_, AzureSubscriptionID_, CurrentGroup_, Nsg_);
            securityRulePriority_ = await task2GetNetworkRuleVM;
            //прибавляем к нему 1 и это будет нашим правилом
            futureSecurityRulePriority = securityRulePriority_ + 1;
            nsrule.Properties.priority = futureSecurityRulePriority;
            nsrule.Properties.sourcePortRange = "*";
            nsrule.Properties.protocol = "TCP";
            nsrule.Properties.description = "It was created: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            securityRulePayload = JsonConvert.SerializeObject(nsrule);
            //
            //Имя для создаваемого правила задается в виде параметра и выглядит как "Access_ИмяРабочейСтанцииГдеЗапущеноПриложение" securityRuleName
            var task2SetNetworkRuleVM = AzNetworkRESTHelper.SetAccessForWorkstation(AzureTokenRESTAPI_, AzureSubscriptionID_, CurrentGroup_, Nsg_, securityRuleName, securityRulePayload);
            await task2SetNetworkRuleVM;
            //
            ProgressDataLoadPanel.Visibility = Visibility.Hidden;
            DataPanel.IsEnabled = true;
            this.Title = "The security rule: " + securityRuleName + " was created/updated with " + futureSecurityRulePriority + " priority";
        }
    }
}
