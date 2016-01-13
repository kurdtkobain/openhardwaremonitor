/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2014 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Globalization;

namespace OpenHardwareMonitor.Hardware.ATI
{
    internal sealed class ATIGPU : Hardware
    {

        private readonly int adapterIndex;
        private readonly int busNumber;
        private readonly int deviceNumber;
        private readonly Sensor temperature;
        private readonly Sensor fan;
        private readonly Sensor coreClock;
        private readonly Sensor memoryClock;
        private readonly Sensor coreVoltage;
        private readonly Sensor coreLoad;
        private readonly Sensor controlSensor;
        private readonly Control fanControl;

        public ATIGPU(string name, int adapterIndex, int busNumber, int deviceNumber, ISettings settings)
            : base(name, new Identifier("atigpu", adapterIndex.ToString(CultureInfo.InvariantCulture)), settings)
        {
            this.adapterIndex = adapterIndex;
            this.busNumber = busNumber;
            this.deviceNumber = deviceNumber;

            this.temperature = new Sensor("GPU Core", 0, SensorType.Temperature, this, settings);
            this.fan = new Sensor("GPU Fan", 0, SensorType.Fan, this, settings);
            this.coreClock = new Sensor("GPU Core", 0, SensorType.Clock, this, settings);
            this.memoryClock = new Sensor("GPU Memory", 1, SensorType.Clock, this, settings);
            this.coreVoltage = new Sensor("GPU Core", 0, SensorType.Voltage, this, settings);
            this.coreLoad = new Sensor("GPU Core", 0, SensorType.Load, this, settings);
            this.controlSensor = new Sensor("GPU Fan", 0, SensorType.Control, this, settings);

            ADLOD6ThermalControllerCaps adltcc = new ADLOD6ThermalControllerCaps();
            if (ADL.ADL_Overdrive6_ThermalController_Caps(adapterIndex, ref adltcc) != ADL.ADL_OK)
            {
                adltcc.iFanMinPercent = 0;
                adltcc.iFanMaxPercent = 100;
            }

            this.fanControl = new Control(controlSensor, settings, adltcc.iFanMinPercent, adltcc.iFanMaxPercent);
            this.fanControl.ControlModeChanged += ControlModeChanged;
            this.fanControl.SoftwareControlValueChanged += SoftwareControlValueChanged;
            ControlModeChanged(fanControl);
            this.controlSensor.Control = fanControl;
            Update();
        }

        private void SoftwareControlValueChanged(IControl control)
        {
            if (control.ControlMode == ControlMode.Software)
            {
                ADLOD6FanSpeedValue adlf = new ADLOD6FanSpeedValue();
                adlf.iSpeedType = ADL.ADL_OD6_FANSPEED_TYPE_PERCENT;
                adlf.iFanSpeed = (int)control.SoftwareValue;
                ADL.ADL_Overdrive6_FanSpeed_Set(adapterIndex, ref adlf);
            }
        }

        private void ControlModeChanged(IControl control)
        {
            switch (control.ControlMode)
            {
                case ControlMode.Undefined:
                    return;
                case ControlMode.Default:
                    SetDefaultFanSpeed();
                    break;
                case ControlMode.Software:
                    SoftwareControlValueChanged(control);
                    break;
                default:
                    return;
            }
        }

        private void SetDefaultFanSpeed()
        {
            ADL.ADL_Overdrive6_FanSpeed_Reset(adapterIndex);
        }

        public int BusNumber { get { return busNumber; } }

        public int DeviceNumber { get { return deviceNumber; } }


        public override HardwareType HardwareType
        {
            get { return HardwareType.GpuAti; }
        }

        public override void Update()
        {
            int adlt = 0;
            if (ADL.ADL_Overdrive6_Temperature_Get(adapterIndex, ref adlt) == ADL.ADL_OK)
            {
                temperature.Value = 0.001f * adlt;
                ActivateSensor(temperature);
            }
            else
            {
                temperature.Value = null;
            }

            ADLOD6FanSpeedInfo adlf = new ADLOD6FanSpeedInfo();
            adlf.iSpeedType = ADL.ADL_OD6_FANSPEED_TYPE_RPM;
            if (ADL.ADL_Overdrive6_FanSpeed_Get(adapterIndex, ref adlf) == ADL.ADL_OK)
            {
                fan.Value = adlf.iFanSpeedRPM;
                ActivateSensor(fan);
            }
            else
            {
                fan.Value = null;
            }

            //adlf = new _ADLOD6FanSpeedValue();
            adlf.iSpeedType = ADL.ADL_OD6_FANSPEED_TYPE_PERCENT;
            if (ADL.ADL_Overdrive6_FanSpeed_Get(adapterIndex, ref adlf) == ADL.ADL_OK)
            {
                controlSensor.Value = adlf.iFanSpeedPercent;
                ActivateSensor(controlSensor);
            }
            else
            {
                controlSensor.Value = null;
            }

            ADLOD6CurrentStatus adlcs = new ADLOD6CurrentStatus();
            if (ADL.ADL_Overdrive6_CurrentStatus_Get(adapterIndex, ref adlcs) == ADL.ADL_OK)
            {
                if (adlcs.iEngineClock > 0)
                {
                    coreClock.Value = 0.01f * adlcs.iEngineClock;
                    ActivateSensor(coreClock);
                }
                else
                {
                    coreClock.Value = null;
                }

                if (adlcs.iMemoryClock > 0)
                {
                    memoryClock.Value = 0.01f * adlcs.iMemoryClock;
                    ActivateSensor(memoryClock);
                }
                else
                {
                    memoryClock.Value = null;
                }
                int curval, defaultval = 0;
                if (ADL.ADL_Overdrive6_VoltageControl_Get(adapterIndex, out curval, out defaultval)!= ADL.ADL_OK)
                {
                    coreVoltage.Value = 0.001f * curval;
                    ActivateSensor(coreVoltage);
                }
                else
                {
                coreVoltage.Value = null;
                 }

                coreLoad.Value = Math.Min(adlcs.iActivityPercent, 100);
                ActivateSensor(coreLoad);
            }
            else
            {
                coreClock.Value = null;
                memoryClock.Value = null;
                coreVoltage.Value = null;
                coreLoad.Value = null;
            }
        }

        public override void Close()
        {
            this.fanControl.ControlModeChanged -= ControlModeChanged;
            this.fanControl.SoftwareControlValueChanged -= SoftwareControlValueChanged;

            if (this.fanControl.ControlMode != ControlMode.Undefined)
                SetDefaultFanSpeed();
            base.Close();
        }
    }
}
