using System;

namespace GamePartsApp
{
    // ====================================================================
    // Playerクラス：アクションゲームの、プレイヤーキャラクター
    // ====================================================================
    /// <summary>
    ///  プレイヤーの位置・HP・攻撃・回避など、操作に関する状態と振る舞いを管理します。
    /// </summary>
    public class Player
    {
        public float X { get; set; }
        public float Y { get; set; }

        public int HP { get; private set; } = 100;
        public int AttackPower { get; set; } = 20;
        public float Radius { get; set; } = 15f;

        // ------------------------------------------------------------
        // 回避に関する状態
        // ------------------------------------------------------------
        public bool IsInvincible { get; private set; } = false;
        private int invincibleFramesLeft = 0;

        private const int DODGE_DURATION = 15;
        private const int DODGE_COOLDOWN = 60;
        private int cooldownFramesLeft = 0;

        // ------------------------------------------------------------
        // 攻撃（近接・遠距離）の状態
        // ------------------------------------------------------------
        public enum AttackType { None, Melee, Ranged }
        public AttackType CurrentAttack { get; private set; } = AttackType.None;
        public bool IsAttackWarning { get; private set; } = false;
        public bool IsAttackActive { get; private set; } = false;

        private int attackWarningFramesLeft = 0;
        private int attackActiveFramesLeft = 0;

        private const int MELEE_WARNING_DURATION = 20;
        private const int MELEE_ACTIVE_DURATION = 8;
        private const int RANGED_WARNING_DURATION = 30;
        private const int RANGED_ACTIVE_DURATION = 10;
        public bool HasDealtDamageThisAttack { get; private set; } = false;

        // ------------------------------------------------------------
        // ⚠️つまづきポイント①：「攻撃中は、二重に始めない」ガード
        // ------------------------------------------------------------
        // すでに予告中(IsAttackWarning)か発動中(IsAttackActive)なら、
        // 何もせず return する。
        //
        // これがないと、例えば「予告中にもう一度右クリック」したとき、
        // attackWarningFramesLeft が上書きされてしまい、
        // カウントダウンが変な挙動になるバグの原因になる。
        public void StartMeleeAttack()
        {
            if (IsAttackWarning || IsAttackActive) return;

            CurrentAttack = AttackType.Melee;
            IsAttackWarning = true;
            attackWarningFramesLeft = MELEE_WARNING_DURATION;
            HasDealtDamageThisAttack = false;
        }

        public void StartRangedAttack()
        {
            if (IsAttackWarning || IsAttackActive) return;

            CurrentAttack = AttackType.Ranged;
            IsAttackWarning = true;
            attackWarningFramesLeft = RANGED_WARNING_DURATION;
            HasDealtDamageThisAttack = false;
        }

        public Player(float startX, float startY)
        {
            X = startX;
            Y = startY;
        }

        // ------------------------------------------------------------
        // ⚠️つまづきポイント②：Move の中の早期リターン
        // ------------------------------------------------------------
        // if (IsStunned) return; がここにあることで、
        // 「スタン中は、いくらWASDを押しても、実際には動かない」
        // という仕様が実現されている。
        //
        // 呼び出す側（Form1.cs）は、IsStunnedを意識せず
        // 常に player.Move(dx, dy, speed) を呼んでいるだけでよい。
        // 「動けるかどうかの判断」は、Playerクラス自身の責任にしている。
        public void Move(float dx, float dy, float speed)
        {
            if (IsStunned) return;
            X += dx * speed;
            Y += dy * speed;
        }

        public void MarkDamageDealt()
        {
            HasDealtDamageThisAttack = true;
        }

        // ------------------------------------------------------------
        // 回避：Eキーが押されたときに呼ぶ
        // ------------------------------------------------------------
        public float DodgeRotation { get; private set; } = 0f;

        public void Dodge()
        {
            // クールダウン中は、回避できないようにする
            if (cooldownFramesLeft > 0) return;

            IsInvincible = true;
            invincibleFramesLeft = DODGE_DURATION;
            cooldownFramesLeft = DODGE_COOLDOWN;
            DodgeRotation = 0f;
        }

        public bool IsStunned { get; private set; } = false;
        private int stunFramesLeft = 0;
        private const int STUN_DURATION = 10;

        public void ApplyStun()
        {
            IsStunned = true;
            stunFramesLeft = STUN_DURATION;
        }

        // ------------------------------------------------------------
        // ⚠️つまづきポイント③：UpdateFrameの中身、4つの独立したブロック
        // ------------------------------------------------------------
        // このメソッドの中には、
        //   ①無敵（回避）のカウントダウン
        //   ②クールダウンのカウントダウン
        //   ③スタンのカウントダウン
        //   ④攻撃（予告→発動）のカウントダウン
        // という、4つの"別々の仕組み"が、並んで書かれている。
        //
        // これらは互いに独立しており、
        // 「①が終わっていないと④が動かない」というような
        // 依存関係はない。毎フレーム、4つとも同時にチェックされる。
        //
        // 読むときのコツ：
        // 1つのif文のカタマリごとに「これは何のカウントダウンか」
        // ラベルをつけながら読むと、混乱しにくい。
        public void UpdateFrame()
        {
            // ①無敵時間のカウントダウン（＋回転角度の計算）
            if (invincibleFramesLeft > 0)
            {
                invincibleFramesLeft--;

                // DODGE_DURATION（15フレーム）で、ちょうど360度回るように
                // 1フレームあたり (360 / 15) 度ずつ、回転を進めている。
                DodgeRotation += 360f / DODGE_DURATION;

                if (invincibleFramesLeft == 0)
                {
                    IsInvincible = false;
                }
            }

            // ②クールダウンのカウントダウン（回避の再使用までの待ち時間）
            if (cooldownFramesLeft > 0)
            {
                cooldownFramesLeft--;
            }

            // ③スタンのカウントダウン
            if (stunFramesLeft > 0)
            {
                stunFramesLeft--;
                if (stunFramesLeft == 0)
                {
                    IsStunned = false;
                }
            }

            // ------------------------------------------------------------
            // ④攻撃（予告→発動）のカウントダウン
            // ------------------------------------------------------------
            // ⚠️ここが if / else if の関係になっている点に注意。
            // IsAttackWarning が true の間は、IsAttackActive側は見られない。
            // 「予告→発動」は、同時には起こらない、一直線の流れだから。
            if (IsAttackWarning)
            {
                attackWarningFramesLeft--;
                if (attackWarningFramesLeft <= 0)
                {
                    IsAttackWarning = false;
                    IsAttackActive = true;
                    // 三項演算子：近接ならMELEE、遠距離ならRANGEDの
                    // ACTIVE_DURATIONを選ぶ
                    attackActiveFramesLeft = CurrentAttack == AttackType.Melee
                        ? MELEE_ACTIVE_DURATION
                        : RANGED_ACTIVE_DURATION;
                }
            }
            else if (IsAttackActive)
            {
                attackActiveFramesLeft--;
                if (attackActiveFramesLeft <= 0)
                {
                    IsAttackActive = false;
                    CurrentAttack = AttackType.None;
                }
            }
        }

        // ------------------------------------------------------------
        // ダメージを受ける（無敵中は無効）
        // ------------------------------------------------------------
        public void TakeDamage(int damage)
        {
            if (IsInvincible) return;

            HP = Math.Max(HP - damage, 0);
        }

        public bool IsAlive => HP > 0;
    }
}