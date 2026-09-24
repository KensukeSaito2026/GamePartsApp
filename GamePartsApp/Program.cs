// ==============================================================
// ファイル: Program.cs
// 概要: アプリケーションのエントリポイント。アプリ起動時の設定初期化と
//       メインフォーム(Form1)の起動を行います。
// 注意: アプリ全体の実行開始処理のみを担当しており、画面表示やゲームロジックは
//       主に Form1.cs に実装されています。
// ==============================================================
namespace GamePartsApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}
