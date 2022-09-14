using Sandbox;
using SandboxEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Library( "bl_weapon_spawner" ), Title( "Weapon Spawner" ), Category("Entity")]
[Description( "A random weapon entity spawnpoint with a chance of spawning per round" ), Icon( "Location_On" )]
[HammerEntity]
public class WeaponSpawnpoint : Entity
{
	[Property, Title( "Chance for Spawn" )]
	public double BaseChance { get; set; } = 50.0;

	public override void Spawn()
	{
		base.Spawn();
	}

	public bool ShouldSpawn()
	{
		BaseChance = BaseChance.Clamp( 1, 100 );

		return BaseChance >= Rand.Int( 1, 100 );
	}

	public void SpawnEntity()
	{
		if ( !ShouldSpawn() ) return;

		string wepToSpawn = "";

		switch ( Rand.Int( 1, 4 ) )
		{
			case 1:
				wepToSpawn = "DoubleBarrel";
				break;
			case 2:
				wepToSpawn = "Shotgun";
				break;
			case 3:
				wepToSpawn = "Winchester";
				break;
			case 4:
				wepToSpawn = "Stake";
				break;		
		}

		if ( string.IsNullOrEmpty( wepToSpawn ) ) return;

		BLWeaponsBase wep = TypeLibrary.Create<BLWeaponsBase>( wepToSpawn );

		wep.Transform = Transform;
		wep.Spawn();
	}
}

[Library( "bl_ammo_spawner" ), Title( "Ammo Spawner" ), Category( "Entity" )]
[Description( "A random ammo entity spawnpoint with a chance of spawning per round" ), Icon( "Location_On" )]
[HammerEntity]
public class AmmoSpawnpoint : Entity
{
	[Property, Title( "Chance for Spawn" )]
	public double BaseChance { get; set; } = 50.0;

	public override void Spawn()
	{
		base.Spawn();
	}

	public bool ShouldSpawn()
	{
		BaseChance = BaseChance.Clamp( 1, 100 );

		return BaseChance >= Rand.Int( 1, 100 );
	}

	public void SpawnEntity()
	{
		if ( !ShouldSpawn() ) return;

		string ammoToSpawn = "";

		switch ( Rand.Int( 1, 3 ) )
		{
			case 1:
				ammoToSpawn = "PistolBox";
				break;
			case 2:
				ammoToSpawn = "ShotgunBox";
				break;
			case 3:
				ammoToSpawn = "RifleBox";
				break;
		}

		if ( string.IsNullOrEmpty( ammoToSpawn ) ) return;

		BLAmmoBase ammo = TypeLibrary.Create<BLAmmoBase>( ammoToSpawn );

		ammo.Transform = Transform;
		ammo.Spawn();
	}
}
