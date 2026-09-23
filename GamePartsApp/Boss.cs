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
        // ★修正：各攻撃の前に「予告（Warning）」を追加した
        // ------------------------------------------------------------
        // 以前は、いきなり Circular/Laser/Summoning になっていたため、
        // 黄色の予告が見えず、避ける猶予がないまま被弾する
        // 「一撃死」バグの原因になっていた。
        public enum BossState
        {
            Idle,
            CircularWarning,
            Circular,
            LaserWarning,
            Laser,
            SummonWarning,
            Summoning,
            Exposed
        }

        public BossState CurrentState { get; private set; } = BossState.Idle;

        private int stateFramesLeft = 0;

        // ------------------------------------------------------------
        // 各行動の、基本の持続時間（HP割合で調整される「基準値」）
        // ------------------------------------------------------------
        private const int WARNING_DURATION_BASE = 45;   // 予告の基本時間
        private const int CIRCULAR_DURATION_BASE = 20;
        private const int LASER_DURATION_BASE = 15;
        private const int SUMMON_DURATION_BASE = 30;
        private const int EXPOSED_DURATION = 60;
        private const int IDLE_DURATION_BASE = 90;

        private Random rand = new Random();

        public List<Enemy> SummonedMinions { get; private set; } = new List<Enemy>();

        public float CircularRadius { get; private set; } = 120f;

        // ------------------------------------------------------------
        // ★修正：円形・レーザー、それぞれ「1回の発動で1回だけ」ダメージ
        // ------------------------------------------------------------
        // これがないと、発動中の数フレームの間、
        // 毎フレームダメージが入り、一瞬でHPが0になってしまう
        // （「スリップダメージのようになる」バグの原因）。
        public bool HasDealtCircularDamage { get; private set; } = false;
        public bool HasDealtLaserDamage { get; private set; } = false;

        public Boss(float startX, float startY)
            : base("ボス", startX, startY, maxHp: 300, attackPower: 30)
        {
        }

        public bool IsExposed => CurrentState == BossState.Exposed;

        // ------------------------------------------------------------
        // HPが減るほど、行動が速くなる
        // ------------------------------------------------------------
        // HP100%のとき：基準値そのまま（倍率1.0）
        // HP0%のとき　：基準値の半分（倍率0.5、かなり速い）
        private float GetSpeedMultiplier()
        {
            float hpRatio = (float)HP / MaxHP;
            return 0.5f + (hpRatio * 0.5f);
        }

        public void MarkCircularDamageDealt()
        {
            HasDealtCircularDamage = true;
        }

        public void MarkLaserDamageDealt()
        {
            HasDealtLaserDamage = true;
        }

        // ------------------------------------------------------------
        // 毎フレームの更新
        // ------------------------------------------------------------
        public override void UpdateFrame(float playerX, float playerY)
        {
            stateFramesLeft--;

            if (stateFramesLeft > 0)
            {
                return;
            }

            float speedMult = GetSpeedMultiplier();

            switch (CurrentState)
            {
                case BossState.Idle:
                    ChooseNextAction();
                    break;

                // ------------------------------------------------------------
                // ★追加：予告が終わったら、実際の発動に切り替える
                // ------------------------------------------------------------
                case BossState.CircularWarning:
                    CurrentState = BossState.Circular;
                    stateFramesLeft = (int)(CIRCULAR_DURATION_BASE * speedMult);
                    HasDealtCircularDamage = false;  // 新しい発動なので、ダメージ制御をリセット
                    break;

                case BossState.LaserWarning:
                    CurrentState = BossState.Laser;
                    stateFramesLeft = (int)(LASER_DURATION_BASE * speedMult);
                    HasDealtLaserDamage = false;
                    break;

                case BossState.SummonWarning:
                    CurrentState = BossState.Summoning;
                    stateFramesLeft = (int)(SUMMON_DURATION_BASE * speedMult);
                    SummonMinions();
                    break;

                case BossState.Circular:
                case BossState.Laser:
                    // 30%の確率で、隙をスキップして連続攻撃する
                    if (rand.Next(0, 100) < 30)
                    {
                        ChooseNextAction();
                    }
                    else
                    {
                        CurrentState = BossState.Exposed;
                        stateFramesLeft = EXPOSED_DURATION;
                    }
                    break;

                case BossState.Summoning:
                    CurrentState = BossState.Idle;
                    stateFramesLeft = (int)(IDLE_DURATION_BASE * speedMult);
                    break;

                case BossState.Exposed:
                    CurrentState = BossState.Idle;
                    stateFramesLeft = (int)(IDLE_DURATION_BASE * speedMult);
                    break;
            }
        }

        // ------------------------------------------------------------
        // 次の行動を、ランダムに選ぶ（必ず「予告」から始める）
        // ------------------------------------------------------------
        private void ChooseNextAction()
        {
            int choice = rand.Next(0, 3);
            float speedMult = GetSpeedMultiplier();
            int warningDuration = (int)(WARNING_DURATION_BASE * speedMult);

            switch (choice)
            {
                case 0:
                    CurrentState = BossState.CircularWarning;
                    stateFramesLeft = warningDuration;
                    break;

                case 1:
                    CurrentState = BossState.LaserWarning;
                    stateFramesLeft = warningDuration;
                    break;

                case 2:
                    CurrentState = BossState.SummonWarning;
                    stateFramesLeft = warningDuration;
                    break;
            }
        }

        private void SummonMinions()
        {
            SummonedMinions.RemoveAll(minion => !minion.IsAlive);

            Enemy minion1 = new Enemy("召喚された雑魚", X - 50, Y, maxHp: 40, attackPower: 10);
            Enemy minion2 = new Enemy("召喚された雑魚", X + 50, Y, maxHp: 40, attackPower: 10);

            SummonedMinions.Add(minion1);
            SummonedMinions.Add(minion2);
        }
    }
}