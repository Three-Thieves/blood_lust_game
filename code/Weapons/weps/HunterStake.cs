using Sandbox;
using System.ComponentModel;
using SandboxEditor;

[Library( "bl_stake_hunter" )]
[HideInEditor]
partial class HunterStake : BLWeaponsBase
{
	public static Model WorldModel = Model.Load( "models/weapons/w_stake.vmdl" );
	public override string ViewModelPath => "models/weapons/v_stake.vmdl";
	public override float PrimaryRate => 2.0f;
	public override float SecondaryRate => 1.0f;
	public override float ReloadTime => 3.0f;
	public override AmmoType AmmoType => AmmoType.None;
	public override int ClipSize => 0;
	public override int BucketWeight => 200;
	public override int Bucket => 0;

	public override SlotEnum Slot => SlotEnum.Melee;
	public override void Spawn()
	{
		base.Spawn();

		Model = WorldModel;
		AmmoClip = 0;
	}

	public override bool CanPrimaryAttack()
	{
		return base.CanPrimaryAttack();
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		Rand.SetSeed( Time.Tick );

		var forward = Owner.EyeRotation.Forward;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * 0.1f;
		forward = forward.Normal;

		foreach ( var tr in TraceBullet( Owner.EyePosition, Owner.EyePosition + forward * 70, 15 ) )
		{
			tr.Surface.DoBulletImpact( tr );

			if ( !IsServer ) continue;
			if ( !tr.Entity.IsValid() ) continue;

			var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 32, 25 )
				.UsingTraceResult( tr )
				.WithAttacker( Owner )
				.WithWeapon( this );

			damageInfo.Damage = 15.0f;

			tr.Entity.TakeDamage( damageInfo );

			if ( tr.Entity is BLRagdoll ragdoll && IsServer )
			{
				if ( ragdoll.CorpseTeam == BLPawn.BLTeams.Vampire )
					ragdoll.CorpseOwner.Staked();
				else
					ragdoll.IsStaked = true;
			}
		}

		ViewModelEntity?.SetAnimParameter( "attack_has_hit", true );
		ViewModelEntity?.SetAnimParameter( "attack", true );
		ViewModelEntity?.SetAnimParameter( "holdtype_attack", false ? 2 : 1 );
		
		if ( Owner is BLPawn player )
		{
			player.SetAnimParameter( "b_attack", true );
			ViewModelEntity?.SetAnimParameter( "fire", true );
		}
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 5 ); // TODO this is shit
		anim.SetAnimParameter( "aim_body_weight", 1.0f );

		if ( Owner.IsValid() )
		{
			ViewModelEntity?.SetAnimParameter( "b_grounded", Owner.GroundEntity.IsValid() );
			ViewModelEntity?.SetAnimParameter( "aim_pitch", Owner.EyeRotation.Pitch() );

		}
	}
}
