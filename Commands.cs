using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

public class Commands : BaseCommandModule
{
	[Command("Hello")]
	public async Task GreetCommand(CommandContext ctx)
	{
		await ctx.RespondAsync("Hello World");
	}
}