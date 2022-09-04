using System.ComponentModel;
using Sandbox;
using SandboxEditor;

[Title( "Winchester Rifle" ), Category( "Weapons" ), Icon( "weapon" )]
[Library( "bl_rifle" )]
[EditorModel( "models/weapons/w_rifle.vmdl" )]
[HammerEntity]
partial class Winchester : BLWeaponsBase
{
	public static readonly Model WorldModel = Model.Load( "models/weapons/w_rifle.vmdl" );
	public override string ViewModelPath => "models/weapons/v_rifle.vmdl";
	public override float PrimaryRate => 0.95f;
	public override float SecondaryRate => -1;
	public override AmmoType AmmoType => AmmoType.Rifle;
	public override int ClipSize => 7;
	public override float ReloadTime => 1.5f;
	public override int Bucket => 3;
	public override int BucketWeight => 200;
	public override SlotEnum Slot => SlotEnum.Primary;

	[Net, Predicted]
	public bool StopReloading { get; set; }

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
			StopReloading = true;
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
		PlaySound( "rust_pumpshotgun.shoot" );

		//
		// Shoot the bullets
		//
		ShootBullet( 0.05f, 0.85f, 45.0f, 2.0f );
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
		var stop = StopReloading;

		StopReloading = false;
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
