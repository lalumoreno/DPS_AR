using System.IO;
using UnityEngine;
using GSdkNet.Board;

namespace GITEICaptoglove
{
    /* 
        Class: MyHand
        Handles Captoglove module configured as hand sensor.

    	Author: 
		Laura Moreno - laamorenoro@unal.edu.co 
		
		Copyrigth:		
		Copyrigth 2020 GITEI Universidad Nacional de Colombia, all rigths reserved. 
		
    */    
    public class MyHand : Module
    {
        /* 
           Enum: eHandType
           List of possible ways to use Captoglove module with this class:

           TYPE_RIGHT_HAND - As right hand sensor
           TYPE_LEFT_HAND - As left hand sensor       
       */
        public enum eHandType
        {
            TYPE_RIGHT_HAND,
            TYPE_LEFT_HAND
        }

        private eHandType _eHandType;
        private eModuleAxis _ePitchAxis, _eYawAxis, _eRollAxis;

        private float _fHandXAngle, _fHandYAngle, _fHandZAngle;
        private int _nThumbPos, _nIndexPos, _nMiddlePos, _nRingPos, _nPinkyPos, _nPressurePos;
        private int _nOmittedDegrees;
        private bool _bFingerVariablesSet; //A and B for equation

        private float[] _faSensorValue;
        private float[] _faSensorTrigger;

        private float _fPitchVarA, _fPitchVarB, _fYawVarA, _fYawVarB, _fRollVarA, _fRollVarB;
        private float[] _faFingerVarB;
        private float[] _faFingerVarA;
        private float[] _faFingerChild1VarA;
        private float[] _faFingerChild1VarB;
        private float[] _faFingerChild2VarA;
        private float[] _faFingerChild2VarB;

        private Vector3[] _vaFingerAngle;
        private Vector3[] _vaPrevFingerAngle;
        private Vector3[] _vaFingerChild1Angle;
        private Vector3[] _vaFingerChild2Angle;

        private float[] _faFingerMinRotation;
        private float[] _faFingerMaxRotation;
        private float[] _faFingerChild1MinRotation;
        private float[] _faFingerChild1MaxRotation;
        private float[] _faFingerChild2MinRotation;
        private float[] _faFingerChild2MaxRotation;

        private Transform _tHand = null;
        private Transform[] _taFinger;
        //Dynamic size
        private Transform[] _taThumbChild;
        private Transform[] _taIndexChild;
        private Transform[] _taMiddleChild;
        private Transform[] _taRingChild;
        private Transform[] _taPinkyChild;

        private StreamWriter swHandWriter = null;
        private StreamWriter swFingerWriter = null;
        private bool bHandFile = false;
        private bool bFingerFile = false;

        /* 
            Constructor: MyHand
            Initializes variables for Captoglove module configuration.

            Parameters:
            nID - Captoglove ID (4 digits number)
            etype - Captoglove use mode

            Example:
            --- Code
            MyHand RightHand = new MyHand(2496, MyHand.eHandType.TYPE_RIGHT_HAND);        
            ---
        */
        public MyHand(int nID, eHandType eType)
        {
            SetHandType(eType);

            if (eType == eHandType.TYPE_RIGHT_HAND)
            {
                InitModule(nID, Module.peModuleType.TYPE_RIGHT_HAND);
                SetFingerSensor(1, 3, 5, 7, 9, 2);
            }
            else
            {
                InitModule(nID, Module.peModuleType.TYPE_LEFT_HAND);
                SetFingerSensor(10, 8, 6, 3, 2, 9);
            }

            SetFingerAlgorithmReady(false);

            _faSensorValue = new float[10];
            _faSensorTrigger = new float[10];

            _faFingerVarA = new float[10];
            _faFingerVarB = new float[10];
            _faFingerChild1VarA = new float[10];
            _faFingerChild1VarB = new float[10];
            _faFingerChild2VarA = new float[10];
            _faFingerChild2VarB = new float[10];

            _vaFingerAngle = new Vector3[10];
            _vaPrevFingerAngle = new Vector3[10];
            _vaFingerChild1Angle = new Vector3[10];
            _vaFingerChild2Angle = new Vector3[10];

            _faFingerMinRotation = new float[10];
            _faFingerMaxRotation = new float[10];
            _faFingerChild1MinRotation = new float[10];
            _faFingerChild1MaxRotation = new float[10];
            _faFingerChild2MinRotation = new float[10];
            _faFingerChild2MaxRotation = new float[10];

            _taFinger = new Transform[10];

            for (int i = 0; i < 10; i++)
            {
                _faSensorValue[i] = 0f;
                _faSensorTrigger[i] = 0f;

                _faFingerVarA[i] = 0f;
                _faFingerVarB[i] = 0f;
                _faFingerChild1VarA[i] = 0f;
                _faFingerChild1VarB[i] = 0f;
                _faFingerChild2VarA[i] = 0f;
                _faFingerChild2VarB[i] = 0f;

                _vaFingerAngle[i] = new Vector3(0, 0, 0);
                _vaPrevFingerAngle[i] = _vaFingerAngle[i];
                _vaFingerChild1Angle[i] = new Vector3(0, 0, 0);
                _vaFingerChild2Angle[i] = new Vector3(0, 0, 0);

                _taFinger[i] = null;
            }

            SetDefaultRotLimits();
        }

        /* 
             Function: SetHandType
             Saves Captoglove module use mode.

             Parameters:
             eType - Captoglove module use mode

             Example:
             --- Code
             SetHandType(MyHand.eHandType.TYPE_RIGHT_HAND);
             ---
         */
        private void SetHandType(eHandType eType)
        {
            _eHandType = eType;
        }

        /* 
            Function: GetHandtype
            Returns:
                Captoglove module use mode
        */
        public eHandType GetHandtype()
        {
            return _eHandType;
        }

        /* 
            Function: SetFingerAlgorithmReady
            Saves whether the algorithm for finger movement has been created or not.

            Parameters:
            b - true or false

            Example:
            --- Code
            SetFingerAlgorithmReady(true);
            ---

            Notes: 
            Normally used after SetFingerAlgorithm() function is completed.
        */
        private void SetFingerAlgorithmReady(bool b)
        {
            _bFingerVariablesSet = b;
        }

        /* 
            Function: GetFingerAlgorithmReady
            Returns:
            true - Finger algorithm has been created
            false - Finger algorithm has NOT been created
        */
        private bool GetFingerAlgorithmReady()
        {
            return _bFingerVariablesSet;
        }

        /* 
            Function: SetOmittedDegrees
            Saves the number of degrees that must be omitted in the movement of the fingers to avoid shaking.

            Parameters:
            nDegrees - Number of degrees that must be omitted in the movement of the fingers

            Example:
            --- Code
            SetOmittedDegrees(2);
            ---

            Notes:
            Usually small values between 0 and 5 to keep fast response in simulation.
        */
        private void SetOmittedDegrees(int nDegrees)
        {
            _nOmittedDegrees = nDegrees;
        }

        /* 
            Function: GetOmittedDegrees
            Returns:
                Number of degrees that are omitted in the movement of the fingers
        */
        public int GetOmittedDegrees()
        {
            return _nOmittedDegrees;
        }

        /* 
            Function: SetHandTransform
            Attaches Captoglove module movement to hand transform.     

            Parameters:
            tHandObj - Hand transform
            ePitchAxis - Transform axis for pitch movement 
            eYawAxis   - Transform axis for yaw movement 
            eRollAxis  - Transform axis for roll movement 

            Returns: 
            0 - Success
            -1 - Error: Transform error

            Example:
            --- Code
            SetHandTransform(transRH, Module.eModuleAxis.AXIS_X, Module.eModuleAxis.AXIS_Z, Module.eModuleAxis.AXIS_Y);
            ---

            Notes: 
            Place the hand transform horizontally in the scene before assigning it in this function.        
        */
        public int SetHandTransform(Transform tHandObj, eModuleAxis ePitchAxis, eModuleAxis eYawAxis, eModuleAxis eRollAxis)
        {
            if (tHandObj == null)
            {
                TraceLog("Hand transform error");
                return -1;
            }

            _tHand = tHandObj;
            _ePitchAxis = ePitchAxis;
            _eYawAxis = eYawAxis;
            _eRollAxis = eRollAxis;

            _fHandXAngle = _tHand.localEulerAngles.x;
            _fHandYAngle = _tHand.localEulerAngles.y;
            _fHandZAngle = _tHand.localEulerAngles.z;

            return 0;
        }

        /* 
            Function: SetFingerTransform
            Attaches Captoglove sensor movement to finger transform. 

            Parameters:
            tThumbObj  - Thumb finger transform
            tIndexObj  - Index finger transform
            tMiddleObj - Middle finger transform
            tRingObj   - Ring finger transform
            tPinkyObj  - Pinky finger transform        

            Returns: 
            0 - Success
            -1 - Error: Finger transform error
            -2 - Error: Child transform error

            Example:
            --- Code
            SetFingerTransform(transThuR,transIndR,transMidR,transRinR, transPinR);
            ---

            Notes: 
            _This function expects each finger transform to have at least 2 children to simulate phalanges movement._ 
            Place the finger transform horizontally in the scene before assigning it in this function.    

    */
        public int SetFingerTransform(Transform tThumbObj, Transform tIndexObj, Transform tMiddleObj,
                                       Transform tRingObj, Transform tPinkyObj)
        {
            int nChildCnt = 2;

            if (tThumbObj == null ||
                tIndexObj == null ||
                tMiddleObj == null ||
                tRingObj == null ||
                tPinkyObj == null)
            {
                TraceLog("Finger transform error");
                return -1;
            }

            _taThumbChild = tThumbObj.GetComponentsInChildren<Transform>();
            _taIndexChild = tIndexObj.GetComponentsInChildren<Transform>();
            _taMiddleChild = tMiddleObj.GetComponentsInChildren<Transform>();
            _taRingChild = tRingObj.GetComponentsInChildren<Transform>();
            _taPinkyChild = tPinkyObj.GetComponentsInChildren<Transform>();

            if (_taThumbChild.Length < nChildCnt ||
                _taIndexChild.Length < nChildCnt ||
                _taMiddleChild.Length < nChildCnt ||
                _taRingChild.Length < nChildCnt ||
                _taPinkyChild.Length < nChildCnt)
            {
                TraceLog("Child transform error");
                return -2;
            }

            _taFinger[_nThumbPos] = tThumbObj;
            _taFinger[_nIndexPos] = tIndexObj;
            _taFinger[_nMiddlePos] = tMiddleObj;
            _taFinger[_nRingPos] = tRingObj;
            _taFinger[_nPinkyPos] = tPinkyObj;

            for (int i = 0; i < 10; i++)
            {
                if (_taFinger[i] != null)
                    _vaFingerAngle[i] = _taFinger[i].localEulerAngles;
            }

            _vaFingerChild1Angle[_nThumbPos] = _taThumbChild[nChildCnt - 1].localEulerAngles;
            _vaFingerChild1Angle[_nIndexPos] = _taIndexChild[nChildCnt - 1].localEulerAngles;
            _vaFingerChild1Angle[_nMiddlePos] = _taMiddleChild[nChildCnt - 1].localEulerAngles;
            _vaFingerChild1Angle[_nRingPos] = _taRingChild[nChildCnt - 1].localEulerAngles;
            _vaFingerChild1Angle[_nPinkyPos] = _taPinkyChild[nChildCnt - 1].localEulerAngles;

            _vaFingerChild2Angle[_nThumbPos] = _taThumbChild[nChildCnt].localEulerAngles;
            _vaFingerChild2Angle[_nIndexPos] = _taIndexChild[nChildCnt].localEulerAngles;
            _vaFingerChild2Angle[_nMiddlePos] = _taMiddleChild[nChildCnt].localEulerAngles;
            _vaFingerChild2Angle[_nRingPos] = _taRingChild[nChildCnt].localEulerAngles;
            _vaFingerChild2Angle[_nPinkyPos] = _taPinkyChild[nChildCnt].localEulerAngles;

            return 0;
        }


        /* 
            Function: SetFingerSensor
            Saves the sensor ID assigned in Captoglove module to each finger.

            Parameters:
            nThumbSensor - Captoglove sensor ID for thumb finger
            nIndexSensor - Captoglove sensor ID for index finger
            nMiddleSensor - Captoglove sensor ID for middle finger 
            nRingSensor - Captoglove sensor ID for ring finger
            nPinkySensor - Captoglove sensor ID for pinky finger
            nPressureSensor - Captoglove sensor ID for pressure sensor

            Example:
            --- Code
            SetFingerSensor(1, 3, 5, 7, 9, 2);
            ---

            Returns:
            0 - Success
            -1 - Error: Sensor ID error

            Notes:
            Sensor ID can be verified in Captoglove documentation. Usually a number between 1 and 10.
        */
        private int SetFingerSensor(int nThumbSensor, int nIndexSensor, int nMiddleSensor, int nRingSensor, int nPinkySensor,
                                       int nPressureSensor)
        {
            if (nThumbSensor < 0 || nThumbSensor > 10 ||
                nIndexSensor < 0 || nIndexSensor > 10 ||
                nMiddleSensor < 0 || nMiddleSensor > 10 ||
                nRingSensor < 0 || nRingSensor > 10 ||
                nPinkySensor < 0 || nPinkySensor > 10 ||
                nPressureSensor < 0 || nPressureSensor > 10)
            {
                TraceLog("Sensor ID error");
                return -1;
            }

            //ArrayPos = SensorID - 1 
            _nThumbPos = nThumbSensor - 1;
            _nIndexPos = nIndexSensor - 1;
            _nMiddlePos = nMiddleSensor - 1;
            _nRingPos = nRingSensor - 1;
            _nPinkyPos = nPinkySensor - 1;
            _nPressurePos = nPressureSensor - 1;

            return 0;
        }

        /* 
            Function: SetDefaultRotLimits
            Set the limits for the rotation of the hand transform and each finger transform.

            Notes: 
            The values configured in this function are valid only for the hand model delivered with these libraries.
        */
        private void SetDefaultRotLimits()
        {
            if (GetHandtype() == eHandType.TYPE_RIGHT_HAND)
            {
                SetPitchLimits(90, -90);
                SetYawLimits(90, -90);
                SetRollLimits(-180, 180);

                SetThumbRotLimits(-9.475f, -60, -6.888f, -50, -6.334f, -50);
                SetIndexRotLimits(-23.606f, -80, 5.069f, -75, 2.359f, -75);
                SetMiddleRotLimits(-26.575f, -80, 10.864f, -75, -3.127f, -75);
                SetRingRotLimits(-27.302f, -80, 11.405f, -75, -1.038f, -75);
                SetPinkyRotLimits(-24.763f, -80, 6.326f, -75, 5.373f, -75);
            }
            else
            {
                SetPitchLimits(-90, 90);
                SetYawLimits(-90, 90);
                SetRollLimits(-180, 180);

                SetThumbRotLimits(12.681f, 60, -0.992f, 50, 6.269001f, 50);
                SetIndexRotLimits(21.155f, 80, -5.408f, 75, -2.203f, 75);
                SetMiddleRotLimits(24.201f, 80, -10.915f, 75, 3.174f, 75);
                SetRingRotLimits(24.854f, 80, -10.759f, 75, 0.541f, 75);
                SetPinkyRotLimits(22.229f, 80, -5.971f, 75, -5.211f, 75);
            }

            SetOmittedDegrees(2);
        }

        /* 
            Function: SetPitchLimits
            Creates the algorithm for pitch movement of the hand. 

            Parameters:
            fMaxUpRotation - Angle of rotation where the hand is pointing upward in the pitch movement
            fMaxDownRotation - Angle of rotation where the hand is pointing downward in the pitch movement

            Example:
            --- Code
            SetPitchLimits(90, -90);
            ---

            Notes: 
            These rotation values must be set as they are read in Unity enviroment for the hand transform.
        */
        private void SetPitchLimits(float fMaxUpRotation, float fMaxDownRotation)
        {
            float fCaptogloveUpLimit = 0.5f;
            float fCaptogloveDownLimit = -0.5f;

            _fPitchVarA = (fMaxUpRotation - fMaxDownRotation) / (fCaptogloveUpLimit - fCaptogloveDownLimit);
            _fPitchVarB = fMaxDownRotation - _fPitchVarA * fCaptogloveDownLimit;
        }

        /* 
            Function: SetYawLimits
            Creates the algorithm for yaw movement of the hand. 

            Parameters:
            fMaxRightRotation - Angle of rotation where the hand is pointing to the right in the yaw movement
            fMaxLeftRotation - Angle of rotation where the hand is pointing to the left in the yaw movement

            Example:
            --- Code
            SetYawLimits(90, -90);
            ---

            Notes: 
            These rotation values must be set as they are read in Unity enviroment for the hand transform.
        */
        private void SetYawLimits(float fMaxRightRotation, float fMaxLeftRotation)
        {
            float fCaptogloveRightLimit = 0.5f;
            float fCaptogloveLeftLimit = -0.5f;

            _fYawVarA = (fMaxLeftRotation - fMaxRightRotation) / (fCaptogloveRightLimit - fCaptogloveLeftLimit);
            _fYawVarB = fMaxRightRotation - _fYawVarA * fCaptogloveLeftLimit;
        }

        /* 
            Function: SetRollLimits
            Creates the algorithm for roll movement of the hand. 

            Parameters:
            fMaxRightRotation - Angle of rotation where the hand is face up after turning it to the right.
            fMaxLeftRotation - Angle of rotation where the hand is face up after turning it to the left.

            Example:
            --- Code
            SetRollLimits(90, -90);
            ---

            Notes: 
            These rotation values must be set as they are read in Unity enviroment for the hand transform.
        */
        private void SetRollLimits(float fMaxRightRotation, float fMaxLeftRotation)
        {
            float fCaptogloveRightLimit = 1f;
            float fCaptogloveLeftLimit = -1f;

            _fRollVarA = (fMaxLeftRotation - fMaxRightRotation) / (fCaptogloveRightLimit - fCaptogloveLeftLimit);
            _fRollVarB = fMaxRightRotation - _fRollVarA * fCaptogloveLeftLimit;
        }

        /* 
            Function: SetThumbRotLimits
            Saves the rotation limits for the thumb finger transform.

            Parameters:
            fMinRotation - Angle of rotation where the thumb finger is fully extended
            fMaxRotation - Angle of rotation where the thumb finger is fully bent
            fMinRotation1 - Angle of rotation where the first child is fully extended
            fMaxRotation1 - Angle of rotation where the first child is fully bent
            fMinRotation2 - Angle of rotation where the second child is fully extended
            fMaxRotation2 - Angle of rotation where the second child is fully bent

            Example:
            --- Code
            SetThumbRotLimits(-9.475f, -60, -6.888f, -50, -6.334f, -50);
            ---

            Notes: 
            These rotation values must be set as they are read in Unity enviroment for the finger transform.
        */
        private void SetThumbRotLimits(float fMinRotation, float fMaxRotation,
                                     float fMinRotation1, float fMaxRotation1,
                                     float fMinRotation2, float fMaxRotation2)
        {
            _faFingerMinRotation[_nThumbPos] = fMinRotation;
            _faFingerMaxRotation[_nThumbPos] = fMaxRotation;

            _faFingerChild1MinRotation[_nThumbPos] = fMinRotation1;
            _faFingerChild1MaxRotation[_nThumbPos] = fMaxRotation1;

            _faFingerChild2MinRotation[_nThumbPos] = fMinRotation2;
            _faFingerChild2MaxRotation[_nThumbPos] = fMaxRotation2;
        }

        /* 
            Function: SetIndexRotLimits
            Saves the rotation limits for the index finger transform.

            Parameters:
            fMinRotation - Angle of rotation where the index finger is fully extended
            fMaxRotation - Angle of rotation where the index finger is fully bent
            fMinRotation1 - Angle of rotation where the first child is fully extended
            fMaxRotation1 - Angle of rotation where the first child is fully bent
            fMinRotation2 - Angle of rotation where the second child is fully extended
            fMaxRotation2 - Angle of rotation where the second child is fully bent

            Example:
            --- Code
            SetIndexRotLimits(-9.475f, -60, -6.888f, -50, -6.334f, -50);
            ---

            Notes: 
            These rotation values must be set as they are read in Unity enviroment for the finger transform.
        */
        private void SetIndexRotLimits(float fMinRotation, float fMaxRotation,
                                     float fMinRotation1, float fMaxRotation1,
                                     float fMinRotation2, float fMaxRotation2)
        {
            _faFingerMinRotation[_nIndexPos] = fMinRotation;
            _faFingerMaxRotation[_nIndexPos] = fMaxRotation;

            _faFingerChild1MinRotation[_nIndexPos] = fMinRotation1;
            _faFingerChild1MaxRotation[_nIndexPos] = fMaxRotation1;

            _faFingerChild2MinRotation[_nIndexPos] = fMinRotation2;
            _faFingerChild2MaxRotation[_nIndexPos] = fMaxRotation2;
        }

        /* 
            Function: SetMiddleRotLimits
            Saves the rotation limits for the middle finger transform.

            Parameters:
            fMinRotation - Angle of rotation where the middle finger is fully extended
            fMaxRotation - Angle of rotation where the middle finger is fully bent
            fMinRotation1 - Angle of rotation where the first child is fully extended
            fMaxRotation1 - Angle of rotation where the first child is fully bent
            fMinRotation2 - Angle of rotation where the second child is fully extended
            fMaxRotation2 - Angle of rotation where the second child is fully bent

            Example:
            --- Code
            SetMiddleRotLimits(-9.475f, -60, -6.888f, -50, -6.334f, -50);
            ---

            Notes: 
            These rotation values must be set as they are read in Unity enviroment for the finger transform.
        */
        private void SetMiddleRotLimits(float fMinRotation, float fMaxRotation,
                                      float fMinRotation1, float fMaxRotation1,
                                      float fMinRotation2, float fMaxRotation2)
        {
            _faFingerMinRotation[_nMiddlePos] = fMinRotation;
            _faFingerMaxRotation[_nMiddlePos] = fMaxRotation;

            _faFingerChild1MinRotation[_nMiddlePos] = fMinRotation1;
            _faFingerChild1MaxRotation[_nMiddlePos] = fMaxRotation1;

            _faFingerChild2MinRotation[_nMiddlePos] = fMinRotation2;
            _faFingerChild2MaxRotation[_nMiddlePos] = fMaxRotation2;
        }

        /* 
            Function: SetRingRotLimits
            Saves the rotation limits for the ring finger transform.

            Parameters:
            fMinRotation - Angle of rotation where the ring finger is fully extended
            fMaxRotation - Angle of rotation where the ring finger is fully bent
            fMinRotation1 - Angle of rotation where the first child is fully extended
            fMaxRotation1 - Angle of rotation where the first child is fully bent
            fMinRotation2 - Angle of rotation where the second child is fully extended
            fMaxRotation2 - Angle of rotation where the second child is fully bent

            Example:
            --- Code
            SetRingRotLimits(-9.475f, -60, -6.888f, -50, -6.334f, -50);
            ---

            Notes: 
            These rotation values must be set as they are read in Unity enviroment for the finger transform.
        */
        private void SetRingRotLimits(float fMinRotation, float fMaxRotation,
                                    float fMinRotation1, float fMaxRotation1,
                                    float fMinRotation2, float fMaxRotation2)
        {
            _faFingerMinRotation[_nRingPos] = fMinRotation;
            _faFingerMaxRotation[_nRingPos] = fMaxRotation;

            _faFingerChild1MinRotation[_nRingPos] = fMinRotation1;
            _faFingerChild1MaxRotation[_nRingPos] = fMaxRotation1;

            _faFingerChild2MinRotation[_nRingPos] = fMinRotation2;
            _faFingerChild2MaxRotation[_nRingPos] = fMaxRotation2;
        }

        /* 
            Function: SetPinkyRotLimits
            Saves the rotation limits for the pinky finger transform.

            Parameters:
            fMinRotation - Angle of rotation where the pinky finger is fully extended
            fMaxRotation - Angle of rotation where the pinky finger is fully bent
            fMinRotation1 - Angle of rotation where the first child is fully extended
            fMaxRotation1 - Angle of rotation where the first child is fully bent
            fMinRotation2 - Angle of rotation where the second child is fully extended
            fMaxRotation2 - Angle of rotation where the second child is fully bent

            Example:
            --- Code
            SetPinkyRotLimits(-9.475f, -60, -6.888f, -50, -6.334f, -50);
            ---

            Notes: 
            These rotation values must be set as they are read in Unity enviroment for the finger transform.
        */
        private void SetPinkyRotLimits(float fMinRotation, float fMaxRotation,
                                     float fMinRotation1, float fMaxRotation1,
                                     float fMinRotation2, float fMaxRotation2)
        {
            _faFingerMinRotation[_nPinkyPos] = fMinRotation;
            _faFingerMaxRotation[_nPinkyPos] = fMaxRotation;

            _faFingerChild1MinRotation[_nPinkyPos] = fMinRotation1;
            _faFingerChild1MaxRotation[_nPinkyPos] = fMaxRotation1;

            _faFingerChild2MinRotation[_nPinkyPos] = fMinRotation2;
            _faFingerChild2MaxRotation[_nPinkyPos] = fMaxRotation2;
        }

        /* 
            Function: MoveHand
            Updates hand transform rotation according with Captoglove module movement.

            Notes: 
            Call this function in the Update() of your app to simulate hand movement.
        */
        public void MoveHand()
        {
            if (GetModuleStarted())
                SetHandNewAngle(true);

            //If hand transform was assigned
            if (_tHand != null)
                _tHand.localEulerAngles = new Vector3(_fHandXAngle, _fHandYAngle, _fHandZAngle);
        }

        /* 
            Function: MoveHandNoYaw
            Updates hand transform rotation according with Captoglove module movement. Yaw movement is ommited.

            Notes: 
            Call this function in the Update() of your app to simulate hand movement.
            Normally used when arm simulation is also running so the yaw movement is done by the arm.
        */
        public void MoveHandNoYaw()
        {
            if (GetModuleStarted())
                SetHandNewAngle(false);

            //If hand transform was assigned
            if (_tHand != null)
                _tHand.localEulerAngles = new Vector3(_fHandXAngle, _fHandYAngle, _fHandZAngle);
        }

        /* 
            Function: SetHandNewAngle
            Calculates hand transform rotation according with Captoglove module movement.   

            Parameters:
                bYaw - true or false to simulate yaw movement

        */
        private void SetHandNewAngle(bool bYaw)
        {
            var args = psEventTaredQuart as BoardQuaternionEventArgs;
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
                rollAngle = quaternionZ * _fRollVarA + _fRollVarB;

                if (!bYaw)
                    yawAngle = 0;

                /*
                //SetPitchRotation when hand is upside down TODO IMPROVE THIS MOVEMENT 
                if((eType == ModuleType.TYPE_LEFT_HAND &&	quaternionZ>0.9) ||
                    (eType == ModuleType.TYPE_RIGHT_HAND && quaternionZ<-0.9)) //Boca arriba
                {
                    //	pitchAngle = -pitchAngle;
                    yawAngle  = -yawAngle;
                    AsignAngle2Axes(yawAngle, pitchAngle, rollAngle);
                }
                else*/
                {
                    AsignAngle2Axes(pitchAngle, yawAngle, rollAngle);

                }
            }
        }

        /* 
            Function: AsignAngle2Axes
            Set rotation angle to each axis of the hand transform. 
        */
        private void AsignAngle2Axes(float fPitchA, float fYawA, float fRollA)
        {
            switch (_ePitchAxis)
            {
                case eModuleAxis.AXIS_X:
                    _fHandXAngle = fPitchA;
                    break;
                case eModuleAxis.AXIS_Y:
                    _fHandYAngle = fPitchA;
                    break;
                case eModuleAxis.AXIS_Z:
                    _fHandZAngle = fPitchA;
                    break;
            }

            switch (_eYawAxis)
            {
                case eModuleAxis.AXIS_X:
                    _fHandXAngle = fYawA;
                    break;
                case eModuleAxis.AXIS_Y:
                    _fHandYAngle = fYawA;
                    break;
                case eModuleAxis.AXIS_Z:
                    _fHandZAngle = fYawA;
                    break;
            }

            switch (_eRollAxis)
            {
                case eModuleAxis.AXIS_X:
                    _fHandXAngle = fRollA;
                    break;
                case eModuleAxis.AXIS_Y:
                    _fHandYAngle = fRollA;
                    break;
                case eModuleAxis.AXIS_Z:
                    _fHandZAngle = fRollA;
                    break;
            }
        }

        /* 
            Function: MoveFingers
            Updates each finger transform rotation according with Captoglove sensor movement.

            Notes: 
            Call this function in the Update() of your app to simulate fingers movement.
        */
        public void MoveFingers()
        {
            if (GetModuleStarted())
                SetFingersNewAngle();

            //If finger transform was assigned
            if (_taFinger[_nThumbPos] != null)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (_taFinger[i] != null)
                        _taFinger[i].localEulerAngles = _vaFingerAngle[i];
                }

                _taThumbChild[1].localEulerAngles = _vaFingerChild1Angle[_nThumbPos];
                _taIndexChild[1].localEulerAngles = _vaFingerChild1Angle[_nIndexPos];
                _taMiddleChild[1].localEulerAngles = _vaFingerChild1Angle[_nMiddlePos];
                _taRingChild[1].localEulerAngles = _vaFingerChild1Angle[_nRingPos];
                _taPinkyChild[1].localEulerAngles = _vaFingerChild1Angle[_nPinkyPos];

                _taThumbChild[2].localEulerAngles = _vaFingerChild2Angle[_nThumbPos];
                _taIndexChild[2].localEulerAngles = _vaFingerChild2Angle[_nIndexPos];
                _taMiddleChild[2].localEulerAngles = _vaFingerChild2Angle[_nMiddlePos];
                _taRingChild[2].localEulerAngles = _vaFingerChild2Angle[_nRingPos];
                _taPinkyChild[2].localEulerAngles = _vaFingerChild2Angle[_nPinkyPos];

            }
        }

        /* 
            Function: SetFingersNewAngle
            Calculates each finger transform rotation according with Captoglove sensor movement.              
        */
        private void SetFingersNewAngle()
        {
            var args = psEventSensorState as BoardFloatSequenceEventArgs;
            float temp;

            if (args != null)
            {
                if (!GetFingerAlgorithmReady())
                {
                    SetFingerAlgorithm();
                    for (int i = 0; i < 10; i++)
                    {
                        _faSensorTrigger[i] = (pfaFingerSensorMaxValue[i] - pfaFingerSensorMinValue[i]) / 2;
                    }
                }

                for (int i = 0; i < 10; i++)
                {
                    _faSensorValue[i] = args.Value[i];
                    temp = _faSensorValue[i] * _faFingerVarA[i] + _faFingerVarB[i];

                    //Avoid shaking
                    if (Mathf.Abs(temp - _vaFingerAngle[i].x) > _nOmittedDegrees)
                    {
                        _vaFingerAngle[i].x = temp;
                        _vaFingerChild1Angle[i].x = args.Value[i] * _faFingerChild1VarA[i] + _faFingerChild1VarB[i];
                        _vaFingerChild2Angle[i].x = args.Value[i] * _faFingerChild2VarA[i] + _faFingerChild2VarB[i];
                    }
                    
                    if (GetHandtype() == eHandType.TYPE_RIGHT_HAND && 
                        (_vaFingerAngle[i].x > _faFingerMinRotation[i]))                        
                    {
                        _vaFingerAngle[i].x = _faFingerMinRotation[i];
                        _vaFingerChild1Angle[i].x = _faFingerChild1MinRotation[i];
                        _vaFingerChild2Angle[i].x = _faFingerChild2MinRotation[i];
                    }
                    else if (GetHandtype() == eHandType.TYPE_LEFT_HAND &&
                            (_vaFingerAngle[i].x < _faFingerMinRotation[i]))
                    {
                        _vaFingerAngle[i].x = _faFingerMinRotation[i];
                        _vaFingerChild1Angle[i].x = _faFingerChild1MinRotation[i];
                        _vaFingerChild2Angle[i].x = _faFingerChild2MinRotation[i];
                    }
                }
            }
        }

        /* 
            Function: SetFingerAlgorithm
            Creates algorithm for finger movement.
        */
        private void SetFingerAlgorithm()
        {
            float num = 0f;

            for (int i = 0; i < 10; i++)
            {
                num = pfaFingerSensorMinValue[i] - pfaFingerSensorMaxValue[i];

                if (num != 0f)
                {
                    _faFingerVarA[i] = (_faFingerMaxRotation[i] - _faFingerMinRotation[i]) / num;
                    _faFingerChild1VarA[i] = (_faFingerChild1MaxRotation[i] - _faFingerChild1MinRotation[i]) / num;
                    _faFingerChild2VarA[i] = (_faFingerChild2MaxRotation[i] - _faFingerChild2MinRotation[i]) / num;
                }

                _faFingerVarB[i] = _faFingerMinRotation[i] - (_faFingerVarA[i] * pfaFingerSensorMaxValue[i]);
                _faFingerChild1VarB[i] = _faFingerChild1MinRotation[i] - (_faFingerChild1VarA[i] * pfaFingerSensorMaxValue[i]);
                _faFingerChild2VarB[i] = _faFingerChild2MinRotation[i] - (_faFingerChild2VarA[i] * pfaFingerSensorMaxValue[i]);
            }

            //A and B are set correctly after the properties are read
            if (GetPropertiesRead())
            {
                TraceLog("Finger algorithm ready");
                SetFingerAlgorithmReady(true);
            }
        }

        /* 
            Function: GetHandPosition
            Returns:
                Global position of hand transform
        */
        public Vector3 GetHandPosition()
        {
            if (_tHand != null)
                return _tHand.position;
            else
                return new Vector3(0, 0, 0);
        }

        /* 
            Function: GetHandRotation
            Returns:
                Global euler angles of hand transform
        */
        public Vector3 GetHandRotation()
        {
            if (_tHand != null)
                return _tHand.eulerAngles;
            else
                return new Vector3(0, 0, 0);
        }

        /* 
            Function: GetThumbPosition
            Returns:
                Global position of thumb finger transform
        */
        public Vector3 GetThumbPosition()
        {
            if (_taFinger[_nThumbPos] != null)
                return _taFinger[_nThumbPos].position;
            else
                return new Vector3(0, 0, 0);
        }

        /* 
            Function: GetIndexPosition
            Returns:
                Global position of index finger transform
        */
        public Vector3 GetIndexPosition()
        {
            if (_taFinger[_nIndexPos] != null)
                return _taFinger[_nIndexPos].position;
            else
                return new Vector3(0, 0, 0);
        }

        /* 
            Function: GetMiddlePosition
            Returns:
                Global position of middle finger transform
        */
        public Vector3 GetMiddlePosition()
        {
            if (_taFinger[_nMiddlePos] != null)
                return _taFinger[_nMiddlePos].position;
            else
                return new Vector3(0, 0, 0);
        }

        /* 
            Function: GetRingPosition
            Returns:
                Global position of ring finger transform
        */
        public Vector3 GetRingPosition()
        {
            if (_taFinger[_nRingPos] != null)
                return _taFinger[_nRingPos].position;
            else
                return new Vector3(0, 0, 0);
        }

        /* 
            Function: GetPinkyPosition
            Returns:
                Global position of pinky finger transform
        */
        public Vector3 GetPinkyPosition()
        {
            if (_taFinger[_nPinkyPos] != null)
                return _taFinger[_nPinkyPos].position;
            else
                return new Vector3(0, 0, 0);
        }

        /* 
            Function: IsSensorPressed
            Returns:
                true - Pressure sensor is being pressed
                false - Pressure sensor is released        
        */
        public bool IsSensorPressed()
        {
            bool bRet = false;

            if (GetModuleStarted())
            {
                if (_faSensorValue[_nPressurePos] > _faSensorTrigger[_nPressurePos])
                    bRet = true;
            }

            return bRet;
        }

        /* 
            Function: IsHandClosed
            Returns:
                true - All fingers are bent more than 50%
                false - All fingers are extended or bent less than 50%
        */
        public bool IsHandClosed()
        {
            bool bRet = false;

            if (GetModuleStarted())
            {
                if (_faSensorValue[_nIndexPos] < _faSensorTrigger[_nIndexPos] &&
                    _faSensorValue[_nMiddlePos] < _faSensorTrigger[_nMiddlePos] &&
                    _faSensorValue[_nRingPos] < _faSensorTrigger[_nRingPos] &&
                    _faSensorValue[_nPinkyPos] < _faSensorTrigger[_nPinkyPos])
                {

                    bRet = true;
                }
            }

            return bRet;
        }

        /* 
            Function: FingerGesture1
            Returns:
                true - Index finger is extended or bent less than 50% and the other fingers are bent more than 50%
                false - Condition is not met
        */
        public bool FingerGesture1()
        {
            bool bRet = false;

            if (GetModuleStarted())
            {
                if (_faSensorValue[_nIndexPos] > _faSensorTrigger[_nIndexPos] &&
                    _faSensorValue[_nMiddlePos] < _faSensorTrigger[_nMiddlePos] &&
                    _faSensorValue[_nRingPos] < _faSensorTrigger[_nRingPos] &&
                    _faSensorValue[_nPinkyPos] < _faSensorTrigger[_nPinkyPos])
                {
                    bRet = true;
                }
            }

            return bRet;
        }

        /* 
            Function: FingerGesture2
            Returns:
            true - Index and middle finger are extended or bent less than 50% and the other fingers are bent more than 50%
            false - Condition is not met
        */
        public bool FingerGesture2()
        {
            bool bRet = false;

            if (GetModuleStarted())
            {
                if (_faSensorValue[_nIndexPos] > _faSensorTrigger[_nIndexPos] &&
                    _faSensorValue[_nMiddlePos] > _faSensorTrigger[_nMiddlePos] &&
                    _faSensorValue[_nRingPos] < _faSensorTrigger[_nRingPos] &&
                    _faSensorValue[_nPinkyPos] < _faSensorTrigger[_nPinkyPos])
                {
                    bRet = true;
                }
            }

            return bRet;
        }

        /* 
            Function: FingerGesture3
            Returns:
            true - Index, middle and ring finger are extended or bent less than 50% and the other fingers are bent more than 50%
            false - Condition is not met
        */
        public bool FingerGesture3()
        {
            bool bRet = false;

            if (GetModuleStarted())
            {
                if (_faSensorValue[_nIndexPos] > _faSensorTrigger[_nIndexPos] &&
                    _faSensorValue[_nMiddlePos] > _faSensorTrigger[_nMiddlePos] &&
                    _faSensorValue[_nRingPos] > _faSensorTrigger[_nRingPos] &&
                    _faSensorValue[_nPinkyPos] < _faSensorTrigger[_nPinkyPos])
                {
                    bRet = true;
                }
            }

            return bRet;
        }

        /* 
            Function: SaveHandMovInFile
            Saves module data in file with following format: x hand rotation; y hand rotation; z hand rotation  

            Parameters:
                sFileName - File name

            Example:
            --- Code
            SaveHandMovInFile("RightHandMov.csv");
            ---

            Notes:
            Call this function in Updated() of your app to save data continuously 
        */
        public void SaveHandMovInFile(string sFileName)
        {
            var args = psEventTaredQuart as BoardQuaternionEventArgs;
            string serializedData;

            if (args != null)
            {
                if (!bHandFile)
                {
                    swHandWriter = new StreamWriter(sFileName, true);
                    bHandFile = true;
                }

                float quaternionX = args.Value.X;
                float quaternionY = args.Value.Y;
                float quaternionZ = args.Value.Z;

                serializedData =
                    quaternionX.ToString() + ";" +
                    quaternionY.ToString() + ";" +
                    quaternionZ.ToString() + "\n";

                // Write to disk
                if (swHandWriter != null)
                    swHandWriter.Write(serializedData);

            }
        }

        /* 
            Function: SaveFingerMovInFile
            Saves sensor data in file with following format: thumb finger; index finger; middle finger; ring finger; pinky finger; pressure sensor

            Parameters:
                sFileName - File name

            Example:
            --- Code
            SaveFingerMovInFile("RightFingerMov.csv");
            ---

            Notes:
            Call this function in Updated() of your app to save data continuously 
        */
        public void SaveFingerMovInFile(string sFileName)
        {
            var args = psEventSensorState as BoardFloatSequenceEventArgs;
            string serializedData;

            if (args != null)
            {
                if (!bFingerFile)
                {
                    swFingerWriter = new StreamWriter(sFileName, true);
                    bFingerFile = true;
                }

                for (int i = 0; i < 10; i++)
                {
                    _faSensorValue[i] = args.Value[i];
                }

                serializedData =
                    _faSensorValue[_nThumbPos].ToString() + ";" +
                    _faSensorValue[_nIndexPos].ToString() + ";" +
                    _faSensorValue[_nMiddlePos].ToString() + ";" +
                    _faSensorValue[_nRingPos].ToString() + ";" +
                    _faSensorValue[_nPinkyPos].ToString() + ";" +
                    _faSensorValue[_nPressurePos].ToString() + "\n";

                // Write to disk
                if (swFingerWriter != null)
                    swFingerWriter.Write(serializedData);

            }
        }

    }
}
