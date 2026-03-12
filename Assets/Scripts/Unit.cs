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

    public float stepDistance = 1.0f; //前に出る距離
    public float moveSpeed = 10f; //移動スピード

    //死亡状態を管理
    [HideInInspector] public bool isDead = false;
    //攻撃演出などで元の場所に戻るために使う
    [HideInInspector] public Vector3 startPosition;

    private Animator anim;

    private void Awake()
    {
        startPosition = transform.position;
        anim = GetComponent<Animator>();//Animatorを取得
    }

    private void Start()
    {
        //バーの最大値と現在地を設定
        if(hpSlider != null)
        {
            hpSlider.minValue = 0;
            hpSlider.maxValue = maxHP;
            hpSlider.value = hp;
        }
    }

    public void TakeDamage(int dmg)
    {
        //すでに死んでいる場合は何もしない
        if (isDead) return;

        hp -= dmg;
        if(hp < 0) hp = 0;

        //HPバーの値を更新
        if(hpSlider != null)
        {
            hpSlider.value = hp;
        }

        StartCoroutine(DamagedEffect());

        Debug.Log(unitName + "は" + dmg + "のダメージを受けた！ 残りHP: " + hp);
    }

    public bool IsDead()
    {
        return hp <= 0;
    }


    // --- 演出関連 --- //
    /// <summary>
    /// 前方に倒れる演出
    /// </summary>
    public IEnumerator Die()
    {
        if (isDead) yield break; //二重に死なないように
        isDead = true;

        anim.enabled = false; // アニメーションを止めて物理的に倒す

        // 向いている方向を維持したまま、手前に90度倒す回転
        Quaternion targetRotation = Quaternion.Euler(90, transform.eulerAngles.y, 0);
        // 足が浮かないよう少し沈める
        Vector3 targetPosition = transform.position + Vector3.up * 0.4f;

        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, percent);
            transform.position = Vector3.Lerp(transform.position, targetPosition, percent);
            yield return null;
        }

        // HPバーを非表示にする
        if (hpSlider != null) hpSlider.gameObject.SetActive(false);
    } 

    public IEnumerator StepForward()
    {
        anim.Play("Running");
        //向いている方向にstepDistance分だけ進んだ目標地点を計算
        Vector3 targetPos = startPosition + transform.forward * stepDistance;
        yield return MoveToPoint(targetPos);
        anim.Play("Idle");
      
    }

    //元の位置に戻る演出
    public IEnumerator StepBack()
    {
        //死亡していたら戻る必要がない
        if(isDead) yield break;
        anim.Play("Running");
        yield return MoveToPoint(startPosition);
        anim.Play("Idle");
    }

    //移動の共通処理
    private IEnumerator MoveToPoint(Vector3 targetPos)
    {
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }


    // 攻撃時のジャンプ（放物線を描くように動かす）
    public IEnumerator JumpAttack(Unit target)
    {
        Vector3 currentStartPos = transform.position;
        Vector3 targetJumpPos = transform.position + transform.forward * 0.5f;
        float duration = 0.5f; // 少し速くしました
        float elapsed = 0f;

        anim.Play("Jump"); // ジャンプアニメ開始

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;

            // 空中で落下アニメーションに切り替える（頂点を過ぎたあたり）
            if (percent > 0.5f) anim.Play("Fall");

            Vector3 currentPos = Vector3.Lerp(currentStartPos, targetJumpPos, percent);
            // Sin波でジャンプ。PIの範囲(0~180度)で0→1→0となる
            currentPos.y += Mathf.Sin(percent * Mathf.PI) * 1.2f;

            transform.position = currentPos;
            yield return null;
        }

        anim.Play("Idle");
    }


    //ダメージを受けたときにのけぞる演出
    public IEnumerator DamagedEffect()
    {
        if (isDead) yield break;//死んでいるときは無効

        //1.後ろに少し傾ける
        //元の回転を保持
        Quaternion originalRotation = transform.rotation;
        //30度位後ろに倒す角度を計算
        Quaternion leanBackRotation = originalRotation * Quaternion.Euler(-30, 0, 0);

        float elapsed = 0f;
        float duration = 0.15f; // 素早く仰け反る

        // 仰け反る
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(originalRotation, leanBackRotation, elapsed / duration);
            yield return null;
        }

        // 2. 元に戻る
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(leanBackRotation, originalRotation, elapsed / duration);
            yield return null;
        }

        // 最後に念のため完全に元の回転に戻す
        transform.rotation = originalRotation;
    }


}
