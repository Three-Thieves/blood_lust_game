using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class BLGame
{
	public enum WinningEnum
	{
		Draw,
		Humanity,
		Vampires
	}

	public List<string> takenMaleNames;
	public List<string> takenFemaleNames;
	public List<string> takenHunterNames;

	public IList<BLPawn> GetTeamMembers( BLPawn.BLTeams teamType )
	{
		IList<BLPawn> teamMembers = new List<BLPawn>();

		foreach ( var clients in Client.All )
		{
			if ( clients.Pawn is BLPawn player )
			{
				if ( player.CurTeam == teamType )
					teamMembers.Add( player );
			}
		}

		return teamMembers;
	}

	public void CheckRoundStatus()
	{
		var curHumans = GetTeamMembers( BLPawn.BLTeams.Human ).Count + GetTeamMembers(BLPawn.BLTeams.Hunter).Count;
		var curVampire = GetTeamMembers( BLPawn.BLTeams.Vampire ).Count;
	
		if ( curHumans <= 0 )
			EndRound( WinningEnum.Vampires );
			
		if ( curVampire <= 0 )
			EndRound( WinningEnum.Humanity );
	}

	[ClientRpc]
	public void PlayGameplaySounds(string sound)
	{
		PlaySound( sound );
	}

	public void StartRound()
	{
		takenMaleNames.Clear();
		takenFemaleNames.Clear();
		takenHunterNames.Clear();

		PlayGameplaySounds( To.Everyone, "roundstart" );

		All.OfType<BLPawn>().ToList().ForEach( x =>
		{
			x.Respawn();
			x.GiveHands();
			x.UpdatePlayerTeam( BLPawn.BLTeams.Spectator );
		} );

		Map.Reset( BLCleanupFilter );

		int vampLimit = Math.Abs( Client.All.Count / 6) + 1;
		int hunterLimit = Math.Abs( Client.All.Count / 6 );

		//Randomly select spectator players to join the vampires

		for ( int i = 0; i < vampLimit; i++ )
		{
			bool check = false;

			while ( !check )
			{
				var randClient = Client.All.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

				if ( randClient.Pawn is BLPawn player && player.CurTeam == BLPawn.BLTeams.Spectator )
				{
					player.UpdatePlayerTeam( BLPawn.BLTeams.Vampire );
					player.LastTeam = BLPawn.BLTeams.Vampire;
					player.SetUpVampire();
					check = true;
				}
			}
		}

		//If we have enough players for hunters, select random players in spectator

		if ( hunterLimit > 0 )
		{
			for ( int i = 0; i < hunterLimit; i++ )
			{
				bool check = false;

				while ( !check )
				{
					var randClient = Client.All.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

					if ( randClient.Pawn is BLPawn player && player.CurTeam == BLPawn.BLTeams.Spectator )
					{
						player.UpdatePlayerTeam( BLPawn.BLTeams.Hunter );
						player.LastTeam = BLPawn.BLTeams.Hunter;
						player.SetUpHunter();
						check = true;
					}
				}
			}
		}
		//The rest of the players are set to humans from spectator
		foreach ( var client in Client.All )
		{
			if ( client.Pawn is BLPawn player && player.CurTeam == BLPawn.BLTeams.Spectator )
			{
				player.UpdatePlayerTeam( BLPawn.BLTeams.Human );
				player.LastTeam = BLPawn.BLTeams.Human;
			}
		}

		All.OfType<BLPawn>().ToList().ForEach( x => x.SetIdentity() );

		StateTimer = 8.0f;
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
			return;
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
			return;
		}

		if ( GameState == GameStates.Post )
			StartRound();
		
	}

	public void EndRound(WinningEnum winningTeam)
	{
		if(winningTeam == WinningEnum.Draw)
			Log.Info( "Draw" );
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

		SetEndResultsClient( To.Everyone, winningTeam );

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

	[ClientRpc]
	public void SetEndResultsClient( WinningEnum winningTeam )
	{
		BLHud.Current.AddChild<EndResults>();
		EndResults.Current.SetResults(winningTeam);
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

