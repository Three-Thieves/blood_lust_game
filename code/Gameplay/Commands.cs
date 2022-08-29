using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class BLGame
{
	[ConCmd.Admin("bl_spawn_weapon")]
	public static void CMD_SpawnWep(string wepName)
	{
		var player = ConsoleSystem.Caller.Pawn as BLPawn;

		if ( player == null ) return;

		var wep = TypeLibrary.Create<BLWeaponsBase>( wepName );

		if ( wep == null ) return;

		wep.Position = player.EyePosition + player.EyeRotation.Forward * 65;
	}
}

