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

	[ConCmd.Admin("bl_ammo_giveall")]
	public static void CMD_GiveAllAmmo()
	{
		var player = ConsoleSystem.Caller.Pawn as BLPawn;

		if ( player == null ) return;

		player.SetAmmo( AmmoType.Pistol, player.MaxAmmo(AmmoType.Pistol) );
		player.SetAmmo( AmmoType.Rifle, player.MaxAmmo( AmmoType.Rifle ) );
		player.SetAmmo( AmmoType.Buckshot, player.MaxAmmo( AmmoType.Buckshot ) );
		player.SetAmmo( AmmoType.Crossbow, player.MaxAmmo( AmmoType.Crossbow ) );
	}
}

