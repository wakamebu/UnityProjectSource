using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: Updater.exe <UpdateZipPath> <AppExePath> <InstallDir>");
            return;
        }

        // 監視開始メッセージ
        Console.WriteLine("メインアプリケーションの起動状況を監視します。");

        string updateZipPath = args[0];
        string appExePath = args[1];
        string installDir = args[2];

        // メインアプリケーションのプロセス名を取得
        string appProcessName = Path.GetFileNameWithoutExtension(appExePath);

        // メインアプリケーションのプロセスが終了するまで待機
        bool isRunning = true;
        while (isRunning)
        {
            isRunning = false;
            Process[] processes = Process.GetProcessesByName(appProcessName);
            foreach (Process process in processes)
            {
                isRunning = true;
                process.WaitForExit(3000); // 3秒待機
                Console.WriteLine("メインアプリケーションがまだ起動中です。終了を待機します...");
            }
        }

        // 更新を適用
        try
        {
            Console.WriteLine("更新を適用しています...");

            // ZipFileExtractionではなく、ZipArhiveを使用し進捗を取れるように
            using (ZipArchive archive = ZipFile.OpenRead(updateZipPath))
            {
                // 進捗用変数
                int totalEntries = archive.Entries.Count;
                int currentCount = 0;

                foreach(var entry in archive.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name))
                    {
                        // ディレクトリの場合はスキップ
                        string directoryPath = Path.Combine(installDir, entry.FullName);
                        Directory.CreateDirectory(directoryPath);
                    }
                    else
                    {
                        // ファイルの展開
                        string destinationPath = Path.Combine(installDir, entry.FullName);
                        Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                        
                        Console.WriteLine($"解凍中: {entry.FullName}");
                        entry.ExtractToFile(destinationPath, true);
                    }

                    currentCount++;
                    double progress = (double)currentCount / totalEntries * 100.0;
                    Console.WriteLine($"進捗: {progress:F1}%");
                }

            }
            File.Delete(updateZipPath);
            Console.WriteLine("更新終了");
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to apply update: " + e.Message);
        }

        // メインアプリケーションを再起動
        Console.WriteLine("メインアプリケーションを再起動します。何か入力して Enter を押してください...");
        Console.ReadLine();
        
        try
        {
            Process.Start(appExePath);
        }
        catch (Exception e)
        {
            Console.WriteLine("メインアプリケーションの起動に失敗しました: " + e.Message);
        }
    }
}