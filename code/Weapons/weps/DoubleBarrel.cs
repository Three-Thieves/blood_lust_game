﻿using System.ComponentModel;
using Sandbox;
using SandboxEditor;

[Title( "Double Barrel Shotgun" ), Category("Weapons"), Icon("weapon")]
[Library( "bl_doublebarrel") ]
[EditorModel( "models/weapons/w_doublebarrel.vmdl" )]
[HammerEntity]
partial class DoubleBarrel : BLWeaponsBase
{
	public static readonly Model WorldModel = Model.Load( "models/weapons/w_doublebarrel.vmdl" );
	public override string ViewModelPath => "models/weapons/v_doublebarrel.vmdl";
	public override float PrimaryRate => 0.65f;
	public override float SecondaryRate => 0.35f;
	public override AmmoType AmmoType => AmmoType.Buckshot;
	public override int ClipSize => 2;
	public override float ReloadTime => 1.35f;
	public override int Bucket => 2;
	public override int BucketWeight => 200;
	public override SlotEnum Slot => SlotEnum.Primary;
	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;
		AmmoClip = ClipSize;
	}

	public override void Simulate( Client owner )
	{
		base.Simulate( owner );
	}

	[ClientRpc]
	public override void DryFire()
	{
		PlaySound( "primary_dryfire" );
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( !TakeAmmo( 1 ) )
		{
			DryFire();
			return;
		}

		(Owner as AnimatedEntity).SetAnimParameter( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		ShootEffects();
		PlaySound( "shotgun_fire" );

		//
		// Shoot the bullets
		//
		ShootBullet( 0.2f, 0.3f, 15.0f, 2.0f, 4 );
	}

	public override void AttackSecondary()
	{
		TimeSincePrimaryAttack = -0.5f;
		TimeSinceSecondaryAttack = -0.5f;

		if ( !TakeAmmo( 2 ) )
		{
			DryFire();
			return;
		}

		(Owner as AnimatedEntity).SetAnimParameter( "b_attack", true );

		//
		// Tell the clients to play the shoot effects
		//
		DoubleShootEffects();
		PlaySound( "rust_pumpshotgun.shootdouble" );

		//
		// Shoot the bullets
		//
		ShootBullet( 0.4f, 0.75f, 15.0f, 2.0f, 8 );
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		//Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

		ViewModelEntity?.SetAnimParameter( "fire", true );
	}

	[ClientRpc]
	protected virtual void DoubleShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		ViewModelEntity?.SetAnimParameter( "fire_double", true );
	}

	public override void OnReloadFinish()
	{
		//var stop = StopReloading;

		//StopReloading = false;
		IsReloading = false;

		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		if ( AmmoClip >= ClipSize )
			return;

		if ( Owner is BLPawn player )
		{
			var ammo = player.TakeAmmo( AmmoType, 1 );
			if ( ammo == 0 )
				return;

			AmmoClip += ammo;

			if ( AmmoClip < ClipSize )
			{
				Reload();
			}
			else
			{
				FinishReload();
			}
		}
	}

	[ClientRpc]
	protected virtual void FinishReload()
	{
		ViewModelEntity?.SetAnimParameter( "reload_finished", true );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 3 );
		anim.SetAnimParameter( "aim_body_weight", 1.0f );
	}
}
