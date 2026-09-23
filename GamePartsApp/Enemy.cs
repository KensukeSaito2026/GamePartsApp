using System;

namespace GamePartsApp
{
    // ====================================================================
    // Enemyクラス：すべての敵の「共通の親」となる基底クラス
    // ====================================================================
    public class Enemy
    {
        public float X { get; set; }
        public float Y { get; set; }

        public int HP { get; protected set; }
        public int MaxHP { get; protected set; }
        public int AttackPower { get; protected set; }
        public float Radius { get; set; } = 20f;

        // ------------------------------------------------------------
        // ★変更：移動速度アップ（1.5f → 2.2f）
        // ------------------------------------------------------------
        public float MoveSpeed { get; protected set; } = 2.2f;

        public string Name { get; protected set; } = "敵";

        public Enemy(string name, float startX, float startY, int maxHp, int attackPower)
        {
            Name = name;
            X = startX;
            Y = startY;
            MaxHP = maxHp;
            HP = maxHp;
            AttackPower = attackPower;
        }

        public virtual void TakeDamage(int damage)
        {
            HP = Math.Max(HP - damage, 0);
        }

        public bool IsAlive => HP > 0;

        protected float GetDistance(float x1, float y1, float x2, float y2)
        {
            float dx = x1 - x2;
            float dy = y1 - y2;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public virtual void UpdateFrame(float playerX, float playerY)
        {
            // 基底クラスでは、特に何もしない（子クラスで実装する）
        }
    }
}