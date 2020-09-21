using System.IO;
using UnityEngine;
using GSdkNet.Board;

namespace GITEICaptoglove
{
    /* 
        Class: MyArm
        Handles Captoglove module configured as forearm sensor.

    	Author: 
		Laura Moreno - laamorenoro@unal.edu.co 
		
		Copyrigth:		
		Copyrigth 2020 GITEI Universidad Nacional de Colombia, all rigths reserved. 	    
    */
    public class MyArm : Module
    {
        /* 
            Enum: eArmType
            List of possible ways to use Captoglove module with this class:

            TYPE_RIGHT_FOREARM - As right forearm sensor
            TYPE_LEFT_FOREARM - As left forearm sensor       
        */
        public enum eArmType
        {
            TYPE_RIGHT_FOREARM,
            TYPE_LEFT_FOREARM
        }

        private eArmType _eArmType;
        private eModuleAxis _ePitchAxis, _eYawAxis, _eRollAxis;

        private float _fArmXAngle, _fArmYAngle, _fArmZAngle;
        private float _fPitchVarA, _fPitchVarB, _fYawVarA, _fYawVarB;

        private Transform _tArm = null;

        private StreamWriter swArmWriter = null;
        private bool bArmFile = false;

        /* 
            Constructor: MyArm
            Initializes variables for Captoglove module configuration.

            Parameters:
            nID - Captoglove ID (4 digits number)
            etype - Captoglove use mode

            Example:
            --- Code
            MyArm RightArm = new MyArm(2469, MyArm.eArmType.TYPE_RIGHT_FOREARM);
            ---
        */
        public MyArm(int nID, eArmType eType)
        {
            SetArmType(eType);

            if (eType == eArmType.TYPE_RIGHT_FOREARM)
            {
                InitModule(nID, Module.peModuleType.TYPE_RIGHT_ARM);
            }
            else
            {
                InitModule(nID, Module.peModuleType.TYPE_LEFT_ARM);
            }

            SetDefaultRotLimits();
        }

        /* 
            Function: SetArmType
            Saves Captoglove module use mode.

            Parameters:
            eType - Captoglove module use mode

            Example:
            --- Code
            SetArmType(MyHand.eHandType.TYPE_RIGHT_FOREARM);
            ---
        */
        private void SetArmType(eArmType eType)
        {
            _eArmType = eType;
        }

        /* 
            Function: GetArmType
            Returns:
                Captoglove module use mode
        */
        public eArmType GetArmType()
        {
            return _eArmType;
        }

        /* 
            Function: SetArmTransform
            Attaches Captoglove module movement to arm transform.     

            Parameters:
            tArmObj - Forearm transform
            ePitchAxis - Transform axis for pitch movement 
            eYawAxis   - Transform axis for yaw movement 
            eRollAxis  - Transform axis for roll movement 

            Returns: 
            0 - Success
            -1 - Error: Transform error

            Example:
            --- Code
            SetArmTransform(transRA, Module.eModuleAxis.AXIS_X, Module.eModuleAxis.AXIS_Z, Module.eModuleAxis.AXIS_Y);
            ---

            Notes: 
            Place the arm transform horizontally in the scene before assigning it in this function.        
        */
        public int SetArmTransform(Transform tArmObj, eModuleAxis ePitchAxis, eModuleAxis eYawAxis, eModuleAxis eRollAxis)
        {
            if (tArmObj == null)
            {
                TraceLog("Arm transform error");
                return -1;
            }
            _tArm = tArmObj;
            _ePitchAxis = ePitchAxis;
            _eYawAxis = eYawAxis;
            _eRollAxis = eRollAxis;

            _fArmXAngle = _tArm.localEulerAngles.x;
            _fArmYAngle = _tArm.localEulerAngles.y;
            _fArmZAngle = _tArm.localEulerAngles.z;

            return 0;
        }

        /* 
            Function: SetInitialArmRot
            Saves initial rotation for arm transform.

            Parameters:
            fRotX - Forearm transform
            fRotY - Transform axis for pitch movement 
            fRotZ   - Transform axis for yaw movement      

            Example:
            --- Code
            SetInitialArmRot(0, 90, -90);
            ---

            Notes: 
            Use this function to manually set the initial rotation of arm transform in case SetArmTransform() is saving wrong values by default.
            Use this function after SetArmTransform().
        */
        public void SetInitialArmRot(float fRotX, float fRotY, float fRotZ)
        {
            _fArmXAngle = fRotX;
            _fArmYAngle = fRotY;
            _fArmZAngle = fRotZ;
        }

        /* 
            Function: SetDefaultRotLimits
            Set the limits for the rotation of the arm transform.

            Notes: 
            The values configured in this function are valid only for the arm model delivered with these libraries.
        */
        private void SetDefaultRotLimits()
        {
            if (GetArmType() == eArmType.TYPE_RIGHT_FOREARM)
            {
                SetPitchLimits(-90, 90);
                SetYawLimits(0, -180);
            }
            else
            {
                SetPitchLimits(90, -90);
                SetYawLimits(0, 180);
            }
        }

        /* 
            Function: SetPitchLimits
            Creates the algorithm for pitch movement of the arm. 

            Parameters:
            fMaxUpRotation - Angle of rotation where the arm is pointing upward in the pitch movement
            fMaxDownRotation - Angle of rotation where the arm is pointing downward in the pitch movement

            Example:
            --- Code
            SetPitchLimits(90, -90);
            ---

            Notes: 
            These rotation values must be set as they are read in Unity enviroment for the arm transform.
        */
        public void SetPitchLimits(float fMaxUpRotation, float fMaxDownRotation)
        {
            float fCaptogloveUpLimit = 0.5f;
            float fCaptogloveDownLimit = -0.5f;

            _fPitchVarA = (fMaxUpRotation - fMaxDownRotation) / (fCaptogloveUpLimit - fCaptogloveDownLimit);
            _fPitchVarB = fMaxDownRotation - _fPitchVarA * fCaptogloveDownLimit;
        }

        /* 
            Function: SetYawLimits
            Creates the algorithm for yaw movement of the arm. 

            Parameters:
            fMaxRightRotation - Angle of rotation where the arm is pointing to the right in the yaw movement
            fMaxLeftRotation - Angle of rotation where the arm is pointing to the left in the yaw movement

            Example:
            --- Code
            SetYawLimits(90, -90);
            ---

            Notes: 
            These rotation values must be set as they are read in Unity enviroment for the arm transform.
        */
        public void SetYawLimits(float fMaxRightRotation, float fMaxLeftRotation)
        {
            float fCaptogloveRightLimit = 0.5f;
            float fCaptogloveLeftLimit = -0.5f;

            _fYawVarA = (fMaxLeftRotation - fMaxRightRotation) / (fCaptogloveRightLimit - fCaptogloveLeftLimit);
            _fYawVarB = fMaxRightRotation - _fYawVarA * fCaptogloveLeftLimit;
        }

        /* 
            Function: MoveArm
            Updates arm transform rotation according with Captoglove module movement.

            Notes: 
            Call this function in the Update() of your app to simulate arm movement.
        */
        public void MoveArm()
        {
            if (GetModuleInitialized())
                SetArmNewAngle();

            //If hand transform was assigned
            if (GetModuleInitialized() && _tArm != null)
                _tArm.localEulerAngles = new Vector3(_fArmXAngle, _fArmYAngle, _fArmZAngle);
        }

        /* 
            Function: SetArmNewAngle
            Calculates arm transform rotation according with Captoglove module movement.    
        */
        private void SetArmNewAngle()
        {
            var args = psEventTaredQuart as BoardQuaternionEventArgs;
            var args2 = psEventLinearAcceleration as BoardFloatVectorEventArgs;
            float pitchAngle;
            float yawAngle;
            float rollAngle;

            if (args != null)
            {
                float quaternionX = args.Value.X;
                float quaternionY = args.Value.Y;
                float quaternionZ = args.Value.Z;

                pitchAngle = quaternionX * _fPitchVarA + _fPitchVarB;
                yawAngle = quaternionY * _fYawVarA + _fYawVarB;
                rollAngle = _fArmXAngle;

                AsignAngle2Axes(pitchAngle, yawAngle, rollAngle);
                /*
                            if (args2 != null)
                            {
                                float ZAcc = args2.Value.Z;

                                if (ZAcc > 0.3f &&
                                    (Mathf.Abs(quaternionX) < 0.05f) &&
                                    (Mathf.Abs(quaternionY) < 0.05f) &&
                                    (Mathf.Abs(quaternionZ) < 0.05f))
                                {
                                    TraceLog("Move arm backguard");
                                }
                            }*/
            }
        }

        /* 
            Function: AsignAngle2Axes
            Set rotation angle to each axis of the arm transform. 
        */
        private void AsignAngle2Axes(float fPitchA, float fYawA, float fRollA)
        {
            switch (_ePitchAxis)
            {
                case eModuleAxis.AXIS_X:
                    _fArmXAngle = fPitchA;
                    break;
                case eModuleAxis.AXIS_Y:
                    _fArmYAngle = fPitchA;
                    break;
                case eModuleAxis.AXIS_Z:
                    _fArmZAngle = fPitchA;
                    break;
            }

            switch (_eYawAxis)
            {
                case eModuleAxis.AXIS_X:
                    _fArmXAngle = fYawA;
                    break;
                case eModuleAxis.AXIS_Y:
                    _fArmYAngle = fYawA;
                    break;
                case eModuleAxis.AXIS_Z:
                    _fArmZAngle = fYawA;
                    break;
            }

            switch (_eRollAxis)
            {
                case eModuleAxis.AXIS_X:
                    _fArmXAngle = fRollA;
                    break;
                case eModuleAxis.AXIS_Y:
                    _fArmYAngle = fRollA;
                    break;
                case eModuleAxis.AXIS_Z:
                    _fArmZAngle = fRollA;
                    break;
            }
        }

        /* 
            Function: SaveArmMovInFile
            Saves module data in file with following format: x arm rotation; y arm rotation; z arm rotation  

            Parameters:
                sFileName - File name

            Example:
            --- Code
            SaveArmMovInFile("RightArmMov.csv");
            ---

            Notes:
            Call this function in Updated() of your app to save data continuously 
        */
        public void SaveArmMovInFile(string sFileName)
        {
            var args = psEventTaredQuart as BoardQuaternionEventArgs;
            string serializedData;

            if (args != null)
            {
                if (!bArmFile)
                {
                    swArmWriter = new StreamWriter(sFileName, true);
                    bArmFile = true;
                }

                float quaternionX = args.Value.X;
                float quaternionY = args.Value.Y;
                float quaternionZ = args.Value.Z;

                serializedData =
                    quaternionX.ToString() + ";" +
                    quaternionY.ToString() + ";" +
                    quaternionZ.ToString() + "\n";

                // Write to disk
                if (swArmWriter != null)
                    swArmWriter.Write(serializedData);

            }
        }
    }
}
