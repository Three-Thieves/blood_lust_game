
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public partial class BLGame : Game
{
	[Net]
	BLHud Hud { get; set; }

	public BLGame()
	{
		if(IsServer)
		{
			_ = BLGameLoopAsync();
		}

		if(IsClient)
		{
			Hud = new BLHud();
		}
	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		var pawn = new BLPawn(client);
		client.Pawn = pawn;

		pawn.Spawn();

		if( HasEnoughPlayers() )
		{
			GameState = GameStates.Start;
		}
	}
}
