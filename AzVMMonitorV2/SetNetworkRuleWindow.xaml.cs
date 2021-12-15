using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;

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
        private int securityRulePriority_;

        public SetNetworkRuleWindow(string AzureTokenRESTAPI, string AzureSubscriptionID, string CurrentGroup, string Nsg, int securityRulePayload, string PortsList)
        {
            InitializeComponent();
            //
            AzureTokenRESTAPI_ = AzureTokenRESTAPI;
            AzureSubscriptionID_ = AzureSubscriptionID;
            CurrentGroup_ = CurrentGroup;
            Nsg_ = Nsg;
            PortsList_ = PortsList;
            securityRulePriority_ = securityRulePayload;
            //имя будущего правила доступа, Access_+имя компьютера
            securityRuleName = "Access_" + Environment.MachineName.ToString();
            LabelSecurityRuleName.Text = securityRuleName;

            ProgressDataLoadPanel.Visibility = Visibility.Hidden;
            DataPanel.IsEnabled = true;
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
            //заполянем структуру будущего правила доступа, где IP - ткекущий IP компьютера
            List<string> PortRanges = PortsList_.Split(',').ToList();
            AzVMMonitorNetworkSecurity.NetworkSecurity nsrule = new AzVMMonitorNetworkSecurity.NetworkSecurity();
            nsrule.properties.sourceAddressPrefix = strCurIP;
            nsrule.properties.access = "Allow";
            nsrule.properties.destinationPortRanges = PortRanges;
            nsrule.properties.destinationAddressPrefix = "*";
            nsrule.properties.direction = "Inbound";
            nsrule.properties.priority = securityRulePriority_;
            nsrule.properties.sourcePortRange = "*";
            nsrule.properties.protocol = "TCP";
            securityRulePayload = JsonConvert.SerializeObject(nsrule);
            //
            SetUpdateSecurityRule();
        }

        //обёртка для отправки запроса по устаноновлению правила доступа
        private async void SetUpdateSecurityRule()
        {
            ProgressDataLoadPanel.Visibility = Visibility.Visible;
            DataPanel.IsEnabled = false;
            //
            var task2SetNetworkRuleVM = AzNetworkRESTHelper.SetAccessForWorkstation(AzureTokenRESTAPI_, AzureSubscriptionID_, CurrentGroup_, Nsg_, securityRuleName, securityRulePayload);
            await task2SetNetworkRuleVM;
            //
            ProgressDataLoadPanel.Visibility = Visibility.Hidden;
            DataPanel.IsEnabled = true;
        }
    }
}