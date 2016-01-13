/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2012 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace OpenHardwareMonitor.Hardware.ATI
{

    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLAdapterInfo
    {
        public int Size;
        public int AdapterIndex;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)]
        public string UDID;
        public int BusNumber;
        public int DeviceNumber;
        public int FunctionNumber;
        public int VendorID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)]
        public string AdapterName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)]
        public string DisplayName;
        public int Present;
        public int Exist;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)]
        public string DriverPath;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)]
        public string DriverPathExt;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)]
        public string PNPString;
        public int OSDisplayIndex;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLOD6ThermalControllerCaps
    {
        /// Contains a bitmap of thermal controller capability flags. Possible values: \ref ADL_OD6_TCCAPS_THERMAL_CONTROLLER, \ref ADL_OD6_TCCAPS_FANSPEED_CONTROL,
        /// \ref ADL_OD6_TCCAPS_FANSPEED_PERCENT_READ, \ref ADL_OD6_TCCAPS_FANSPEED_PERCENT_WRITE, \ref ADL_OD6_TCCAPS_FANSPEED_RPM_READ, \ref ADL_OD6_TCCAPS_FANSPEED_RPM_WRITE
        public int iCapabilities;
        /// Minimum fan speed expressed as a percentage
        public int iFanMinPercent;
        /// Maximum fan speed expressed as a percentage
        public int iFanMaxPercent;
        /// Minimum fan speed expressed in revolutions-per-minute
        public int iFanMinRPM;
        /// Maximum fan speed expressed in revolutions-per-minute
        public int iFanMaxRPM;
        /// Value for future extension
        public int iExtValue;
        /// Mask for future extension
        public int iExtMask;

    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLOD6FanSpeedInfo
    {
        /// Contains a bitmap of the valid fan speed type flags.  Possible values: \ref ADL_OD6_FANSPEED_TYPE_PERCENT, \ref ADL_OD6_FANSPEED_TYPE_RPM, \ref ADL_OD6_FANSPEED_USER_DEFINED
        public int iSpeedType;
        /// Contains current fan speed in percent (if valid flag exists in iSpeedType)
        public int iFanSpeedPercent;
        /// Contains current fan speed in RPM (if valid flag exists in iSpeedType)
        public int iFanSpeedRPM;
        /// Value for future extension
        public int iExtValue;
        /// Mask for future extension
        public int iExtMask;

    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLOD6FanSpeedValue
    {
        /// Indicates the units of the fan speed.  Possible values: \ref ADL_OD6_FANSPEED_TYPE_PERCENT, \ref ADL_OD6_FANSPEED_TYPE_RPM
        public int iSpeedType;
        /// Fan speed value (units as indicated above)
        public int iFanSpeed;
        /// Value for future extension
        public int iExtValue;
        /// Mask for future extension
        public int iExtMask;

    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLOD6CurrentStatus
    {
        /// Current engine clock in 10 KHz.
        public int iEngineClock;
        /// Current memory clock in 10 KHz.
        public int iMemoryClock;
        /// Current GPU activity in percent.  This
        /// indicates how "busy" the GPU is.
        public int iActivityPercent;
        /// Not used.  Reserved for future use.
        public int iCurrentPerformanceLevel;
        /// Current PCI-E bus speed
        public int iCurrentBusSpeed;
        /// Current PCI-E bus # of lanes
        public int iCurrentBusLanes;
        /// Maximum possible PCI-E bus # of lanes
        public int iMaximumBusLanes;
        /// Value for future extension
        public int iExtValue;
        /// Mask for future extension
        public int iExtMask;

    }

    internal class ADL
    {
        public const int ADL_MAX_PATH = 256;
        public const int ADL_MAX_ADAPTERS = 40;
        public const int ADL_MAX_DISPLAYS = 40;
        public const int ADL_MAX_DEVICENAME = 32;
        public const int ADL_OK = 0;
        public const int ADL_ERR = -1;
        public const int ADL_DRIVER_OK = 0;
        public const int ADL_MAX_GLSYNC_PORTS = 8;
        public const int ADL_MAX_GLSYNC_PORT_LEDS = 8;
        public const int ADL_MAX_NUM_DISPLAYMODES = 1024;

        public const int ADL_DL_FANCTRL_SPEED_TYPE_PERCENT = 1;
        public const int ADL_DL_FANCTRL_SPEED_TYPE_RPM = 2;

        public const int ADL_DL_FANCTRL_SUPPORTS_PERCENT_READ = 1;
        public const int ADL_DL_FANCTRL_SUPPORTS_PERCENT_WRITE = 2;
        public const int ADL_DL_FANCTRL_SUPPORTS_RPM_READ = 4;
        public const int ADL_DL_FANCTRL_SUPPORTS_RPM_WRITE = 8;
        public const int ADL_DL_FANCTRL_FLAG_USER_DEFINED_SPEED = 1;

        /// Fan speed reported in percentage.
        public const int ADL_OD6_FANSPEED_TYPE_PERCENT = 0x00000001;
        /// Fan speed reported in RPM.
        public const int ADL_OD6_FANSPEED_TYPE_RPM = 0x00000002;

        public const int ATI_VENDOR_ID = 0x1002;

        private delegate int ADL_Main_Control_CreateDelegate(ADL_Main_Memory_AllocDelegate callback, int enumConnectedAdapters);
        private delegate int ADL_Adapter_AdapterInfo_GetDelegate(IntPtr info, int size);

        public delegate int ADL_Main_Control_DestroyDelegate();
        public delegate int ADL_Adapter_NumberOfAdapters_GetDelegate(ref int numAdapters);
        public delegate int ADL_Adapter_ID_GetDelegate(int adapterIndex, out int adapterID);
        public delegate int ADL_Display_AdapterID_GetDelegate(int adapterIndex, out int adapterID);
        public delegate int ADL_Adapter_Active_GetDelegate(int adapterIndex, out int status);
        public delegate int ADL_Overdrive6_CurrentStatus_GetDelegate(int iAdapterIndex, ref ADLOD6CurrentStatus activity);
        public delegate int ADL_Overdrive6_ThermalController_CapsDelegate(int iAdapterIndex, ref ADLOD6ThermalControllerCaps lpThermalControllerCaps);
        public delegate int ADL_Overdrive6_Temperature_GetDelegate(int adapterIndex, ref int temperature);
        public delegate int ADL_Overdrive6_FanSpeed_GetDelegate(int adapterIndex, ref ADLOD6FanSpeedInfo fanSpeedValue);
        public delegate int ADL_Overdrive6_FanSpeed_ResetDelegate(int adapterIndex);
        public delegate int ADL_Overdrive6_FanSpeed_SetDelegate(int adapterIndex, ref ADLOD6FanSpeedValue fanSpeedValue);
        public delegate int ADL_Overdrive6_VoltageControl_GetDelegate(int iAdapterIndex, out int lpCurrentValue, out int lpDefaultValue);

        private static ADL_Main_Control_CreateDelegate _ADL_Main_Control_Create;
        private static ADL_Adapter_AdapterInfo_GetDelegate _ADL_Adapter_AdapterInfo_Get;

        public static ADL_Main_Control_DestroyDelegate ADL_Main_Control_Destroy;
        public static ADL_Adapter_NumberOfAdapters_GetDelegate ADL_Adapter_NumberOfAdapters_Get;
        public static ADL_Adapter_ID_GetDelegate _ADL_Adapter_ID_Get;
        public static ADL_Display_AdapterID_GetDelegate _ADL_Display_AdapterID_Get;
        public static ADL_Adapter_Active_GetDelegate ADL_Adapter_Active_Get;
        public static ADL_Overdrive6_CurrentStatus_GetDelegate ADL_Overdrive6_CurrentStatus_Get;
        public static ADL_Overdrive6_ThermalController_CapsDelegate ADL_Overdrive6_ThermalController_Caps;
        public static ADL_Overdrive6_Temperature_GetDelegate ADL_Overdrive6_Temperature_Get;
        public static ADL_Overdrive6_FanSpeed_GetDelegate ADL_Overdrive6_FanSpeed_Get;
        public static ADL_Overdrive6_FanSpeed_ResetDelegate ADL_Overdrive6_FanSpeed_Reset;
        public static ADL_Overdrive6_FanSpeed_SetDelegate ADL_Overdrive6_FanSpeed_Set;
        public static ADL_Overdrive6_VoltageControl_GetDelegate ADL_Overdrive6_VoltageControl_Get;

        private static string dllName;

        private static void GetDelegate<T>(string entryPoint, out T newDelegate) where T : class
        {
            DllImportAttribute attribute = new DllImportAttribute(dllName);
            attribute.CallingConvention = CallingConvention.Cdecl;
            attribute.PreserveSig = true;
            attribute.EntryPoint = entryPoint;
            PInvokeDelegateFactory.CreateDelegate(attribute, out newDelegate);
        }

        private static void CreateDelegates(string name)
        {
            int p = (int)Environment.OSVersion.Platform;
            if ((p == 4) || (p == 128))
                dllName = name + ".so";
            else
                dllName = name + ".dll";

            GetDelegate("ADL_Main_Control_Create", out _ADL_Main_Control_Create);
            GetDelegate("ADL_Adapter_AdapterInfo_Get", out _ADL_Adapter_AdapterInfo_Get);
            GetDelegate("ADL_Main_Control_Destroy", out ADL_Main_Control_Destroy);
            GetDelegate("ADL_Adapter_NumberOfAdapters_Get", out ADL_Adapter_NumberOfAdapters_Get);
            GetDelegate("ADL_Adapter_ID_Get", out _ADL_Adapter_ID_Get);
            GetDelegate("ADL_Display_AdapterID_Get", out _ADL_Display_AdapterID_Get);
            GetDelegate("ADL_Adapter_Active_Get", out ADL_Adapter_Active_Get);
            GetDelegate("ADL_Overdrive6_CurrentStatus_Get", out ADL_Overdrive6_CurrentStatus_Get);
            GetDelegate("ADL_Overdrive6_ThermalController_Caps", out ADL_Overdrive6_ThermalController_Caps);
            GetDelegate("ADL_Overdrive6_Temperature_Get", out ADL_Overdrive6_Temperature_Get);
            GetDelegate("ADL_Overdrive6_FanSpeed_Get", out ADL_Overdrive6_FanSpeed_Get);
            GetDelegate("ADL_Overdrive6_FanSpeed_Reset", out ADL_Overdrive6_FanSpeed_Reset);
            GetDelegate("ADL_Overdrive6_FanSpeed_Set", out ADL_Overdrive6_FanSpeed_Set);
            GetDelegate("ADL_Overdrive6_VoltageControl_Get", out ADL_Overdrive6_VoltageControl_Get);
        }

        static ADL()
        {
            CreateDelegates("atiadlxx");
        }

        private ADL() { }

        public static int ADL_Main_Control_Create(int enumConnectedAdapters)
        {
            try
            {
                try
                {
                    return _ADL_Main_Control_Create(Main_Memory_Alloc, enumConnectedAdapters);
                }
                catch
                {
                    CreateDelegates("atiadlxy");
                    return _ADL_Main_Control_Create(Main_Memory_Alloc, enumConnectedAdapters);
                }
            }
            catch
            {
                return ADL_ERR;
            }
        }

        public static int ADL_Adapter_AdapterInfo_Get(ADLAdapterInfo[] info)
        {
            int elementSize = Marshal.SizeOf(typeof(ADLAdapterInfo));
            int size = info.Length * elementSize;
            IntPtr ptr = Marshal.AllocHGlobal(size);
            int result = _ADL_Adapter_AdapterInfo_Get(ptr, size);
            for (int i = 0; i < info.Length; i++)
                info[i] = (ADLAdapterInfo)Marshal.PtrToStructure((IntPtr)((long)ptr + i * elementSize), typeof(ADLAdapterInfo));
            Marshal.FreeHGlobal(ptr);

            // the ADLAdapterInfo.VendorID field reported by ADL is wrong on 
            // Windows systems (parse error), so we fix this here
            for (int i = 0; i < info.Length; i++)
            {
                // try Windows UDID format
                Match m = Regex.Match(info[i].UDID, "PCI_VEN_([A-Fa-f0-9]{1,4})&.*");
                if (m.Success && m.Groups.Count == 2)
                {
                    info[i].VendorID = Convert.ToInt32(m.Groups[1].Value, 16);
                    continue;
                }
                // if above failed, try Unix UDID format
                m = Regex.Match(info[i].UDID, "[0-9]+:[0-9]+:([0-9]+):[0-9]+:[0-9]+");
                if (m.Success && m.Groups.Count == 2)
                {
                    info[i].VendorID = Convert.ToInt32(m.Groups[1].Value, 10);
                }
            }

            return result;
        }

        public static int ADL_Adapter_ID_Get(int adapterIndex, out int adapterID)
        {
            try
            {
                return _ADL_Adapter_ID_Get(adapterIndex, out adapterID);
            }
            catch (EntryPointNotFoundException)
            {
                try
                {
                    return _ADL_Display_AdapterID_Get(adapterIndex, out adapterID);
                }
                catch (EntryPointNotFoundException)
                {
                    adapterID = 1;
                    return ADL_OK;
                }
            }
        }

        private delegate IntPtr ADL_Main_Memory_AllocDelegate(int size);

        // create a Main_Memory_Alloc delegate and keep it alive
        private static ADL_Main_Memory_AllocDelegate Main_Memory_Alloc = delegate(int size)
          {
              return Marshal.AllocHGlobal(size);
          };

        private static void Main_Memory_Free(IntPtr buffer)
        {
            if (IntPtr.Zero != buffer)
                Marshal.FreeHGlobal(buffer);
        }
    }
}
