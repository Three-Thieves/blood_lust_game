using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;


partial class BLPawn : Player
{
	ClothingContainer container = new();

	TimeSince timeSinceDropped;
	DamageInfo lastDMGInfo;
	TimeUntil timeUntilResurrection;

	float timeToResurrect = 15.0f;

	[Net]
	public float MaxHealth { get; set; } = 100;
	public bool SupressPickupNotices { get; private set; }

	public BLInventory Backpack { get; protected set; }

	[Net]
	List<BLWeaponsBase> wepList { get; set; }

	public BLPawn()
	{
		Backpack = new BLInventory( this );
	}

	public BLPawn( Client cl ) : this()
	{
		container.LoadFromClient( cl );
	}

	public override void Spawn()
	{
		CreateHull();

		Tags.Add( "player" );

		EnableLagCompensation = true;

		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new WalkController();
		CameraMode = new FirstPersonCamera();
		Animator = new StandardPlayerAnimator();

		BLCurTeam = BLTeams.Spectator;

		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		if (BLGame.CurrentState != BLGame.GameStates.Active)
		{
			EnableDrawing = true;
			EnableAllCollisions = true;
			container.DressEntity( this );
		} 
		else
		{
			EnableDrawing = false;
			EnableAllCollisions = false;
		}

		CreatePlayerFlashlight();
		BLGame.Current?.MoveToSpawnpoint( this );
	}

	public override void Respawn()
	{
		base.Respawn();

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
			container.DressEntity( this );
		}
	}

	public void Resurrect()
	{
		CameraMode = new FirstPersonCamera();

		EnableDrawing = true;
		EnableAllCollisions = true;
		container.DressEntity( this );

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
		if ( BLGame.CurrentState == BLGame.GameStates.MapVote )
			return;


		if (BLCurTeam == BLTeams.Vampire && LifeState == LifeState.Dead && IsServer )
		{
			Log.Info( timeUntilResurrection );

			if(Input.Pressed(InputButton.PrimaryAttack) && timeUntilResurrection < 0.0f)
			{
				Resurrect();
			}
			return;
		}

		//SelectWeapon();
		if ( Input.ActiveChild != null )
		{
			ActiveChild = Input.ActiveChild;
		}

		TickPlayerUse();
		SimulateFlashlight();

		if ( Input.Pressed( InputButton.Drop ) )
		{
			Backpack.DropContents();
			
			var dropped = Backpack.DropActive();
			if ( dropped != null )
			{
				wepList.Remove( dropped as BLWeaponsBase );
				if ( dropped.PhysicsGroup != null )
				{
					dropped.PhysicsGroup.Velocity = Velocity + (EyeRotation.Forward + EyeRotation.Up) * 300;
				}

				timeSinceDropped = 0;
			}
		}

		//wepList.Clear();
		
		/*foreach ( var item in Backpack.List )
		{
			if(item is BLWeaponsBase wep)
				wepList.Add( wep );
		}*/

		var controller = GetActiveController();
		controller?.Simulate( cl, this, GetActiveAnimator() );

		SimulateActiveChild( cl, ActiveChild );
	}

	int scroll = 0;

	[Event.BuildInput]
	public void ProcessClientInput( InputBuilder input )
	{
		var player = Local.Pawn as BLPawn;

		if ( player == null ) return;

		if ( wepList.Count <= 0 )
			return;

		scroll -= input.MouseWheel;
		scroll = scroll.Clamp( 0, wepList.Count - 1 );

		var selectedSlot = wepList[scroll];

		if ( selectedSlot == null || selectedSlot == player?.ActiveChild )
			return;

		input.ActiveChild = selectedSlot;
	}

	public override void StartTouch( Entity other )
	{
		if ( timeSinceDropped < 1 ) return;

		base.StartTouch( other );

		if ( other is BLWeaponsBase wep )
		{
			Backpack.Add( wep );
			wepList.Add( wep );
		}
	}

	void BecomeRagdoll( Vector3 force, int forceBone )
	{
		var ent = new BLRagdoll();
		ent.Position = Position;
		ent.Rotation = Rotation;
		ent.UsePhysicsCollision = true;

		ent.EnableTraceAndQueries = true;

		ent.CorpseOwner = this;
		ent.CorpseTeam = BLCurTeam;

		ent.SetModel( GetModelName() );
		ent.CopyBonesFrom( this );
		ent.TakeDecalsFrom( this );
		ent.SetRagdollVelocityFrom( this );

		// Copy the clothes over
		foreach ( var child in Children )
		{
			if ( !child.Tags.Has( "clothes" ) )
				continue;

			if ( child is ModelEntity e )
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

	public override void TakeDamage( DamageInfo info )
	{
		if ( BLGame.CurrentState != BLGame.GameStates.Active )
			return;

		lastDMGInfo = info;

		base.TakeDamage( info );
	}

	public override void OnKilled()
	{
		base.OnKilled();

		EnableDrawing = false;
		EnableAllCollisions = false;
		CameraMode = new SpectateRagdollCamera();

		Inventory.DeleteContents();
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
