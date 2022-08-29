using Sandbox;
using System;
using System.Linq;


partial class BLPawn : Player
{
	[Net, Local, Predicted]
	bool flashlightToggle { get; set; } = false;

	[Net, Local]
	SpotLightEntity light { get; set; }

	ClothingContainer container = new();

	TimeSince timeToggleFlash;

	TimeSince timeSinceDropped;

	DamageInfo lastDMGInfo;

	TimeUntil timeUntilResurrection;

	float timeToResurrect = 15.0f;

	[Net]
	public float MaxHealth { get; set; } = 100;

	public bool SupressPickupNotices { get; private set; }

	public BLPawn()
	{
		Inventory = new BLInventory( this );
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

		BLGame.Current?.MoveToSpawnpoint( this );
	}

	public override void Respawn()
	{
		base.Respawn();

		CameraMode = new FirstPersonCamera();
		if ( light.IsValid() )
		{
			flashlightToggle = false;
			light.Enabled = false;
			light.Delete();
			light = null;
		}

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

		if ( Input.ActiveChild != null )
		{
			ActiveChild = Input.ActiveChild;
		}

		TickPlayerUse();

		if(Input.Pressed(InputButton.Flashlight) && timeToggleFlash > 0.3f)
		{
			flashlightToggle = !flashlightToggle;
			timeToggleFlash = 0;

			switch (flashlightToggle)
			{
				case true:
					if ( IsServer )
					{
						light = new SpotLightEntity
						{
							Enabled = true,
							DynamicShadows = true,
							Range = 512,
							Falloff = 1.0f,
							LinearAttenuation = 0.0f,
							QuadraticAttenuation = 1.0f,
							Brightness = 3,
							Color = Color.White,
							InnerConeAngle = 15,
							OuterConeAngle = 55,
							FogStrength = 1.0f,
							Owner = Owner,
							LightCookie = Texture.Load( "materials/effects/lightcookie.vtex" )
						};
					}
					break;

				case false:
					if(IsServer)
					{
						light.Enabled = false;
						light.Delete();
						light = null;
					}
					break;
			}
		}

		if ( light != null && IsServer )
		{
			light.Position = EyePosition + EyeRotation.Forward * 25;
			light.Rotation = EyeRotation;
		}

		if ( Input.Pressed( InputButton.Drop ) )
		{
			var dropped = Inventory.DropActive();
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
		if ( timeSinceDropped < 1 ) return;

		base.StartTouch( other );

		if(other is BLWeaponsBase wep)
			Inventory.Add( wep );
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
