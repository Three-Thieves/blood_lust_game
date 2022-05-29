using Sandbox;
using System;
using System.Linq;


partial class BLPawn : Player
{
	ClothingContainer container = new();

	TimeSince timeSinceDropped;

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
		base.Spawn();

		SetModel( "models/citizen/citizen.vmdl" );

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		container.DressEntity( this );

		Controller = new WalkController();
		CameraMode = new FirstPersonCamera();
		Animator = new StandardPlayerAnimator();

		base.Respawn();
	}

	public override void Respawn()
	{
		base.Respawn();

		ClearAmmo();
		Inventory.DeleteContents();
	}

	public override void BuildInput( InputBuilder input )
	{
		if ( BLGame.CurrentState == BLGame.GameStates.Post )
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

		base.Simulate( cl );

		if ( Input.ActiveChild != null )
		{
			ActiveChild = Input.ActiveChild;
		}

		if ( LifeState != LifeState.Alive )
			return;

		TickPlayerUse();

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
				SwitchToBestWeapon();
			}
		}

		SimulateActiveChild( cl, ActiveChild );

		if ( ActiveChild is BLWeaponsBase weapon && !weapon.IsUsable() && weapon.TimeSincePrimaryAttack > 0.5f && weapon.TimeSinceSecondaryAttack > 0.5f )
		{
			SwitchToBestWeapon();
		}
	}
	public void SwitchToBestWeapon()
	{
		var best = Children.Select( x => x as BLWeaponsBase )
			.Where( x => x.IsValid() && x.IsUsable() )
			.OrderByDescending( x => x.BucketWeight )
			.FirstOrDefault();

		if ( best == null ) return;

		ActiveChild = best;
	}

	public override void StartTouch( Entity other )
	{
		if ( timeSinceDropped < 1 ) return;

		base.StartTouch( other );
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
