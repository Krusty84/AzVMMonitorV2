/*
SnapShotHelper.cs
22.12.2021 0:25:14
Alexey Sedoykin
*/

namespace AzVMMonitorV2
{
    using Microsoft.Azure.Management.Compute.Fluent;
    using Microsoft.Azure.Management.Fluent;
    using System.Threading.Tasks;

    //класс обёртка для предоставления информации о SnapShot-е
    /// <summary>
    /// Defines the <see cref="SnapShotHelper" />.
    /// </summary>
    public class SnapShotHelper
    {
        /// <summary>
        /// Gets or sets the SSName.
        /// </summary>
        public string SSName { get; set; }

        /// <summary>
        /// Gets or sets the SSTimeCreated.
        /// </summary>
        public string SSTimeCreated { get; set; }

        /// <summary>
        /// Gets or sets the SSOsType.
        /// </summary>
        public string SSOsType { get; set; }

        /// <summary>
        /// Gets or sets the SSDiskSizeGB.
        /// </summary>
        public string SSDiskSizeGB { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SnapShotHelper"/> class.
        /// </summary>
        /// <param name="existsnapshot">The existsnapshot<see cref="ISnapshot"/>.</param>
        public SnapShotHelper(ISnapshot existsnapshot)
        {
            SSName = existsnapshot.Name;
            SSTimeCreated = existsnapshot.Inner.TimeCreated.ToString();
            SSOsType = existsnapshot.Inner.OsType.Value.ToString();
            SSDiskSizeGB = existsnapshot.Inner.DiskSizeGB.ToString();
        }

        //создаем Snapshot для основного диска выбранной VM
        /// <summary>
        /// The CreateSnapshotForVMDisk.
        /// </summary>
        /// <param name="VMOsDiskID">The VMOsDiskID<see cref="string"/>.</param>
        /// <param name="SnapShotName">The SnapShotName<see cref="string"/>.</param>
        /// <param name="Azure">The Azure<see cref="IAzure"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public static async Task CreateSnapshotForVMDisk(string VMOsDiskID, string SnapShotName, IAzure Azure)
        {
            await Task.Delay(5000);
            IDisk vmDisk = Azure.Disks.GetById(VMOsDiskID);
            Azure.Snapshots.Define(SnapShotName).WithRegion(vmDisk.RegionName).WithExistingResourceGroup(vmDisk.ResourceGroupName).WithDataFromDisk(vmDisk).WithIncremental(true).Create();
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
