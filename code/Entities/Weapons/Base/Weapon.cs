using System;
using Sandbox;
using BloodLust.Player;
using System.Collections.Generic;

namespace BloodLust.Weapons;

[Title( "Weapon" ), Icon( "track_changes" )]
public partial class BloodWeapon : AnimatedEntity
{
	public virtual string ViewModelPath => "";
	public virtual string WorldModelPath => "";
	public virtual HoldType HoldType => HoldType.Pistol;
	public virtual float PrimaryRate => 2.0f;
	public virtual float SecondaryRate => 1.0f;
	public virtual bool PrimaryAuto => false;
	public virtual bool SecondaryAuto => false;
	public virtual float BaseDamage => 1.0f;
	public virtual float BaseRange => 1.0f;
	public Model WorldModel => Model.Load( WorldModelPath );
	public AnimatedEntity EffectEntity => ViewModelEntity.IsValid() ? ViewModelEntity : this;
	public WeaponViewModel ViewModelEntity { get; protected set; }
	public BloodPawn Player => Owner as BloodPawn;
	[Net, Predicted] public TimeSince TimeSinceDeploy { get; set; }
	[Net, Predicted] public TimeSince TimeSincePrimaryAttack { get; set; }
	[Net, Predicted] public TimeSince TimeSinceSecondaryAttack { get; set; }
	[Net, Predicted] public TimeSince TimeSinceReload { get; set; }
	[Net, Predicted] public bool IsReloading { get; set; }
	public virtual float TimeToReload => 1.0f;
	public virtual float TimeToDeploy => 1.0f;

	public override void Spawn()
	{
		Model = WorldModel;

		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
		EnableDrawing = false;
	}

	/// <summary>
	/// Can we holster the weapon right now? Reasons to reject this could be that we're reloading the weapon..
	/// </summary>
	/// <returns></returns>
	public bool CanHolster( BloodPawn player )
	{
		if ( IsReloading ) return false;

		return true;
	}

	/// <summary>
	/// Called when the weapon gets holstered.
	/// </summary>
	public void OnHolster( BloodPawn player )
	{
		EnableDrawing = false;

		if ( Game.IsServer )
			DestroyViewModel( To.Single( player ) );
	}

	/// <summary>
	/// Can we deploy this weapon? Reasons to reject this could be that we're performing an action.
	/// </summary>
	/// <returns></returns>
	public bool CanDeploy( BloodPawn player )
	{
		return true;
	}

	/// <summary>
	/// Called when the weapon gets deployed.
	/// </summary>
	public void OnDeploy( BloodPawn player )
	{
		SetParent( player, true );
		Owner = player;

		EnableDrawing = true;
		TimeSinceDeploy = 0.0f;

		if ( Game.IsServer )
			CreateViewModel( To.Single( player ) );
	}

	public virtual bool CanPrimaryAttack()
	{
		if ( !Player.IsValid() ) return false;
		if ( IsReloading ) return false;
		if ( TimeSinceDeploy <= TimeToDeploy ) return false;

		if ( PrimaryAuto )
		{
			if ( !Input.Down( InputButton.PrimaryAttack ) ) return false;
		}
		else
		{
			if ( !Input.Pressed( InputButton.PrimaryAttack ) ) return false;
		}

		var rate = PrimaryRate;
		if ( rate <= 0 ) return true;

		return TimeSincePrimaryAttack > (1 / rate);
	}

	public virtual bool CanSecondaryAttack()
	{
		if ( !Player.IsValid() ) return false;
		if ( IsReloading ) return false;
		if ( TimeSinceDeploy <= TimeToDeploy ) return false;

		if ( SecondaryAuto )
		{
			if ( !Input.Down( InputButton.SecondaryAttack ) ) return false;
		}
		else
		{
			if ( !Input.Pressed( InputButton.SecondaryAttack ) ) return false;
		}

		var rate = SecondaryRate;
		if ( rate <= 0 ) return true;

		return TimeSinceSecondaryAttack > (1 / rate);
	}

	public virtual bool CanReload()
	{
		if ( TimeSinceDeploy <= TimeToDeploy ) return false;

		if ( IsReloading ) return false;

		if ( !Player.IsValid() || !Input.Down( InputButton.Reload ) ) return false;

		return true;
	}

	public virtual void Reload()
	{
		IsReloading = true;
		TimeSinceReload = 0.0f;
	}

	public void DoReloading()
	{
		if ( TimeSinceReload >= TimeToReload )
			FinishReload();
	}

	public virtual void FinishReload()
	{
		IsReloading = false;
	}

	[ClientRpc]
	public void CreateViewModel()
	{
		var vm = new WeaponViewModel( this );
		vm.Model = Model.Load( ViewModelPath );

		ViewModelEntity = vm;

		ViewModelEntity.Owner = Player;
		ViewModelEntity.SetParent( Player );
		ViewModelEntity.Position = Position;
		ViewModelEntity.EnableViewmodelRendering = true;
	}

	[ClientRpc]
	public void DestroyViewModel()
	{
		ViewModelEntity?.Delete();
		ViewModelEntity = null;
	}

	public virtual void PrimaryAttack()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;
	}

	public virtual void SecondaryAttack()
	{
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;
	}

	[ClientRpc]
	public virtual void DryFire()
	{

	}

	public override void Simulate( IClient cl )
	{
		if ( IsReloading )
		{
			DoReloading();
			return;
		}

		if ( CanReload() )
		{
			Reload();
		}

		if ( !Owner.IsValid() )
			return;

		if ( CanPrimaryAttack() )
		{
			using ( LagCompensation() )
			{
				PrimaryAttack();
			}
		}

		if ( !Owner.IsValid() )
			return;

		if ( CanSecondaryAttack() )
		{
			using ( LagCompensation() )
			{
				SecondaryAttack();
			}
		}
	}

	public virtual IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2.0f )
	{
		bool underWater = Trace.TestPoint( start, "water" );

		var trace = Trace.Ray( start, end )
				.UseHitboxes()
				.WithAnyTags( "solid", "player", "npc" )
				.Ignore( Owner )
				.Size( radius );

		if ( !underWater )
			trace = trace.WithAnyTags( "water" );

		var tr = trace.Run();

		if ( tr.Hit )
			yield return tr;
	}

	public virtual void ShootBullet( float spread, float force, float damage, float bulletSize, int bulletCount = 1 )
	{
		//
		// Seed rand using the tick, so bullet cones match on client and server
		//
		Game.SetRandomSeed( Time.Tick );
		var aim = Owner.AimRay;

		for ( int i = 0; i < bulletCount; i++ )
		{
			var forward = aim.Forward;
			forward += (Vector3.Random + Vector3.Random + Vector3.Random + Vector3.Random) * spread * 0.25f;
			forward = forward.Normal;

			//
			// ShootBullet is coded in a way where we can have bullets pass through shit
			// or bounce off shit, in which case it'll return multiple results
			//
			foreach ( var tr in TraceBullet( aim.Position, aim.Position + forward * 5000, bulletSize ) )
			{
				tr.Surface.DoBulletImpact( tr );

				if ( !Game.IsServer ) continue;
				if ( !tr.Entity.IsValid() ) continue;

				var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * force, damage )
					.UsingTraceResult( tr )
					.WithAttacker( Owner )
					.WithWeapon( this );

				tr.Entity.TakeDamage( damageInfo );
			}
		}
	}

	protected override void OnDestroy()
	{
		ViewModelEntity?.Delete();
	}

	[ConCmd.Server("bloodlust.weapon.spawn")]
	public static void SpawnWeapon( string wepName, bool inInv = false )
	{
		var player = ConsoleSystem.Caller.Pawn as BloodPawn;
		if ( player == null ) return;

		var wep = TypeLibrary.Create<BloodWeapon>( wepName );
		if ( wep == null ) return;

		wep.Spawn();

		if ( inInv )
			player.Inventory.AddWeapon( wep, true );
		else
			wep.Position = player.GetEyeTrace( 999.0f ).EndPosition;
	}
}
