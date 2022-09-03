﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using SandboxEditor;


[Title( "Shotgun" ), Category( "BL Weapons" ), Icon( "weapon" )]
[Library( "bl_shotgun" )]
[EditorModel( "models/weapons/w_shotgun.vmdl" )]
[HammerEntity]
partial class Shotgun : BLWeaponsBase
{
	public static readonly Model WorldModel = Model.Load( "models/weapons/w_shotgun.vmdl" );
	public override string ViewModelPath => "models/weapons/v_shotgun.vmdl";
	public override float PrimaryRate => 1;
	public override float SecondaryRate => 1;
	public override AmmoType AmmoType => AmmoType.Buckshot;
	public override int ClipSize => 7;
	public override float ReloadTime => 0.75f;
	public override int Bucket => 2;
	public override int BucketWeight => 300;

	[Net, Predicted]
	bool InterruptReload { get; set; } = false;

	[Net, Predicted]
	bool PumpAction { get; set; } = false;

	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;
		AmmoClip = ClipSize;
	}
	public override void Simulate( Client owner )
	{
		base.Simulate( owner );

		if ( IsReloading && Input.Pressed( InputButton.PrimaryAttack ) )
			InterruptReload = true;
		else if (!IsReloading && PumpAction )
		{
			PumpAction = false;
		}
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
		ShootBullet( 0.2f, 0.3f, 20.0f, 2.0f, 4 );
	}

	public override void AttackSecondary()
	{
		
	}

	[ClientRpc]
	protected override void ShootEffects()
	{
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		//Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

		ViewModelEntity?.SetAnimParameter( "fire", true );
	}

	public override void OnReloadFinish()
	{
		var stop = InterruptReload;

		InterruptReload = false;
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

			if ( AmmoClip < ClipSize && !stop )
				Reload();
			else
			{
				FinishReload();
			}
		}
	}

	public override void Reload()
	{
		if ( AmmoClip <= 0 )
			PumpAction = true;

		base.Reload();
	}

	[ClientRpc]
	public override void StartReloadEffects()
	{
		ViewModelEntity?.SetAnimParameter( "reload", true );
		ViewModelEntity?.SetAnimParameter( "pump", PumpAction );

		// TODO - player third person model reload
	}

	[ClientRpc]
	protected virtual void FinishReload()
	{
		ViewModelEntity?.SetAnimParameter( "reload_finished", true );
		//ViewModelEntity?.SetAnimParameter( "pump", false );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 3 );
		anim.SetAnimParameter( "aim_body_weight", 1.0f );
	}
}