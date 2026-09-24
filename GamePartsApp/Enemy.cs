using System;

namespace GamePartsApp
{
    // ====================================================================
    // Enemyクラス：すべての敵の「共通の親」となる基底クラス
    // ====================================================================
    /// <summary>
    ///  敵キャラクターの共通の状態と基本機能を提供します。
    ///  HPや攻撃力、移動、ダメージ処理などの基礎を定義します。
    /// </summary>
    // Boss, WeakEnemy1, WeakEnemy2 は、すべてこのEnemyを継承している。
    // 「共通して持つべきデータ・機能」だけを、ここにまとめている。
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
        public int HP { get; protected set; }
        public int MaxHP { get; protected set; }
        public int AttackPower { get; protected set; }
        public float Radius { get; set; } = 20f;

        // 移動速度（1.5f → 2.2fに変更済み）
        public float MoveSpeed { get; protected set; } = 2.2f;

        public string Name { get; protected set; } = "敵";

        // ------------------------------------------------------------
        // コンストラクタ
        // ------------------------------------------------------------
        // 子クラス（Boss, WeakEnemy1, WeakEnemy2）は、
        // 自分のコンストラクタの中で
        //   : base("名前", x, y, maxHp, attackPower)
        // という書き方で、必ずこのコンストラクタを呼び出す必要がある。
        // これを「基底クラスのコンストラクタを呼ぶ」という。
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
        public virtual void TakeDamage(int damage)
        {
            HP = Math.Max(HP - damage, 0);
        }

        // ------------------------------------------------------------
        // 式形式プロパティ（=> の書き方）
        // ------------------------------------------------------------
        // public bool IsAlive { get { return HP > 0; } } と、全く同じ意味。
        // 「HPを直接見なくても、生きているかどうかを聞ける」窓口。
        public bool IsAlive => HP > 0;

        // ------------------------------------------------------------
        // protected：Enemyと、その子クラスの中からしか呼べないメソッド
        // ------------------------------------------------------------
        // Form1.cs（外部）から enemy.GetDistance(...) とは書けない。
        // 「敵クラスの内部だけで使う、共通の計算道具」という位置づけ。
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
        public virtual void UpdateFrame(float playerX, float playerY)
        {
            // 基底クラスでは、特に何もしない（子クラスで実装する）
        }
    }
}