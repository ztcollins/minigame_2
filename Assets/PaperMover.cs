using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperMover : MonoBehaviour
{

    [SerializeField] float speed = 1.0f; //how fast it shakes
    [SerializeField] float amount = 1.0f; //how much it shakes
    Vector3 startPos;

    // Start is called before the first frame update
    void Start()
    {
        startPos = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        float xShake = startPos.x + Mathf.Sin(Time.time * speed) * amount;
        float dogPull = 0.005f;
        float playerPull = -0.01f;

        float currZ = transform.position.z;
        currZ += dogPull;

        if(Input.GetKey(KeyCode.Space)) {
            Debug.Log("space key was pressed!");
            currZ += playerPull;
        }

        transform.position = new Vector3(xShake,0,currZ);
        
    }
}
