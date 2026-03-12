using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//HPバーが常にカメラを向くようにする
public class Billboard : MonoBehaviour
{
    private void LateUpdate()
    {
        //常にメインカメラの方を向く
        transform.forward = Camera.main.transform.forward;
    }
}