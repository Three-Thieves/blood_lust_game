using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;


partial class BLPawn : Player
{
	TimeSince timeSinceDropped;
	DamageInfo lastDMGInfo;
	TimeUntil timeUntilResurrection;
	public TimeSince TimeSinceDamage { get; private set; } = 1.0f;

	float timeToResurrect = 15.0f;

	[Net]
	public float MaxHealth { get; set; } = 100;

	public BLInventory Backpack { get; protected set; }

	public BLPawn()
	{
		Backpack = new BLInventory( this );
	}

	public override void Spawn()
	{
		CreateHull();
		oldClothing = new List<ModelEntity>();

		Tags.Add( "blplayer" );
		EnableLagCompensation = true;

		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new WalkController()
		{
			SprintSpeed = 130.0f,
		};

		
		Animator = new StandardPlayerAnimator();

		BLCurTeam = BLTeams.Spectator;

		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		if (BLGame.CurrentState != BLGame.GameStates.Active && BLGame.CurrentState != BLGame.GameStates.Start)
		{
			CameraMode = new FirstPersonCamera();
			EnableDrawing = true;
			EnableAllCollisions = true;
		} 
		else
		{
			CameraMode = new SpectatorCamera();
			EnableDrawing = false;
			EnableAllCollisions = false;
		}

		CreatePlayerFlashlight();
		BLGame.Instance.MoveToSpawnpoint( this );
	}

	public void GiveHands()
	{
		Host.AssertServer();

		var hands = new Hands();
		if ( Backpack.Contains( hands ) )
		{
			hands.Delete();
			return;
		}

		Backpack.Add( hands );
	}

	public override void Respawn()
	{
		Host.AssertServer();

		LifeState = LifeState.Alive;
		Health = 100;
		Velocity = Vector3.Zero;
		WaterLevel = 0;

		CreateHull();
		ClearAmmo();

		BLGame.Instance.MoveToSpawnpoint( this );
		ResetInterpolation();

		hat?.Delete();
		hat = null;

		Backpack.DeleteContents();

		RemoveAllDecals();

		CameraMode = new FirstPersonCamera();
		
		if ( wFlash.IsValid )
			DeleteFlashlight();

		CreatePlayerFlashlight();

		if ( BLGame.CurrentState == BLGame.GameStates.Active )
		{
			EnableDrawing = false;
			EnableAllCollisions = false;
		} 
		else
		{
			EnableDrawing = true;
			EnableAllCollisions = true;
		}

		Skills.Clear();
	}

	public void Resurrect()
	{
		CameraMode = new FirstPersonCamera();

		EnableDrawing = true;
		EnableAllCollisions = true;

		LifeState = LifeState.Alive;
		Health = 25;

		if ( Corpse == null )
			return;

		Position = Corpse.Position;

		Corpse.Delete();
		Corpse = null;
	}

	public override void BuildInput( InputBuilder input )
	{
		if ( BLGame.CurrentState == BLGame.GameStates.MapVote )
		{
			input.ViewAngles = input.OriginalViewAngles;
			return;
		};

		base.BuildInput( input );
	}

	public override void Simulate( Client cl )
	{
		if ( BLGame.CurrentState == BLGame.GameStates.MapVote || BLGame.CurrentState == BLGame.GameStates.Start )
			return;

		if (BLCurTeam == BLTeams.Vampire && LifeState == LifeState.Dead && IsServer )
		{
			if(Input.Pressed(InputButton.PrimaryAttack) && timeUntilResurrection < 0.0f)
				Resurrect();

			return;
		}

		//SelectWeapon();
		if ( Input.ActiveChild != null )
		{
			ActiveChild = Input.ActiveChild;
		}

		TickPlayerUse();
		SimulateFlashlight();
		
		if(BLCurTeam != BLTeams.Vampire)
			DoHeartbeat( To.Single( this ), Health > 0 && Health <= 30.0f );

		if ( Input.Pressed( InputButton.Drop ) )
		{	
			var dropped = Backpack.DropActive();

			if ( dropped != null )
			{
				if ( dropped.PhysicsGroup != null )
				{
					dropped.PhysicsGroup.Velocity = Velocity + (EyeRotation.Forward + EyeRotation.Up) * 300;
				}

				timeSinceDropped = 0;
			}
		}

		var controller = GetActiveController();
		controller?.Simulate( cl, this, GetActiveAnimator() );

		SimulateActiveChild( cl, ActiveChild );
	}

	public override void StartTouch( Entity other )
	{

		if ( BLGame.CurrentState != BLGame.GameStates.Active )	return;

		if ( timeSinceDropped < 1 ) return;

		if ( other is BLWeaponsBase wep )
		{
			if ( wep is HunterStake && BLCurTeam == BLTeams.Vampire )
				return;

			if ( wep is Stake && BLCurTeam == BLTeams.Hunter )
				return;

			Backpack.Add( wep );
		}

		base.StartTouch(other);
	}

	void BecomeRagdoll( Vector3 force, int forceBone )
	{
		var ent = new BLRagdoll();

		ent.Position = Position;
		ent.Rotation = Rotation;
		ent.UsePhysicsCollision = true;
		ent.PhysicsEnabled = true;
		ent.EnableTraceAndQueries = true;

		ent.CorpseOwner = this;
		ent.CorpseTeam = BLCurTeam;
		ent.CorpseIdent = Identity;
		ent.CorpseName = PlayerIdentity;

		ent.SetModel( GetModelName() );
		ent.CopyBonesFrom( this );
		ent.TakeDecalsFrom( this );
		ent.SetRagdollVelocityFrom( this );

		// Copy the clothes over
		foreach ( var child in Children )
		{
			if ( child is ModelEntity e && child is not BLWeaponsBase)
			{
				var clothing = new ModelEntity();
				clothing.Model = e.Model;
				clothing.SetParent( ent, true );
			}
		}

		ent.PhysicsGroup.AddVelocity( force );

		if ( forceBone >= 0 )
		{
			var body = ent.GetBonePhysicsBody( forceBone );
			if ( body != null )
			{
				body.ApplyForce( force * 1000 );
			}
			else
			{
				ent.PhysicsGroup.AddVelocity( force );
			}
		}

		Corpse = ent;
	}

	Sound nearDeathSnd;

	public override void TakeDamage( DamageInfo info )
	{
		if ( BLGame.CurrentState != BLGame.GameStates.Active )
			return;

		switch(info.BoneIndex)
		{
			case 11:
				info.Damage *= 2;
				break;
		}

		lastDMGInfo = info;

		TookDamage( To.Single( this ) );
		TimeSinceDamage = 0;

		base.TakeDamage( info );
	}

	[ClientRpc]
	public void DoHeartbeat(bool lowHP)
	{
		if ( !nearDeathSnd.Finished )
			return;

		if ( lowHP  )
			nearDeathSnd = PlaySound( "heartbeat" );
		else
			nearDeathSnd.Stop();
	}

	[ClientRpc]
	public void TookDamage()
	{
		//DebugOverlay.Sphere( pos, 10.0f, Color.Red, true, 10.0f );

		DamageIndicator.Current?.OnHit();
	}

	protected override void UseFail()
	{
		
	}

	public override void OnKilled()
	{
		base.OnKilled();

		EnableDrawing = false;
		EnableAllCollisions = false;

		if ( BLCurTeam == BLTeams.Vampire )
			CameraMode = new SpectateRagdollCamera();
		else
			CameraMode = new SpectatorCamera();

		if(Backpack.Count() > 0)
			Backpack.DropContents();

		ClearAmmo();

		BecomeRagdoll( lastDMGInfo.Force, lastDMGInfo.HitboxIndex );

		if(BLCurTeam == BLTeams.Human || BLCurTeam == BLTeams.Hunter)
		{
			UpdatePlayerTeam( BLTeams.Spectator );
			BLGame.GameCurrent.CheckRoundStatus();
		}

		if ( BLCurTeam == BLTeams.Vampire )
		{
			timeUntilResurrection = timeToResurrect;
		}
	}

	public override void PostCameraSetup( ref CameraSetup setup )
	{
		setup.ZNear = 0.1f;

		//if ( BLGame.CurrentState == BLGame.GameStates.GameEnd )
		//	return;

		base.PostCameraSetup( ref setup );

		if ( setup.Viewer != null )
		{
			AddCameraEffects( ref setup );
		}
	}

	float walkBob = 0;
	float lean = 0;
	float fov = 0;

	private void AddCameraEffects( ref CameraSetup setup )
	{
		var speed = Velocity.Length.LerpInverse( 0, 320 );
		var forwardspeed = Velocity.Normal.Dot( setup.Rotation.Forward );

		var left = setup.Rotation.Left;
		var up = setup.Rotation.Up;

		if ( GroundEntity != null )
		{
			walkBob += Time.Delta * 25.0f * speed;
		}

		setup.Position += up * MathF.Sin( walkBob ) * speed * 2;
		setup.Position += left * MathF.Sin( walkBob * 0.6f ) * speed * 1;

		// Camera lean
		lean = lean.LerpTo( Velocity.Dot( setup.Rotation.Right ) * 0.01f, Time.Delta * 15.0f );

		var appliedLean = lean;
		appliedLean += MathF.Sin( walkBob ) * speed * 0.3f;
		setup.Rotation *= Rotation.From( 0, 0, appliedLean );

		speed = (speed - 0.7f).Clamp( 0, 1 ) * 3.0f;

		fov = fov.LerpTo( speed * 20 * MathF.Abs( forwardspeed ), Time.Delta * 4.0f );

		setup.FieldOfView += fov;

	}

}
