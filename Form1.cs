using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;

namespace hardware_info
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            txtDisk.SelectedIndex = 0;
        }

        #region mac_address_defines

        private class IpHlpConstants
        {
            public const int MaxAdapterName = 128;
            public const int MaxAdapterNameLength = 256;
            public const int MaxAdapterDescriptionLength = 128;
            public const int MaxAdapterAddressLength = 8;
            public const uint ErrorBufferOverflow = (uint)111;
            public const int ErrorSuccess = 0;
            public const int MibIfTypeEthernet = 6;
            public const int MibIfTypeTokenring = 9;
            public const int MibIfTypeFddi = 15;
            public const int MibIfTypePpp = 23;
            public const int MibIfTypeLoopback = 24;
            public const int MibIfTypeSlip = 28;
            public const int MibIfTypeOther = 1;
        }

        /// <summary>
        /// IP_ADDRESS_STRING - http://msdn2.microsoft.com/en-us/library/aa366067.aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class IpAddressString
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string address;
        };

        /// <summary>
        /// IP_MASK_STRING - a clone of IP_ADDRESS_STRING used for retrieving subnet masks.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public abstract class IpMaskString
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string address;
        };


        /// <summary>
        /// IP_ADDR_STRING - http://msdn2.microsoft.com/en-us/library/aa366068.aspx
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public abstract class IpAddrString
        {
            public int Next;      /* struct _IP_ADDR_STRING* */
            public IpAddressString IpAddress;
            public IpMaskString IpMask;
            public uint Context;
        }


        /// <summary>
        /// IP_ADAPTER_INFO - http://msdn2.microsoft.com/en-us/library/aa366062.aspx
        /// I have added _LEGACY to indicate that it is being deprecated by the IP_ADAPTER_ADDRESSES structure starting from Windows XP 
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class IpAdapterInfo
        {
            public IntPtr Next;
            public uint ComboIndex;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (IpHlpConstants.MaxAdapterNameLength + 4))]
            public string AdapterName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = IpHlpConstants.MaxAdapterDescriptionLength + 4)]
            public string Description;
            public int AddressLength;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = IpHlpConstants.MaxAdapterAddressLength)]
            public byte[] Address;
            public int Index;
            public int Type;
            public int DhcpEnabled;
            public uint CurrentIpAddress; /* RESERVED */
            public IpAddrString IpAddressList;
            public IpAddrString GatewayList;
            public IpAddrString DhcpServer;
            [MarshalAs(UnmanagedType.Bool)]
            public bool HaveWins;
            public IpAddrString PrimaryWinsServer;
            public IpAddrString SecondaryWinsServer;
            public uint/*time_t*/ LeaseObtained;
            public uint/*time_t*/ LeaseExpires;
        }

        #endregion

        #region hwProfile_defines


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private class HwProfile
        {
            private int dwDockInfo;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 39)]
            private string szHwProfileGuid;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            private string szHwProfileName;

            public string SzHwProfileName
            {
                get => szHwProfileName;
                set => szHwProfileName = value;
            }

            public string SzHwProfileGuid
            {
                get => szHwProfileGuid;
                set => szHwProfileGuid = value;
            }

            public int DwDockInfo
            {
                get => dwDockInfo;
                set => dwDockInfo = value;
            }
        }

        #endregion

        #region reg_get_value_defines

        private enum Hkey : uint
        {
            HKEY_CLASSES_ROOT = 0x80000000,
            HKEY_CURRENT_USER = 0x80000001,
            HKEY_LOCAL_MACHINE = 0x80000002,
            HKEY_USERS = 0x80000003,
            HKEY_PERFORMANCE_DATA = 0x80000004,
            HKEY_CURRENT_CONFIG = 0x80000005,
            HKEY_DYN_DATA = 0x80000006,
        }
        private enum RType
        {
            REG_NONE = 0,
            REG_SZ = 1,
            REG_EXPAND_SZ = 2,
            REG_MULTI_SZ = 7,
            REG_BINARY = 3,
            REG_DWORD = 4,
            REG_DWORD_LITTLE_ENDIAN = 4,
            REG_DWORD_BIG_ENDIAN = 5,
            REG_LINK = 6,
            REG_RESOURCE_LIST = 8,
            REG_FULL_RESOURCE_DESCRIPTOR = 9,
            REG_RESOURCE_REQUIREMENTS_LIST = 10,
        }
        private enum RFlags
        {
            ANY = 65535,
            REG_NONE = 1,
            NOEXPAND = 268435456,
            REG_BINARY = 8,
            DWORD = 24,
            REG_DWORD = 16,
            QWORD = 72,
            REG_QWORD = 64,
            REG_SZ = 2,
            REG_MULTI_SZ = 32,
            REG_EXPAND_SZ = 4,
            RRF_ZEROONFAILURE = 536870912
        }

        #endregion        

        #region dll_imports

        [DllImport("kernel32.dll")]
        private static extern long GetVolumeInformation(
            string pathName,
            StringBuilder volumeNameBuffer,
            uint volumeNameSize,
            ref uint volumeSerialNumber,
            ref uint maximumComponentLength,
            ref uint fileSystemFlags,
            StringBuilder fileSystemNameBuffer,
            uint fileSystemNameSize);




        [DllImport("iphlpapi.dll", CharSet = CharSet.Ansi)]
        private static extern int GetAdaptersInfo(IntPtr pAdapterInfo, ref Int64 pBufOutLen);




        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool GetCurrentHwProfile(IntPtr fProfile);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool GetCurrentHwProfileA(IntPtr fProfile);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool GetCurrentHwProfileW(IntPtr fProfile);



        [DllImport("advapi32.dll", EntryPoint = "RegGetValueW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int RegGetValueW(Hkey hkey, string lpSubKey, string lpValue, RFlags dwFlags, out RType pdwType, IntPtr pvData, ref int pcbData);


        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int RegQueryValueExW(UIntPtr hkey, string lpValueName,int lpReserved,ref RegistryValueKind lpType,IntPtr lpData,ref int lpcbData);

        


        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint EnumSystemFirmwareTables(uint firmwareTableProviderSignature, IntPtr firmwareTableBuffer, uint bufferSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint GetSystemFirmwareTable(uint firmwareTableProviderSignature, uint firmwareTableId, IntPtr firmwareTableBuffer, uint bufferSize);

        
        [DllImport("advapi32.dll", SetLastError=true)]
        private static extern uint RegEnumValueW(
            IntPtr hKey,
            uint dwIndex,
            StringBuilder lpValueName,
            ref uint lpcValueName,
            IntPtr lpReserved,
            IntPtr lpType,
            IntPtr lpData,
            IntPtr lpcbData);

        #endregion




        private void button1_Click(object sender, EventArgs e)
        {
            var driveLetter = txtDisk.Text;
            driveLetter = driveLetter.Substring(0, 1) + ":\\";

            uint serialNumber = 0;
            uint maxComponentLength = 0;
            var sbVolumeName = new StringBuilder(256);
            var fileSystemFlags = new uint();
            var sbFileSystemName = new StringBuilder(256);

            if (GetVolumeInformation(driveLetter, sbVolumeName,
                    (uint)sbVolumeName.Capacity, ref serialNumber,
                    ref maxComponentLength, ref fileSystemFlags,
                    sbFileSystemName,
                    (uint)sbFileSystemName.Capacity) == 0)
            {
                MessageBox.Show(
                    "Error getting volume information.",
                    "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
            else
            {
                txtVolumeName.Text = sbVolumeName.ToString();
                txtSerialNumber.Text = serialNumber.ToString();
                txtMaxComponentLength.Text = maxComponentLength.ToString();
                txtFileSystem.Text = sbFileSystemName.ToString();
                txtFlags.Text = fileSystemFlags.ToString();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var table = new DataTable("MAC Adresleri");
            table.Columns.Add(new DataColumn("Description", typeof(string)));
            table.Columns.Add(new DataColumn("Name", typeof(string)));
            table.Columns.Add(new DataColumn("Adress", typeof(string)));


            long structSize = Marshal.SizeOf(typeof(IpAdapterInfo));
            var pArray = Marshal.AllocHGlobal(new IntPtr(structSize));
            var ret = GetAdaptersInfo(pArray, ref structSize);

            if (ret == IpHlpConstants.ErrorBufferOverflow) // ERROR_BUFFER_OVERFLOW == 111
            {
                // Buffer was too small, reallocate the correct size for the buffer.
                pArray = Marshal.ReAllocHGlobal(pArray, new IntPtr(structSize));

                ret = GetAdaptersInfo(pArray, ref structSize);
            } // if

            if (ret == 0)
            {
                // Call Succeeded
                var pEntry = pArray;

                do
                {
                    // Retrieve the adapter info from the memory address
                    var entry = (IpAdapterInfo)Marshal.PtrToStructure(pEntry, typeof(IpAdapterInfo));

                    var tmpString = string.Empty;

                    if (entry != null)
                        for (var i = 0; i < entry.Address.Length - 2; i++)
                        {
                            tmpString += $"{entry.Address[i]:X2}:";
                        }

                    //get adapter desc - guid - mac
                    if (entry == null) continue;
                    table.Rows.Add(entry.Description, entry.AdapterName, tmpString.Remove(tmpString.Length - 1, 1));

                    // Get next adapter (if any)
                    pEntry = entry.Next;
                }
                while (pEntry != IntPtr.Zero);
                dataGridView.DataSource = table;
                Marshal.FreeHGlobal(pArray);

            } 
            else
            {
                Marshal.FreeHGlobal(pArray);
                throw new InvalidOperationException("GetAdaptersInfo failed: " + ret);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

            long structSize = Marshal.SizeOf(typeof(HwProfile));
            var hwInfoPtr = Marshal.AllocHGlobal(new IntPtr(structSize));
            var lProfile = new HwProfile();
            Marshal.StructureToPtr(lProfile, hwInfoPtr, false);

            if (GetCurrentHwProfile(hwInfoPtr))
            {
                Marshal.PtrToStructure(hwInfoPtr, lProfile);
                txtHWID1.Text = lProfile.SzHwProfileGuid.ToString();
            }
            Marshal.FreeHGlobal(hwInfoPtr);


        }

        private void button4_Click(object sender, EventArgs e)
        {
            var pcbData = 512;
            string result = null;
            var pvData = Marshal.AllocHGlobal(pcbData);
            const string key = "SOFTWARE\\Microsoft\\Cryptography";
            const string value = "MachineGuid";
            try
            {
                var hresult = RegGetValueW(Hkey.HKEY_LOCAL_MACHINE, key, value, RFlags.ANY, out var _, pvData, ref pcbData);
                result = Marshal.PtrToStringUni(pvData);
            }
            finally
            {
                Marshal.FreeHGlobal(pvData);
            }
            txtMachineGuid.Text = result;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var pcbData = 512;
            string result = null;
            var pvData = Marshal.AllocHGlobal(pcbData);
            const string key = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\WindowsUpdate";
            const string value = "SusClientId";
            try
            {
                var hresult = RegGetValueW(Hkey.HKEY_LOCAL_MACHINE, key, value, RFlags.ANY, out var _, pvData, ref pcbData);
                result = Marshal.PtrToStringUni(pvData);
            }
            finally
            {
                Marshal.FreeHGlobal(pvData);
            }
            txtSusClientId.Text = result;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var pcbData = 512;
            string result = null;
            var pvData = Marshal.AllocHGlobal(pcbData);
            const string key = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion";
            const string value = "ProductId";
            try
            {
                var hresult = RegGetValueW(Hkey.HKEY_LOCAL_MACHINE, key, value, RFlags.ANY, out var _, pvData, ref pcbData);
                result = Marshal.PtrToStringUni(pvData);
            }
            finally
            {
                Marshal.FreeHGlobal(pvData);
            }
            txtProductId.Text = result;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            var pcbData = 512;
            var result = 0;
            var pvData = Marshal.AllocHGlobal(pcbData);
            const string key = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion";
            const string value = "InstallDate";
            try
            {
                var hresult = RegGetValueW(Hkey.HKEY_LOCAL_MACHINE, key, value, RFlags.ANY, out var _, pvData, ref pcbData);
                result = Marshal.ReadInt32(pvData,0);
            }
            finally
            {
                Marshal.FreeHGlobal(pvData);
            }
            txtInstallDate.Text = result.ToString();
        }


    }
}
