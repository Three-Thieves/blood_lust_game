using Sandbox.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI.Construct;

public class EndResults : Panel
{
	public static EndResults Current;
	public Panel ResultPanel;
	public Label WinnerResult;
	public Label Players;
	public Panel ResultsList;

	public class PlayerResultsList : Panel
	{
		public Label _name;
		public Label _ident;
		public Label _lastTeam;
		public PlayerResultsList(string name, string plrIdent, string plrLastTeam)
		{
			_name = Add.Label(name, "name");
			_ident = Add.Label( plrIdent, "nameGame" );
			_lastTeam = Add.Label( plrLastTeam, "ident" );
		}
	}

	public EndResults()
	{
		Current = this;

		StyleSheet.Load( "/UI/Styles/endresults.scss" );
		ResultPanel = Add.Panel();
		WinnerResult = Add.Label( "Draw", "winnerText" );
		Players = Add.Label( "???", "players" );
		ResultsList = Add.Panel( "plrListEnd" );

		_ = Lifetime();
	}

	public void SetResults( BLGame.WinningEnum winners )
	{
		switch(winners)
		{
			case BLGame.WinningEnum.Humanity:
				WinnerResult.SetText("Humanity has survived the night");
				break;
			case BLGame.WinningEnum.Vampires:
				WinnerResult.SetText("Vampires have prevailed");
				break;
		}

		string playerList = "";

		ResultsList.DeleteChildren();

		foreach ( var cl in Client.All )
		{
			if ( cl.Pawn is BLPawn player )
			{
				if ( player.LastTeam == BLPawn.BLTeams.Unknown )
					continue;

				ResultsList.AddChild( new PlayerResultsList( cl.Name , player.PlayerIdentity , player.LastTeam.ToString() ) );
			}
		}

		Players.SetText( playerList );
	}

	async Task Lifetime()
	{
		await Task.DelaySeconds( 6 );
		AddClass( "hide" );
		await Task.DelaySeconds( 2 );
		Delete();
	}
}
