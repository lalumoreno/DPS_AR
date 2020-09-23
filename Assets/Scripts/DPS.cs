using UnityEngine;

public class DPS : MonoBehaviour
{
    private bool bCollided;
    private GUIStyle styleBig, styleSmall;    
    private Vector3 scaleChange;

    // Start is called before the first frame update
    void Start()
    {
        bCollided = false;
        //taChildren = transform.GetComponentsInChildren<Transform>();

        //Messages in display
        styleBig = new GUIStyle();
        styleBig.normal.textColor = Color.red;
        styleBig.fontSize = 100;

        styleSmall = new GUIStyle();
        styleSmall.normal.textColor = Color.red;
        styleSmall.fontSize = 50;

        scaleChange = new Vector3(0.01f, 0.01f, 0.01f);        

    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale += scaleChange;

        // when the field scale extends 1.2f.
        if (transform.localScale.y > 1.2f || transform.localScale.y > 1.2f)
        {
            transform.localScale = new Vector3(0, 0, 0);
        }
    }

    public void ShowElectricField()
    {
        gameObject.SetActive(true);
    }

    public void HideElectricField()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("EField OnTriggerEnter!!");            
            bCollided = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")                                
            bCollided = false;        
    }

    void OnGUI()
    {
        if (bCollided)
        {
            GUI.Label(new Rect(0,0, 200f, 200f), "PELIGRO", styleBig);
            GUI.Label(new Rect(0,0+100f, 200f, 200f), "Desenergize el DPS", styleSmall);
        }

    }


}
