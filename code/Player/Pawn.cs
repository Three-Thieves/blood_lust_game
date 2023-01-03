using Sandbox;
using System;
using System.Linq;
using BloodLust;
using BloodLust.Weapons;
using BloodLust.Player.Mechanics;
using BloodLust.Player.Animator;

namespace BloodLust.Player;

public partial class BloodPawn : AnimatedEntity
{
	[BindComponent] public PlayerController Controller { get; }
	[BindComponent] public BloodInventory Inventory { get; }
	[BindComponent] public PlayerAnimator Animator { get; }
	public PlayerCamera PlayerCamera { get; protected set; }
	[ClientInput] public Vector3 InputDirection { get; protected set; }
	[ClientInput] public Angles ViewAngles { get; set; }
	[Net, Predicted] public TimeSince TimeSinceDamage { get; set; }
	public BloodWeapon ActiveWeapon => Inventory?.ActiveWeapon;
	public DamageInfo LastDamage { get; protected set; }

	TimeSince TimeSinceFootstep = 0;
	bool SetView;
	Angles NewView;

	public void MoveToSpawnpoint()
	{
		var spawnpoint = All.OfType<SpawnPoint>().OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		if ( spawnpoint != null )
		{
			var tx = spawnpoint.Transform;
			Transform = tx;
			SetViewAngles( To.Single( this ), spawnpoint.Rotation.Angles() );
			ResetInterpolation();
		}
	}

	[ClientRpc]
	public void SetViewAngles(Angles newAngles)
	{
		SetView = true;
		NewView = newAngles;
	}

	public void SetUpHull()
	{
		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, new Vector3( -16, -16, 0 ), new Vector3( 16, 16, 72 ) );

		EnableHitboxes = true;
		EnableLagCompensation = true;
	}

	[ClientRpc]
	public void ClientRespawn()
	{
		PlayerCamera = new PlayerCamera();
	}

	[Event.Client.PostCamera]
	protected void PostCameraUpdate()
	{
		PlayerCamera?.Update( this );

		// Apply camera modifiers after a camera update.
		CameraModifier.Apply();
	}

	[ConCmd.Server("kill")]
	public static void DebugSuicide()
	{
		if ( ConsoleSystem.Caller.Pawn is not BloodPawn player ) return;

		player.Spawn();
	}

	public void SetBasicStats()
	{
		Health = 100;
		LifeState = LifeState.Alive;
		EnableAllCollisions = true;
		EnableDrawing = true;
	}

	public override void Spawn()
	{
		base.Spawn();

		Tags.Add( "player" );

		SetModel( "models/citizen/citizen.vmdl" );
		SetUpHull();
		MoveToSpawnpoint();
		SetUpController();

		ClientRespawn( To.Single( Client ) );

		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;
	}

	public void SetUpController()
	{
		Components.Create<PlayerController>();

		Components.RemoveAny<PlayerControllerMechanic>();

		Components.Create<WalkMechanic>();
		Components.Create<CrouchMechanic>();
		Components.Create<JumpMechanic>();
		Components.Create<AirMoveMechanic>();
		Components.Create<HeavyLandMechanic>();
		Components.Create<InteractionMechanic>();

		Components.Create<CitizenAnimator>();
	}

	[ClientRpc]
	public void SetAudioEffect( string effectName, float strength, float velocity = 20f, float fadeOut = 4f )
	{
		Audio.SetEffect( effectName, strength, velocity: 20.0f, fadeOut: 4.0f * strength );
	}

	public override void TakeDamage( DamageInfo info )
	{
		if ( LifeState != LifeState.Alive )
			return;

		// Check for headshot damage
		var isHeadshot = info.Hitbox.HasTag( "head" );
		if ( isHeadshot )
		{
			info.Damage *= 2.5f;
		}

		// Check if we got hit by a bullet, if we did, play a sound.
		if ( info.HasTag( "bullet" ) )
		{
			Sound.FromScreen( To.Single( Client ), "sounds/player/damage_taken_shot.sound" );
		}

		// Play a deafening effect if we get hit by blast damage.
		if ( info.HasTag( "blast" ) )
		{
			SetAudioEffect( To.Single( Client ), "flasthbang", info.Damage.LerpInverse( 0, 60 ) );
		}

		if ( Health > 0 && info.Damage > 0 )
		{
			TimeSinceDamage = 0;
			Health -= info.Damage;

			if ( Health <= 0 )
			{
				Health = 0;
				OnKilled();
			}
		}

		this.ProceduralHitReaction( info, 0.05f );
	}

	public override void Simulate( IClient cl )
	{
		Rotation = LookInput.WithPitch( 0f ).ToRotation();

		Controller?.Simulate( cl );
		Animator?.Simulate( cl );

		// Simulate our active weapon if we can.
		Inventory?.Simulate( cl );
	}

	public override void FrameSimulate( IClient cl )
	{
		Rotation = LookInput.WithPitch( 0f ).ToRotation();

		Controller?.FrameSimulate( cl );
		Animator?.FrameSimulate( cl );

		// Simulate our active weapon if we can.
		Inventory?.FrameSimulate( cl );
	}
}
