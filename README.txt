SimpleBot is a user-friendly self-hosted discord chat bot built on the godot game engine!

It features message logging as well as the ability to add simple prefix based chat commands, all packaged within an easy to understand interface.

To use, all you need to do is:

1: Download the latest version from the releases tab.
2: Unzip the files into a folder anywhere.
3: Run SimpleBot.exe
4: Go to https://discord.com/developers/applications and create a new application (or use an existing one)
5: Create an invite for your application under the OAuth2 tab.  Just select Bot and Administrator if you're not sure what to pick here, otherwise the bot really only needs all message based intents at this time.
6: Under the Bot tab in the developer discord, check all Privileged Gateway Intents.
7: Under Bot, create a token, and then copy and paste the token into the token field in the first window, then click submit.
8: Click Connect and if you did everything right, you should see a success message in the bottom right window as well as the server list populate with the servers your bot is in!

To add custom commands:
1: On the main page after the bot has been connected press the "Add Command" button.
2: Fill out the required prefix and response fields.  Prefix is what you and your users will type(eg "!type"), and response is what your bot will say in response.
3: Once the fields are filled, simply press submit!

If for whatever reason your token gets reset and you need to update it, just use the Change Token button in the upper right.