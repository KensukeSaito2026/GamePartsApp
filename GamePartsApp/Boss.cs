using System;
using System.Collections.Generic;

namespace GamePartsApp
{
    // ====================================================================
    // Boss：ボスキャラクター（雑魚召喚、円形攻撃、レーザー、隙あり）
    // ====================================================================
    /// <summary>
    ///  ボス戦で使う敵クラス。攻撃パターンの状態管理（予告・発動・隙）や
    ///  召喚処理など、ボス固有の振る舞いを実装します。
    /// </summary>
    public class Boss : Enemy
    {
        // ------------------------------------------------------------
        // ⚠️つまづきポイント①：このenumが、ボスの「今の状態」の全て
        // ------------------------------------------------------------
        // ボスは、常にこの8つの状態のうち、必ずどれか1つにいる。
        // 「○○Warning → ○○」というペアが3組あるのが、パターン。
        //
        // Idle（待機）
        //  → CircularWarning（円形攻撃の予告）→ Circular（発動）
        //  → LaserWarning（レーザーの予告）→ Laser（発動）
        //  → SummonWarning（召喚の予告）→ Summoning（発動）
        // どの攻撃も、発動が終わると Exposed（隙）を経て、また Idle に戻る。
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

        // ------------------------------------------------------------
        // ⚠️つまづきポイント②：stateFramesLeft は「今の状態に、
        // あと何フレーム留まるか」を表す、たった1つの汎用カウンター
        // ------------------------------------------------------------
        // WeakEnemy1/2では「攻撃ごとに別々のフレームカウンター」を
        // 持っていたが、Bossでは状態が8種類もあるため、
        // 「今の状態用に1個だけ」を使い回す設計になっている。
        // 状態が切り替わるたびに、この変数に新しい時間を入れ直す。
        private int stateFramesLeft = 0;

        private const int WARNING_DURATION_BASE = 45;
        private const int CIRCULAR_DURATION_BASE = 20;
        private const int LASER_DURATION_BASE = 15;
        private const int SUMMON_DURATION_BASE = 30;
        private const int EXPOSED_DURATION = 60;
        private const int IDLE_DURATION_BASE = 90;

        private Random rand = new Random();

        public List<Enemy> SummonedMinions { get; private set; } = new List<Enemy>();

        public float CircularRadius { get; private set; } = 120f;

        // 円形・レーザー、それぞれ「1回の発動で1回だけ」ダメージ
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
        // HP100%のとき：0.5 + (1.0 * 0.5) = 1.0倍（通常）
        // HP50%のとき ：0.5 + (0.5 * 0.5) = 0.75倍（少し速い）
        // HP0%のとき  ：0.5 + (0.0 * 0.5) = 0.5倍（一番速い＝時間が半分）
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
        // ⚠️つまづきポイント③：UpdateFrameは「時間切れになった瞬間」
        // にしか、状態を進めない
        // ------------------------------------------------------------
        // stateFramesLeft-- した後、まだ0より大きければ（＝時間が
        // 残っていれば）、return してそれ以上は何もしない。
        //
        // つまり、このswitch文が実際に動くのは、
        // 「ちょうど今のフレームで、時間切れになった瞬間」だけ。
        // 「Circular状態が、あと20フレーム続く」という間は、
        // このswitch文は素通りされ続ける（描画・判定は別の場所で行う）。
        public override void UpdateFrame(float playerX, float playerY)
        {
            stateFramesLeft--;

            if (stateFramesLeft > 0)
            {
                return;
            }

            float speedMult = GetSpeedMultiplier();

            // ------------------------------------------------------------
            // ⚠️つまづきポイント④：この switch は「今の状態」から
            // 「次に、どの状態に進むか」を決めている
            // ------------------------------------------------------------
            // 各case を、日本語で読み替えると分かりやすい：
            //
            //  Idle          → 「待機が終わった。次の攻撃を選ぼう」
            //  CircularWarning → 「予告が終わった。実際に円形攻撃を発動する」
            //  LaserWarning    → 「予告が終わった。実際にレーザーを発動する」
            //  SummonWarning   → 「予告が終わった。実際に雑魚を召喚する」
            //  Circular/Laser  → 「発動が終わった。隙にするか、続けて攻撃するか」
            //  Summoning       → 「召喚モーションが終わった。待機に戻る」
            //  Exposed         → 「隙の時間が終わった。待機に戻る」
            switch (CurrentState)
            {
                case BossState.Idle:
                    ChooseNextAction();
                    break;

                case BossState.CircularWarning:
                    CurrentState = BossState.Circular;
                    stateFramesLeft = (int)(CIRCULAR_DURATION_BASE * speedMult);
                    // 新しい発動が始まるので、「まだこの発動ではダメージを
                    // 与えていない」状態にリセットする。
                    HasDealtCircularDamage = false;
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

                // ------------------------------------------------------------
                // ⚠️つまづきポイント⑤：case文を連結している（breakなしで並べる）
                // ------------------------------------------------------------
                // case BossState.Circular:
                // case BossState.Laser:
                // と、breakを挟まずに2つ並べているのは、
                // 「CircularでもLaserでも、発動が終わったときの処理は、
                //   全く同じでいい」という意味。
                // C#では、このように複数のcaseに同じ処理をまとめられる。
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
        // ⚠️つまづきポイント⑥：ChooseNextActionは「予告」しか始めない
        // ------------------------------------------------------------
        // このメソッドがやっているのは、あくまで
        // 「CircularWarning / LaserWarning / SummonWarning の、
        //   どれか1つに切り替えて、予告時間をセットする」だけ。
        //
        // 実際の攻撃（Circular本体など）は、
        // ここでは発生しない。上のUpdateFrameのswitch文で、
        // 「予告が時間切れになったとき」に、初めて発動へ進む。
        // この「予告→発動」の2段階が、Bossの全攻撃で徹底されている。
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

        // ------------------------------------------------------------
        // ⚠️つまづきポイント⑦：召喚された雑魚は「Enemy型」そのもの
        // ------------------------------------------------------------
        // WeakEnemy1やWeakEnemy2ではなく、あえて一番シンプルな
        // Enemyクラスを直接newしている。
        // 攻撃パターンを持たない、「HPと接触ダメージだけの、
        // シンプルな取り巻き」として設計されているため。
        //
        // RemoveAll(minion => !minion.IsAlive) は、
        // 「もう死んでいる（IsAliveがfalse）雑魚を、
        //   リストから一括で取り除く」処理。
        // 新しく2体を召喚する前に、古い死骸を掃除している。
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