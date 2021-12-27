/*
VMHelper.cs
06.10.2021 22:22:42
Alexey Sedoykin
*/

namespace AzVMMonitorV2
{
    using Microsoft.Azure.Management.Compute.Fluent;
    using Microsoft.Azure.Management.Network.Fluent;

    /// <summary>
    /// Defines the <see cref="VMHelper" />.
    /// </summary>
    public class VMHelper
    {
        /// <summary>
        /// Gets or sets the VMName.
        /// </summary>
        public string VMName { get; set; }

        /// <summary>
        /// Gets or sets the VMState.
        /// </summary>
        public string VMState { get; set; }

        /// <summary>
        /// Gets or sets the VMKey.
        /// </summary>
        public string VMKey { get; set; }

        /// <summary>
        /// Gets or sets the VMSize.
        /// </summary>
        public string VMSize { get; set; }

        /// <summary>
        /// Gets or sets the VMUpTime.
        /// </summary>
        public string VMUpTime { get; set; }

        /// <summary>
        /// Gets or sets the VMPublicIP.
        /// </summary>
        public string VMPublicIP { get; set; }

        /// <summary>
        /// Gets or sets the VMGroupName.
        /// </summary>
        public string VMGroupName { get; set; }

        /// <summary>
        /// Gets or sets the VMDiskCount.
        /// </summary>
        public int VMDiskCount { get; set; }

        /// <summary>
        /// Gets or sets the VMDiskFirst.
        /// </summary>
        public string VMDiskFirst { get; set; }

        /// <summary>
        /// Gets or sets the VMNetwork.
        /// </summary>
        public string VMNetwork { get; set; }

        /// <summary>
        /// Gets or sets the VMOsDiskID.
        /// </summary>
        public string VMOsDiskID { get; set; }

        /// <summary>
        /// Gets or sets the VMCurrent.
        /// </summary>
        public IVirtualMachine VMCurrent { get; set; }

        /// <summary>
        /// Gets or sets the VMNET.
        /// </summary>
        public INetworkInterface VMNET { get; set; }

        /// <summary>
        /// Gets or sets the VMInterestUser.
        /// </summary>
        public string VMInterestUser { get; set; }

        /// <summary>
        /// Gets or sets the VMDescription.
        /// </summary>
        public string VMDescription { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VMHelper"/> class.
        /// </summary>
        /// <param name="selectedvm">The selectedvm<see cref="IVirtualMachine"/>.</param>
        public VMHelper(IVirtualMachine selectedvm)
        {
            //
            VMName = selectedvm.Name.ToString();
            VMState = "Updating";
            if (selectedvm.PowerState != null)
            {
                VMState = selectedvm.PowerState.Value.Substring(11).ToString();
            }
            VMKey = selectedvm.Inner.VmId;
            VMSize = selectedvm.Size.ToString();
            VMPublicIP = selectedvm.GetPrimaryPublicIPAddress().IPAddress;
            VMGroupName = selectedvm.ResourceGroupName.ToString();

            VMDiskCount = selectedvm.InstanceView.Disks.Count;
            VMDiskFirst = selectedvm.InstanceView.Disks[0].Name;
            VMNetwork = selectedvm.GetPrimaryPublicIPAddress().Name;
            VMOsDiskID = selectedvm.OSDiskId;

            foreach (var tag in selectedvm.Tags)
            {
                if (tag.Key.StartsWith("interest"))
                {
                    VMInterestUser += tag.Key.Remove(0, 8).ToString() + "/";
                }
            }
            foreach (var tag in selectedvm.Tags)
            {
                if (tag.Key.StartsWith(VMName))
                {
                    VMDescription = tag.Value;
                }
            }
            //Вернем vm как IVirtualMachine в VMCurrent
            VMCurrent = selectedvm;
        }

        /// <summary>
        /// The ToString.
        /// </summary>
        /// <returns>The <see cref="string"/>.</returns>
        public override string ToString()
        {
            // return $"Name: {VMName}, __State: {VMState}, __Key: {VMKey}";
            return VMName;
        }
    }
}
