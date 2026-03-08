using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DoMoveTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.DOMoveX(transform.position.x + 2, 2f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
