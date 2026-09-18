using System;
using System.Collections.Generic;

namespace GamePartsApp
{
    // ====================================================================
    // Boss：ボスキャラクター（雑魚召喚、円形攻撃、レーザー、隙あり）
    // ====================================================================
    public class Boss : Enemy
    {
        // ------------------------------------------------------------
        // ボスの「今の行動状態」を表す
        // ------------------------------------------------------------
        // enum（以前のCardSuit, Gender等と同じ考え方）を使い、
        // 「今、何をしている最中か」を、名前の付いた選択肢で管理する。
        //
        // int（0,1,2...）で管理するより、
        // BossState.Attacking のように書けるので、
        // タイポの心配がなく、コードも読みやすくなる。
        public enum BossState
        {
            Idle,        // 待機中（次の行動を選ぶ前）
            Circular,    // 円形の広範囲攻撃中
            Laser,       // レーザー攻撃中
            Summoning,   // 雑魚を召喚中
            Exposed      // 隙（無防備）な状態
        }

        public BossState CurrentState { get; private set; } = BossState.Idle;

        // ------------------------------------------------------------
        // 各行動の、残り時間（フレーム数）
        // ------------------------------------------------------------
        private int stateFramesLeft = 0;

        private const int CIRCULAR_DURATION = 30;   // 円形攻撃の持続時間
        private const int LASER_DURATION = 20;      // レーザーの持続時間（速い）
        private const int SUMMON_DURATION = 30;      // 召喚モーションの時間
        private const int EXPOSED_DURATION = 60;     // 隙の時間（約1秒）
        private const int IDLE_DURATION = 60;        // 次の行動までの、待機時間

        private Random rand = new Random();

        // ------------------------------------------------------------
        // 召喚された雑魚（2体、2発で倒せる弱い個体）を、外部から見えるようにする
        // ------------------------------------------------------------
        // List<Enemy> にしているのは、
        // 「召喚される個体は、Enemyの一種でありさえすればいい」
        // という考え方（今日のList<GachaItem>等と同じ発想）。
        public List<Enemy> SummonedMinions { get; private set; } = new List<Enemy>();

        // ------------------------------------------------------------
        // コンストラクタ
        // ------------------------------------------------------------
        public Boss(float startX, float startY)
            : base("ボス", startX, startY, maxHp: 300, attackPower: 30)
        {
        }

        // ------------------------------------------------------------
        // 「隙（Exposed）」状態かどうか、外部から判定しやすくするプロパティ
        // ------------------------------------------------------------
        // これは、今日何度も使ったswitch式ではなく、
        // 単純な比較（==）を使った式形式プロパティ。
        public bool IsExposed => CurrentState == BossState.Exposed;

        // ------------------------------------------------------------
        // 毎フレームの更新：ボスの行動サイクルを管理する
        // ------------------------------------------------------------
        public override void UpdateFrame(float playerX, float playerY)
        {
            stateFramesLeft--;

            if (stateFramesLeft > 0)
            {
                // まだ今の行動の途中なら、何もしない
                return;
            }

            // ------------------------------------------------------------
            // 現在の状態が終わったら、次に何をするか決める
            // ------------------------------------------------------------
            // switch式で、「今の状態」から「次に何をすべきか」を判定する。
            switch (CurrentState)
            {
                case BossState.Idle:
                    // 待機明け：ランダムに次の行動を選ぶ
                    ChooseNextAction();
                    break;

                case BossState.Circular:
                case BossState.Laser:
                    // ------------------------------------------------------------
                    // 大技（円形攻撃・レーザー）の後は、必ず「隙」ができる
                    // ------------------------------------------------------------
                    // これが、要件にあった「大技のあと隙」の実装部分。
                    CurrentState = BossState.Exposed;
                    stateFramesLeft = EXPOSED_DURATION;
                    break;

                case BossState.Summoning:
                    // 召喚が終わったら、待機に戻る
                    CurrentState = BossState.Idle;
                    stateFramesLeft = IDLE_DURATION;
                    break;

                case BossState.Exposed:
                    // 隙が終わったら、待機に戻る
                    CurrentState = BossState.Idle;
                    stateFramesLeft = IDLE_DURATION;
                    break;
            }
        }

        // ------------------------------------------------------------
        // 次の行動を、ランダムに選ぶ
        // ------------------------------------------------------------
        private void ChooseNextAction()
        {
            // ------------------------------------------------------------
            // 0〜2の、ランダムな整数で行動を決める（今日と同じRandomの使い方）
            // ------------------------------------------------------------
            int choice = rand.Next(0, 3);

            switch (choice)
            {
                case 0:
                    CurrentState = BossState.Circular;
                    stateFramesLeft = CIRCULAR_DURATION;
                    break;

                case 1:
                    CurrentState = BossState.Laser;
                    stateFramesLeft = LASER_DURATION;
                    break;

                case 2:
                    CurrentState = BossState.Summoning;
                    stateFramesLeft = SUMMON_DURATION;
                    SummonMinions();
                    break;
            }
        }

        // ------------------------------------------------------------
        // 雑魚を2体、召喚する
        // ------------------------------------------------------------
        private void SummonMinions()
        {
            // ------------------------------------------------------------
            // すでに召喚済みの雑魚がいたら、まず整理する
            // ------------------------------------------------------------
            // 今日のList<T>操作の応用：
            // RemoveAll は「条件に合う要素を、まとめて削除する」メソッド。
            // 「もう死んでいる（IsAliveがfalse）」個体を、リストから消している。
            SummonedMinions.RemoveAll(minion => !minion.IsAlive);

            // ------------------------------------------------------------
            // 「2発で死ぬ」弱い個体を、Enemyクラスから直接2体作る
            // ------------------------------------------------------------
            // WeakEnemy1/2という「専用クラス」を作るほどでもない、
            // 本当にシンプルな雑魚なので、
            // Enemyクラスそのものを、そのまま使っている。
            //
            // 自分の攻撃力20で「2発で死ぬ」ようにしたいので、
            // maxHp を 21〜40 くらいに設定する（40なら2発で確実に倒せる）。
            Enemy minion1 = new Enemy("召喚された雑魚", X - 50, Y, maxHp: 40, attackPower: 10);
            Enemy minion2 = new Enemy("召喚された雑魚", X + 50, Y, maxHp: 40, attackPower: 10);

            SummonedMinions.Add(minion1);
            SummonedMinions.Add(minion2);
        }
    }
}