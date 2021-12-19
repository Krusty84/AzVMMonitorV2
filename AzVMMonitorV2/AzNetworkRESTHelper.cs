/*
AzNetworkRESTHelper.cs
15.12.2021 1:43:17
Alexey Sedoykin
*/

namespace AzVMMonitorNetworkSecurity
{
    using System.Collections.Generic;

    //обёртка для будущего json с правилом доступа к VM
    /// <summary>
    /// Defines the <see cref="Properties" />.
    /// </summary>
    public class Properties
    {
        /// <summary>
        /// Gets or sets the protocol.
        /// </summary>
        public string protocol { get; set; }

        /// <summary>
        /// Gets or sets the sourceAddressPrefix.
        /// </summary>
        public string sourceAddressPrefix { get; set; }

        /// <summary>
        /// Gets or sets the destinationAddressPrefix.
        /// </summary>
        public string destinationAddressPrefix { get; set; }

        /// <summary>
        /// Gets or sets the access.
        /// </summary>
        public string access { get; set; }

        /// <summary>
        /// Gets or sets the destinationPortRanges.
        /// </summary>
        public List<string> destinationPortRanges { get; set; }

        /// <summary>
        /// Gets or sets the sourcePortRange.
        /// </summary>
        public string sourcePortRange { get; set; }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        public int priority { get; set; }

        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        public string direction { get; set; }

        public string description { get; set; }
    }

    //обёртка для будущего json с правилом доступа к VM
    /// <summary>
    /// Defines the <see cref="NetworkSecurity" />.
    /// </summary>
    public class NetworkSecurity
    {
        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        public Properties properties { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkSecurity"/> class.
        /// </summary>
        public NetworkSecurity()
        {
            this.properties = new Properties();
        }
    }
}

namespace AzVMMonitorExistNetworkSecurity
{
    using System.Collections.Generic;

    public class Properties
    {
        public string provisioningState { get; set; }
        public string protocol { get; set; }
        public string sourcePortRange { get; set; }
        public string destinationPortRange { get; set; }
        public string sourceAddressPrefix { get; set; }
        public string destinationAddressPrefix { get; set; }
        public string access { get; set; }
        public int priority { get; set; }
        public string direction { get; set; }
        public List<object> sourcePortRanges { get; set; }
        public List<string> destinationPortRanges { get; set; }
        public List<string> sourceAddressPrefixes { get; set; }
        public List<object> destinationAddressPrefixes { get; set; }
        public string description { get; set; }
    }

    public class Value
    {
        public string name { get; set; }
        public string id { get; set; }
        public string etag { get; set; }
        public string type { get; set; }
        public Properties properties { get; set; }

        public Value()
        {
            this.properties = new Properties();
        }
    }

    public class ExistNetworkSecurity
    {
        public List<Value> value { get; set; }

        /*public ExistNetworkSecurity()
        {
            this.value = new Value();
        }*/
    }
}

namespace AzVMMonitorV2
{
    using AzVMMonitorExistNetworkSecurity;
    using Newtonsoft.Json;
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="AzNetworkRESTHelper" />.
    /// </summary>
    internal static class AzNetworkRESTHelper
    {
        /// <summary>
        /// Defines the <see cref="AzureDetails" />.
        /// </summary>
        public static class AzureDetails
        {
            /// <summary>
            /// Gets or sets the Response.
            /// </summary>
            public static string Response { get; set; }
        }

        //отправка запроса PUT на Azure для установления правила доступа к VM
        /// <summary>
        /// The SetAccessForWorkstation.
        /// </summary>
        /// <param name="accesstoken">The accesstoken<see cref="string"/>.</param>
        /// <param name="subscriptionid">The subscriptionid<see cref="string"/>.</param>
        /// <param name="groupname">The groupname<see cref="string"/>.</param>
        /// <param name="nsg">The nsg<see cref="string"/>.</param>
        /// <param name="rulename">The rulename<see cref="string"/>.</param>
        /// <param name="securityrule">The securityrule<see cref="string"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public static async Task SetAccessForWorkstation(string accesstoken, string subscriptionid, string groupname, string nsg, string rulename, string securityrule)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + subscriptionid + "/resourceGroups/" + groupname + "/providers/Microsoft.Network/networkSecurityGroups/" + nsg + "/securityRules/" + rulename + "?api-version=2021-03-01");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accesstoken);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, client.BaseAddress);
                request.Content = new StringContent(securityrule, Encoding.UTF8, "application/json");
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                //ждём ответа.....
                await Task.Delay(10000);
                AzureDetails.Response = content.ToString();
                //Console.WriteLine("Result of call:  " + content.ToString());
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e);
            }
        }

        public static async Task<int> GetExistSecurityRule(string accesstoken, string subscriptionid, string groupname, string nsg)
        {
            var LatestAllowPriorityNumber = 0;
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + subscriptionid + "/resourceGroups/" + groupname + "/providers/Microsoft.Network/networkSecurityGroups/" + nsg + "/securityRules?api-version=2021-03-01");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accesstoken);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, client.BaseAddress);
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                //ждём ответа.....
                await Task.Delay(10000);
                AzureDetails.Response = content.ToString();
                //Console.WriteLine("Result of call:  " + content.ToString());
                ExistNetworkSecurity ensr = JsonConvert.DeserializeObject<ExistNetworkSecurity>(content.ToString());
                //Console.WriteLine("Security Rule Number:  " + ensr.value.Count.ToString());
                //Console.WriteLine("Latest Priority Number:  " + ensr.value[ensr.value.Count - 1].properties.priority.ToString());
                LatestAllowPriorityNumber = ensr.value[ensr.value.Count - 1].properties.priority;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine(e);
            }
            return LatestAllowPriorityNumber;
        }
    }
}