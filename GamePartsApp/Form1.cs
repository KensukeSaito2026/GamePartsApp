using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GamePartsApp
{
    // ====================================================================
    // Form1：GamePartsAppのメイン画面（3つのタブを持つ「指揮者」役）
    // ====================================================================
    // このクラスの役割は、大きく分けて3つ：
    //
    // ① SkillSelector, GachaManager のような「専門クラス」を持ち、
    //    それぞれのメソッドを呼び出す
    // ② ボタンが押された、マウスが乗った、などの「イベント」を受け取る
    // ③ ①で得た結果を、画面上のLabel・PictureBoxなどに反映する
    //
    // 複雑な計算（重み付き抽選、色マスク合成など）は、
    // すべて専門クラス側に任せているため、
    // このForm1.csは「何が起きているか」を追いやすい構成になっている。
    public partial class Form1 : Form
    {
        // ====================================================================
        // フィールド①：スキル選択関連
        // ====================================================================
        // SkillSelectorのインスタンスを、フィールドとして1つだけ持つ。
        // = new SkillSelector() で、フィールド宣言と同時にインスタンスを作る、
        // という書き方（今日何度も使ってきたパターン）。
        private SkillSelector skillSelector = new SkillSelector();

        // ====================================================================
        // フィールド②：ガチャガチャ関連
        // ====================================================================
        private GachaManager gachaManager = new GachaManager();

        // ------------------------------------------------------------
        // ガチャの「くじ引き演出」で使う、4枚の画像パスのリスト
        // ------------------------------------------------------------
        // List<string> の初期化子で、あらかじめ4枚分のパスを
        // まとめて用意しておく。
        private List<string> gachaAnimationFrames = new List<string>
        {
            @"images\gacha1.png",
            @"images\gacha2.png",
            @"images\gacha3.png",
            @"images\gacha4.png"
        };

        // 今、演出アニメの何枚目を表示しているか
        private int currentFrame = 0;

        // ------------------------------------------------------------
        // pendingResult：抽選で決まった「今回のガチャ結果」を覚えておく
        // ------------------------------------------------------------
        // GachaItem? の「?」は、null許容型（以前学んだ内容）。
        // 「最初はまだ何も引いていない（null）かもしれない」ことを
        // 正直に宣言している。
        private GachaItem? pendingResult;

        // 歩行アニメの、今何枚目を表示しているか
        private int walkFrameIndex = 0;

        // ====================================================================
        // フィールド③：スロット関連
        // ====================================================================
        // ガチャと同じアイテムのリストを、スロットでも再利用する。
        // 新しいデータを作り直さず、既存のGachaManagerから借りてくる。
        private List<GachaItem> slotItems;

        // ------------------------------------------------------------
        // 3つの窓（左・真ん中・右）、それぞれの「今、何コマ目か」
        // ------------------------------------------------------------
        private int leftCurrentIndex = 0;
        private int centerCurrentIndex = 0;
        private int rightCurrentIndex = 0;

        // ------------------------------------------------------------
        // slotStopCount：ストップボタンを、今まで何回押したか
        // ------------------------------------------------------------
        // 0 = まだ押していない
        // 1 = 左が止まった
        // 2 = 真ん中も止まった
        // 3 = 右も止まった（全部止まった＝判定へ）
        private int slotStopCount = 0;

        // ====================================================================
        // コンストラクタ：Form1が作られた瞬間（アプリ起動時）の初期化
        // ====================================================================
        public Form1()
        {
            // InitializeComponent()：
            // フォームデザイン画面で配置した部品（Button, PictureBoxなど）を
            // 実際に画面上に生成する、自動生成されたメソッド。
            // これを呼ばないと、部品が何も表示されない。
            InitializeComponent();

            // ------------------------------------------------------------
            // スキル選択の3つのPanelに、あらかじめ枠線を付けておく
            // ------------------------------------------------------------
            skill1Panel.BorderStyle = BorderStyle.FixedSingle;
            skill2Panel.BorderStyle = BorderStyle.FixedSingle;
            skill3Panel.BorderStyle = BorderStyle.FixedSingle;

            // ------------------------------------------------------------
            // スロット用のアイテムリストを、GachaManagerから取得する
            // ------------------------------------------------------------
            // GetAllItems()というメソッド経由で、
            // GachaManagerが内部に持っているリストを「読み取り」で借りる。
            slotItems = gachaManager.GetAllItems();

            // ------------------------------------------------------------
            // 起動直後に、スキル選択の最初の3つを表示しておく
            // ------------------------------------------------------------
            RollSkills();
        }

        // ====================================================================
        // ★スキル選択機能
        // ====================================================================

        // ------------------------------------------------------------
        // マウスが乗った瞬間のハイライト（3パネル分）
        // ------------------------------------------------------------
        // MouseEnterイベント：マウスカーソルが、そのコントロールの上に
        // 乗った瞬間に発生する。
        private void skill1Panel_MouseEnter(object sender, EventArgs e)
        {
            skill1Panel.BackColor = Color.LightYellow;
        }

        // MouseLeaveイベント：マウスが、そのコントロールから離れた瞬間に発生。
        // SystemColors.Control は、Windowsの標準的な背景色（元の色に戻す）。
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

        // ------------------------------------------------------------
        // 各パネルがクリックされたときの処理
        // ------------------------------------------------------------
        // skillSelector.CurrentChoices[0] のように、
        // 「今表示されている3つのスキル」から、対応する1つを取り出して、
        // 共通の確認処理（ConfirmSkillSelection）に渡している。
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
        // ConfirmSkillSelection：確認ダイアログ→確定ポップアップの流れ
        // ------------------------------------------------------------
        // 引数として Skill を1つ受け取ることで、
        // どのパネルがクリックされても、この1つのメソッドで
        // 共通の処理ができるようになっている（コードの重複を防ぐ）。
        private void ConfirmSkillSelection(Skill skill)
        {
            // ------------------------------------------------------------
            // MessageBoxButtons.YesNo：はい/いいえの2択ダイアログ
            // ------------------------------------------------------------
            // 戻り値は DialogResult型で、
            // どちらのボタンが押されたかが分かる。
            DialogResult result = MessageBox.Show(
                $"「{skill.Name}」を選びますか？\n\n{skill.Description}",
                "スキル選択の確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            // 「はい」が押された場合だけ、確定ポップアップを出す
            if (result == DialogResult.Yes)
            {
                MessageBox.Show(
                    $"「{skill.Name}」を獲得しました！",
                    "スキル確定",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            // 「いいえ」の場合は、何もせず選択画面に戻るだけ
        }

        // ------------------------------------------------------------
        // リロールボタン：3つのスキルを選び直す
        // ------------------------------------------------------------
        private void rerollButton_Click(object sender, EventArgs e)
        {
            RollSkills();
        }

        // ------------------------------------------------------------
        // RollSkills：3つのスキルを選び直し、画面に反映するメソッド
        // ------------------------------------------------------------
        private void RollSkills()
        {
            // SkillSelectorに「選び直して」とお願いする
            skillSelector.RerollSkills();

            // 選ばれた結果（3つ）を取得する
            var choices = skillSelector.CurrentChoices;

            // ------------------------------------------------------------
            // choices[0]（1つ目）の情報を、名前・説明・レア度の
            // 各Labelに反映し、さらにレア度に応じた色も設定する
            // ------------------------------------------------------------
            skill1Label.Text = choices[0].Name;
            skill1DescLabel.Text = choices[0].Description;
            skill1RarityLabel.Text = choices[0].GetRarityStars();
            skill1Label.ForeColor = choices[0].GetRarityColor();
            skill1DescLabel.ForeColor = choices[0].GetRarityColor();
            skill1RarityLabel.ForeColor = choices[0].GetRarityColor();

            // choices[1]（2つ目）も、全く同じパターンで反映する
            skill2Label.Text = choices[1].Name;
            skill2DescLabel.Text = choices[1].Description;
            skill2RarityLabel.Text = choices[1].GetRarityStars();
            skill2Label.ForeColor = choices[1].GetRarityColor();
            skill2DescLabel.ForeColor = choices[1].GetRarityColor();
            skill2RarityLabel.ForeColor = choices[1].GetRarityColor();

            // choices[2]（3つ目）も同様
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

        // ------------------------------------------------------------
        // ガチャボタンが押されたときの処理
        // ------------------------------------------------------------
        private void gachaButton_Click(object sender, EventArgs e)
        {
            // ------------------------------------------------------------
            // 【重要なバグ修正箇所】
            // ------------------------------------------------------------
            // もし前回、歩行アニメ（歩くペンギン）が表示されていて、
            // walkAnimationTimerが動いたままだったら、ここで必ず止める。
            //
            // これを書かないと、以下の順で0除算エラーが起きていた：
            //   ① 歩くペンギンが出る → walkAnimationTimer.Start()
            //   ② その状態で、もう一度ガチャを引く
            //   ③ pendingResultが「歩くペンギンではない、別のアイテム」に変わる
            //   ④ でもwalkAnimationTimerは動いたまま
            //   ⑤ walkAnimationTimer_Tickが呼ばれ続け、
            //      WalkFrames.Countが0のアイテムに対して % しようとしてエラー
            //
            // ボタンを押した瞬間に必ずStop()することで、
            // この「前回の状態が残ったまま」という事故を防いでいる。
            walkAnimationTimer.Stop();

            // ------------------------------------------------------------
            // ① 先に抽選結果を決めておく（この時点では、まだ表示しない）
            // ------------------------------------------------------------
            pendingResult = gachaManager.DrawGacha();

            // ------------------------------------------------------------
            // ② アニメーションを、1枚目から開始する
            // ------------------------------------------------------------
            currentFrame = 0;
            gachaPictureBox.ImageLocation = gachaAnimationFrames[currentFrame];

            // ボタンの連打を防ぐため、一時的に押せなくする
            gachaButton.Enabled = false;

            // アニメーション用のTimerを開始する
            gachaTimer.Start();
        }

        // ------------------------------------------------------------
        // gachaTimer_Tick：くじ引き演出のアニメーション本体
        // ------------------------------------------------------------
        private void gachaTimer_Tick(object sender, EventArgs e)
        {
            currentFrame++;

            if (currentFrame < gachaAnimationFrames.Count)
            {
                // ------------------------------------------------------------
                // まだアニメーションの途中：次の画像を表示する
                // ------------------------------------------------------------
                gachaPictureBox.ImageLocation = gachaAnimationFrames[currentFrame];
            }
            else
            {
                // ------------------------------------------------------------
                // アニメーションが最後まで終わった：結果を表示する
                // ------------------------------------------------------------
                gachaTimer.Stop();

                // pendingResultがnullでないか、念のため確認してから使う
                if (pendingResult != null)
                {
                    // ------------------------------------------------------------
                    // 歩行アニメを持つかどうかで、表示方法を分岐する
                    // ------------------------------------------------------------
                    if (pendingResult.HasWalkAnimation())
                    {
                        // 歩くペンギンなら、1枚目からループアニメを開始する
                        walkFrameIndex = 0;
                        gachaResultPictureBox.ImageLocation = pendingResult.WalkFrames[0];
                        walkAnimationTimer.Start();
                    }
                    else
                    {
                        // 通常のアイテムは、レア度の色マスク付き画像を作って表示する
                        gachaResultPictureBox.Image = pendingResult.CreateOverlaidImage();
                    }

                    // 結果発表のポップアップ
                    MessageBox.Show(
                        $"「{pendingResult.Name}」が出ました！\n（レア度：{pendingResult.GetRarityLabel()}）",
                        "ガチャ結果",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }

                // ボタンを、また押せる状態に戻す
                gachaButton.Enabled = true;
            }
        }

        // ------------------------------------------------------------
        // walkAnimationTimer_Tick：歩行アニメを、永遠にループさせる
        // ------------------------------------------------------------
        private void walkAnimationTimer_Tick(object sender, EventArgs e)
        {
            // pendingResultがnullなら、これ以上何もしない（早期リターン）
            if (pendingResult == null) return;

            // ------------------------------------------------------------
            // % を使った循環：0→1→2→0→1→2→... と繰り返す
            // ------------------------------------------------------------
            walkFrameIndex = (walkFrameIndex + 1) % pendingResult.WalkFrames.Count;
            gachaResultPictureBox.ImageLocation = pendingResult.WalkFrames[walkFrameIndex];
        }

        // ====================================================================
        // ★スロット機能：シンプル方式（3窓、レア度で速度変化）
        // ====================================================================

        // ------------------------------------------------------------
        // スロットスタートボタン：3つの窓を、同時に回し始める
        // ------------------------------------------------------------
        private void slotStartButton_Click(object sender, EventArgs e)
        {
            // 何回目のストップ操作か、0にリセットしておく
            slotStopCount = 0;

            // 3つのTimerを、全部同時にスタートさせる
            leftTimer.Start();
            centerTimer.Start();
            rightTimer.Start();

            // スタートボタンを押せなくし、ストップボタンを押せるようにする
            slotStartButton.Enabled = false;
            slotStopButton.Enabled = true;
            slotStopButton.Text = "左を止める";
        }

        // ------------------------------------------------------------
        // GetIntervalByRarity：レア度から、Timer間隔（ミリ秒）を決める
        // ------------------------------------------------------------
        // レア度が高いほど、間隔（Interval）が短くなる＝
        // 切り替わりが速くなる＝止めにくくなる、という難易度表現。
        private int GetIntervalByRarity(int rarity)
        {
            return rarity switch
            {
                1 => 400,  // 銅：ゆっくり（止めやすい）
                2 => 300,
                3 => 200,
                4 => 100,  // 虹：高速（止めにくい）
                _ => 300
            };
        }

        // ------------------------------------------------------------
        // 左窓：1コマ進めて表示し、レア度で次の速度を変える
        // ------------------------------------------------------------
        private void leftTimer_Tick(object sender, EventArgs e)
        {
            // 循環：% を使って、最後まで行ったら先頭に戻る
            leftCurrentIndex = (leftCurrentIndex + 1) % slotItems.Count;

            // 5つのPictureBoxを、まとめて更新する（共通メソッドに任せる）
            UpdateReelDisplay("left", leftCurrentIndex);

            // 今、中央に来ている図柄のレア度に応じて、次の間隔を変える
            var currentItem = slotItems[leftCurrentIndex];
            leftTimer.Interval = GetIntervalByRarity(currentItem.Rarity);
        }

        // ------------------------------------------------------------
        // 真ん中窓：左窓と、全く同じ考え方
        // ------------------------------------------------------------
        private void centerTimer_Tick(object sender, EventArgs e)
        {
            centerCurrentIndex = (centerCurrentIndex + 1) % slotItems.Count;
            UpdateReelDisplay("center", centerCurrentIndex);

            var currentItem = slotItems[centerCurrentIndex];
            centerTimer.Interval = GetIntervalByRarity(currentItem.Rarity);
        }

        // ------------------------------------------------------------
        // 右窓：同じく同じ考え方
        // ------------------------------------------------------------
        private void rightTimer_Tick(object sender, EventArgs e)
        {
            rightCurrentIndex = (rightCurrentIndex + 1) % slotItems.Count;
            UpdateReelDisplay("right", rightCurrentIndex);

            var currentItem = slotItems[rightCurrentIndex];
            rightTimer.Interval = GetIntervalByRarity(currentItem.Rarity);
        }

        // ------------------------------------------------------------
        // UpdateReelDisplay：指定した窓の、5つのPictureBoxをまとめて更新する
        // ------------------------------------------------------------
        // 3つの窓（left, center, right）で、
        // 「表示を更新する」という同じ処理を3回繰り返し書かないための、
        // 共通化されたメソッド。
        //
        // reelName（文字列）で「どの窓を更新するか」を受け取り、
        // switch文で振り分けている。
        private void UpdateReelDisplay(string reelName, int currentIndex)
        {
            // ------------------------------------------------------------
            // 前後2コマぶんのインデックスを計算する
            // ------------------------------------------------------------
            // マイナスにならないよう、先にCountを足してから % する、
            // という今日繰り返し使ったテクニック。
            int bottom2Index = (currentIndex - 2 + slotItems.Count) % slotItems.Count;
            int bottom1Index = (currentIndex - 1 + slotItems.Count) % slotItems.Count;
            int top1Index = (currentIndex + 1) % slotItems.Count;
            int top2Index = (currentIndex + 2) % slotItems.Count;

            // ------------------------------------------------------------
            // reelNameの値によって、更新するPictureBoxを切り替える
            // ------------------------------------------------------------
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

        // ------------------------------------------------------------
        // ストップボタン：押すたびに、左→真ん中→右の順で止める
        // ------------------------------------------------------------
        private void slotStopButton_Click(object sender, EventArgs e)
        {
            // 押した回数を1つ増やす
            slotStopCount++;

            // ------------------------------------------------------------
            // 何回目の押下かによって、止める窓を切り替える
            // ------------------------------------------------------------
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

                    // 3つとも止まったので、判定を行う
                    JudgeSlotResult();

                    // 次のプレイに備えて、カウントをリセットする
                    slotStopCount = 0;
                    slotStartButton.Enabled = true;
                    break;
            }
        }

        // ------------------------------------------------------------
        // JudgeSlotResult：3つの窓が「揃ったか」を判定する
        // ------------------------------------------------------------
        private void JudgeSlotResult()
        {
            // 3つの窓の「今の図柄」を、それぞれ取得する
            GachaItem leftItem = slotItems[leftCurrentIndex];
            GachaItem centerItem = slotItems[centerCurrentIndex];
            GachaItem rightItem = slotItems[rightCurrentIndex];

            // ------------------------------------------------------------
            // 3つとも同じ名前（同じ図柄）かどうかを判定する
            // ------------------------------------------------------------
            // && （かつ）を使い、「左＝真ん中」と「真ん中＝右」の
            // 両方が成立するときだけ、isMatchがtrueになる。
            bool isMatch = leftItem.Name == centerItem.Name && centerItem.Name == rightItem.Name;

            if (isMatch)
            {
                // ------------------------------------------------------------
                // 揃った！レア度に応じたコインを計算する
                // ------------------------------------------------------------
                int coinAmount = centerItem.Rarity switch
                {
                    1 => 10,
                    2 => 30,
                    3 => 60,
                    4 => 100,
                    _ => 5
                };

                MessageBox.Show(
                    $"おめでとうございます！「{centerItem.Name}」が揃いました！\n{coinAmount}コインゲットしました！",
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


// ====================================================================
// 【このファイルの構造、まとめ】
// ====================================================================
// フィールド（クラス全体で使う変数）
//   ├─ スキル選択用：skillSelector
//   ├─ ガチャ用：gachaManager, gachaAnimationFrames, currentFrame,
//   │           pendingResult, walkFrameIndex
//   └─ スロット用：slotItems, leftCurrentIndex, centerCurrentIndex,
//               rightCurrentIndex, slotStopCount
//
// コンストラクタ（Form1()）
//   起動時の初期化：部品の見た目設定、アイテムリストの取得、
//   最初のスキル表示
//
// スキル選択機能のメソッド群
//   ハイライト（MouseEnter/Leave）、クリック処理、確認ダイアログ、
//   リロール処理
//
// ガチャガチャ機能のメソッド群
//   ボタンクリック、アニメーションTick、歩行アニメTick
//
// スロット機能のメソッド群
//   スタートボタン、速度計算、3つのTimer Tick、表示更新の共通化、
//   ストップボタン、揃ったかの判定


// ====================================================================
// 【今日の開発を通して、繰り返し使われたパターン、総まとめ】
// ====================================================================
// ① データクラス（Skill, GachaItem）＋ ロジッククラス（SkillSelector,
//    GachaManager）の分離
//
// ② switch式による、レア度に応じた分岐（色、コイン量、速度など）
//
// ③ % を使った循環（歩行アニメ、スロットの3つの窓）
//
// ④ MessageBoxButtons.YesNo による確認ダイアログ
//
// ⑤ Timer を使った、定期的な処理（アニメーション、リールの回転）
//
// ⑥ null許容型（?）と、null チェックによる安全策
//
// ⑦ Enabled プロパティによる、ボタンの連打防止
//
// ⑧ 「Startした場所には、必ずStopの対を用意する」という設計原則
//    （歩行アニメのバグ修正で、身をもって学んだ教訓）