using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

using GSdkNet;
using GSdkNet.BLE;
using GSdkNet.Board;
using GSdkNet.Peripheral;
using GSdkNet.BLE.Winapi;

namespace GITEICaptoglove
{
    /* 
        Class: Module
        Handles Captoglove module. Used by MyHand and MyArm.
    	
        Author: 
		Laura Moreno - laamorenoro@unal.edu.co 
		
		Copyrigth:		
		Copyrigth 2020 GITEI Universidad Nacional de Colombia, all rigths reserved. 		    
    */
    public class Module
    {
        /* 
            Group: Types
            Enum: peModuleType
            List of possible ways to use Captoglove module:

            TYPE_RIGHT_HAND - As right hand sensor
            TYPE_LEFT_HAND - As left hand sensor
            TYPE_RIGHT_ARM - As low right arm sensor
            TYPE_LEFT_ARM - As low left arm sensor
        */
        protected enum peModuleType
        {
            TYPE_RIGHT_HAND,
            TYPE_LEFT_HAND,
            TYPE_RIGHT_ARM,
            TYPE_LEFT_ARM
        }

        /* 
            Enum: eModuleAxis
            List of axes:

            AXIS_X - X axis 
            AXIS_Y - Y axis
            AXIS_Z - Z axis
        */
        public enum eModuleAxis
        {
            AXIS_X,
            AXIS_Y,
            AXIS_Z
        }

        private peModuleType _eModuleType = peModuleType.TYPE_RIGHT_HAND;

        //Have Setters and Getters
        private int _nModuleID = 0;
        private string _sModuleName = " ";
        private bool _bModuleInitialized = false;
        private bool _bModuleStarted = false;
        private bool _bPropertiesRead = false;
        private bool _bLogEnabled = false;

        private IPeripheralCentral _IModuleCentral;
        private IBoardPeripheral _IModuleBoard;

        protected float[] pfaFingerSensorMaxValue;
        protected float[] pfaFingerSensorMinValue;

        protected BoardStreamEventArgs psEventTaredQuart;
        protected BoardStreamEventArgs psEventSensorState;
        protected BoardStreamEventArgs psEventLinearAcceleration;

        /* 
            Function: InitModule
            Initializes variables for Captoglove module configuration.

            Parameters:
            nID - Captoglove ID (4 digits number)
            etype - Captoglove use mode

            Example:
            --- Code
            InitModule(2496, Module.peModuleType.TYPE_RIGHT_HAND);
            ---
        */
        protected void InitModule(int nID, peModuleType eType)
        {
            SetModuleID(nID);
            SetModuleType(eType);
            SetModuleInitialized(false);
            SetModuleStarted(false);
            SetPropertiesRead(false);
            DisableLog();

            //Default values 
            pfaFingerSensorMaxValue = new float[10];
            pfaFingerSensorMinValue = new float[10];

            for (int i = 0; i < 10; i++)
            {
                pfaFingerSensorMaxValue[i] = 0f;
                pfaFingerSensorMinValue[i] = 0f;
            }

            SetModuleInitialized(true);
        }

        /* 
             Function: SetModuleType
             Saves Captoglove module use mode.

             Parameters:
             enType - Captoglove module use mode

             Example:
             --- Code
             SetModuleType(Module.peModuleType.TYPE_RIGHT_HAND);
             ---
         */
        private void SetModuleType(peModuleType eType)
        {
            _eModuleType = eType;
        }

        /* 
            Function: GetModuleType
            Returns:
            Captoglove module use mode
        */
        protected peModuleType GetModuleType()
        {
            return _eModuleType;
        }

        /* 
            Function: SetModuleID
            Saves Captoglove module ID.

            Parameters:
            nID - Captoglove module ID (4 digits number)

            Example:
            --- Code
            SetModuleID(2496);
            ---
        */
        private void SetModuleID(int nID)
        {
            _nModuleID = nID;
            SetModuleName();
        }

        /* 
            Function: GetModuleID
            Returns:
            Captoglove module ID
        */
        protected int GetModuleID()
        {
            return _nModuleID;
        }

        /* 
            Function: SetModuleName
            Creates and saves Captoglove module name.

            Notes:
            SetModuleID() must be called first
        */
        private void SetModuleName()
        {
            _sModuleName = "CaptoGlove" + _nModuleID.ToString();
        }

        /* 
            Function: GetModuleName
            Returns:
            Captoglove module name
        */
        protected string GetModuleName()
        {
            return _sModuleName;
        }

        /* 
            Function: SetModuleInitialized
            Saves whether Captoglove module is initialized or not.

            Parameters:
            b - true or false

            Example:
            --- Code
            SetModuleInitialized(true);
            ---

            Notes: 
            Normally used after InitModule() function is completed.
        */
        private void SetModuleInitialized(bool b)
        {
            _bModuleInitialized = b;
        }

        /* 
            Function: GetModuleInitialized
            Returns:
            true - Captoglove module initialized
            false - Captoglove module NOT initialized
        */
        protected bool GetModuleInitialized()
        {
            return _bModuleInitialized;
        }

        /* 
            Function: SetModuleStarted
            Saves whether Captoglove module is started or not.

            Parameters:
            b - true or false

            Example:
            --- Code
            SetModuleStarted(true);
            ---

            Notes: 
            Normally used after Start() function is completed.
        */
        private void SetModuleStarted(bool b)
        {
            _bModuleStarted = b;
        }

        /* 
            Function: GetModuleStarted
            Returns:
            true - Captoglove module started
            false - Captoglove module NOT started     
        */
        protected bool GetModuleStarted()
        {
            return _bModuleStarted;
        }

        /* 
            Function: SetPropertiesRead
            Saves whether Captoglove module properties have been read or not.

            Parameters:
            b - true or false

            Example:
            --- Code
            SetPropertiesRead(true);
            ---

            Notes: 
            Normally used after ReadProperties() function is completed.
        */
        private void SetPropertiesRead(bool b)
        {
            _bPropertiesRead = b;
        }

        /* 
            Function: GetPropertiesRead
            Returns:
            true - Captoglove module properties have been read
            false - Captoglove module properties have NOT been read         
        */
        public bool GetPropertiesRead()
        {
            return _bPropertiesRead;
        }

        /* 
            Function: EnableLog
            Enables log printing during your app execution.

            Notes:
            Log can be added with TraceLog() function.
        */
        public void EnableLog()
        {
            _bLogEnabled = true;
        }

        /* 
            Function: DisableLog
            Disables log printing during your app execution.        
        */
        public void DisableLog()
        {
            _bLogEnabled = false;
        }

        /* 
            Function: GetLogEnabled
            Returns:
                true - Log printing is enabled 
                false - Log printing is disabled
        */
        private bool GetLogEnabled()
        {
            return _bLogEnabled;
        }

        /*
            Function: Start
            Starts looking for Captoglove module peripheral.

            Returns:
            0  - Success
            -1 - Error: Module not initialized

            Notes: 
            Call this function in the Start() of your application.
        */
        public int Start()
        {
            if (GetModuleInitialized())
            {
                TraceLog("Start, Looking for peripheral");

                var adapterScanner = new AdapterScanner();
                var adapter = adapterScanner.FindAdapter();
                var configurator = new Configurator(adapter);

                _IModuleCentral = configurator.GetBoardCentral();
                _IModuleCentral.PeripheralsChanged += Central_PeripheralsChanged; //If peripheral is detected 
                _IModuleCentral.StartScan(new Dictionary<PeripheralScanFlag, object> {
                    { PeripheralScanFlag.ScanType, BleScanType.Balanced }
                });

                SetModuleStarted(true);
            }
            else
            {
                TraceLog("Module not initialized correctly");
                return -1;
            }

            return 0;
        }

        /* 
            Function: Central_PeripheralsChanged
            Looks for Captoglove module ID among the modules that are connected to bluetooth.

            Parameres: 
            sender - Unused parameter
            e - String received from Captoglove module  
         */
        private async void Central_PeripheralsChanged(object sender, PeripheralsEventArgs e)
        {
            foreach (var peripheral in e.Inserted)
            {
                // Enumerate peripherals and run first connected
                try
                {
                    var board = peripheral as IBoardPeripheral;
                    TraceLog("Trying to connect peripheral");
                    TraceLog("ID: " + board.Id);
                    TraceLog("Name: " + board.Name);

                    //If module ID is found
                    if (board.Name == _sModuleName)
                    {
                        _IModuleBoard = board;
                        _IModuleBoard.PropertyChanged += Peripheral_PropertyChanged; //Set configurations
                        _IModuleBoard.StreamReceived += Peripheral_StreamReceived;   //Read stream
                        await _IModuleBoard.StartAsync();
                    }
                    return;
                }
                catch (Exception ex)
                {
                    TraceLog("Unable to start board " + ex.Message);
                }
            }

        }

        /* 
            Function: Peripheral_PropertyChanged
            Detects if Captoglove module is connected to your app.

            Parameres: 
            sender - Unused parameter
            e - String received from Captoglove module  
        */
        private void Peripheral_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == PeripheralProperty.Status)
            {
                TraceLog("Board status: " + _IModuleBoard.Status.ToString());

                if (_IModuleBoard.Status == PeripheralStatus.Connected)
                {
                    SetProperties();
                    ReadProperties();
                }
            }
        }

        /* 
            Function: Peripheral_StreamReceived
            Captures continuously stream events from Captoglove module connected.

            Parameres: 
            sender - Unused parameter
            e - Stream of events from Captoglove module  
        */
        private void Peripheral_StreamReceived(object sender, BoardStreamEventArgs e)
        {
            if (e.StreamType == BoardStreamType.TaredQuaternion)
            {
                psEventTaredQuart = e;
            }

            if (e.StreamType == BoardStreamType.SensorsState)
            {
                psEventSensorState = e;
            }
            //CAPTURE MORE VALUES 
            if (e.StreamType == BoardStreamType.LinearAcceleration)
            {
                psEventLinearAcceleration = e;
            }
        }

        /* 
            Function: SetProperties
            Set Captoglove module properties by default: Calibrate module, Tare module, Set time slots and Commit changes.

            Notes: 
            This function overwrites previous module configuration. 
        */
        private void SetProperties()
        {
            StreamTimeslots st = new StreamTimeslots();

            TraceLog("1. Calibrate module");
            _IModuleBoard.CalibrateGyroAsync();

            TraceLog("2. Tare module");
            _IModuleBoard.TareAsync();

            TraceLog("3. Set timeslots");
            st.Set(6, BoardStreamType.TaredQuaternion);     //MoveArm movement
                                                            //TODO Verify what is this for 
            st.Set(1, BoardStreamType.LinearAcceleration);  //LinearAcc

            if (_eModuleType == peModuleType.TYPE_LEFT_HAND || _eModuleType == peModuleType.TYPE_RIGHT_HAND)
            {
                st.Set(6, BoardStreamType.SensorsState);    //Fingers movement
            }

            _IModuleBoard.StreamTimeslots.WriteAsync(st);

            // Without commit previous configuration is temporal
            TraceLog("4. Commit changes");
            _IModuleBoard.CommitChangesAsync();

            //TODO VERIFY IF THIS CALIBRATION WORKS 
        }

        /* 
            Function: ReadProperties
            Read Captoglove module properties: Firmware version, Emulation mode, Time slots and Sensors calibration.       
        */
        private async void ReadProperties()
        {
            SensorDescriptor sensorDescriptor = new SensorDescriptor();
            EmulationModes emulationModes = new EmulationModes();
            StreamTimeslots streamTimeSlots = new StreamTimeslots();

            TraceLog("1. Read firmware version");
            TraceLog("" + _IModuleBoard.FirmwareVersion);

            TraceLog("2. Read emulation Mode");
            await _IModuleBoard.EmulationModes.ReadAsync();
            emulationModes = _IModuleBoard.EmulationModes.Value;
            TraceLog(emulationModes.ToString());

            TraceLog("3. Read timeslots");
            await _IModuleBoard.StreamTimeslots.ReadAsync();
            streamTimeSlots = _IModuleBoard.StreamTimeslots.Value;
            TraceLog(streamTimeSlots.ToString());

            if (_eModuleType == peModuleType.TYPE_LEFT_HAND || _eModuleType == peModuleType.TYPE_RIGHT_HAND)
            {
                TraceLog("4. Read sensor calibration");

                for (int i = 0; i < 10; i++)
                {
                    await _IModuleBoard.SensorDescriptors[i].ReadAsync();
                    sensorDescriptor = _IModuleBoard.SensorDescriptors[i].Value;
                    TraceLog("Sensor [" + i + "]" + sensorDescriptor.ToString());
                    pfaFingerSensorMinValue[i] = sensorDescriptor.MinValue;
                    pfaFingerSensorMaxValue[i] = sensorDescriptor.MaxValue;
                }
            }

            SetPropertiesRead(true);
        }

        /* 
            Function: Stop
            Stops communication with Captoglove module.

            Notes: 
            Call this function in the OnDestroy() of your application.
        */
        public async void Stop()
        {
            TraceLog("Stopping");

            if (_IModuleBoard != null)
            {
                TraceLog("Stop Peripheral");
                _IModuleBoard.StreamReceived -= Peripheral_StreamReceived;
                _IModuleBoard.PropertyChanged -= Peripheral_PropertyChanged;
                await _IModuleBoard.StopAsync();
                _IModuleBoard.Dispose();
                _IModuleBoard = null;
            }

            if (_IModuleCentral != null)
            {
                TraceLog("Stop Central");
                _IModuleCentral.StopScan();
                _IModuleCentral.PeripheralsChanged -= Central_PeripheralsChanged;
                _IModuleCentral = null;
            }

            TraceLog("Stopped");
        }

        /* 
            Function: TraceLog
            Prints log lines during your app execution.

            Example: 
            --- Code
            TraceLog("This is a log message in my app"); 
            ---

            Notes:  
            Log is printed only if EnableLog() function is called first.
        */
        protected void TraceLog(string s)
        {
            if (GetLogEnabled())
                Debug.Log(_sModuleName + " >>> " + s);
        }

    }
}

