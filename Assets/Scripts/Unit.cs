using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public string unitName;
    public int hp;
    public int maxHP;
    public int attack;
    public bool isPlayer;

    //UŒ‚‰‰o‚È‚Ç‚ÅŒ³‚ÌêŠ‚É–ß‚é‚½‚ß‚Ég‚¤
    [HideInInspector] public Vector3 startPosition;

    private void Awake()
    {
        startPosition = transform.position;
    }
}
