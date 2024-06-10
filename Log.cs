
using System;
using System.IO;
using System.Threading.Tasks;
using Godot;

internal class LogHistory()
{
    internal static Task LogMessageHistory(string message)
	{
		if (!Directory.Exists("logs"))
        {
            Directory.CreateDirectory("logs");
        }
        File.AppendAllText("logs/chatlogs.txt", $"{DateTime.Now}: {message}");
		return Task.CompletedTask;
	}

    internal static Task LogConsoleHistory(string message, RichTextLabel consoleWindow)
    {
        consoleWindow.Text += message;
        if (!Directory.Exists("logs"))
        {
            Directory.CreateDirectory("logs");
        }
        File.AppendAllText("logs/consolelogs.txt", $"{DateTime.Now}: {message}");
        return Task.CompletedTask;
    }
}