
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public partial class BLGame : Game
{
	public static BLGame Instance { get; private set; } = Current as BLGame;

	public BLGame()
	{
		if(IsServer)
		{
			takenMaleNames = new();
			takenFemaleNames = new();
			takenHunterNames = new();

			MaxRounds = 8;
		}

		if(IsClient)
		{
			_ = new BLHud();
			postProcess = new StandardPostProcess();
			PostProcess.Add( postProcess );
		}
	}

	public override void DoPlayerSuicide( Client cl )
	{
		return;
	}

	[Event.Hotload]
	public void Hotload()
	{
		if ( IsClient )
		{
			_ = new BLHud();
			PostProcess.Remove( postProcess );

			postProcess = new StandardPostProcess();
			PostProcess.Add( postProcess );
		}
	}

	public override void PostLevelLoaded()
	{
		//AdjustMapEnvironment();
	}

	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		CheckRoundStatus();
	}

	public override void MoveToSpawnpoint( Entity pawn )
	{
		SpawnPoint spawnpoint = null;
		int attempts = 3;

		while(spawnpoint == null)
		{
			spawnpoint = All.OfType<SpawnPoint>()
						.OrderBy( x => Guid.NewGuid() )
						.FirstOrDefault();

			if ( FindInBox( spawnpoint.WorldSpaceBounds ).FirstOrDefault() is BLPawn )
			{
				if ( attempts <= 0 )
					break;

				spawnpoint = null;
				attempts--;

			}
		}

		pawn.Transform = spawnpoint.Transform;
	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		var pawn = new BLPawn();
		client.Pawn = pawn;

		pawn.Spawn();

		MoveToSpawnpoint( pawn );

		if ( HasEnoughPlayers() && GameState == GameStates.Waiting )
		{
			StateTimer = 10;
			GameState = GameStates.Idle;
		}
	}
}
