using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class BLGame : Game
{
	public enum WinningEnum
	{
		Draw,
		Humanity,
		Vampires
	}

	public IList<BLPawn> GetTeamMembers( BLPawn.BLTeams teamType )
	{
		IList<BLPawn> teamMembers = new List<BLPawn>();

		foreach ( var clients in Client.All )
		{
			if ( clients.Pawn is BLPawn player )
			{
				if ( player.BLCurTeam == teamType )
					teamMembers.Add( player );
			}
		}

		return teamMembers;
	}

	public void CheckRoundStatus()
	{
		var curHumans = GetTeamMembers( BLPawn.BLTeams.Human ).Count + GetTeamMembers(BLPawn.BLTeams.Hunter).Count;
		var curVampire = GetTeamMembers(BLPawn.BLTeams.Vampire).Count;

		if( GameState == GameStates.Active )
		{
			if ( curHumans <= 0 )
				EndRound( WinningEnum.Vampires );
			else if ( curVampire <= 0 )
				EndRound( WinningEnum.Humanity );
		}
	}

	public void EndRound(WinningEnum winningTeam)
	{
		GameState = GameStates.Post;
	}

	private bool HasEnoughPlayers()
	{
		if ( All.OfType<BLPawn>().Count() < 2 )
			return false;

		return true;
	}
}

