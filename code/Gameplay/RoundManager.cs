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
		var curVampire = GetTeamMembers( BLPawn.BLTeams.Vampire ).Count;
	
		if( GameState == GameStates.Active )
		{
			if ( curHumans <= 0 )
				EndRound( WinningEnum.Vampires );
			
			if ( curVampire <= 0 )
				EndRound( WinningEnum.Humanity );
		}
	}

	[ClientRpc]
	public void PlayGameplaySounds(string sound)
	{
		PlaySound( sound );
	}

	public void StartRound()
	{
		PlayGameplaySounds( To.Everyone, "roundstart" );

		All.OfType<BLPawn>().ToList().ForEach( x =>
		{
			x.ClearAmmo();
			x.Backpack.DeleteContents();
			x.Respawn();
		} );

		Map.Reset( BLCleanupFilter );

		int vampLimit = 1; //Math.Abs(Client.All.Count / 2);
		int hunterLimit = Math.Abs( Client.All.Count / 4 );

		//Randomly select spectator players to join the vampires

		for ( int i = 0; i < vampLimit; i++ )
		{
			bool check = false;
			
			while( !check )
			{
				var randClient = Client.All.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

				if ( randClient.Pawn is BLPawn player && player.BLCurTeam == BLPawn.BLTeams.Spectator )
				{
					player.UpdatePlayerTeam( BLPawn.BLTeams.Vampire );
					check = true;
				}
			}
		}
		
		/*while ( vampLimit > 0 )
		{
			foreach ( var client in Client.All )
			{
				if ( client.Pawn is BLPawn player && player.BLCurTeam == BLPawn.BLTeams.Spectator && client.Id == Rand.Int( 1, Client.All.Count ) )
				{
					player.UpdatePlayerTeam( BLPawn.BLTeams.Vampire );
					vampLimit--;
				}
			}
		}*/

		//If we have enough players for hunters, select random players in spectator
		//TODO, add hunters

		//The rest of the players are set to humans from spectator
		foreach ( var client in Client.All )
		{
			if ( client.Pawn is BLPawn player && player.BLCurTeam == BLPawn.BLTeams.Spectator )
				player.UpdatePlayerTeam( BLPawn.BLTeams.Human );
		}

		StateTimer = 10.0f;
		GameState = GameStates.Start;
	}

	[Event.Tick.Server]
	public void SimulateRound()
	{
		if ( !HasEnoughPlayers() )
			return;

		if ( StateTimer > 0.0f )
			return;
		
		if ( GameState == GameStates.Idle )
		{
			StartRound();
		}

		if(GameState == GameStates.Start)
		{
			StateTimer = 5 * 60;
			GameState = GameStates.Active;
			return;
		}

		if ( GameState == GameStates.Active )
		{
			EndRound( WinningEnum.Humanity );
		}

		if ( GameState == GameStates.Post )
			StartRound();
		
	}

	public void EndRound(WinningEnum winningTeam)
	{
		if(winningTeam == WinningEnum.Draw)
			Log.Info( "Its a draw, this shouldn't happen" );
		else
			Log.Info( $"{ winningTeam } win");

		switch(winningTeam)
		{
			case WinningEnum.Humanity:
				PlayGameplaySounds( To.Everyone, "roundwin_humanity" );
				break;

			case WinningEnum.Vampires:
				PlayGameplaySounds( To.Everyone, "roundwin_vampire" );
				break;
		}

		StateTimer = 10;
		GameState = GameStates.Post;

		foreach ( Client cl in Client.All )
		{
			if(cl.Pawn is BLPawn player)
				player.UpdatePlayerTeam( BLPawn.BLTeams.Spectator );
		}

		CurRound++;

		/*if ( CurRound >= MaxRounds && IsServer )
		{
			Log.Info( "ROUNDS EXCEED MAX, starting map vote" );
			GameState = GameStates.MapVote;

			var mapVote = new MapVoteEntity();
			mapVote.VoteTimeLeft = 20.0f;

			StateTimer = mapVote.VoteTimeLeft;
			Global.ChangeLevel( mapVote.WinningMap );
		}*/
	}

	private bool HasEnoughPlayers()
	{
		if ( Client.All.Count < 2 )
			return false;

		return true;
	}

	public static bool BLCleanupFilter( string className, Entity ent )
	{
		// Basic Source engine stuff
		if ( className == "player" || className == "worldent" || className == "worldspawn" || className == "soundent" || className == "player_manager" )
		{
			return false;
		}

		if ( className == "snd_event_point" )
			return false;

		// When creating entities we only have classNames to work with..
		// The filtered entities below are created through code at runtime, so we don't want to be deleting them
		if ( ent == null || !ent.IsValid ) return true;

		// Gamemode entity
		if ( ent is GameBase ) return false;

		// HUD entities
		if ( ent.GetType().IsBasedOnGenericType( typeof( HudEntity<> ) ) ) return false;

		// Player related stuff, clothing and weapons
		foreach ( var cl in Client.All )
		{
			if ( ent.Root == cl.Pawn ) return false;
		}

		// Do not delete view model
		if ( ent is BaseViewModel ) return false;

		return true;
	}
}

