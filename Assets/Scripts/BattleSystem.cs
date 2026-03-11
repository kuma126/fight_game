using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, ACTION, WON, LOST }

public class BattleSystem : MonoBehaviour
{
    public BattleState state;
    public GameObject attackButton; //インスペクターでボタンを登録

    //フィールド上の全ユニットを保持
    private List<Unit> allUnits = new List<Unit>();
    private int unitIndex = 0; //今何番目のキャラが行動中か

    void Start()
    {
        //最初にボタンを隠す
        attackButton.SetActive(false);
        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        //1. シーン内の全Unitスクリプトを探してリストに入れる
        //本来は速さでソートするべき
        allUnits.AddRange(FindObjectsOfType<Unit>());
        Debug.Log("戦闘開始！参加者数：" + allUnits.Count);
        yield return new WaitForSeconds(1f);
        NextTurn();
    }

    void NextTurn()
    {
        //全員動いたら最初に戻る(またはターン終了処理)
        if(unitIndex >= allUnits.Count)
        {
            unitIndex = 0;
            Debug.Log("--- 新しいターン ---");
        }

        Unit currentUnit = allUnits[unitIndex];

        if (currentUnit.isPlayer)
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn(currentUnit);
        }
        else
        {
            state = BattleState.ENEMYTURN;
            //敵のターンはボタンを隠す
            attackButton.SetActive(false);//敵のターンはボタンを隠す
            StartCoroutine(EnemyTurn(currentUnit));
        }
    }

    void PlayerTurn(Unit player)
    {
        Debug.Log(player.unitName + "の番です．コマンドを入力してください．");
        //ここでUIを表示する処理を入れる
        //プレイヤーのターンだけボタンを表示
        attackButton.SetActive(true);
    }

    //ボタンをクリックしたときに実行される関数
    public void OnAttackButton()
    {
        if (state != BattleState.PLAYERTURN) return; //プレイヤーのターンでないときは無視

        StartCoroutine(PlayerAttack());
    }

    IEnumerator PlayerAttack()
    {
        Unit currentUnit = allUnits[unitIndex];
        state = BattleState.ACTION;
        attackButton.SetActive(false); //行動中はボタンを隠す

        //1.ターゲットを決める(ここでは敵グループの最初の1人を狙う)
        Unit target = allUnits.Find(u => !u.isPlayer && !u.IsDead());

        if(target != null)
        {
            Debug.Log(target.unitName + "に" + currentUnit.unitName + "の攻撃！");

            //2.ダメージ計算(いまは攻撃力そのまま)
            target.TakeDamage(currentUnit.attack);

            yield return new WaitForSeconds(0.8f);

            //3.敵が倒れたかチェック
            if (target.IsDead())
            {
                Debug.Log(target.unitName + "を倒した！");
                //倒れたキャラをリストから削除するなどの処理を入れる
            }
        }

        unitIndex++;
        CheckBattleOver();
    }

    IEnumerator EnemyTurn(Unit enemy)
    {
        state = BattleState.ACTION;

        //1. ターゲットを決める(ここでは味方グループの最初の1人を狙う)
        Unit target = allUnits.Find(u => u.isPlayer && !u.IsDead());

        if(target != null)
        {
            Debug.Log(enemy.unitName + "の攻撃！");

            //2. ダメージ計算(いまは攻撃力そのまま)
            target.TakeDamage(enemy.attack);

            yield return new WaitForSeconds(0.8f);
        }

        //3.味方が倒れたかチェック
        if (target.IsDead())
        {
            Debug.Log(target.unitName + "が倒れてしまった！");
            //倒れたキャラをリストから削除するなどの処理を入れる
        }

        unitIndex++;
        CheckBattleOver();
    }

    void CheckBattleOver()
    {
        //味方が全滅したか
        bool allPlayersDead = allUnits.FindAll(u => u.isPlayer && !u.IsDead()).Count == 0;
        //敵が全滅したか
        bool allEnemiesDead = allUnits.FindAll(u => !u.isPlayer && !u.IsDead()).Count == 0;

        if (allEnemiesDead)
        {
            state = BattleState.WON;
            Debug.Log("勝利！");
        }
        else if (allPlayersDead)
        {
            state = BattleState.LOST;
            Debug.Log("敗北...");
        }
        else
        {
            NextTurn();
        }
    }

    void Update()
    {
        
    }
}
