using System;
using Sandbox;
using BloodLust.Player;
using Sandbox.UI;

namespace BloodLust.Weapons;

public partial class Colt : BloodWeapon
{
	public override string ViewModelPath => "models/weapons/v_colt.vmdl";
	public override string WorldModelPath => "models/weapons/w_colt.vmdl";
	public override HoldType HoldType => HoldType.Pistol;
	public override float PrimaryRate => 7.5f;
	public override float SecondaryRate => 0.0f;
	public override bool PrimaryAuto => false;
	public override bool SecondaryAuto => false;
	public override float BaseDamage => 15.0f;
	public override float BaseRange => 65.0f;
	public override float TimeToReload => 1.45f;
	public override float TimeToDeploy => 0.95f;

	public override void Spawn()
	{
		base.Spawn();
	}

	public override void Reload()
	{
		base.Reload();

		if ( Game.IsServer )
			DoReloadAnim( To.Single( Player ), false );
	}

	public override void PrimaryAttack()
	{
		base.PrimaryAttack();

		ShootBullet( 0.05f, 25.0f, BaseDamage, 1.0f );

		PlaySound( "colt_fire" );

		if (Game.IsServer)
			DoFiringEffects( To.Single( Player ) );
	}

	[ClientRpc]
	public void DoFiringEffects()
	{
		ViewModelEntity?.SetAnimParameter( "b_fire", true );
		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
	}

	[ClientRpc]
	public void DoReloadAnim(bool wasEmpty)
	{
		ViewModelEntity?.SetAnimParameter( "b_reload", true );
		ViewModelEntity?.SetAnimParameter( "b_empty", wasEmpty);
	}

	public override void SecondaryAttack()
	{
		
	}
}

