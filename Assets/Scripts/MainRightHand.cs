using UnityEngine;
using GITEICaptoglove;

public class MainRightHand : MonoBehaviour
{
    //Assign transforms in Unity editor    
    public Transform tThumb;
    public Transform tIndex;
    public Transform tMiddle;
    public Transform tRing;    
    public Transform tPinky;    
    
    public myLever lever;

    private MyHand RightHand = null;     
    private GUIStyle style;      

    // Start is called before the first frame update
    void Start()
    {
        //Configuration for Captoglove sensor as Right Hand 
        RightHand = new MyHand(2443, MyHand.eHandType.TYPE_RIGHT_HAND);
        RightHand.EnableLog();
        RightHand.SetHandTransform(transform, Module.eModuleAxis.AXIS_X, Module.eModuleAxis.AXIS_Z, Module.eModuleAxis.AXIS_Y);
        RightHand.SetFingerTransform(tThumb, tIndex, tMiddle, tRing, tPinky);    

        //Messages in display
        style = new GUIStyle();
        style.normal.textColor = Color.black;        
        
        //Starts Captoglove sensor 
        RightHand.Start();        
    }

    // Update is called once per frame
    void Update()
    {      
        //Enables finger movement
        RightHand.MoveFingers();        

        //Interaction with myLever
        if (RightHand.IsHandClosed() && lever.bCollided)
        {
            CatchLever();
        }		

    }

    private void CatchLever()
    {
        Vector3 vHandPos = RightHand.GetHandPosition();     
        lever.ChangePosition( vHandPos.z);
    }

    void OnGUI()
    {        
        if (RightHand.GetPropertiesRead())
        {
            GUI.Label(new Rect(Screen.width-100, 0, 200f, 200f), "Right Hand ready", style);
        }
    }

    private void OnDestroy()
    {
        if(RightHand!= null)
            RightHand.Stop();
    }

}
