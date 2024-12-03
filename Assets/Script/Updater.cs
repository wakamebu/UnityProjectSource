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
                process.WaitForExit(1000); // 1秒待機
            }
        }

        // 更新を適用
        try
        {
            Console.WriteLine("Applying update...");
            ZipFile.ExtractToDirectory(updateZipPath, installDir, true);
            File.Delete(updateZipPath);
            Console.WriteLine("Update applied successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to apply update: " + e.Message);
        }

        // メインアプリケーションを再起動
        Console.WriteLine("Restarting application...");
        Process.Start(appExePath);
    }
}