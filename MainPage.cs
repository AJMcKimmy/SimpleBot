using Godot;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using System.Collections.Generic;
using DSharpPlus.Entities;
internal partial class MainPage : Node2D
{
	private CancellationTokenSource tokensource2 = new CancellationTokenSource();
	private CancellationToken ct;
	private bool initialized = false;
	private bool buttonOnCooldown = false;
	private bool fieldActive = false;
	private List<string> commandList = new List<string>();

	UIElementsList uIElements = new UIElementsList();

	//ready runs on launch and pulls all UI elements into our main script here
	public override void _Ready()
	{
		uIElements.startButton = GetNode<Button>("CanvasLayer/Initialize");
		uIElements.consoleWindow = GetNode<RichTextLabel>("CanvasLayer/ConsoleBorder/Console");
		uIElements.buttonTimer = GetNode<Godot.Timer>("ButtonTimer");
		uIElements.guildList = GetNode<ItemList>("CanvasLayer/GuildsList");
		uIElements.chatWindow = GetNode<RichTextLabel>("CanvasLayer/ChatBorder/ChatLog");
		uIElements.submitButton = GetNode<Button>("CanvasLayer/SubmitCommandButton");
		uIElements.prefixField = GetNode<LineEdit>("CanvasLayer/PrefixBox");
		uIElements.responseField = GetNode<TextEdit>("CanvasLayer/ResponseBox");
		uIElements.addCommandButton = GetNode<Button>("CanvasLayer/AddCommandButton");
		uIElements.changeTokenButton = GetNode<Button>("CanvasLayer/ChangeTokenButton");
		uIElements.tokenField = GetNode<LineEdit>("CanvasLayer/TokenField");
		ct = tokensource2.Token;
		if (File.Exists("commandlist.txt"))
		{
			string[] commandFile = File.ReadAllLines("commandlist.txt");
			commandList = new List<string>(commandFile);
		}
		else
		{
			File.Create("commandlist.txt").Close();
		}
	}
	//button press signal linked from godot client
	private async void _on_button_pressed()
	{
		//delay enforced to allow for connection time using godot's built in timer node
		if (!buttonOnCooldown)
		{
			uIElements.buttonTimer.Start();
			buttonOnCooldown = true;
			if (!initialized)
			{
				uIElements.startButton.Text = "Disconnect";
				initialized = true;
				await LogHistory.LogConsoleHistory(">>> Attempting to start bot\n", uIElements.consoleWindow);	
				tokensource2 = new CancellationTokenSource();
				ct = tokensource2.Token;		
				await BotStart(uIElements.startButton, uIElements.consoleWindow, ct, uIElements.guildList, uIElements.chatWindow, commandList);			
			}	
			else if (initialized)
			{
				initialized = false;
				uIElements.startButton.Text = "Connect";
				await LogHistory.LogConsoleHistory(">>> Bot Stopped!\n", uIElements.consoleWindow);
				uIElements.guildList.Clear();
				tokensource2.Cancel();
				tokensource2 = new CancellationTokenSource();
				ct = tokensource2.Token;
			}
		}
	}

	private void _on_button_timer_timeout()
	{
		buttonOnCooldown = false;
	}

	internal async Task BotStart(Button startButton, RichTextLabel consoleWindow, CancellationToken ct, ItemList guildList, RichTextLabel cWindow, List<string> cList)
	{
		JSONReader configJsonFile = new JSONReader();
		await configJsonFile.ReadJSON();

		DiscordClientBuilder builder = DiscordClientBuilder.CreateDefault(configJsonFile.token, DiscordIntents.All);

		//event handler setup for discord client events
		builder.ConfigureEventHandlers
			(
				b => b.HandleMessageCreated(async (s, e) => 
				{
					if (e.Message.Author.GlobalName.Length > 1)
					{
						string message = $"{e.Message.Author.GlobalName} said '{e.Message.Content}' in ({e.Message.Channel.Guild.Name}) {e.Message.Channel.Name}\n";
						cWindow.CallDeferred("add_text", message);
						await LogHistory.LogMessageHistory(message);
						if (e.Message.Content.ToLower().StartsWith("ping"))
						{
							await e.Message.RespondAsync("pong!");
						}
						String[] messageArray = e.Message.Content.ToLower().Split();
						foreach (String x in cList)
						{
							int index = cList.IndexOf(x);
							if (messageArray[0] == x.ToLower())
							{
								await e.Message.RespondAsync(cList[index + 1]);
							}
						}
					}
					
				})
				.HandleGuildDownloadCompleted((s, e) =>
				{
					IReadOnlyDictionary<ulong, DiscordGuild> guilds = e.Guilds;
					foreach (KeyValuePair<ulong, DiscordGuild> guild in guilds)
					{
						String[] stringArray = guild.Value.ToString().Split(" ");
						guildList.CallDeferred("add_item", " " + stringArray[2]);
					}
					return Task.CompletedTask;
				})
			);
		
		DiscordClient client = builder.Build();

		//checks for valid connection
		try 
		{
			await client.ConnectAsync();
			await LogHistory.LogConsoleHistory($">>> Bot successfully connected at: {client.GatewayInfo.Url}\n", uIElements.consoleWindow);
		}
		catch (Exception ex)
		{
			await LogHistory.LogConsoleHistory($"{ex}, {ex.Message}", uIElements.consoleWindow);
			await LogHistory.LogConsoleHistory(">>> Bot failed to connect.  This is most likely due to an incorrect token.\n", uIElements.consoleWindow);
		}

		//Botstop to watch for client disconnect requests
		await BotStop(client, ct);
	}

	static async Task BotStop(DiscordClient client, CancellationToken ct)
	{
		bool moreToDo = true;
		while (moreToDo)
		{
			if (ct.IsCancellationRequested)
			{
				await client.DisconnectAsync();
				moreToDo = false;
			}
			else
			{
				//delayed here to minimize looping, 50ms is more than enough response time for the disconnect request
				await Task.Delay(50);
			}
		}
	}

	//handles custom command submission
	private async void _on_submit_command_button_pressed()
	{
		if (uIElements.prefixField.Text.Contains(" "))
		{
			await LogHistory.LogConsoleHistory(">>> Custom command improperly formatted, do not use any spaces\n", uIElements.consoleWindow);
		}
		else
		{
			commandList.Add(uIElements.prefixField.Text);
			commandList.Add(uIElements.responseField.Text);
			File.AppendAllText("commandlist.txt", uIElements.prefixField.Text + System.Environment.NewLine);
			File.AppendAllText("commandlist.txt", uIElements.responseField.Text + System.Environment.NewLine);
			uIElements.responseField.Clear();
			uIElements.prefixField.Clear();
			await LogHistory.LogConsoleHistory(">>> Added command successfully!\n", uIElements.consoleWindow);
			uIElements.prefixField.Visible = false;
			uIElements.responseField.Visible = false;
			uIElements.submitButton.Visible = false;
			uIElements.addCommandButton.Visible = true;
		}
	}

	//shows fields for custom command submission
	private void _on_add_command_button_pressed()
	{
		uIElements.submitButton.Visible = true;
		uIElements.prefixField.Visible = true;
		uIElements.responseField.Visible = true;
		uIElements.addCommandButton.Visible = false;
	}

	private void _on_change_token_button_pressed()
	{
		if (!fieldActive)
		{
			uIElements.tokenField.Visible = true;
			uIElements.changeTokenButton.Text = "Submit";
			fieldActive = true;
		}
		else if(fieldActive)
		{
			String token = uIElements.tokenField.Text;
			JSONReader configJsonFile = new JSONReader();
			fieldActive = false;
			configJsonFile.WriteJSON(token);
			uIElements.tokenField.Visible = false;
			uIElements.changeTokenButton.Text = "Change Token";
			uIElements.consoleWindow.Text += ">>> Token Updated\n";
		}		
	}
}




