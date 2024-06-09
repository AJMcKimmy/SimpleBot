using Godot;
using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using System.Collections.Generic;
using DSharpPlus.Entities;
public partial class MainPage : Node2D
{

	public Button startButton;
	public RichTextLabel consoleWindow;
	public Godot.Timer buttonTimer;
	public ItemList guildList;
	public RichTextLabel chatWindow;
	public LineEdit prefixField;
	public TextEdit responseField;
	public Button submitButton;
	public Button addCommandButton;
	public Button changeTokenButton;
	public LineEdit tokenField;
	CancellationTokenSource tokensource2 = new CancellationTokenSource();
	public CancellationToken ct;
	public bool initialized = false;
	public bool buttonOnCooldown = false;
	public bool fieldActive = false;
	public List<string> commandList = new List<string>();

	//ready runs on launch and pulls all UI elements into our main script here(not a true main script since godot compiles down to C++ at runtime)
	public override void _Ready()
	{
		startButton = GetNode<Button>("CanvasLayer/Initialize");
		consoleWindow = GetNode<RichTextLabel>("CanvasLayer/ConsoleBorder/Console");
		buttonTimer = GetNode<Godot.Timer>("ButtonTimer");
		guildList = GetNode<ItemList>("CanvasLayer/GuildsList");
		chatWindow = GetNode<RichTextLabel>("CanvasLayer/ChatLog");
		submitButton = GetNode<Button>("CanvasLayer/SubmitCommandButton");
		prefixField = GetNode<LineEdit>("CanvasLayer/PrefixBox");
		responseField = GetNode<TextEdit>("CanvasLayer/ResponseBox");
		addCommandButton = GetNode<Button>("CanvasLayer/AddCommandButton");
		changeTokenButton = GetNode<Button>("CanvasLayer/ChangeTokenButton");
		tokenField = GetNode<LineEdit>("CanvasLayer/TokenField");
		ct = tokensource2.Token;
	}
	//button press signal linked from godot client
	private async void _on_button_pressed()
	{
		//delay enforced to allow for connection time using godot's built in timer node
		if (!buttonOnCooldown)
		{
			buttonTimer.Start();
			buttonOnCooldown = true;
			if (!initialized)
			{
				startButton.Text = "Disconnect";
				initialized = true;
				consoleWindow.Text += ">>> Attempting to start bot\n";	
				tokensource2 = new CancellationTokenSource();
				ct = tokensource2.Token;		
				await BotStart(startButton, consoleWindow, ct, guildList, chatWindow, commandList);			
			}	
			else if (initialized)
			{
				initialized = false;
				startButton.Text = "Connect";
				consoleWindow.Text += ">>> Bot Stopped!\n";
				guildList.Clear();
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

	static async Task BotStart(Button startButton, RichTextLabel consoleWindow, CancellationToken ct, ItemList guildList, RichTextLabel cWindow, List<string> cList)
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
						cWindow.CallDeferred("add_text", (e.Message.Author.GlobalName) + " said " + "'" + (e.Message.Content) + "'" + " in " + "(" + e.Message.Channel.Guild.Name + ")" + (e.Message.Channel.Name)+ "\n");
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

		//sets up commands from Commands.cs
		CommandsNextExtension commands = client.UseCommandsNext(new CommandsNextConfiguration(){StringPrefixes = new[] { "!" }});
		commands.RegisterCommands<Commands>();

		

		//checks for valid connection
		try 
		{
			await client.ConnectAsync();
			consoleWindow.Text += (">>> Bot Successfully connected at: " + client.GatewayInfo.Url + "\n");
		}
		catch (Exception ex)
		{
			consoleWindow.Text += (ex, ex.Message + "\n");
			consoleWindow.Text += (">>> Bot failed to connect.  This is most likely due to an incorrect token.\n");
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
	private void _on_submit_command_button_pressed()
	{
		commandList.Add(prefixField.Text);
		commandList.Add(responseField.Text);
		responseField.Clear();
		prefixField.Clear();
		consoleWindow.Text += ">>> Added command successfully!\n";
		prefixField.Visible = false;
		responseField.Visible = false;
		submitButton.Visible = false;
		addCommandButton.Visible = true;
	}
	private void _on_add_command_button_pressed()
	{
		submitButton.Visible = true;
		prefixField.Visible = true;
		responseField.Visible = true;
		addCommandButton.Visible = false;
	}

	private async void _on_change_token_button_pressed()
	{
		if (!fieldActive)
		{
			tokenField.Visible = true;
			changeTokenButton.Text = "Submit";
			fieldActive = true;
		}
		else if(fieldActive)
		{
			String token = tokenField.Text;
			JSONReader configJsonFile = new JSONReader();
			fieldActive = false;
			await configJsonFile.WriteJSON(token);
			tokenField.Visible = false;
			changeTokenButton.Text = "Change Token";
			consoleWindow.Text += ">>> Token Updated";
		}
		
	}
}




