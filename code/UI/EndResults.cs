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

	public EndResults()
	{
		Current = this;

		StyleSheet.Load( "/UI/Styles/endresults.scss" );
		ResultPanel = Add.Panel();
		WinnerResult = Add.Label( "Draw", "winnerText" );
		Players = Add.Label( "???", "players" );

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

		foreach ( var cl in Client.All )
		{
			if ( cl.Pawn is BLPawn player )
			{
				if ( player.LastTeam == BLPawn.BLTeams.Unknown )
					continue;

				playerList = playerList + cl.Name + " | " + player.PlayerIdentity + " | " + player.LastTeam + "\n"; 
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
