using UnityEngine;

public class myLever : MonoBehaviour
{
    public DPS myDPS;

    public bool bCollided;
    private Vector3 vLocalPosition;
    private float fMaxYPos, fMinYPos, fXPos, fZPos, fTrigger;        
    private Transform[] taChildren;

    // Start is called before the first frame update
    void Start()
    {
        fXPos = transform.localPosition.x;
        fZPos = transform.localPosition.z;
        fMaxYPos = transform.localPosition.y;
        fMinYPos = fMaxYPos - 2.8f;
        fTrigger = (fMinYPos - fMaxYPos) / 2;

        taChildren = transform.GetComponentsInChildren<Transform>();
        bCollided = false;
    }

    // Update is called once per frame
    void Update()
    {
        LimitPosition();
    }


    private void LimitPosition()
    {
        vLocalPosition = transform.localPosition;

        if (vLocalPosition.y > fMaxYPos)
            vLocalPosition.y = fMaxYPos;        
        else if (vLocalPosition.y < fMinYPos)
            vLocalPosition.y = fMinYPos;

        transform.localPosition = new Vector3(fXPos, vLocalPosition.y, fZPos) ;

        CheckLocalPos();        
    }

    private void CheckLocalPos()
    {
        if (transform.localPosition.y > fTrigger)
            myDPS.ShowElectricField();
        else
            myDPS.HideElectricField();
    }

    public void ChangePosition(float vYPos)
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, vYPos);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            Debug.Log("myLever OnTriggerEnter!!");
            foreach (Transform go in taChildren)
            {
                go.GetComponent<Renderer>().material.color = Color.green;
            }

            bCollided = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            foreach (Transform go in taChildren)
            {
                go.GetComponent<Renderer>().material.color = Color.red;
            }
            bCollided = false;
        }
    }
}
