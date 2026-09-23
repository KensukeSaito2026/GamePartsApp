using System;

namespace GamePartsApp
{
    // ====================================================================
    // Playerクラス：アクションゲームの、プレイヤーキャラクター
    // ====================================================================
    public class Player
    {
        // ------------------------------------------------------------
        // 位置に関するプロパティ
        // ------------------------------------------------------------
        public float X { get; set; }
        public float Y { get; set; }

        // ------------------------------------------------------------
        // ステータス
        // ------------------------------------------------------------
        public int HP { get; private set; } = 100;
        public int AttackPower { get; set; } = 20;
        public float Radius { get; set; } = 15f;  // 当たり判定の半径

        // ------------------------------------------------------------
        // 回避に関する状態
        // ------------------------------------------------------------
        public bool IsInvincible { get; private set; } = false;
        private int invincibleFramesLeft = 0;

        private const int DODGE_DURATION = 15;   // 何フレーム、無敵になるか
        private const int DODGE_COOLDOWN = 60;   // 何フレーム、再使用まで待つか
        private int cooldownFramesLeft = 0;
        // Player.cs に追加
        public enum AttackType { None, Melee, Ranged }
        public AttackType CurrentAttack { get; private set; } = AttackType.None;
        public bool IsAttackWarning { get; private set; } = false;
        public bool IsAttackActive { get; private set; } = false;

        private int attackWarningFramesLeft = 0;
        private int attackActiveFramesLeft = 0;

        private const int MELEE_WARNING_DURATION = 20;   // 近接：約0.3秒の予告
        private const int MELEE_ACTIVE_DURATION = 8;      // 近接：短い判定時間
        private const int RANGED_WARNING_DURATION = 30;   // 遠距離：やや長めの予告（ため）
        private const int RANGED_ACTIVE_DURATION = 10;
        public bool HasDealtDamageThisAttack { get; private set; } = false;

        // ------------------------------------------------------------
        // 近接攻撃を開始する
        // ------------------------------------------------------------
        public void StartMeleeAttack()
        {
            if (IsAttackWarning || IsAttackActive) return;  // 攻撃中は、二重に始めない

            CurrentAttack = AttackType.Melee;
            IsAttackWarning = true;
            attackWarningFramesLeft = MELEE_WARNING_DURATION;
            HasDealtDamageThisAttack = false;  // ★追加
        }

        // ------------------------------------------------------------
        // 遠距離攻撃を開始する
        // ------------------------------------------------------------
        public void StartRangedAttack()
        {
            if (IsAttackWarning || IsAttackActive) return;

            CurrentAttack = AttackType.Ranged;
            IsAttackWarning = true;
            attackWarningFramesLeft = RANGED_WARNING_DURATION;
            HasDealtDamageThisAttack = false;  // ★追加：新しい攻撃なので、リセット
        }

        // ------------------------------------------------------------
        // コンストラクタ
        // ------------------------------------------------------------
        public Player(float startX, float startY)
        {
            X = startX;
            Y = startY;
        }

        // ------------------------------------------------------------
        // 移動：方向を受け取り、座標を動かす
        // ------------------------------------------------------------
        public void Move(float dx, float dy, float speed)
        {

            // ------------------------------------------------------------
            // ★追加：スタン中は、移動を受け付けない
            // ------------------------------------------------------------
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
        // Player.cs に追加
        public float DodgeRotation { get; private set; } = 0f;
        public void Dodge()
        {
            // クールダウン中は、回避できないようにする
            if (cooldownFramesLeft > 0) return;

            IsInvincible = true;
            invincibleFramesLeft = DODGE_DURATION;
            cooldownFramesLeft = DODGE_COOLDOWN;
            DodgeRotation = 0f;  // 回転をリセット
        }
        public bool IsStunned { get; private set; } = false;
        private int stunFramesLeft = 0;
        private const int STUN_DURATION = 10;  // 約0.5秒、動けなくなる

        // ------------------------------------------------------------
        // スタンさせる（外部から呼ばれる）
        // ------------------------------------------------------------
        public void ApplyStun()
        {
            IsStunned = true;
            stunFramesLeft = STUN_DURATION;
        }

        // ------------------------------------------------------------
        // 毎フレーム呼ぶ、状態の更新処理
        // ------------------------------------------------------------
        public void UpdateFrame()
        {
            // 無敵時間のカウントダウン
            if (invincibleFramesLeft > 0)
            {
                invincibleFramesLeft--;
                // ------------------------------------------------------------
                // ★追加：無敵中は、回転角度を進める
                // ------------------------------------------------------------
                // DODGE_DURATION（15フレーム）で、ちょうど360度回るように計算
                DodgeRotation += 360f / DODGE_DURATION;
                if (invincibleFramesLeft == 0)
                {
                    IsInvincible = false;
                }
            }

            // クールダウンのカウントダウン
            if (cooldownFramesLeft > 0)
            {
                cooldownFramesLeft--;
            }
            // ------------------------------------------------------------
            // ★追加：スタンのカウントダウン
            // ------------------------------------------------------------
            if (stunFramesLeft > 0)
            {
                stunFramesLeft--;
                if (stunFramesLeft == 0)
                {
                    IsStunned = false;
                }
           
            }
            if (IsAttackWarning)
            {
                attackWarningFramesLeft--;
                if (attackWarningFramesLeft <= 0)
                {
                    IsAttackWarning = false;
                    IsAttackActive = true;
                    attackActiveFramesLeft = CurrentAttack == AttackType.Melee ? MELEE_ACTIVE_DURATION : RANGED_ACTIVE_DURATION;
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
            if (IsInvincible) return;  // 無敵中なら、ダメージを受けない

            HP = Math.Max(HP - damage, 0);
        }


        public bool IsAlive => HP > 0;

    }
}