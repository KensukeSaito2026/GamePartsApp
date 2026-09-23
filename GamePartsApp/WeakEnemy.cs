using System;
using System.Collections.Generic;
using System.Drawing;

namespace GamePartsApp
{
    // ====================================================================
    // WeakEnemy1：近距離の雑魚（円形攻撃、格子攻撃、水玉弾幕、回避AI）
    // ====================================================================
    public class WeakEnemy1 : Enemy
    {
        private Random rand = new Random();

        private int dodgeCooldownFrames = 0;
        private const int DODGE_COOLDOWN = 90;
        private const int DODGE_CHANCE_PERCENT = 25;

        public bool IsWarning { get; private set; } = false;
        public bool IsAttackActive { get; private set; } = false;
        public bool HasDealtDamageThisAttack { get; private set; } = false;

        private int warningFramesLeft = 0;
        private int attackFramesLeft = 0;
        private int cooldownFramesLeft = 0;

        private const int WARNING_DURATION = 28;
        private const int ATTACK_DURATION = 15;
        private const int ATTACK_COOLDOWN = 90;
        public float AttackRadius { get; private set; } = 80f;

        public bool IsGridWarning { get; private set; } = false;
        public bool IsGridActive { get; private set; } = false;

        public const int GRID_COLS = 5;
        public const int GRID_ROWS = 4;
        public bool[,] DangerCells { get; private set; }

        private int gridWarningFramesLeft = 0;
        private int gridActiveFramesLeft = 0;
        private int gridCooldownFramesLeft = 0;

        private const int GRID_WARNING_DURATION = 40;
        private const int GRID_ACTIVE_DURATION = 20;
        private const int GRID_COOLDOWN = 150;

        private Random gridRand = new Random();
        public bool HasDealtGridDamage { get; private set; } = false;

        // ------------------------------------------------------------
        // 水玉弾幕（画面全体に、避難できる隙間を残して埋め尽くす）
        // ------------------------------------------------------------
        public bool IsBubbleWarning { get; private set; } = false;
        public bool IsBubbleActive { get; private set; } = false;
        public bool HasDealtBubbleDamage { get; private set; } = false;

        public List<PointF> DangerBubbles { get; private set; } = new List<PointF>();
        public float BubbleRadius { get; private set; } = 45f;

        private int bubbleWarningFramesLeft = 0;
        private int bubbleActiveFramesLeft = 0;
        private int bubbleCooldownFramesLeft = 0;

        private const int BUBBLE_WARNING_DURATION = 50;
        private const int BUBBLE_ACTIVE_DURATION = 25;
        private const int BUBBLE_COOLDOWN = 180;

        public int ScreenWidth { get; set; } = 700;
        public int ScreenHeight { get; set; } = 400;

        // ------------------------------------------------------------
        // コンストラクタに、省略可能な引数（maxHp, attackPower）を追加
        // ------------------------------------------------------------
        // これにより、通常の雑魚としても、
        // ボスの「強化された分身」としても、同じクラスを使い回せる。
        //
        // 通常呼び出し：new WeakEnemy1(500, 200);
        //   → maxHpは省略されたので、デフォルトの100が使われる
        //
        // ボス用の呼び出し：new WeakEnemy1(300, 200, maxHp: 150, attackPower: 25);
        //   → 明示的に指定した150, 25が使われる
        public WeakEnemy1(float startX, float startY, int maxHp = 100, int attackPower = 20)
            : base("雑魚(近距離)", startX, startY, maxHp, attackPower)
        {
            gridCooldownFramesLeft = 60;
            bubbleCooldownFramesLeft = 200;
        }

        public void TryDodge(float attackerX, float attackerY, float attackRadius)
        {
            if (dodgeCooldownFrames > 0) return;

            float distance = GetDistance(X, Y, attackerX, attackerY);
            if (distance >= attackRadius) return;

            if (rand.Next(0, 100) < DODGE_CHANCE_PERCENT)
            {
                float dx = X - attackerX;
                float dy = Y - attackerY;
                float length = GetDistance(0, 0, dx, dy);

                if (length > 0)
                {
                    X += (dx / length) * 30f;
                    Y += (dy / length) * 30f;
                }

                dodgeCooldownFrames = DODGE_COOLDOWN;
            }
        }

        private void StartGridAttack()
        {
            DangerCells = new bool[GRID_ROWS, GRID_COLS];

            for (int row = 0; row < GRID_ROWS; row++)
            {
                for (int col = 0; col < GRID_COLS; col++)
                {
                    DangerCells[row, col] = gridRand.Next(0, 100) < 60;
                }
            }

            IsGridWarning = true;
            gridWarningFramesLeft = GRID_WARNING_DURATION;
        }

        // ------------------------------------------------------------
        // 水玉弾幕を開始する
        // ------------------------------------------------------------
        private void StartBubbleAttack()
        {
            DangerBubbles.Clear();

            // 12個の円を、ランダムな位置に配置する。
            // 全部を埋め尽くすと避難不可能になるので、
            // 「隙間が必ず残る」程度の個数に調整している。
            int bubbleCount = 12;
            for (int i = 0; i < bubbleCount; i++)
            {
                float bx = rand.Next(0, ScreenWidth);
                float by = rand.Next(0, ScreenHeight);
                DangerBubbles.Add(new PointF(bx, by));
            }

            IsBubbleWarning = true;
            bubbleWarningFramesLeft = BUBBLE_WARNING_DURATION;
        }

        public override void UpdateFrame(float playerX, float playerY)
        {
            if (dodgeCooldownFrames > 0)
            {
                dodgeCooldownFrames--;
            }

            if (!IsWarning && !IsAttackActive)
            {
                float dx = playerX - X;
                float dy = playerY - Y;
                float length = GetDistance(0, 0, dx, dy);

                if (length > 100f)
                {
                    X += (dx / length) * MoveSpeed;
                    Y += (dy / length) * MoveSpeed;
                }
            }

            if (IsGridActive)
            {
                gridActiveFramesLeft--;
                if (gridActiveFramesLeft <= 0)
                {
                    IsGridActive = false;
                    gridCooldownFramesLeft = GRID_COOLDOWN;
                }
            }
            else if (IsGridWarning)
            {
                gridWarningFramesLeft--;
                if (gridWarningFramesLeft <= 0)
                {
                    IsGridWarning = false;
                    IsGridActive = true;
                    gridActiveFramesLeft = GRID_ACTIVE_DURATION;
                    HasDealtGridDamage = false;
                }
            }
            else if (gridCooldownFramesLeft > 0)
            {
                gridCooldownFramesLeft--;
                if (gridCooldownFramesLeft <= 0)
                {
                    StartGridAttack();
                }
            }

            if (IsBubbleActive)
            {
                bubbleActiveFramesLeft--;
                if (bubbleActiveFramesLeft <= 0)
                {
                    IsBubbleActive = false;
                    bubbleCooldownFramesLeft = BUBBLE_COOLDOWN;
                }
            }
            else if (IsBubbleWarning)
            {
                bubbleWarningFramesLeft--;
                if (bubbleWarningFramesLeft <= 0)
                {
                    IsBubbleWarning = false;
                    IsBubbleActive = true;
                    bubbleActiveFramesLeft = BUBBLE_ACTIVE_DURATION;
                    HasDealtBubbleDamage = false;
                }
            }
            else if (bubbleCooldownFramesLeft > 0)
            {
                bubbleCooldownFramesLeft--;
                if (bubbleCooldownFramesLeft <= 0)
                {
                    StartBubbleAttack();
                }
            }

            if (IsAttackActive)
            {
                attackFramesLeft--;
                if (attackFramesLeft <= 0)
                {
                    IsAttackActive = false;
                    cooldownFramesLeft = ATTACK_COOLDOWN;
                }
                return;
            }

            if (IsWarning)
            {
                warningFramesLeft--;
                if (warningFramesLeft <= 0)
                {
                    IsWarning = false;
                    IsAttackActive = true;
                    attackFramesLeft = ATTACK_DURATION;
                    HasDealtDamageThisAttack = false;
                }
                return;
            }

            if (cooldownFramesLeft > 0)
            {
                cooldownFramesLeft--;
                return;
            }

            IsWarning = true;
            warningFramesLeft = WARNING_DURATION;
        }

        public void MarkDamageDealt()
        {
            HasDealtDamageThisAttack = true;
        }

        public void MarkGridDamageDealt()
        {
            HasDealtGridDamage = true;
        }

        public void MarkBubbleDamageDealt()
        {
            HasDealtBubbleDamage = true;
        }
    }


    // ====================================================================
    // WeakEnemy2：遠距離の雑魚（連射攻撃、円形攻撃、水玉弾幕）
    // ====================================================================
    public class WeakEnemy2 : Enemy
    {
        public bool IsWarning { get; private set; } = false;
        public bool IsAttackActive { get; private set; } = false;
        public bool HasDealtDamageThisAttack { get; private set; } = false;

        private int warningFramesLeft = 0;
        private int attackFramesLeft = 0;
        private int cooldownFramesLeft = 0;

        private const int WARNING_DURATION = 30;
        private const int ATTACK_DURATION = 15;
        private const int ATTACK_COOLDOWN = 120;

        public float AttackTargetX { get; private set; }
        public float AttackTargetY { get; private set; }

        private int burstCount = 0;
        private const int BURST_MAX = 2;

        private float lastPlayerX, lastPlayerY;
        private bool hasLastPosition = false;

        // ------------------------------------------------------------
        // 円形（範囲）攻撃
        // ------------------------------------------------------------
        public bool IsCircularWarning { get; private set; } = false;
        public bool IsCircularActive { get; private set; } = false;
        public bool HasDealtCircularDamage { get; private set; } = false;

        private int circularWarningFramesLeft = 0;
        private int circularActiveFramesLeft = 0;
        private int circularCooldownFramesLeft = 0;

        private const int CIRCULAR_WARNING_DURATION = 30;
        private const int CIRCULAR_ACTIVE_DURATION = 12;
        private const int CIRCULAR_COOLDOWN = 130;
        public float CircularRadius { get; private set; } = 70f;

        // ------------------------------------------------------------
        // 水玉弾幕（WeakEnemy1と同じ仕組み）
        // ------------------------------------------------------------
        public bool IsBubbleWarning { get; private set; } = false;
        public bool IsBubbleActive { get; private set; } = false;
        public bool HasDealtBubbleDamage { get; private set; } = false;

        public List<PointF> DangerBubbles { get; private set; } = new List<PointF>();
        public float BubbleRadius { get; private set; } = 45f;

        private int bubbleWarningFramesLeft = 0;
        private int bubbleActiveFramesLeft = 0;
        private int bubbleCooldownFramesLeft = 0;

        private const int BUBBLE_WARNING_DURATION = 50;
        private const int BUBBLE_ACTIVE_DURATION = 25;
        private const int BUBBLE_COOLDOWN = 190;

        private Random rand = new Random();

        public int ScreenWidth { get; set; } = 700;
        public int ScreenHeight { get; set; } = 400;

        // ------------------------------------------------------------
        // コンストラクタに、省略可能な引数を追加（WeakEnemy1と同じ考え方）
        // ------------------------------------------------------------
        public WeakEnemy2(float startX, float startY, int maxHp = 100, int attackPower = 20)
            : base("雑魚(遠距離)", startX, startY, maxHp, attackPower)
        {
            circularCooldownFramesLeft = 80;
            bubbleCooldownFramesLeft = 220;
        }

        private void StartNextShot(float playerX, float playerY)
        {
            if (burstCount == 0 || !hasLastPosition)
            {
                AttackTargetX = playerX;
                AttackTargetY = playerY;
            }
            else
            {
                float moveDx = playerX - lastPlayerX;
                float moveDy = playerY - lastPlayerY;

                AttackTargetX = playerX + moveDx * 3f;
                AttackTargetY = playerY + moveDy * 3f;
            }

            lastPlayerX = playerX;
            lastPlayerY = playerY;
            hasLastPosition = true;

            IsWarning = true;
            warningFramesLeft = WARNING_DURATION;
        }

        // ------------------------------------------------------------
        // 円形攻撃を開始する
        // ------------------------------------------------------------
        private void StartCircularAttack()
        {
            IsCircularWarning = true;
            circularWarningFramesLeft = CIRCULAR_WARNING_DURATION;
        }

        // ------------------------------------------------------------
        // 水玉弾幕を開始する（WeakEnemy1と同じロジック）
        // ------------------------------------------------------------
        private void StartBubbleAttack()
        {
            DangerBubbles.Clear();

            int bubbleCount = 12;
            for (int i = 0; i < bubbleCount; i++)
            {
                float bx = rand.Next(0, ScreenWidth);
                float by = rand.Next(0, ScreenHeight);
                DangerBubbles.Add(new PointF(bx, by));
            }

            IsBubbleWarning = true;
            bubbleWarningFramesLeft = BUBBLE_WARNING_DURATION;
        }

        public override void UpdateFrame(float playerX, float playerY)
        {
            if (!IsWarning && !IsAttackActive)
            {
                float dx = playerX - X;
                float dy = playerY - Y;
                float length = GetDistance(0, 0, dx, dy);

                if (length > 150f)
                {
                    X += (dx / length) * MoveSpeed;
                    Y += (dy / length) * MoveSpeed;
                }
            }

            if (IsCircularActive)
            {
                circularActiveFramesLeft--;
                if (circularActiveFramesLeft <= 0)
                {
                    IsCircularActive = false;
                    circularCooldownFramesLeft = CIRCULAR_COOLDOWN;
                }
            }
            else if (IsCircularWarning)
            {
                circularWarningFramesLeft--;
                if (circularWarningFramesLeft <= 0)
                {
                    IsCircularWarning = false;
                    IsCircularActive = true;
                    circularActiveFramesLeft = CIRCULAR_ACTIVE_DURATION;
                    HasDealtCircularDamage = false;
                }
            }
            else if (circularCooldownFramesLeft > 0)
            {
                circularCooldownFramesLeft--;
                if (circularCooldownFramesLeft <= 0)
                {
                    StartCircularAttack();
                }
            }

            if (IsBubbleActive)
            {
                bubbleActiveFramesLeft--;
                if (bubbleActiveFramesLeft <= 0)
                {
                    IsBubbleActive = false;
                    bubbleCooldownFramesLeft = BUBBLE_COOLDOWN;
                }
            }
            else if (IsBubbleWarning)
            {
                bubbleWarningFramesLeft--;
                if (bubbleWarningFramesLeft <= 0)
                {
                    IsBubbleWarning = false;
                    IsBubbleActive = true;
                    bubbleActiveFramesLeft = BUBBLE_ACTIVE_DURATION;
                    HasDealtBubbleDamage = false;
                }
            }
            else if (bubbleCooldownFramesLeft > 0)
            {
                bubbleCooldownFramesLeft--;
                if (bubbleCooldownFramesLeft <= 0)
                {
                    StartBubbleAttack();
                }
            }

            if (IsAttackActive)
            {
                attackFramesLeft--;
                if (attackFramesLeft <= 0)
                {
                    IsAttackActive = false;

                    burstCount++;
                    if (burstCount < BURST_MAX)
                    {
                        StartNextShot(playerX, playerY);
                    }
                    else
                    {
                        cooldownFramesLeft = ATTACK_COOLDOWN;
                        burstCount = 0;
                    }
                }
                return;
            }

            if (IsWarning)
            {
                warningFramesLeft--;
                if (warningFramesLeft <= 0)
                {
                    IsWarning = false;
                    IsAttackActive = true;
                    attackFramesLeft = ATTACK_DURATION;
                    HasDealtDamageThisAttack = false;
                }
                return;
            }

            if (cooldownFramesLeft > 0)
            {
                cooldownFramesLeft--;
                return;
            }

            StartNextShot(playerX, playerY);
        }

        public void MarkDamageDealt()
        {
            HasDealtDamageThisAttack = true;
        }

        public void MarkCircularDamageDealt()
        {
            HasDealtCircularDamage = true;
        }

        public void MarkBubbleDamageDealt()
        {
            HasDealtBubbleDamage = true;
        }
    }
}