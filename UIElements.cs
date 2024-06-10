using Godot;

//moved all the UI declarations to this class to debloat the beginning of the MainPage script.  If the project were to ever be scaled further 
//this would allow for significantly fewer lines of code
internal class UIElementsList()
{
    public Button startButton;
	public RichTextLabel consoleWindow;
	public Timer buttonTimer;
	public ItemList guildList;
	public RichTextLabel chatWindow;
	public LineEdit prefixField;
	public TextEdit responseField;
	public Button submitButton;
	public Button addCommandButton;
	public Button changeTokenButton;
	public LineEdit tokenField;
}