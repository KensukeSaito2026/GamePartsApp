using System;
using System.Collections.Generic;
using System.Drawing;

namespace GamePartsApp
{
    // ====================================================================
    // WeakEnemy1：近距離の雑魚（円形攻撃、格子攻撃、水玉弾幕、回避AI）
    // ====================================================================
    /// <summary>
    ///  近距離系の雑魚。複数の攻撃サイクル（円形・格子・水玉）を並行して管理します。
    /// </summary>
    // ⚠️全体を読む前に、まず知っておくべきこと：
    // このクラスは「円形攻撃」「格子攻撃」「水玉弾幕」という
    // 3つの攻撃サイクルを、"同時並行で"動かしている。
    // Bossのように「1つずつ順番に」ではなく、
    // 3つとも、独立したタイマーで、バラバラのタイミングで発動する。
    // （実際にプレイすると、格子と水玉が同時に来ることもある）
    public class WeakEnemy1 : Enemy
    {
        private Random rand = new Random();

        private int dodgeCooldownFrames = 0;
        private const int DODGE_COOLDOWN = 90;
        private const int DODGE_CHANCE_PERCENT = 25;

        // ------------------------------------------------------------
        // サイクル①：円形攻撃
        // ------------------------------------------------------------
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

        // ------------------------------------------------------------
        // サイクル②：格子攻撃
        // ------------------------------------------------------------
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
        // サイクル③：水玉弾幕
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

        // ------------------------------------------------------------
        // ⚠️つまづきポイント①：ScreenWidth/Heightを、なぜ持っているか
        // ------------------------------------------------------------
        // 水玉を「画面のどこに配置するか」を計算するには、
        // 「画面の実際の幅・高さ」を知る必要がある。
        // でも、このEnemyクラス自身は、Form1.csの画面サイズを
        // 直接知る手段がない。
        // そこで、Form1.cs側から「これが画面サイズだよ」と
        // 教えてもらうための、書き込み可能なプロパティになっている。
        // （gameStartButton_Clickの中で、
        //   enemy1.ScreenWidth = gameTabPage.Width; のように設定している）
        public int ScreenWidth { get; set; } = 700;
        public int ScreenHeight { get; set; } = 400;

        // ------------------------------------------------------------
        // ⚠️つまづきポイント②：コンストラクタの「省略可能な引数」
        // ------------------------------------------------------------
        // maxHp = 100, attackPower = 20 という書き方により、
        //
        //   new WeakEnemy1(500, 200)
        //     → maxHp, attackPower を省略 → 自動的に100, 20になる
        //
        //   new WeakEnemy1(300, 200, maxHp: 150, attackPower: 25)
        //     → 明示的に指定した150, 25が使われる（ボス分身用）
        //
        // 同じクラスを、通常の雑魚にも、強化されたボス分身にも
        // 使い回せる、今日の設計の要になっている部分。
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

        // ------------------------------------------------------------
        // ⚠️つまづきポイント③：UpdateFrameの中は、
        // 「移動」＋「3つの独立サイクル」＋「円形攻撃のreturn付きサイクル」
        // という、4つのブロックが順番に並んでいる
        // ------------------------------------------------------------
        // 読むときのコツ：
        // 上から順番に「これは何を処理しているブロックか」を、
        // コメントで区切って追っていくとよい。
        // 格子攻撃・水玉弾幕の2ブロックには return がないので、
        // どちらも必ず最後まで実行される（並行して動く理由はここ）。
        // 円形攻撃のブロックだけ return があるので、
        // 「予告中/発動中」なら、そこでメソッドが終わる。
        public override void UpdateFrame(float playerX, float playerY)
        {
            // 【ブロックA】回避のクールダウン
            if (dodgeCooldownFrames > 0)
            {
                dodgeCooldownFrames--;
            }

            // 【ブロックB】プレイヤーへの接近移動
            // 円形攻撃の予告/発動中でなければ、動く。
            // （格子・水玉の予告/発動中でも、この移動は止まらない点に注意）
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

            // 【ブロックC】格子攻撃のサイクル（returnなし＝必ず最後まで実行）
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

            // 【ブロックD】水玉弾幕のサイクル（returnなし＝Cと並行して動く）
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

            // 【ブロックE】円形攻撃のサイクル（ここだけ return がある）
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
    // ⚠️WeakEnemy1と、ほぼ同じ「並行サイクル」の構造。
    // 違いは、メインの攻撃が「円形」ではなく「連射（burst）」である点。
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

        // ------------------------------------------------------------
        // ⚠️つまづきポイント④：burstCount と hasLastPosition の役割
        // ------------------------------------------------------------
        // burstCount：今、連射の何発目か（0＝1発目、1＝2発目）
        // hasLastPosition：「1つ前の、プレイヤーの位置」を、
        //   まだ記録していない（ゲーム開始直後などの）状態を区別するフラグ。
        //   これがfalseのうちは、"先読み"の計算ができないので、
        //   1発目と同じ「今の位置」を狙うようにしている。
        private int burstCount = 0;
        private const int BURST_MAX = 4;

        private float lastPlayerX, lastPlayerY;
        private bool hasLastPosition = false;

        // ------------------------------------------------------------
        // サイクル：円形（範囲）攻撃
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
        // サイクル：水玉弾幕（WeakEnemy1と全く同じ仕組み）
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

        public WeakEnemy2(float startX, float startY, int maxHp = 100, int attackPower = 20)
            : base("雑魚(遠距離)", startX, startY, maxHp, attackPower)
        {
            circularCooldownFramesLeft = 80;
            bubbleCooldownFramesLeft = 220;
        }

        // ------------------------------------------------------------
        // ⚠️つまづきポイント⑤：先読み計算の中身
        // ------------------------------------------------------------
        // moveDx, moveDy は「前回位置から、今の位置までの移動量」。
        // それを3倍にして「今の位置」に足すことで、
        // 「今と同じ速度・方向で、あと3フレーム分動いたら
        //   いるであろう位置」を狙い撃ちしている。
        // これが「移動方向の先読み」の正体。
        private void StartNextShot(float playerX, float playerY)
        {
            if (burstCount == 0 || !hasLastPosition)
            {
                // 1発目、または前回位置がまだない場合は、今の位置をそのまま狙う
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

        private void StartCircularAttack()
        {
            IsCircularWarning = true;
            circularWarningFramesLeft = CIRCULAR_WARNING_DURATION;
        }

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
            // 移動（連射攻撃の予告/発動中でなければ）
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

            // 円形攻撃のサイクル（returnなし、並行して動く）
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

            // 水玉弾幕のサイクル（returnなし、これも並行して動く）
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

            // 遠距離（連射）攻撃のサイクル（ここだけ return がある）
            if (IsAttackActive)
            {
                attackFramesLeft--;
                if (attackFramesLeft <= 0)
                {
                    IsAttackActive = false;

                    // ------------------------------------------------------------
                    // ⚠️つまづきポイント⑥：連射の継続判定
                    // ------------------------------------------------------------
                    // burstCount を増やした後、まだBURST_MAX未満なら、
                    // クールダウンを挟まず、すぐに次の弾（2発目）の予告を始める。
                    // BURST_MAXに達していたら、初めて通常のクールダウンに入り、
                    // burstCountを0に戻して「次回はまた1発目から」にする。
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