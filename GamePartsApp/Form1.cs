using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GamePartsApp
{
    /// <summary>
    ///  メインフォーム。ユーザー入力、UI、ゲームループ（タイマー）および描画処理を担当します。
    ///  ゲームの状態管理や画面遷移もここで行われます。
    /// </summary>
    public partial class Form1 : Form
    {
        // ====================================================================
        // ★アクションゲーム関連：フィールド
        // ====================================================================
        private Player player;
        private WeakEnemy1 enemy1;
        private WeakEnemy2 enemy2;

        // ------------------------------------------------------------
        // ⚠️つまづきポイント①：ボスは「単独の1体」ではなく、
        // 「WeakEnemy1/2を強化した、分身2体」として管理している
        // ------------------------------------------------------------
        // Boss.csというクラスは別に存在するが、
        // 今のボス戦（stageIndex==2）では、実はそちらを使っていない。
        // 代わりに、雑魚1・雑魚2と同じ型（WeakEnemy1, WeakEnemy2）を、
        // パラメータだけ強くして、2体同時に登場させている。
        private WeakEnemy1 bossClone1;
        private WeakEnemy2 bossClone2;

        // ------------------------------------------------------------
        // ⚠️つまづきポイント②：currentTarget は「雑魚1体だけを指す」
        // 変数。ボス戦のときは、この変数は使わない（nullのまま）
        // ------------------------------------------------------------
        // 「今、誰と戦っているか」を表す変数だが、
        // ボス戦（分身2体）は、bossClone1/2という
        // 別々のフィールドで管理しているため、
        // stageIndex == 2 のときは currentTarget = null にしてある。
        // コード中で「stageIndexが2かどうか」を、
        // 何度も分岐条件として使っているのは、このため。
        private Enemy currentTarget;
        private int stageIndex = 0;

        private bool isWPressed, isAPressed, isSPressed, isDPressed;
        private int mouseX, mouseY;

        private int playerCoins = 0;

        // ------------------------------------------------------------
        // コインアニメーション用フィールド（ゲームタブ内の表示用）
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

        // ------------------------------------------------------------
        // ゲームスタートボタン：全キャラクターを、ここで一斉に生成する
        // ------------------------------------------------------------
        private void gameStartButton_Click(object sender, EventArgs e)
        {
            player = new Player(200, 200);
            enemy1 = new WeakEnemy1(500, 200);
            enemy2 = new WeakEnemy2(500, 200);

            // ------------------------------------------------------------
            // ⚠️つまづきポイント③：ボスの分身は、コンストラクタの
            // 省略可能引数（maxHp:, attackPower:）を使って強化している
            // ------------------------------------------------------------
            // enemy1/2と全く同じクラスなのに、
            // maxHp: 150, attackPower: 25 と明示的に指定することで、
            // 通常の雑魚（HP100, 攻撃力20）より強い個体として作られる。
            bossClone1 = new WeakEnemy1(150, 180, maxHp: 150, attackPower: 25);
            bossClone2 = new WeakEnemy2(650, 220, maxHp: 150, attackPower: 25);

            // ------------------------------------------------------------
            // ⚠️つまづきポイント④：水玉弾幕は、画面サイズを"教えて
            // もらわないと"正しい範囲に配置できない
            // ------------------------------------------------------------
            // WeakEnemy1/2クラス自身は、Form1.csの画面サイズを知らない。
            // そこで、敵を作った直後に、必ずこの4行で
            // 「今の画面サイズ」を、それぞれの敵に教えている。
            // これを忘れると、水玉が画面外（見えない場所）に
            // 配置されてしまうバグになる。
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

        // ------------------------------------------------------------
        // ⚠️つまづきポイント⑤：gameTimer.Stop() は、必ず
        // MessageBox.Show の"前"に置く
        // ------------------------------------------------------------
        // MessageBox.Show は、それ以降のコードの実行を止めるが、
        // すでに動いているTimer自体は、裏側で動き続けてしまう。
        // なので「ポップアップを出す前に、まずTimerを止める」
        // という順番を、絶対に守る必要がある。
        // この順番を間違えると、「ポップアップを見ている間に、
        // 裏で敵がまだ攻撃してくる」というバグになる（今日、実際に直したバグ）。
        private void OnEnemyDefeated()
        {
            if (stageIndex == 0)
            {
                stageIndex = 1;
                currentTarget = enemy2;

                playerCoins += 20;
                UpdateCoinDisplays();
                gameTimer.Stop();  // ← 必ずMessageBoxより前
                MessageBox.Show($"雑魚1を倒した！20コイン獲得！\n（所持コイン：{playerCoins}）");

                mainTabControl.SelectedTab = skillTabPage;
            }
            else if (stageIndex == 1)
            {
                stageIndex = 2;
                currentTarget = null;  // ボス戦に入るので、currentTargetはもう使わない

                playerCoins += 30;
                UpdateCoinDisplays();
                gameTimer.Stop();
                MessageBox.Show($"雑魚2を倒した！30コイン獲得！\n（所持コイン：{playerCoins}）\n\nボスの分身が現れた！");

                mainTabControl.SelectedTab = skillTabPage;
            }
        }

        // ------------------------------------------------------------
        // ⚠️つまづきポイント⑥：分身が「片方だけ」倒されたときは、
        // まだクリアにならない
        // ------------------------------------------------------------
        // このメソッドは、分身のどちらか1体が倒されるたびに呼ばれるが、
        // 中身は「両方とも死んでいるか」を確認してから、
        // 初めてクリア処理を実行する。
        // 「1体だけ倒しても、まだゲームは続く」という仕様を、
        // このif文1つで実現している。
        private void OnBossCloneDefeated()
        {
            bool anyAlive = (bossClone1 != null && bossClone1.IsAlive)
                          || (bossClone2 != null && bossClone2.IsAlive);

            if (!anyAlive)
            {
                stageIndex = 3;
                playerCoins += 100;
                UpdateCoinDisplays();
                gameTimer.Stop();
                MessageBox.Show(
                    "🎉 おめでとうございます！ 🎉\n\nボスの分身を全て撃破し、ゲームクリアです！\n\n獲得コイン：100枚",
                    "GAME CLEAR!!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                gameStartButton.Enabled = true;
            }
            // anyAliveがtrueのときは、何もしない
            // （もう片方の分身が、まだ生きているので、戦闘続行）
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

        // ------------------------------------------------------------
        // ⚠️つまづきポイント⑦：CheckPlayerHitは、「誰に対して
        // 判定するか」を、引数で受け取る"共通の判定装置"
        // ------------------------------------------------------------
        // このメソッド自身は「target」という抽象的な敵を受け取るだけで、
        // それがenemy1なのか、bossClone2なのかは気にしない。
        // 呼び出す側が「今どの敵と戦っているか」を判断して、
        // このメソッドに渡すだけでいい設計になっている。
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
                        // 内積（ドット積）：2つの方向がどれくらい
                        // 同じ向きかを、-1〜1の数値で表す
                        float dot = (rdx / rLength) * (toEnemyX / toEnemyLength)
                                  + (rdy / rLength) * (toEnemyY / toEnemyLength);
                        return dot > 0.9f;
                    }
                }
            }
            return false;
        }

        // ------------------------------------------------------------
        // ⚠️つまづきポイント⑧：ProcessWeakEnemy1Attacksは、
        // 「敵の種類ごとの攻撃判定を、1箇所にまとめた」メソッド
        // ------------------------------------------------------------
        // 引数の w1 には、通常の雑魚1（enemy1）が渡されることもあれば、
        // ボス分身（bossClone1）が渡されることもある。
        // どちらも同じ「WeakEnemy1型」なので、
        // 中の判定ロジック（円形・格子・水玉）は、全く同じコードで動く。
        //
        // メソッドの中で return しているのは、
        // 「プレイヤーがこの攻撃で死んでしまったら、
        //   これ以降の判定（他の攻撃タイプ）はもうチェックしない」
        // という意味。1フレームの間に、複数の攻撃で
        // 立て続けにダメージが入るのを防いでいる。
        private void ProcessWeakEnemy1Attacks(WeakEnemy1 w1)
        {
            if (w1 == null || !w1.IsAlive) return;

            // 円形攻撃の判定
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

            // 格子攻撃の判定（プレイヤーが今いるマスが危険かどうか）
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

            // 水玉弾幕の判定（プレイヤーが、どれか1つの水玉に触れていないか）
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
                        // 1個の水玉に当たったら、他の水玉はチェックしなくていい
                        break;
                    }
                }
            }
        }

        // ------------------------------------------------------------
        // ProcessWeakEnemy2Attacksも、⑧と全く同じ考え方
        // （通常の雑魚2にも、ボス分身2にも、共通で使われる）
        // ------------------------------------------------------------
        private void ProcessWeakEnemy2Attacks(WeakEnemy2 w2)
        {
            if (w2 == null || !w2.IsAlive) return;

            // 連射攻撃の判定
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

            // 円形攻撃の判定
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

            // 水玉弾幕の判定
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

        // ------------------------------------------------------------
        // ⚠️つまづきポイント⑨：接触ダメージも、targetを引数で受け取る
        // 「共通メソッド」になっている
        // ------------------------------------------------------------
        // これも⑦⑧と同じ設計思想：
        // 「誰に対する処理か」を外側（呼び出し元）が決め、
        // このメソッド自身は「渡されたtargetに対して、
        // 同じ計算をするだけ」という、汎用的な役割に徹している。
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

                // プレイヤーを、敵の外側まで押し出す（すり抜け防止）
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

        // ------------------------------------------------------------
        // gameTimer_Tick：毎フレーム、状態を更新する（ゲーム全体の心臓部）
        // ------------------------------------------------------------
        // ⚠️つまづきポイント⑩：このメソッドの中に、
        // 「stageIndex == 2 かどうか」の分岐が、複数回出てくる
        //
        // ①プレイヤーの攻撃判定（誰に当たるかの分岐）
        // ②敵の更新・攻撃判定（bossClone1/2 か、currentTarget かの分岐）
        //
        // 読むときは、「今はボス戦か、そうでないか」を、
        // 一度頭の中で切り替えてから、該当するブロックだけを追うとよい。
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

            // コインアニメーションのフレームを進める
            // ※ *6 しているのは、切り替わりを少しゆっくりにするため
            //   （毎フレーム切り替えると速すぎて見えない）
            if (coinFrames.Count > 0)
            {
                coinFrameIndex = (coinFrameIndex + 1) % (coinFrames.Count * 6);
            }

            // ------------------------------------------------------------
            // ①プレイヤーの攻撃判定：ボス戦か、それ以外かで分岐
            // ------------------------------------------------------------
            if (player.IsAttackActive && !player.HasDealtDamageThisAttack)
            {
                if (stageIndex == 2)
                {
                    // ボス戦：bossClone1、bossClone2の順に、
                    // 「先に見つかった、当たった方」だけにダメージを与える
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

            // ------------------------------------------------------------
            // ②敵の更新・攻撃判定：これも、ボス戦かどうかで分岐
            // ------------------------------------------------------------
            if (stageIndex == 2)
            {
                // ボス戦：分身2体を、それぞれ独立して更新する
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
                // 通常の雑魚戦：currentTargetが、
                // WeakEnemy1かWeakEnemy2かで、呼ぶメソッドを分ける
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

        // ------------------------------------------------------------
        // DrawWeakEnemy1Attacks：見た目の描画も、判定処理⑧と
        // 全く同じ「共通メソッド」の考え方で作られている
        // ------------------------------------------------------------
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

        // ------------------------------------------------------------
        // gameTabPage の描画処理：gameTimer_Tickと、ほぼ同じ構造の分岐
        // ------------------------------------------------------------
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

            // ⚠️gameTimer_Tickと同じ「stageIndex==2かどうか」の分岐が、
            // 描画側でも繰り返されている
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

            // コインアニメーション（ゲームタブ内）
            if (coinFrames.Count > 0)
            {
                int frameToShow = coinFrameIndex / 6;
                if (frameToShow >= coinFrames.Count) frameToShow = 0;

                e.Graphics.DrawImage(coinFrames[frameToShow], 20, 35, 40, 40);
                e.Graphics.DrawString($"x {playerCoins}", this.Font, Brushes.Blue, 70, 38);
            }
            else
            {
                // 画像が読み込めなかった場合の、文字だけのフォールバック表示
                e.Graphics.DrawString($"コイン: {playerCoins}", this.Font, Brushes.Blue, 30, 40);
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


        // ====================================================================
        // フィールド①：スキル選択関連
        // ====================================================================
        private SkillSelector skillSelector = new SkillSelector();

        // ====================================================================
        // フィールド②：ガチャガチャ関連
        // ====================================================================
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

        // ====================================================================
        // フィールド③：スロット関連
        // ====================================================================
        private List<GachaItem> slotItems;
        private int leftCurrentIndex = 0;
        private int centerCurrentIndex = 0;
        private int rightCurrentIndex = 0;
        private int slotStopCount = 0;

        // ====================================================================
        // コンストラクタ
        // ====================================================================
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
            // ⚠️つまづきポイント⑪：コイン画像の読み込みは、
            // 失敗しても「アプリが落ちない」ように、try-catchで守られている
            // ------------------------------------------------------------
            // もし images\coin_frame1.jpg 〜 4.jpg のどれかが
            // 見つからなかった場合、Image.FromFileが例外を投げる。
            // catchブロックでそれを受け止め、coinFrames.Clear() することで、
            // 「アプリ全体が落ちる」代わりに、
            // 「コイン表示が、文字だけのフォールバックになる」という、
            // 安全な失敗の仕方にしている。
            try
            {
                coinFrames.Add(Image.FromFile(@"images\coin_frame1.jpg"));
                coinFrames.Add(Image.FromFile(@"images\coin_frame2.jpg"));
                coinFrames.Add(Image.FromFile(@"images\coin_frame3.jpg"));
                coinFrames.Add(Image.FromFile(@"images\coin_frame4.jpg"));

                // ガチャ・スロット欄のPictureBoxにも、最初の1枚をセット
                // （coinAnimTimerが動き出すまでの、一瞬の空白を防ぐため）
                gachaCoinPictureBox.Image = coinFrames[0];
                gachaCoinPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

                slotCoinPictureBox.Image = coinFrames[0];
                slotCoinPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            }
            catch
            {
                coinFrames.Clear();
            }
        }

        // ------------------------------------------------------------
        // ⚠️つまづきポイント⑫：ガチャ・スロット欄のコインは、
        // gameTimerとは"別の"、独立したTimerで動いている
        // ------------------------------------------------------------
        // ゲームタブのコイン（gameTabPage_Paint内）は、
        // gameTimer（ゲームが始まっているときだけ動く）に乗っかっている。
        //
        // でもガチャ・スロット欄は、
        // 「ゲームをまだ始めていなくても」表示され続ける必要があるので、
        // coinAnimTimerという、常時動く専用のTimerを別に用意している。
        private int coinIconFrameIndex = 0;

        private void coinAnimTimer_Tick(object sender, EventArgs e)
        {
            if (coinFrames.Count == 0) return;

            coinIconFrameIndex = (coinIconFrameIndex + 1) % coinFrames.Count;

            gachaCoinPictureBox.Image = coinFrames[coinIconFrameIndex];
            slotCoinPictureBox.Image = coinFrames[coinIconFrameIndex];
        }

        // ====================================================================
        // ★スキル選択機能
        // ====================================================================
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

        // ------------------------------------------------------------
        // ⚠️つまづきポイント⑬：スキル選択後、ゲームタブに戻ると同時に
        // gameTimerを再開している
        // ------------------------------------------------------------
        // 雑魚を倒した瞬間（OnEnemyDefeated内）でgameTimerを止めたので、
        // スキルを選び終えたこのタイミングで、
        // 対になる「Start」を呼ぶ必要がある。
        // if (!gameTimer.Enabled && ...) というガードは、
        // 「まだ止まっている場合だけ」再開する、という安全策。
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

        // ====================================================================
        // ★ガチャガチャ機能
        // ====================================================================
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

        // ====================================================================
        // ★スロット機能
        // ====================================================================
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