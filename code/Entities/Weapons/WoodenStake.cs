using System;
using Sandbox;
using BloodLust.Player;
using Sandbox.UI;

namespace BloodLust.Weapons;

public partial class WoodenStake : BloodWeapon
{
	public override string ViewModelPath => "models/weapons/v_stake.vmdl";
	public override string WorldModelPath => "models/weapons/w_stake.vmdl";
	public override HoldType HoldType => HoldType.Fists;
	public override float PrimaryRate => 1.5f;
	public override float SecondaryRate => 0.0f;
	public override bool PrimaryAuto => true;
	public override bool SecondaryAuto => false;
	public override float BaseDamage => 15.0f;
	public override float BaseRange => 65.0f;
	public override float TimeToDeploy => 1.25f;

	public override float TimeToReload => 0.0f;

	public override void Spawn()
	{
		base.Spawn();
	}

	public override void PrimaryAttack()
	{
		base.PrimaryAttack();

		DoStaking( 15.0f );
		
		if(Game.IsServer)
			DoMeleeAnims( To.Single( Player ) );
	}

	void DoStaking(float force )
	{
		Game.SetRandomSeed( Time.Tick );

		var result = Player.GetEyeTrace( BaseRange );

		foreach ( var tr in TraceBullet( result.StartPosition, result.EndPosition ) )
		{
			tr.Surface.DoBulletImpact( tr );

			if ( !Game.IsServer ) continue;
			if ( !tr.Entity.IsValid() ) continue;

			var damageInfo = DamageInfo.FromBullet( tr.EndPosition, Player.AimRay.Forward * force, BaseDamage )
				.UsingTraceResult( tr )
				.WithAttacker( Owner )
				.WithWeapon( this );

			tr.Entity.TakeDamage( damageInfo );
		}
	}

	[ClientRpc]
	public void DoMeleeAnims()
	{
		ViewModelEntity?.SetAnimParameter( "fire", true );
	}

	public override void SecondaryAttack()
	{
		
	}
}

