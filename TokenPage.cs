using Godot;
using System.IO;

internal partial class TokenPage : Node2D
{
	private Button submitButton;
	private LineEdit tokenField;
	
	public override void _Ready()
	{
		if (File.Exists("config/token.json"))
		{
			GetTree().ChangeSceneToFile("res://scenes/MainPage.tscn");
		}
		else
		{
			submitButton = GetNode<Button>("CanvasLayer/Submit");
			tokenField = GetNode<LineEdit>("CanvasLayer/TokenField");
			Directory.CreateDirectory("config");
		}		
	}
	
	private async void _on_submit_pressed()
	{
		JSONReader configJsonFile = new JSONReader();
		await configJsonFile.WriteJSON(tokenField.Text);
		GetTree().ChangeSceneToFile("res://scenes/MainPage.tscn");
	}
}



