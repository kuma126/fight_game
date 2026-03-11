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

    //攻撃演出などで元の場所に戻るために使う
    [HideInInspector] public Vector3 startPosition;

    private void Awake()
    {
        startPosition = transform.position;
    }

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        if(hp < 0) hp = 0;

        Debug.Log(unitName + "は" + dmg + "のダメージを受けた！ 残りHP: " + hp);
    }

    public bool IsDead()
    {
        return hp <= 0;
    }
}
