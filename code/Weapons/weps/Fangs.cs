using Sandbox;
using System.ComponentModel;
using SandboxEditor;

[Library( "bl_fangs" )]
[HideInEditor]
partial class Fangs : BLWeaponsBase
{
	public static Model WorldModel = null;
	public override string ViewModelPath => "models/first_person/first_person_arms.vmdl";
	public override float PrimaryRate => 2.0f;
	public override float SecondaryRate => 1.0f;
	public override float ReloadTime => 3.0f;
	public override AmmoType AmmoType => AmmoType.None;
	public override int ClipSize => 0;
	public override int Bucket => 0;
	public override int BucketWeight => 100;
	public override bool IsDroppable => false;

	public override void Spawn()
	{
		SetModel( "" );
	}

	public override void AttackPrimary()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		MeleeAttack();
	}

	private bool MeleeAttack()
	{
		var forward = Owner.EyeRotation.Forward;
		forward = forward.Normal;

		bool hit = false;

		foreach ( var tr in TraceBullet( Owner.EyePosition, Owner.EyePosition + forward * 80, 20.0f ) )
		{
			if ( !tr.Entity.IsValid() ) continue;

			if( !tr.Entity.IsWorld )
				tr.Surface.DoBulletImpact( tr );

			hit = true;

			if ( !IsServer ) continue;

			if(tr.Entity is BLRagdoll body)
			{
				if ( body.CorpseTeam == BLPawn.BLTeams.Vampire )
					continue;

				if ( body.BloodAmount <= 0 )
					continue;

				var biter = Owner as BLPawn;

				body.BloodAmount -= 10.0f;

				biter.Health += 20.0f;
				biter.Health = biter.Health.Clamp( 1, biter.MaxHealth );
				biter.IncreaseBloodBar( 32.0f );
			}

			using ( Prediction.Off() )
			{
				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100, 25 )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				damageInfo.Damage = 10.0f;

				switch( tr.HitboxIndex )
				{
					case 3:
						damageInfo.Damage += 15.0f;
						break;
					case 5:
						damageInfo.Damage += 15.0f;
						break;
					case 7:
						damageInfo.Damage += 15.0f;
						break;
				}


				tr.Entity.TakeDamage( damageInfo );
			}
		}

		return hit;
	}

	public override void CreateViewModel()
	{
		Host.AssertClient();

		if ( string.IsNullOrEmpty( ViewModelPath ) )
			return;

		ViewModelEntity = new BLViewModel
		{
			Position = Position,
			Owner = Owner,
			EnableViewmodelRendering = true,
		};

		ViewModelEntity.SetModel( ViewModelPath );
		ViewModelEntity.SetAnimGraph( "models/first_person/first_person_arms_punching.vanmgrph" );
	}

	public override void SimulateAnimator( PawnAnimator anim )
	{
		anim.SetAnimParameter( "holdtype", 0 ); // TODO this is shit
		anim.SetAnimParameter( "aim_body_weight", 1.0f );

		if ( Owner.IsValid() )
		{
			ViewModelEntity?.SetAnimParameter( "b_grounded", Owner.GroundEntity.IsValid() );
			ViewModelEntity?.SetAnimParameter( "aim_pitch", Owner.EyeRotation.Pitch() );
		}
	}
}
