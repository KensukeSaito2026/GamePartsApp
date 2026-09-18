using System;

namespace GamePartsApp
{
    // ====================================================================
    // WeakEnemy1：近距離の雑魚（円形攻撃、たまに回避してくる）
    // ====================================================================
    // : Enemy と書くことで、「WeakEnemy1 is a Enemy」という
    // 継承の関係を表している。
    //
    // Enemyクラスが持っていた X, Y, HP, TakeDamage などは、
    // 自動的にすべて、このWeakEnemy1でも使える状態になっている。
    public class WeakEnemy1 : Enemy
    {
        private Random rand = new Random();

        // ------------------------------------------------------------
        // 回避のクールダウン管理
        // ------------------------------------------------------------
        private int dodgeCooldownFrames = 0;
        private const int DODGE_COOLDOWN = 90;   // 約1.5秒（30FPS想定）
        private const int DODGE_CHANCE_PERCENT = 25;  // 回避成功率25%（低め）

        // ------------------------------------------------------------
        // コンストラクタ
        // ------------------------------------------------------------
        // base(...) で、親クラス（Enemy）のコンストラクタを呼び出す。
        // 「名前、初期位置、HP100、攻撃力20」で初期化している。
        public WeakEnemy1(float startX, float startY)
            : base("雑魚(近距離)", startX, startY, maxHp: 100, attackPower: 20)
        {
        }

        // ------------------------------------------------------------
        // プレイヤーの攻撃が「当たりそう」なとき、外部（Form1.cs）から呼ばれる
        // ------------------------------------------------------------
        public void TryDodge(float attackerX, float attackerY, float attackRadius)
        {
            // クールダウン中は、何もしない（早期リターン）
            if (dodgeCooldownFrames > 0) return;

            // 攻撃の範囲外なら、そもそも回避不要
            float distance = GetDistance(X, Y, attackerX, attackerY);
            if (distance >= attackRadius) return;

            // ------------------------------------------------------------
            // 25%の確率で、回避を試みる
            // ------------------------------------------------------------
            if (rand.Next(0, 100) < DODGE_CHANCE_PERCENT)
            {
                // ------------------------------------------------------------
                // 攻撃者から離れる方向を計算する（正規化）
                // ------------------------------------------------------------
                float dx = X - attackerX;
                float dy = Y - attackerY;
                float length = GetDistance(0, 0, dx, dy);

                if (length > 0)
                {
                    // 方向だけを取り出し、30ピクセル分だけ移動する
                    X += (dx / length) * 30f;
                    Y += (dy / length) * 30f;
                }

                dodgeCooldownFrames = DODGE_COOLDOWN;
            }
        }

        // ------------------------------------------------------------
        // 毎フレームの更新（Enemyの virtual メソッドを override する）
        // ------------------------------------------------------------
        // override：親クラス（Enemy）の UpdateFrame を、
        // 「このクラス専用の中身」で、上書きしている。
        //
        // これが「ポリモーフィズム」の実践：
        // 外側（Form1.cs）からは、
        // どのEnemyでも同じ enemy.UpdateFrame(...) という呼び方で、
        // それぞれの敵に合った処理が実行される。
        public override void UpdateFrame(float playerX, float playerY)
        {
            // クールダウンのカウントダウン
            if (dodgeCooldownFrames > 0)
            {
                dodgeCooldownFrames--;
            }
        }
    }


    // ====================================================================
    // WeakEnemy2：遠距離の雑魚（予告→発動の攻撃）
    // ====================================================================
    public class WeakEnemy2 : Enemy
    {
        // ------------------------------------------------------------
        // 攻撃の「予告→発動」に関する状態
        // ------------------------------------------------------------
        // IsWarning：黄色く警告表示している段階
        // IsAttacking：赤く変わって、実際に当たり判定がある段階
        public bool IsWarning { get; private set; } = false;
        public bool IsAttackActive { get; private set; } = false;

        private int warningFramesLeft = 0;
        private int attackFramesLeft = 0;
        private int cooldownFramesLeft = 0;

        private const int WARNING_DURATION = 45;   // 約0.75秒、警告表示の長さ
        private const int ATTACK_DURATION = 15;    // 約0.25秒、当たり判定が出る長さ
        private const int ATTACK_COOLDOWN = 120;   // 約2秒、次の攻撃までの間隔

        // 攻撃が発生する場所（プレイヤーの位置を狙う想定）
        public float AttackTargetX { get; private set; }
        public float AttackTargetY { get; private set; }

        public WeakEnemy2(float startX, float startY)
            : base("雑魚(遠距離)", startX, startY, maxHp: 100, attackPower: 20)
        {
        }

        // ------------------------------------------------------------
        // 毎フレームの更新：予告→発動のサイクルを管理する
        // ------------------------------------------------------------
        public override void UpdateFrame(float playerX, float playerY)
        {
            // ------------------------------------------------------------
            // 攻撃中（赤、当たり判定あり）の処理
            // ------------------------------------------------------------
            if (IsAttackActive)
            {
                attackFramesLeft--;
                if (attackFramesLeft <= 0)
                {
                    IsAttackActive = false;
                    cooldownFramesLeft = ATTACK_COOLDOWN;
                }
                return;  // 攻撃中は、他の判定をしない
            }

            // ------------------------------------------------------------
            // 警告中（黄色、まだ当たらない）の処理
            // ------------------------------------------------------------
            if (IsWarning)
            {
                warningFramesLeft--;
                if (warningFramesLeft <= 0)
                {
                    // 警告が終わったら、実際の攻撃に切り替える
                    IsWarning = false;
                    IsAttackActive = true;
                    attackFramesLeft = ATTACK_DURATION;
                }
                return;
            }

            // ------------------------------------------------------------
            // クールダウン中
            // ------------------------------------------------------------
            if (cooldownFramesLeft > 0)
            {
                cooldownFramesLeft--;
                return;
            }

            // ------------------------------------------------------------
            // クールダウンが終わったら、次の攻撃の「予告」を開始する
            // ------------------------------------------------------------
            AttackTargetX = playerX;  // プレイヤーの、今の位置を狙う
            AttackTargetY = playerY;
            IsWarning = true;
            warningFramesLeft = WARNING_DURATION;
        }
    }
}