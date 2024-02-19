using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dog_follow : MonoBehaviour
{
    public Transform paper;
    public Transform lookAt;
    // Start is called before the first frame update
    private Vector3 localPos;
    void Start()
    {
        localPos = this.transform.position - paper.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate(){
        this.transform.position = paper.position + localPos;
        this.transform.LookAt(lookAt);
    }
}
