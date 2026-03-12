using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    public string unitName;
    public int hp;
    public int maxHP;
    public int attack;
    public bool isPlayer;

    public Slider hpSlider; //インスペクターでスライダーを登録

    //攻撃演出などで元の場所に戻るために使う
    [HideInInspector] public Vector3 startPosition;

    private void Awake()
    {
        startPosition = transform.position;
    }

    private void Start()
    {
        //バーの最大値と現在地を設定
        if(hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = hp;

        }
    }

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        if(hp < 0) hp = 0;

        //HPバーの値を更新
        if(hpSlider != null)
        {
            hpSlider.value = hp;
        }

        Debug.Log(unitName + "は" + dmg + "のダメージを受けた！ 残りHP: " + hp);
    }

    public bool IsDead()
    {
        return hp <= 0;
    }
}
