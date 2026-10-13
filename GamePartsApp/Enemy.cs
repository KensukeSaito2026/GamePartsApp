using System;

namespace GamePartsApp
{
    // ====================================================================
    // Enemyクラス：すべての敵の「共通の親」となる基底クラス
    // ====================================================================
    // Boss, WeakEnemy1, WeakEnemy2 は、すべてこのEnemyを継承している。
    // 「共通して持つべきデータ・機能」だけを、ここにまとめている。

    /// <summary>
    /// すべての敵キャラクターの、共通の親となる基底クラス。
    /// 座標・HP・攻撃力などの基本データと、
    /// ダメージ処理・生存判定・距離計算といった共通機能を提供する。
    /// </summary>
    public class Enemy
    {
        public float X { get; set; }
        public float Y { get; set; }

        // ------------------------------------------------------------
        // ⚠️つまづきポイント①：{ get; protected set; } の意味
        // ------------------------------------------------------------
        // protected set は「このクラス自身と、継承した子クラス
        // （Boss, WeakEnemy1, WeakEnemy2）の"内部からだけ"書き込める」
        // という意味。
        //
        // Form1.cs（外部）から
        //   enemy.HP = 50;
        // のように直接書き込むことはできない（コンパイルエラーになる）。
        // HPを変えたいときは、必ず TakeDamage(...) を経由する必要がある。
        // これは「うっかり外から直接HPをいじってしまう事故」を防ぐ設計。

        /// <summary>
        /// 現在のHP（体力）。0未満にはならない。
        /// 外部からの書き換えはできず、<see cref="TakeDamage"/> を通してのみ減少する。
        /// </summary>
        public int HP { get; protected set; }

        /// <summary>
        /// 最大HP（体力の上限値）。
        /// </summary>
        public int MaxHP { get; protected set; }

        /// <summary>
        /// この敵の攻撃力。ダメージ計算の基準値として使われる。
        /// </summary>
        public int AttackPower { get; protected set; }

        /// <summary>
        /// 当たり判定に使う、円の半径。
        /// </summary>
        public float Radius { get; set; } = 20f;

        // 移動速度（1.5f → 2.2fに変更済み）
        /// <summary>
        /// 1フレームあたりの移動量。値が大きいほど、素早く移動する。
        /// </summary>
        public float MoveSpeed { get; protected set; } = 2.2f;

        /// <summary>
        /// 敵の表示名（画面表示やデバッグに使用）。
        /// </summary>
        public string Name { get; protected set; } = "敵";

        // ------------------------------------------------------------
        // コンストラクタ
        // ------------------------------------------------------------
        // 子クラス（Boss, WeakEnemy1, WeakEnemy2）は、
        // 自分のコンストラクタの中で
        //   : base("名前", x, y, maxHp, attackPower)
        // という書き方で、必ずこのコンストラクタを呼び出す必要がある。

        /// <summary>
        /// Enemyの新しいインスタンスを作成する。
        /// </summary>
        /// <param name="name">敵の表示名</param>
        /// <param name="startX">初期X座標</param>
        /// <param name="startY">初期Y座標</param>
        /// <param name="maxHp">最大HP（同時にHPの初期値にもなる）</param>
        /// <param name="attackPower">攻撃力</param>
        public Enemy(string name, float startX, float startY, int maxHp, int attackPower)
        {
            Name = name;
            X = startX;
            Y = startY;
            MaxHP = maxHp;
            HP = maxHp;
            AttackPower = attackPower;
        }

        // ------------------------------------------------------------
        // ⚠️つまづきポイント②：virtual の意味
        // ------------------------------------------------------------
        // virtual が付いているメソッドは、
        // 子クラス側で override（上書き）することができる。
        //
        // 今のところ TakeDamage を override している子クラスはないので、
        // Boss.TakeDamage(10) と書いても、
        // 実際にはこの Enemy.TakeDamage が実行される。
        // 将来「ボスだけ特別な被弾処理をしたい」ときに、
        // Boss側で override void TakeDamage(...) と書けば、
        // そちらが優先されるようになる。

        /// <summary>
        /// 敵にダメージを与える。HPが0を下回らないように制限される。
        /// 子クラスで override することで、独自の被弾処理を追加できる。
        /// </summary>
        /// <param name="damage">与えるダメージ量</param>
        public virtual void TakeDamage(int damage)
        {
            HP = Math.Max(HP - damage, 0);
        }

        // ------------------------------------------------------------
        // 式形式プロパティ（=> の書き方）
        // ------------------------------------------------------------
        // public bool IsAlive { get { return HP > 0; } } と、全く同じ意味。
        // 「HPを直接見なくても、生きているかどうかを聞ける」窓口。

        /// <summary>
        /// この敵が、まだ生きているかどうか。HPが0より大きければ true。
        /// </summary>
        public bool IsAlive => HP > 0;

        // ------------------------------------------------------------
        // protected：Enemyと、その子クラスの中からしか呼べないメソッド
        // ------------------------------------------------------------
        // Form1.cs（外部）から enemy.GetDistance(...) とは書けない。
        // 「敵クラスの内部だけで使う、共通の計算道具」という位置づけ。

        /// <summary>
        /// 2点間の直線距離を計算する（三平方の定理）。
        /// 当たり判定や、方向計算（正規化）のベースとして使われる。
        /// </summary>
        /// <param name="x1">1点目のX座標</param>
        /// <param name="y1">1点目のY座標</param>
        /// <param name="x2">2点目のX座標</param>
        /// <param name="y2">2点目のY座標</param>
        /// <returns>2点間の距離</returns>
        protected float GetDistance(float x1, float y1, float x2, float y2)
        {
            float dx = x1 - x2;
            float dy = y1 - y2;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        // ------------------------------------------------------------
        // ⚠️つまづきポイント③：中身が空っぽの virtual メソッド
        // ------------------------------------------------------------
        // Enemy自身は、UpdateFrameで「何もしない」。
        // これは、Boss, WeakEnemy1, WeakEnemy2 それぞれが、
        // 自分専用の override void UpdateFrame(...) を持っているため。
        //
        // ただし、SummonMinions()で作られる「召喚された雑魚」だけは、
        // このEnemyクラスを"そのまま"使っており、
        // 専用の攻撃パターンを持たない（ただの接触ダメージ要員）。
        // そのため、召喚された雑魚のUpdateFrameは、常にこの「空っぽ」が呼ばれる。

        /// <summary>
        /// 毎フレーム呼び出される、状態更新処理。
        /// 基底クラスでは何もしない。子クラス（Boss, WeakEnemy1, WeakEnemy2）が
        /// override して、それぞれ固有の移動・攻撃サイクルを実装する。
        /// </summary>
        /// <param name="playerX">プレイヤーの現在のX座標</param>
        /// <param name="playerY">プレイヤーの現在のY座標</param>
        public virtual void UpdateFrame(float playerX, float playerY)
        {
            // 基底クラスでは、特に何もしない（子クラスで実装する）
        }
    }
}