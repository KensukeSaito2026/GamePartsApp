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
            X += dx * speed;
            Y += dy * speed;
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