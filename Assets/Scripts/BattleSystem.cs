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

        // すでに死亡しているユニットはスキップして次へ
        if (currentUnit.IsDead())
        {
            unitIndex++;
            NextTurn();
            return;
        }

        //準備コルーチン呼び出し
        StartCoroutine(PrepareTurn(currentUnit));

    }

    IEnumerator PrepareTurn(Unit unit)
    {
        //1.一歩前に出る
        yield return StartCoroutine(unit.StepForward());

        if (unit.isPlayer)
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn(unit);
        }
        else
        {
            state = BattleState.ENEMYTURN;
            yield return new WaitForSeconds (0.5f);
            StartCoroutine (EnemyTurn(unit));
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

        StartCoroutine(ExecutePlayerAction());
    }

    /*
    IEnumerator PlayerAttack()
    {
        Unit currentUnit = allUnits[unitIndex];
        state = BattleState.ACTION;
        attackButton.SetActive(false); //行動中はボタンを隠す

        //1.一歩前に出る
        yield return StartCoroutine(currentUnit.StepForward());

        //2.ターゲットを決める(ここでは敵グループの最初の1人を狙う)
        Unit target = allUnits.Find(u => !u.isPlayer && !u.IsDead());

        if (target != null)
        {
            Debug.Log(target.unitName + "に" + currentUnit.unitName + "の攻撃！");

            //ダメージ計算(いまは攻撃力そのまま)
            target.TakeDamage(currentUnit.attack);

            yield return new WaitForSeconds(0.8f);

            //敵が倒れたかチェック
            if (target.IsDead())
            {
                Debug.Log(target.unitName + "を倒した！");
                //倒れたキャラをリストから削除するなどの処理を入れる
            }
        }

        //3.元の位置に戻る
        yield return StartCoroutine(currentUnit.StepBack());

        unitIndex++;
        CheckBattleOver();
    }*/

    IEnumerator ExecutePlayerAction()
    {
        state = BattleState.ACTION;
        attackButton.SetActive(false);

        Unit currentUnit = allUnits[unitIndex];
        Unit target = allUnits.Find(u => !u.isPlayer && !u.IsDead());//敵の最初の一人を狙う

        if (target != null)
        {
            //2.ジャンプして攻撃
            Debug.Log(currentUnit.unitName + "の攻撃！");
            yield return StartCoroutine(currentUnit.JumpAttack(target));
            target.TakeDamage(currentUnit.attack);
            if (target.IsDead())
            {
                yield return StartCoroutine(target.Die()); // 倒れるのを待つ
            }
        }

        //3.元の位置に戻る
        yield return StartCoroutine(currentUnit.StepBack());

        unitIndex++;
        CheckBattleOver();
    }
  

    IEnumerator EnemyTurn(Unit enemy)
    {
        state = BattleState.ACTION;

        //2. ターゲットを決める(ここでは味方グループの最初の1人を狙う)
        Unit target = allUnits.Find(u => u.isPlayer && !u.IsDead());

        if(target != null)
        {
            Debug.Log(enemy.unitName + "の攻撃！");
            yield return StartCoroutine(enemy.JumpAttack(target));
            //. ダメージ計算(いまは攻撃力そのまま)
            target.TakeDamage(enemy.attack);
            yield return new WaitForSeconds(0.8f);
        }

        if (target.IsDead())
        {
            Debug.Log(target.unitName + "が倒れてしまった！");
            yield return StartCoroutine(target.Die()); // 倒れるのを待つ
        }

        //3.元の位置に戻る
        yield return StartCoroutine(enemy.StepBack());

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
