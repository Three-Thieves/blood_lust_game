using Sandbox;
using Sandbox.UI.Construct;
using BloodLust.Player;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BloodLust.UI;

namespace Sandbox.Gameplay;

public partial class BLGame : GameManager
{
	public static BLGame Instance => Current as BLGame;

	public BLGame()
	{
		if(Game.IsServer)
		{

		}

		if ( Game.IsClient )
		{
			_ = new BloodHud();
		}
	}

	[Event.Hotload]
	public void HotloadGame()
	{
		if ( Game.IsServer )
		{

		}

		if ( Game.IsClient )
		{
			_ = new BloodHud();
		}
	}

	public override void ClientJoined( IClient client )
	{
		base.ClientJoined( client );

		var pawn = new BloodPawn();
		client.Pawn = pawn;
		pawn.Spawn();
	}

	public override void ClientDisconnect( IClient cl, NetworkDisconnectionReason reason )
	{
		base.ClientDisconnect( cl, reason );

		//TODO: Round condition checks
	}
}
