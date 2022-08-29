
using Sandbox;
using Sandbox.UI.Construct;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public partial class BLGame : Game
{

	public BLGame()
	{
		if(IsServer)
		{
			
		}

		if(IsClient)
		{
			_ = new BLHud();
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
		}
	}

	public override void ClientDisconnect( Client cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		CheckRoundStatus();
	}

	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		var pawn = new BLPawn(client);
		client.Pawn = pawn;

		pawn.Spawn();

		if ( HasEnoughPlayers() && GameState == GameStates.Waiting )
		{
			StateTimer = 10;
			GameState = GameStates.Idle;
		}
	}
}
