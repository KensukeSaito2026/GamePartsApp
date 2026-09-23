using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GamePartsApp
{
    public partial class Form1 : Form
    {
        private Player player;
        private WeakEnemy1 enemy1;
        private WeakEnemy2 enemy2;

        // ------------------------------------------------------------
        // 単独のBossではなく、分身2体を個別のフィールドで管理する
        // ------------------------------------------------------------
        private WeakEnemy1 bossClone1;
        private WeakEnemy2 bossClone2;

        private Enemy currentTarget;
        private int stageIndex = 0;

        private bool isWPressed, isAPressed, isSPressed, isDPressed;
        private int mouseX, mouseY;

        private int playerCoins = 0;

        // ------------------------------------------------------------
        // コインアニメーション用フィールド
        // ------------------------------------------------------------
        private List<Image> coinFrames = new List<Image>();
        private int coinFrameIndex = 0;

        private void UpdateCoinDisplays()
        {
            gachaCoinLabel.Text = $"所持コイン: {playerCoins}";
            slotCoinLabel.Text = $"所持コイン: {playerCoins}";
        }

        private void gameTabPage_MouseMove(object sender, MouseEventArgs e)
        {
            mouseX = e.X;
            mouseY = e.Y;
            gameTabPage.Invalidate();
        }

        private void gameStartButton_Click(object sender, EventArgs e)
        {
            player = new Player(200, 200);
            enemy1 = new WeakEnemy1(500, 200);
            enemy2 = new WeakEnemy2(500, 200);

            // ------------------------------------------------------------
            // ボスの分身2体を、少し強化したパラメータで作る
            // ------------------------------------------------------------
            bossClone1 = new WeakEnemy1(150, 180, maxHp: 150, attackPower: 25);
            bossClone2 = new WeakEnemy2(650, 220, maxHp: 150, attackPower: 25);

            bossClone1.ScreenWidth = gameTabPage.Width;
            bossClone1.ScreenHeight = gameTabPage.Height;
            bossClone2.ScreenWidth = gameTabPage.Width;
            bossClone2.ScreenHeight = gameTabPage.Height;
            enemy1.ScreenWidth = gameTabPage.Width;
            enemy1.ScreenHeight = gameTabPage.Height;
            enemy2.ScreenWidth = gameTabPage.Width;
            enemy2.ScreenHeight = gameTabPage.Height;

            stageIndex = 0;
            currentTarget = enemy1;

            isWPressed = false;
            isAPressed = false;
            isSPressed = false;
            isDPressed = false;

            this.KeyPreview = true;

            gameTimer.Start();
            gameStartButton.Enabled = false;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W) isWPressed = true;
            if (e.KeyCode == Keys.A) isAPressed = true;
            if (e.KeyCode == Keys.S) isSPressed = true;
            if (e.KeyCode == Keys.D) isDPressed = true;

            if (e.KeyCode == Keys.E && player != null)
            {
                player.Dodge();
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W) isWPressed = false;
            if (e.KeyCode == Keys.A) isAPressed = false;
            if (e.KeyCode == Keys.S) isSPressed = false;
            if (e.KeyCode == Keys.D) isDPressed = false;

            if (e.KeyCode == Keys.Q && player != null)
            {
                player.StartRangedAttack();
            }
        }

        private void gameTabPage_MouseDown(object sender, MouseEventArgs e)
        {
            if (player == null) return;

            if (e.Button == MouseButtons.Right)
            {
                player.StartMeleeAttack();
            }
        }

        private void OnEnemyDefeated()
        {
            if (stageIndex == 0)
            {
                stageIndex = 1;
                currentTarget = enemy2;

                playerCoins += 20;
                UpdateCoinDisplays();
                MessageBox.Show($"雑魚1を倒した！20コイン獲得！\n（所持コイン：{playerCoins}）");

                gameTimer.Stop();
                mainTabControl.SelectedTab = skillTabPage;
            }
            else if (stageIndex == 1)
            {
                stageIndex = 2;
                currentTarget = null;

                playerCoins += 30;
                UpdateCoinDisplays();
                MessageBox.Show($"雑魚2を倒した！30コイン獲得！\n（所持コイン：{playerCoins}）\n\nボスの分身が現れた！");

                gameTimer.Stop();
                mainTabControl.SelectedTab = skillTabPage;
            }
        }

        private void OnBossCloneDefeated()
        {
            bool anyAlive = (bossClone1 != null && bossClone1.IsAlive)
                          || (bossClone2 != null && bossClone2.IsAlive);

            if (!anyAlive)
            {
                stageIndex = 3;
                playerCoins += 100;
                UpdateCoinDisplays();

                MessageBox.Show(
                    "🎉 おめでとうございます！ 🎉\n\nボスの分身を全て撃破し、ゲームクリアです！\n\n獲得コイン：100枚",
                    "GAME CLEAR!!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                gameTimer.Stop();
                gameStartButton.Enabled = true;
            }
        }

        private void HandlePlayerDefeat()
        {
            MessageBox.Show("敗北...");
            gameTimer.Stop();
            gameStartButton.Enabled = true;

            isWPressed = false;
            isAPressed = false;
            isSPressed = false;
            isDPressed = false;
        }

        private bool CheckPlayerHit(Enemy target)
        {
            if (player.CurrentAttack == Player.AttackType.Melee)
            {
                float mdx = player.X - target.X;
                float mdy = player.Y - target.Y;
                float mDistance = (float)Math.Sqrt(mdx * mdx + mdy * mdy);
                return mDistance <= 60f;
            }
            else if (player.CurrentAttack == Player.AttackType.Ranged)
            {
                float rdx = mouseX - player.X;
                float rdy = mouseY - player.Y;
                float rLength = (float)Math.Sqrt(rdx * rdx + rdy * rdy);

                if (rLength > 0)
                {
                    float toEnemyX = target.X - player.X;
                    float toEnemyY = target.Y - player.Y;
                    float toEnemyLength = (float)Math.Sqrt(toEnemyX * toEnemyX + toEnemyY * toEnemyY);

                    if (toEnemyLength > 0 && toEnemyLength <= 300f)
                    {
                        float dot = (rdx / rLength) * (toEnemyX / toEnemyLength)
                                  + (rdy / rLength) * (toEnemyY / toEnemyLength);
                        return dot > 0.9f;
                    }
                }
            }
            return false;
        }

        // ------------------------------------------------------------
        // 指定したWeakEnemy1（通常雑魚orボス分身、共通）の攻撃を判定する
        // ------------------------------------------------------------
        private void ProcessWeakEnemy1Attacks(WeakEnemy1 w1)
        {
            if (w1 == null || !w1.IsAlive) return;

            if (w1.IsAttackActive && !w1.HasDealtDamageThisAttack)
            {
                float edx = player.X - w1.X;
                float edy = player.Y - w1.Y;
                float eDistance = (float)Math.Sqrt(edx * edx + edy * edy);

                if (eDistance <= w1.AttackRadius && player.IsAlive)
                {
                    player.TakeDamage(w1.AttackPower);
                    w1.MarkDamageDealt();
                    if (!player.IsStunned) player.ApplyStun();

                    if (!player.IsAlive)
                    {
                        HandlePlayerDefeat();
                        return;
                    }
                }
            }

            if (w1.IsGridActive && !w1.HasDealtGridDamage)
            {
                float cellW = gameTabPage.Width / (float)WeakEnemy1.GRID_COLS;
                float cellH = gameTabPage.Height / (float)WeakEnemy1.GRID_ROWS;
                int playerCol = (int)(player.X / cellW);
                int playerRow = (int)(player.Y / cellH);

                if (playerRow >= 0 && playerRow < WeakEnemy1.GRID_ROWS &&
                    playerCol >= 0 && playerCol < WeakEnemy1.GRID_COLS)
                {
                    if (w1.DangerCells[playerRow, playerCol] && player.IsAlive)
                    {
                        player.TakeDamage(15);
                        w1.MarkGridDamageDealt();
                        if (!player.IsStunned) player.ApplyStun();

                        if (!player.IsAlive)
                        {
                            HandlePlayerDefeat();
                            return;
                        }
                    }
                }
            }

            if (w1.IsBubbleActive && !w1.HasDealtBubbleDamage)
            {
                foreach (var bubble in w1.DangerBubbles)
                {
                    float bdx = player.X - bubble.X;
                    float bdy = player.Y - bubble.Y;
                    float bDist = (float)Math.Sqrt(bdx * bdx + bdy * bdy);

                    if (bDist <= w1.BubbleRadius && player.IsAlive)
                    {
                        player.TakeDamage(12);
                        w1.MarkBubbleDamageDealt();
                        if (!player.IsStunned) player.ApplyStun();

                        if (!player.IsAlive)
                        {
                            HandlePlayerDefeat();
                            return;
                        }
                        break;
                    }
                }
            }
        }

        private void ProcessWeakEnemy2Attacks(WeakEnemy2 w2)
        {
            if (w2 == null || !w2.IsAlive) return;

            if (w2.IsAttackActive && !w2.HasDealtDamageThisAttack)
            {
                float edx = player.X - w2.AttackTargetX;
                float edy = player.Y - w2.AttackTargetY;
                float eDistance = (float)Math.Sqrt(edx * edx + edy * edy);

                if (eDistance <= 40f && player.IsAlive)
                {
                    player.TakeDamage(w2.AttackPower);
                    w2.MarkDamageDealt();
                    if (!player.IsStunned) player.ApplyStun();

                    if (!player.IsAlive)
                    {
                        HandlePlayerDefeat();
                        return;
                    }
                }
            }

            if (w2.IsCircularActive && !w2.HasDealtCircularDamage)
            {
                float cdx = player.X - w2.X;
                float cdy = player.Y - w2.Y;
                float cDist = (float)Math.Sqrt(cdx * cdx + cdy * cdy);

                if (cDist <= w2.CircularRadius && player.IsAlive)
                {
                    player.TakeDamage(w2.AttackPower);
                    w2.MarkCircularDamageDealt();
                    if (!player.IsStunned) player.ApplyStun();

                    if (!player.IsAlive)
                    {
                        HandlePlayerDefeat();
                        return;
                    }
                }
            }

            if (w2.IsBubbleActive && !w2.HasDealtBubbleDamage)
            {
                foreach (var bubble in w2.DangerBubbles)
                {
                    float bdx = player.X - bubble.X;
                    float bdy = player.Y - bubble.Y;
                    float bDist = (float)Math.Sqrt(bdx * bdx + bdy * bdy);

                    if (bDist <= w2.BubbleRadius && player.IsAlive)
                    {
                        player.TakeDamage(12);
                        w2.MarkBubbleDamageDealt();
                        if (!player.IsStunned) player.ApplyStun();

                        if (!player.IsAlive)
                        {
                            HandlePlayerDefeat();
                            return;
                        }
                        break;
                    }
                }
            }
        }

        private void ProcessContactDamage(Enemy target)
        {
            if (target == null || !target.IsAlive) return;

            float dx2 = player.X - target.X;
            float dy2 = player.Y - target.Y;
            float distance = (float)Math.Sqrt(dx2 * dx2 + dy2 * dy2);

            if (distance < target.Radius + player.Radius)
            {
                if (!player.IsAlive) return;

                if (!player.IsStunned)
                {
                    player.TakeDamage(1);
                    player.ApplyStun();
                }

                float pushDx = player.X - target.X;
                float pushDy = player.Y - target.Y;
                float pushLength = (float)Math.Sqrt(pushDx * pushDx + pushDy * pushDy);

                if (pushLength > 0)
                {
                    float minDistance = target.Radius + player.Radius + 5f;
                    player.X = target.X + (pushDx / pushLength) * minDistance;
                    player.Y = target.Y + (pushDy / pushLength) * minDistance;
                }

                if (!player.IsAlive)
                {
                    HandlePlayerDefeat();
                }
            }
        }

        private void gameTimer_Tick(object sender, EventArgs e)
        {
            if (player == null) return;

            float dx = 0, dy = 0;
            if (isWPressed) dy -= 1;
            if (isSPressed) dy += 1;
            if (isAPressed) dx -= 1;
            if (isDPressed) dx += 1;

            player.Move(dx, dy, 4f);
            player.UpdateFrame();

            player.X = Math.Clamp(player.X, player.Radius, gameTabPage.Width - player.Radius);
            player.Y = Math.Clamp(player.Y, player.Radius, gameTabPage.Height - player.Radius);

            if (coinFrames.Count > 0)
            {
                coinFrameIndex = (coinFrameIndex + 1) % (coinFrames.Count * 6);
            }

            if (player.IsAttackActive && !player.HasDealtDamageThisAttack)
            {
                if (stageIndex == 2)
                {
                    if (bossClone1 != null && bossClone1.IsAlive && CheckPlayerHit(bossClone1))
                    {
                        bossClone1.TakeDamage(player.AttackPower);
                        player.MarkDamageDealt();
                        if (!bossClone1.IsAlive) OnBossCloneDefeated();
                    }
                    else if (bossClone2 != null && bossClone2.IsAlive && CheckPlayerHit(bossClone2))
                    {
                        bossClone2.TakeDamage(player.AttackPower);
                        player.MarkDamageDealt();
                        if (!bossClone2.IsAlive) OnBossCloneDefeated();
                    }
                }
                else if (currentTarget != null && currentTarget.IsAlive && CheckPlayerHit(currentTarget))
                {
                    currentTarget.TakeDamage(player.AttackPower);
                    player.MarkDamageDealt();

                    if (!currentTarget.IsAlive)
                    {
                        OnEnemyDefeated();
                    }
                }
            }

            if (stageIndex == 2)
            {
                if (bossClone1 != null && bossClone1.IsAlive)
                {
                    bossClone1.UpdateFrame(player.X, player.Y);
                    ProcessWeakEnemy1Attacks(bossClone1);
                    ProcessContactDamage(bossClone1);
                }

                if (bossClone2 != null && bossClone2.IsAlive)
                {
                    bossClone2.UpdateFrame(player.X, player.Y);
                    ProcessWeakEnemy2Attacks(bossClone2);
                    ProcessContactDamage(bossClone2);
                }
            }
            else if (currentTarget != null && currentTarget.IsAlive)
            {
                currentTarget.UpdateFrame(player.X, player.Y);

                if (currentTarget is WeakEnemy1 w1)
                {
                    ProcessWeakEnemy1Attacks(w1);
                }
                else if (currentTarget is WeakEnemy2 w2)
                {
                    ProcessWeakEnemy2Attacks(w2);
                }

                ProcessContactDamage(currentTarget);
            }

            gameTabPage.Invalidate();
        }

        private void DrawWeakEnemy1Attacks(Graphics g, WeakEnemy1 w1)
        {
            if (w1.IsWarning || w1.IsAttackActive)
            {
                Color c = w1.IsWarning ? Color.Yellow : Color.Red;
                using (SolidBrush b = new SolidBrush(Color.FromArgb(120, c)))
                {
                    g.FillEllipse(b, w1.X - w1.AttackRadius, w1.Y - w1.AttackRadius, w1.AttackRadius * 2, w1.AttackRadius * 2);
                }
            }

            if (w1.IsGridWarning || w1.IsGridActive)
            {
                float cellWidth = gameTabPage.Width / (float)WeakEnemy1.GRID_COLS;
                float cellHeight = gameTabPage.Height / (float)WeakEnemy1.GRID_ROWS;

                Color dangerColor = w1.IsGridWarning ? Color.Yellow : Color.Red;

                for (int row = 0; row < WeakEnemy1.GRID_ROWS; row++)
                {
                    for (int col = 0; col < WeakEnemy1.GRID_COLS; col++)
                    {
                        if (w1.DangerCells[row, col])
                        {
                            using (SolidBrush cellBrush = new SolidBrush(Color.FromArgb(100, dangerColor)))
                            {
                                g.FillRectangle(cellBrush, col * cellWidth, row * cellHeight, cellWidth, cellHeight);
                            }
                        }
                    }
                }
            }

            if (w1.IsBubbleWarning || w1.IsBubbleActive)
            {
                Color bubbleColor = w1.IsBubbleWarning ? Color.Yellow : Color.Red;
                using (SolidBrush b = new SolidBrush(Color.FromArgb(130, bubbleColor)))
                {
                    foreach (var bubble in w1.DangerBubbles)
                    {
                        g.FillEllipse(b, bubble.X - w1.BubbleRadius, bubble.Y - w1.BubbleRadius, w1.BubbleRadius * 2, w1.BubbleRadius * 2);
                    }
                }
            }
        }

        private void DrawWeakEnemy2Attacks(Graphics g, WeakEnemy2 w2)
        {
            if (w2.IsWarning || w2.IsAttackActive)
            {
                Color c = w2.IsWarning ? Color.Yellow : Color.Red;
                float r = 40f;
                using (SolidBrush b = new SolidBrush(Color.FromArgb(150, c)))
                {
                    g.FillEllipse(b, w2.AttackTargetX - r, w2.AttackTargetY - r, r * 2, r * 2);
                }
            }

            if (w2.IsCircularWarning || w2.IsCircularActive)
            {
                Color c = w2.IsCircularWarning ? Color.Yellow : Color.Red;
                using (SolidBrush b = new SolidBrush(Color.FromArgb(120, c)))
                {
                    g.FillEllipse(b, w2.X - w2.CircularRadius, w2.Y - w2.CircularRadius, w2.CircularRadius * 2, w2.CircularRadius * 2);
                }
            }

            if (w2.IsBubbleWarning || w2.IsBubbleActive)
            {
                Color bubbleColor = w2.IsBubbleWarning ? Color.Yellow : Color.Red;
                using (SolidBrush b = new SolidBrush(Color.FromArgb(130, bubbleColor)))
                {
                    foreach (var bubble in w2.DangerBubbles)
                    {
                        g.FillEllipse(b, bubble.X - w2.BubbleRadius, bubble.Y - w2.BubbleRadius, w2.BubbleRadius * 2, w2.BubbleRadius * 2);
                    }
                }
            }
        }

        private void DrawEnemyBody(Graphics g, Enemy enemy)
        {
            using (SolidBrush enemyBrush = new SolidBrush(Color.Red))
            {
                g.FillEllipse(enemyBrush, enemy.X - enemy.Radius, enemy.Y - enemy.Radius, enemy.Radius * 2, enemy.Radius * 2);
            }

            float hpBarWidth = 60f;
            float hpRatio = (float)enemy.HP / enemy.MaxHP;
            g.FillRectangle(Brushes.Gray, enemy.X - hpBarWidth / 2, enemy.Y - enemy.Radius - 15, hpBarWidth, 6);
            g.FillRectangle(Brushes.Lime, enemy.X - hpBarWidth / 2, enemy.Y - enemy.Radius - 15, hpBarWidth * hpRatio, 6);
        }

        private void gameTabPage_Paint(object sender, PaintEventArgs e)
        {
            if (player == null) return;

            var state = e.Graphics.Save();

            if (player.IsInvincible)
            {
                e.Graphics.TranslateTransform(player.X, player.Y);
                e.Graphics.RotateTransform(player.DodgeRotation);
                e.Graphics.TranslateTransform(-player.X, -player.Y);
            }

            Color playerColor;
            if (player.IsInvincible) playerColor = Color.LightBlue;
            else if (player.IsStunned) playerColor = Color.Orange;
            else playerColor = Color.Blue;

            using (SolidBrush playerBrush = new SolidBrush(playerColor))
            {
                e.Graphics.FillEllipse(playerBrush, player.X - player.Radius, player.Y - player.Radius, player.Radius * 2, player.Radius * 2);

                using (Pen linePen = new Pen(Color.White, 3))
                {
                    e.Graphics.DrawLine(linePen, player.X, player.Y, player.X + player.Radius, player.Y);
                }
            }

            if (player.IsAttackWarning || player.IsAttackActive)
            {
                Color attackColor = player.IsAttackWarning ? Color.Yellow : Color.Red;

                if (player.CurrentAttack == Player.AttackType.Melee)
                {
                    float meleeRadius = 60f;
                    using (SolidBrush attackBrush = new SolidBrush(Color.FromArgb(120, attackColor)))
                    {
                        e.Graphics.FillEllipse(attackBrush,
                            player.X - meleeRadius, player.Y - meleeRadius,
                            meleeRadius * 2, meleeRadius * 2);
                    }
                }
                else if (player.CurrentAttack == Player.AttackType.Ranged)
                {
                    float rangedDx = mouseX - player.X;
                    float rangedDy = mouseY - player.Y;
                    float rangedLength = (float)Math.Sqrt(rangedDx * rangedDx + rangedDy * rangedDy);

                    if (rangedLength > 0)
                    {
                        float laserEndX = player.X + (rangedDx / rangedLength) * 300f;
                        float laserEndY = player.Y + (rangedDy / rangedLength) * 300f;

                        using (Pen laserPen = new Pen(attackColor, 8))
                        {
                            e.Graphics.DrawLine(laserPen, player.X, player.Y, laserEndX, laserEndY);
                        }
                    }
                }
            }

            e.Graphics.Restore(state);

            if (stageIndex == 2)
            {
                if (bossClone1 != null && bossClone1.IsAlive)
                {
                    DrawEnemyBody(e.Graphics, bossClone1);
                    DrawWeakEnemy1Attacks(e.Graphics, bossClone1);
                }
                if (bossClone2 != null && bossClone2.IsAlive)
                {
                    DrawEnemyBody(e.Graphics, bossClone2);
                    DrawWeakEnemy2Attacks(e.Graphics, bossClone2);
                }
            }
            else if (currentTarget != null && currentTarget.IsAlive)
            {
                DrawEnemyBody(e.Graphics, currentTarget);

                if (currentTarget is WeakEnemy1 w1)
                {
                    DrawWeakEnemy1Attacks(e.Graphics, w1);
                }
                else if (currentTarget is WeakEnemy2 w2)
                {
                    DrawWeakEnemy2Attacks(e.Graphics, w2);
                }
            }

            float playerHpRatio = (float)player.HP / 100;
            e.Graphics.FillRectangle(Brushes.Gray, 10, 10, 150, 20);
            e.Graphics.FillRectangle(Brushes.Lime, 10, 10, 150 * playerHpRatio, 20);
            e.Graphics.DrawString($"HP: {player.HP}/100", this.Font, Brushes.White, 15, 11);

            if (coinFrames.Count > 0)
            {
                int frameToShow = coinFrameIndex / 6;
                if (frameToShow >= coinFrames.Count) frameToShow = 0;

                e.Graphics.DrawImage(coinFrames[frameToShow], 15, 35, 24, 24);
                e.Graphics.DrawString($"x {playerCoins}", this.Font, Brushes.Yellow, 42, 38);
            }
            else
            {
                e.Graphics.DrawString($"コイン: {playerCoins}", this.Font, Brushes.Yellow, 15, 35);
            }

            float dirDx = mouseX - player.X;
            float dirDy = mouseY - player.Y;
            float dirLength = (float)Math.Sqrt(dirDx * dirDx + dirDy * dirDy);

            if (dirLength > 0)
            {
                float lineEndX = player.X + (dirDx / dirLength) * 40f;
                float lineEndY = player.Y + (dirDy / dirLength) * 40f;

                using (Pen aimPen = new Pen(Color.Yellow, 3))
                {
                    e.Graphics.DrawLine(aimPen, player.X, player.Y, lineEndX, lineEndY);
                }
            }
        }


        private SkillSelector skillSelector = new SkillSelector();
        private GachaManager gachaManager = new GachaManager();

        private List<string> gachaAnimationFrames = new List<string>
        {
            @"images\gacha1.png",
            @"images\gacha2.png",
            @"images\gacha3.png",
            @"images\gacha4.png"
        };

        private int currentFrame = 0;
        private GachaItem? pendingResult;
        private int walkFrameIndex = 0;

        private List<GachaItem> slotItems;
        private int leftCurrentIndex = 0;
        private int centerCurrentIndex = 0;
        private int rightCurrentIndex = 0;
        private int slotStopCount = 0;

        public Form1()
        {
            InitializeComponent();

            typeof(Panel).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, gameTabPage, new object[] { true });

            skill1Panel.BorderStyle = BorderStyle.FixedSingle;
            skill2Panel.BorderStyle = BorderStyle.FixedSingle;
            skill3Panel.BorderStyle = BorderStyle.FixedSingle;

            slotItems = gachaManager.GetAllItems();

            RollSkills();

            UpdateCoinDisplays();

            // ------------------------------------------------------------
            // コインアニメーション用の画像を、あらかじめ読み込んでおく
            // ------------------------------------------------------------
            // try-catchで囲むことで、画像ファイルが見つからなくても
            // アプリ全体がクラッシュしないようにしている。
            // 見つからなかった場合は、gameTabPage_Paintの中で
            // 「文字だけの表示」にフォールバックする。
            try
            {
                coinFrames.Add(Image.FromFile(@"images\coin_frame1.jpg"));
                coinFrames.Add(Image.FromFile(@"images\coin_frame2.jpg"));
                coinFrames.Add(Image.FromFile(@"images\coin_frame3.jpg"));
                coinFrames.Add(Image.FromFile(@"images\coin_frame4.jpg"));
            }
            catch
            {
                coinFrames.Clear();
            }
        }

        private void skill1Panel_MouseEnter(object sender, EventArgs e)
        {
            skill1Panel.BackColor = Color.LightYellow;
        }

        private void skill1Panel_MouseLeave(object sender, EventArgs e)
        {
            skill1Panel.BackColor = SystemColors.Control;
        }

        private void skill2Panel_MouseEnter(object sender, EventArgs e)
        {
            skill2Panel.BackColor = Color.LightYellow;
        }

        private void skill2Panel_MouseLeave(object sender, EventArgs e)
        {
            skill2Panel.BackColor = SystemColors.Control;
        }

        private void skill3Panel_MouseEnter(object sender, EventArgs e)
        {
            skill3Panel.BackColor = Color.LightYellow;
        }

        private void skill3Panel_MouseLeave(object sender, EventArgs e)
        {
            skill3Panel.BackColor = SystemColors.Control;
        }

        private void skill1Panel_Click(object sender, EventArgs e)
        {
            ConfirmSkillSelection(skillSelector.CurrentChoices[0]);
        }

        private void skill2Panel_Click(object sender, EventArgs e)
        {
            ConfirmSkillSelection(skillSelector.CurrentChoices[1]);
        }

        private void skill3Panel_Click(object sender, EventArgs e)
        {
            ConfirmSkillSelection(skillSelector.CurrentChoices[2]);
        }

        private void ConfirmSkillSelection(Skill skill)
        {
            DialogResult result = MessageBox.Show(
                $"「{skill.Name}」を選びますか？\n\n{skill.Description}",
                "スキル選択の確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                MessageBox.Show(
                    $"「{skill.Name}」を獲得しました！",
                    "スキル確定",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                if (!gameTimer.Enabled && player != null && player.IsAlive)
                {
                    mainTabControl.SelectedTab = gameTabPage;
                    gameTimer.Start();
                }
            }
        }

        private void rerollButton_Click(object sender, EventArgs e)
        {
            RollSkills();
        }

        private void RollSkills()
        {
            skillSelector.RerollSkills();
            var choices = skillSelector.CurrentChoices;

            skill1Label.Text = choices[0].Name;
            skill1DescLabel.Text = choices[0].Description;
            skill1RarityLabel.Text = choices[0].GetRarityStars();
            skill1Label.ForeColor = choices[0].GetRarityColor();
            skill1DescLabel.ForeColor = choices[0].GetRarityColor();
            skill1RarityLabel.ForeColor = choices[0].GetRarityColor();

            skill2Label.Text = choices[1].Name;
            skill2DescLabel.Text = choices[1].Description;
            skill2RarityLabel.Text = choices[1].GetRarityStars();
            skill2Label.ForeColor = choices[1].GetRarityColor();
            skill2DescLabel.ForeColor = choices[1].GetRarityColor();
            skill2RarityLabel.ForeColor = choices[1].GetRarityColor();

            skill3Label.Text = choices[2].Name;
            skill3DescLabel.Text = choices[2].Description;
            skill3RarityLabel.Text = choices[2].GetRarityStars();
            skill3Label.ForeColor = choices[2].GetRarityColor();
            skill3DescLabel.ForeColor = choices[2].GetRarityColor();
            skill3RarityLabel.ForeColor = choices[2].GetRarityColor();
        }

        private void gachaButton_Click(object sender, EventArgs e)
        {
            const int GACHA_COST = 10;

            if (playerCoins < GACHA_COST)
            {
                MessageBox.Show($"コインが足りません！（必要：{GACHA_COST}枚、所持：{playerCoins}枚）");
                return;
            }

            playerCoins -= GACHA_COST;
            UpdateCoinDisplays();

            walkAnimationTimer.Stop();

            pendingResult = gachaManager.DrawGacha();

            currentFrame = 0;
            gachaPictureBox.ImageLocation = gachaAnimationFrames[currentFrame];

            gachaButton.Enabled = false;
            gachaTimer.Start();
        }

        private void gachaTimer_Tick(object sender, EventArgs e)
        {
            currentFrame++;

            if (currentFrame < gachaAnimationFrames.Count)
            {
                gachaPictureBox.ImageLocation = gachaAnimationFrames[currentFrame];
            }
            else
            {
                gachaTimer.Stop();

                if (pendingResult != null)
                {
                    if (pendingResult.HasWalkAnimation())
                    {
                        walkFrameIndex = 0;
                        gachaResultPictureBox.ImageLocation = pendingResult.WalkFrames[0];
                        walkAnimationTimer.Start();
                    }
                    else
                    {
                        gachaResultPictureBox.Image = pendingResult.CreateOverlaidImage();
                    }

                    MessageBox.Show(
                        $"「{pendingResult.Name}」が出ました！\n（レア度：{pendingResult.GetRarityLabel()}）",
                        "ガチャ結果",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }

                gachaButton.Enabled = true;
            }
        }

        private void walkAnimationTimer_Tick(object sender, EventArgs e)
        {
            if (pendingResult == null) return;

            if (pendingResult.WalkFrames.Count == 0)
            {
                walkAnimationTimer.Stop();
                return;
            }

            walkFrameIndex = (walkFrameIndex + 1) % pendingResult.WalkFrames.Count;
            gachaResultPictureBox.ImageLocation = pendingResult.WalkFrames[walkFrameIndex];
        }

        private void slotStartButton_Click(object sender, EventArgs e)
        {
            const int SLOT_COST = 5;

            if (playerCoins < SLOT_COST)
            {
                MessageBox.Show($"コインが足りません！（必要：{SLOT_COST}枚、所持：{playerCoins}枚）");
                return;
            }

            playerCoins -= SLOT_COST;
            UpdateCoinDisplays();

            slotStopCount = 0;

            leftTimer.Start();
            centerTimer.Start();
            rightTimer.Start();

            slotStartButton.Enabled = false;
            slotStopButton.Enabled = true;
            slotStopButton.Text = "左を止める";
        }

        private int GetIntervalByRarity(int rarity)
        {
            return rarity switch
            {
                1 => 400,
                2 => 300,
                3 => 200,
                4 => 100,
                _ => 300
            };
        }

        private void leftTimer_Tick(object sender, EventArgs e)
        {
            leftCurrentIndex = (leftCurrentIndex + 1) % slotItems.Count;
            UpdateReelDisplay("left", leftCurrentIndex);

            var currentItem = slotItems[leftCurrentIndex];
            leftTimer.Interval = GetIntervalByRarity(currentItem.Rarity);
        }

        private void centerTimer_Tick(object sender, EventArgs e)
        {
            centerCurrentIndex = (centerCurrentIndex + 1) % slotItems.Count;
            UpdateReelDisplay("center", centerCurrentIndex);

            var currentItem = slotItems[centerCurrentIndex];
            centerTimer.Interval = GetIntervalByRarity(currentItem.Rarity);
        }

        private void rightTimer_Tick(object sender, EventArgs e)
        {
            rightCurrentIndex = (rightCurrentIndex + 1) % slotItems.Count;
            UpdateReelDisplay("right", rightCurrentIndex);

            var currentItem = slotItems[rightCurrentIndex];
            rightTimer.Interval = GetIntervalByRarity(currentItem.Rarity);
        }

        private void UpdateReelDisplay(string reelName, int currentIndex)
        {
            int bottom2Index = (currentIndex - 2 + slotItems.Count) % slotItems.Count;
            int bottom1Index = (currentIndex - 1 + slotItems.Count) % slotItems.Count;
            int top1Index = (currentIndex + 1) % slotItems.Count;
            int top2Index = (currentIndex + 2) % slotItems.Count;

            switch (reelName)
            {
                case "left":
                    leftTop2PictureBox.ImageLocation = slotItems[top2Index].ImagePath;
                    leftTop1PictureBox.ImageLocation = slotItems[top1Index].ImagePath;
                    leftCenterPictureBox.ImageLocation = slotItems[currentIndex].ImagePath;
                    leftBottom1PictureBox.ImageLocation = slotItems[bottom1Index].ImagePath;
                    leftBottom2PictureBox.ImageLocation = slotItems[bottom2Index].ImagePath;
                    break;

                case "center":
                    centerTop2PictureBox.ImageLocation = slotItems[top2Index].ImagePath;
                    centerTop1PictureBox.ImageLocation = slotItems[top1Index].ImagePath;
                    centerCenterPictureBox.ImageLocation = slotItems[currentIndex].ImagePath;
                    centerBottom1PictureBox.ImageLocation = slotItems[bottom1Index].ImagePath;
                    centerBottom2PictureBox.ImageLocation = slotItems[bottom2Index].ImagePath;
                    break;

                case "right":
                    rightTop2PictureBox.ImageLocation = slotItems[top2Index].ImagePath;
                    rightTop1PictureBox.ImageLocation = slotItems[top1Index].ImagePath;
                    rightCenterPictureBox.ImageLocation = slotItems[currentIndex].ImagePath;
                    rightBottom1PictureBox.ImageLocation = slotItems[bottom1Index].ImagePath;
                    rightBottom2PictureBox.ImageLocation = slotItems[bottom2Index].ImagePath;
                    break;
            }
        }

        private void slotStopButton_Click(object sender, EventArgs e)
        {
            slotStopCount++;

            switch (slotStopCount)
            {
                case 1:
                    leftTimer.Stop();
                    slotStopButton.Text = "真ん中を止める";
                    break;

                case 2:
                    centerTimer.Stop();
                    slotStopButton.Text = "右を止める";
                    break;

                case 3:
                    rightTimer.Stop();
                    slotStopButton.Text = "スタート";
                    slotStopButton.Enabled = false;

                    JudgeSlotResult();

                    slotStopCount = 0;
                    slotStartButton.Enabled = true;
                    break;
            }
        }

        private void JudgeSlotResult()
        {
            GachaItem leftItem = slotItems[leftCurrentIndex];
            GachaItem centerItem = slotItems[centerCurrentIndex];
            GachaItem rightItem = slotItems[rightCurrentIndex];

            bool isMatch = leftItem.Name == centerItem.Name && centerItem.Name == rightItem.Name;

            if (isMatch)
            {
                int coinAmount = centerItem.Rarity switch
                {
                    1 => 10,
                    2 => 30,
                    3 => 60,
                    4 => 100,
                    _ => 5
                };

                playerCoins += coinAmount;
                UpdateCoinDisplays();

                MessageBox.Show(
                    $"おめでとうございます！「{centerItem.Name}」が揃いました！\n{coinAmount}コインゲットしました！\n（所持コイン：{playerCoins}）",
                    "スロット成功",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else
            {
                MessageBox.Show(
                    "残念、もう一回！",
                    "スロット失敗",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }
    }
}