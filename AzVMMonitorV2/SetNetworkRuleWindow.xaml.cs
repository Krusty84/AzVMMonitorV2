using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace AzVMMonitorV2
{
    /// <summary>
    /// Interaction logic for SetNetworkRuleWindow.xaml
    /// </summary>
    ///
    //Класс для последующей десерилазиции ответа от myip
    public partial class YourCurrentIP
    {
        [JsonProperty("ip")]
        public string Ip { get; set; }

        /*[JsonProperty("country")]
        public string Country { get; set; }
        */

        [JsonProperty("country")]
        public string country { get; set; }
    }

    public partial class SetNetworkRuleWindow : Window
    {
        private string AzureTokenRESTAPI_, AzureSubscriptionID_, CurrentGroup_, strCurIP, Nsg_, PortsList_, securityRuleName, securityRulePayload;
        private int securityRulePriority_, futureSecurityRulePriority;

        /// <summary>
        /// Defines the xmlConfigFile.
        /// </summary>
        private XDocument xmlConfigFile = XDocument.Load("configuration.xml");

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
            }
            catch (FileNotFoundException)
            {
                //label_WarningErrorNotes.Text = "Configuration.xml is missing";
                // label_WarningErrorNotes.Visible = true;
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
            }
            catch (WebException)
            {
            }
        }

        private void ButtonCloseWindow_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void ButtonProvideAccess_Click(object sender, RoutedEventArgs e)
        {
            CreateNewSecurityRuleForCurrentWorkstation();
        }

        private async Task CreateNewSecurityRuleForCurrentWorkstation()
        {
            //заполянем структуру будущего правила доступа, где IP - ткекущий IP компьютера
            List<string> PortRanges = PortsList_.Split(',').ToList();
            AzVMMonitorNetworkSecurity.NetworkSecurity nsrule = new AzVMMonitorNetworkSecurity.NetworkSecurity();
            nsrule.properties.sourceAddressPrefix = strCurIP;
            nsrule.properties.access = "Allow";
            nsrule.properties.destinationPortRanges = PortRanges;
            nsrule.properties.destinationAddressPrefix = "*";
            nsrule.properties.direction = "Inbound";
            //
            ProgressDataLoadPanel.Visibility = Visibility.Visible;
            DataPanel.IsEnabled = false;
            //
            var task2GetNetworkRuleVM = AzNetworkRESTHelper.GetExistSecurityRule(AzureTokenRESTAPI_, AzureSubscriptionID_, CurrentGroup_, Nsg_);
            securityRulePriority_ = await task2GetNetworkRuleVM;
            //прибавляем к нему 1 и это будет нашим правилом
            futureSecurityRulePriority = securityRulePriority_ + 1;
            //
            nsrule.properties.priority = futureSecurityRulePriority;
            nsrule.properties.sourcePortRange = "*";
            nsrule.properties.protocol = "TCP";
            nsrule.properties.description = "It was created: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            securityRulePayload = JsonConvert.SerializeObject(nsrule);
            //
            //Имя для создаваемого правила задается в виде параметра и выглядит как "Access_ИмяРабочейСтанцииГдеЗапущеноПриложение"
            var task2SetNetworkRuleVM = AzNetworkRESTHelper.SetAccessForWorkstation(AzureTokenRESTAPI_, AzureSubscriptionID_, CurrentGroup_, Nsg_, securityRuleName, securityRulePayload);
            await task2SetNetworkRuleVM;
            //
            ProgressDataLoadPanel.Visibility = Visibility.Hidden;
            DataPanel.IsEnabled = true;
        }
    }
}