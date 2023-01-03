using System;
using Sandbox;
using BloodLust.Player;
using Sandbox.UI;
using static Sandbox.Event;

namespace BloodLust.Weapons;

public partial class Shotgun : BloodWeapon
{
	public override string ViewModelPath => "models/weapons/v_shotgun.vmdl";
	public override string WorldModelPath => "models/weapons/w_shotgun.vmdl";
	public override HoldType HoldType => HoldType.Shotgun;
	public override float PrimaryRate => 1.15f;
	public override float SecondaryRate => 0.0f;
	public override bool PrimaryAuto => false;
	public override bool SecondaryAuto => false;
	public override float BaseDamage => 7.65f;
	public override float TimeToReload => 0.75f;
	public override float TimeToDeploy => 1.15f;

	[Net, Predicted]
	bool InterruptReload { get; set; } = false;

	[Net, Predicted]
	bool PumpAction { get; set; } = false;

	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;
		//AmmoClip = ClipSize;
	}

	[ClientRpc]
	public override void DryFire()
	{
		PlaySound( "primary_dryfire" );
	}

	public override void Simulate( IClient cl )
	{
		if ( IsReloading && Input.Pressed( InputButton.PrimaryAttack ) )
			InterruptReload = true;
		else if ( !IsReloading && PumpAction )
			PumpAction = false;
		
		base.Simulate( cl );
	}
	public override void PrimaryAttack()
	{
		/*if ( !TakeAmmo( 1 ) )
		{
			DryFire();
			return;
		}*/

		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		(Owner as AnimatedEntity).SetAnimParameter( "b_attack", true );

		DoFiringEffects();
		PlaySound( "shotgun_fire" );

		ShootBullet( 0.4f, 0.75f, 20.0f, 1.5f, 4 );
	}

	public override void SecondaryAttack()
	{

	}

	[ClientRpc]
	public void DoFiringEffects()
	{
		ViewModelEntity?.SetAnimParameter( "b_fire", true );
		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
	}

	public override void FinishReload()
	{
		var stop = InterruptReload;

		InterruptReload = false;
		IsReloading = false;

		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		/*if ( AmmoClip >= ClipSize )
			return;*/

		/*if ( Player.AmmoCount( AmmoType ) - 1 <= 0 )
		{
			var lastShell = Player.TakeAmmo( AmmoType, 1 );

			AmmoClip += lastShell;

			IsReloading = false;
			FinishReload();
			return;
		}

		var ammo = Player.TakeAmmo( AmmoType, 1 );

		if ( ammo == 0 )
			return;


		AmmoClip += ammo;

		if ( AmmoClip < ClipSize && !stop )
			Reload();
		else
		{*/
			//FinishReload();
		//}
		
	}

	public override void Reload()
	{
		//if ( AmmoClip <= 0 )
		//	PumpAction = true;

		base.Reload();

		DoReloadAnim(To.Single(Player));
	}
	[ClientRpc]
	public void DoReloadAnim()
	{
		ViewModelEntity?.SetAnimParameter( "b_reload", true );
		ViewModelEntity?.SetAnimParameter( "b_pump", false );
	}
}

