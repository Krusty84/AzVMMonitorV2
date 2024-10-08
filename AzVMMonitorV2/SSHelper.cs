﻿/*
SnapShotHelper.cs
22.12.2021 0:25:14
Alexey Sedoykin
*/

namespace AzVMMonitorV2
{
    using Microsoft.Azure.Management.Compute.Fluent;
    using Microsoft.Azure.Management.Fluent;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="SSHelper" />.
    /// </summary>
    public class SSHelper
    {
        //логгер NLog
        /// <summary>
        /// Defines the _logger.
        /// </summary>
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

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

        /// <summary>
        /// Gets or sets the SSName.
        /// </summary>
        public string SSName { get; set; }

        /// <summary>
        /// Gets or sets the SSGroupName.
        /// </summary>
        public string SSGroupName { get; set; }

        /// <summary>
        /// Gets or sets the SSTimeCreated.
        /// </summary>
        public string SSTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the SSSrcDiskID.
        /// </summary>
        public string SSSrcDiskID { get; set; }

        /// <summary>
        /// Gets or sets the SSDiskSizeGB.
        /// </summary>
        public string SSDiskSizeGB { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SSHelper"/> class.
        /// </summary>
        /// <param name="existsnapshot">The existsnapshot<see cref="ISnapshot"/>.</param>
        public SSHelper(ISnapshot existsnapshot)
        {
            SSName = existsnapshot.Name;
            SSTimeCreated = existsnapshot.Inner.TimeCreated.ToString();
            SSSrcDiskID = existsnapshot.Inner.CreationData.SourceResourceId.ToString();
            SSDiskSizeGB = existsnapshot.Inner.DiskSizeGB.ToString();
            SSGroupName = existsnapshot.ResourceGroupName;
        }

        /// <summary>
        /// The CreateSnapshotForVMDisk.
        /// </summary>
        /// <param name="VMOsDiskID">The VMOsDiskID<see cref="string"/>.</param>
        /// <param name="SnapShotName">The SnapShotName<see cref="string"/>.</param>
        /// <param name="Azure">The Azure<see cref="IAzure"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public static async Task CreateSnapshotForVMDisk(string VMOsDiskID, string SnapShotName, IAzure Azure)
        {
            try
            {
                await Task.Delay(5000);
                IDisk vmDisk = Azure.Disks.GetById(VMOsDiskID);
                //оставноился тут проблема с SKU
                Azure.Snapshots.Define(SnapShotName).WithRegion(vmDisk.RegionName).WithExistingResourceGroup(vmDisk.ResourceGroupName).WithDataFromDisk(vmDisk).WithSku(SnapshotSkuType.StandardLRS).WithIncremental(true).Create();
                _logger.Info("CreateSnapshotForVMDisk - ok");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Something wrong with CreateSnapshotForVMDisk");
            }
        }

        /// <summary>
        /// The DeleteSnapshotForVMDisk.
        /// </summary>
        /// <param name="accesstoken">The accesstoken<see cref="string"/>.</param>
        /// <param name="subscriptionid">The subscriptionid<see cref="string"/>.</param>
        /// <param name="groupname">The groupname<see cref="string"/>.</param>
        /// <param name="snapshotname">The snapshotname<see cref="string"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public static async Task DeleteSnapshotForVMDisk(string accesstoken, string subscriptionid, string groupname, string snapshotname)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("https://management.azure.com/subscriptions/" + subscriptionid + "/resourceGroups/" + groupname + "/providers/Microsoft.Compute/snapshots/" + snapshotname + "?api-version=2020-12-01");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accesstoken);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, client.BaseAddress);
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                //ждём ответа.....
                await Task.Delay(5000);
                AzureDetails.Response = content.ToString();
                _logger.Info("DeleteSnapshotForVMDisk - ok");
            }
            catch (HttpRequestException ex)
            {
                _logger.Error(ex, "Something wrong with DeleteSnapshotForVMDisk");
            }
        }

        /// <summary>
        /// The ToString.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public override string ToString()
        {
            // return $"Name: {SSName}, __State: {SSState}, __Key: {SSKey}";
            return SSName;
        }
    }
}
