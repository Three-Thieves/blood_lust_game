using Sandbox.UI;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.UI.Construct;

public partial class BLChat : Panel
{
	static BLChat Current;

	public Panel Canvas { get; protected set; }
	public TextEntry Input { get; protected set; }

	public BLChat()
	{
		Current = this;

		StyleSheet.Load( "/ui/styles/blchat.scss" );

		Canvas = Add.Panel( "chat_canvas" );

		Input = Add.TextEntry( "" );
		Input.AddEventListener( "onsubmit", () => Submit() );
		Input.AddEventListener( "onblur", () => Close() );
		Input.AcceptsFocus = true;
		Input.AllowEmojiReplace = true;
	}

	void Open()
	{
		AddClass( "open" );
		Input.Focus();
	}

	void Close()
	{
		RemoveClass( "open" );
		Input.Blur();
	}

	public override void Tick()
	{
		base.Tick();

		if ( Sandbox.Input.Pressed( InputButton.Chat ) )
		{
			Open();
		}
	}

	void Submit()
	{
		Close();

		var msg = Input.Text.Trim();
		Input.Text = "";

		if ( string.IsNullOrWhiteSpace( msg ) )
			return;

		Say( msg );
	}

	public void AddEntry( string name, string message, string avatar, string lobbyState = null )
	{
		var e = Canvas.AddChild<ChatEntry>();

		e.Message.Text = message;
		e.NameLabel.Text = name;
		e.Avatar.SetTexture( avatar );

		e.SetClass( "noname", string.IsNullOrEmpty( name ) );
		e.SetClass( "noavatar", string.IsNullOrEmpty( avatar ) );

		if ( lobbyState == "ready" || lobbyState == "staging" )
		{
			e.SetClass( "is-lobby", true );
		}
	}


	[ConCmd.Client( "chat_add", CanBeCalledFromServer = true )]
	public static void AddChatEntry( string name, string message, string avatar = null, string lobbyState = null )
	{
		Current?.AddEntry( name, message, avatar, lobbyState );

		// Only log clientside if we're not the listen server host
		if ( !Global.IsListenServer )
		{
			Log.Info( $"{name}: {message}" );
		}
	}

	[ConCmd.Client( "chat_addinfo", CanBeCalledFromServer = true )]
	public static void AddInformation( string message, string avatar = null )
	{
		Current?.AddEntry( null, message, avatar );
	}

	[ConCmd.Server( "say" )]
	public static void Say( string message )
	{
		Assert.NotNull( ConsoleSystem.Caller );

		// todo - reject more stuff
		if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
			return;

		string identityName = "";

		if( ConsoleSystem.Caller.Pawn is BLPawn consolePlayer)
		{
			identityName = consolePlayer.PlayerIdentity;
			if ( consolePlayer.CurTeam == BLPawn.BLTeams.Spectator )
				return;
		}

		if ( Local.Pawn is BLPawn player)
		{
			identityName = player.PlayerIdentity;
			if ( player.CurTeam == BLPawn.BLTeams.Spectator )
				return;
		}

		Log.Info( $"{ConsoleSystem.Caller}: {message}" );
		AddChatEntry( To.Everyone, identityName, message );
	}
}
