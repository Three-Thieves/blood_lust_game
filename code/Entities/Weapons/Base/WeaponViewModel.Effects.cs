using BloodLust.Player.Mechanics;
using Sandbox;
using System;

namespace BloodLust.Weapons;

public partial class WeaponViewModel
{

	// Fields
	Vector3 SmoothedVelocity;
	Vector3 velocity;
	Vector3 acceleration;
	float VelocityClamp => 20f;
	float walkBob = 0;
	float upDownOffset = 0;
	float avoidance = 0;
	float burstSprintLerp = 0;
	float sprintLerp = 0;
	float aimLerp = 0;
	float crouchLerp = 0;
	float airLerp = 0;
	float slideLerp = 0;
	float sideLerp = 0;

	protected float MouseDeltaLerpX;
	protected float MouseDeltaLerpY;

	Vector3 positionOffsetTarget = Vector3.Zero;
	Rotation rotationOffsetTarget = Rotation.Identity;

	Vector3 realPositionOffset;
	Rotation realRotationOffset;

	protected void ApplyPositionOffset( Vector3 offset, float delta )
	{
		var left = Camera.Rotation.Left;
		var up = Camera.Rotation.Up;
		var forward = Camera.Rotation.Forward;

		positionOffsetTarget += forward * offset.x * delta;
		positionOffsetTarget += left * offset.y * delta;
		positionOffsetTarget += up * offset.z * delta;
	}

	private float WalkCycle( float speed, float power, bool abs = false )
	{
		var sin = MathF.Sin( walkBob * speed );
		var sign = Math.Sign( sin );

		if ( abs )
		{
			sign = 1;
		}

		return MathF.Pow( sin, power ) * sign;
	}

	private void LerpTowards( ref float value, float desired, float speed )
	{
		var delta = (desired - value) * speed * Time.Delta;
		var deltaAbs = MathF.Min( MathF.Abs( delta ), MathF.Abs( desired - value ) ) * MathF.Sign( delta );

		if ( MathF.Abs( desired - value ) < 0.001f )
		{
			value = desired;

			return;
		}

		value += deltaAbs;
	}

	private void ApplyDamping( ref Vector3 value, float damping )
	{
		var magnitude = value.Length;

		if ( magnitude != 0 )
		{
			var drop = magnitude * damping * Time.Delta;
			value *= Math.Max( magnitude - drop, 0 ) / magnitude;
		}
	}

	public void AddEffects()
	{
		var player = Weapon.Player;
		var controller = player?.Controller;
		if ( controller == null )
			return;

		SmoothedVelocity += (controller.Velocity - SmoothedVelocity) * 5f * Time.Delta;

		var isGrounded = controller.GroundEntity != null;
		var speed = controller.Velocity.Length.LerpInverse( 0, 750 );
		var sideSpeed = controller.Velocity.Length.LerpInverse( 0, 350 );
		var bobSpeed = SmoothedVelocity.Length.LerpInverse( -250, 700 );
		//var isSprinting = controller.IsMechanicActive<SprintMechanic>();
		var left = Camera.Rotation.Left;
		var up = Camera.Rotation.Up;
		var forward = Camera.Rotation.Forward;
		var isCrouching = controller.IsMechanicActive<CrouchMechanic>();
		//var isAiming = Weapon.GetComponent<Aim>()?.IsActive ?? false;
		//var isSliding = controller.IsMechanicActive<SlideMechanic>();
		//var timeSinceFired = Weapon.GetComponent<PrimaryFire>().TimeSinceActivated;

		LerpTowards( ref aimLerp, 0, 10f );
		//LerpTowards( ref sprintLerp, isSprinting ? 1 : 0, 10f );
		//LerpTowards( ref burstSprintLerp, burstSprint && !sliding ? 1 : 0, 8f );

		LerpTowards( ref crouchLerp, isCrouching ? 1 : 0, 7f );

		//LerpTowards( ref crouchLerp, isCrouching && !isAiming && !isSliding ? 1 : 0, 7f );
		//LerpTowards( ref slideLerp, isSliding ? ((float)timeSinceFired).Remap( 0.35f, 0.65f, 0, 1 ).Clamp( 0, 1 ) : 0, 7f );
		LerpTowards( ref airLerp, (isGrounded ? 0 : 1) * (1 - aimLerp), 10f );
		//LerpTowards( ref speedLerp, (aim || sliding || sprint) ? 0.0f : speed, 10f );
		//LerpTowards( ref vaultLerp, (vaulting) ? 1.0f : 0.0f, 10f );

		var leftAmt = left.WithZ( 0 ).Normal.Dot( controller.Velocity.Normal );
		LerpTowards( ref sideLerp, leftAmt * (1 - aimLerp), 5f );

		bobSpeed *= 1 - sprintLerp * 0.25f;
		bobSpeed *= 1 - burstSprintLerp * 2f;

		bobSpeed *= 1 - slideLerp;

		if ( isGrounded )
		{
			walkBob += Time.Delta * 30.0f * bobSpeed;
		}

		walkBob %= 360;

		var mouseDeltaX = -Input.MouseDelta.x * Time.Delta * 2;
		var mouseDeltaY = -Input.MouseDelta.y * Time.Delta * 2;

		acceleration += Vector3.Left * mouseDeltaX * -1f;
		acceleration += Vector3.Up * mouseDeltaY * -2f;
		acceleration += -velocity * 2 * Time.Delta;

		// Apply horizontal offsets based on walking direction
		var horizontalForwardBob = WalkCycle( 0.5f, 3f ) * speed * Time.Delta;

		acceleration += forward.WithZ( 0 ).Normal.Dot( controller.Velocity.Normal ) * Vector3.Forward * horizontalForwardBob;

		velocity += acceleration * Time.Delta;

		ApplyDamping( ref acceleration, 2.5f );
		ApplyDamping( ref velocity, 2 * (1 + aimLerp) );
		velocity = velocity.Normal * Math.Clamp( velocity.Length, 0, VelocityClamp );

		var avoidanceTrace = Trace.Ray( Camera.Position, Camera.Position + forward * 50f )
			.WorldAndEntities()
			.WithoutTags( "trigger" )
			.Ignore( Weapon )
			.Ignore( this )
			.Run();

		var avoidanceVal = avoidanceTrace.Hit ? (1f - avoidanceTrace.Fraction) : 0;
		avoidanceVal *= 1 - (aimLerp * 0.8f);

		LerpTowards( ref avoidance, avoidanceVal, 10f );

		Position = player.EyePosition;
		Rotation = player.EyeRotation;

		positionOffsetTarget = Vector3.Zero;
		rotationOffsetTarget = Rotation.Identity;

		{
			float velocityScale = 0.5f;

			// Global
			//rotationOffsetTarget *= Rotation.From( Data.GlobalAngleOffset );
			positionOffsetTarget += forward * velocity.x * velocityScale;
			//positionOffsetTarget += left * velocity.y * velocityScale;
			positionOffsetTarget += up * velocity.z * velocityScale / 2;

			float cycle = Time.Now * 10.0f;
			float sideCycle = Time.Now * 5f;
			var forwardAmount = velocity.x;
			var rightAmount = velocity.y;
			Camera.Rotation *= Rotation.From(
				new Angles(
					MathF.Abs( MathF.Sin( cycle ) ),
					MathF.Cos( cycle ) * 0.5f,
					0
				) * forwardAmount * 5f );

			Camera.Rotation *= Rotation.From(
				new Angles(
					MathF.Abs( Sandbox.Utility.Easing.QuadraticIn( MathF.Sin( sideCycle ) ) * 0.5f ),
					Sandbox.Utility.Easing.QuadraticIn( MathF.Cos( sideCycle ) ) * 5f,
					0
				) * rightAmount * -3f );

			// Crouching
			//rotationOffsetTarget *= Rotation.From( Data.CrouchAngleOffset * crouchLerp );
			//ApplyPositionOffset( Data.CrouchPositionOffset, crouchLerp );

			// Air
			ApplyPositionOffset( new( 0, 0, 1 ), airLerp );

			// Avoidance
			//rotationOffsetTarget *= Rotation.From( Data.AvoidanceAngleOffset * avoidance );
			//ApplyPositionOffset( Data.AvoidancePositionOffset, avoidance );

			// Aim Down Sights
			//rotationOffsetTarget *= Rotation.From( Data.AimAngleOffset * aimLerp );
			//ApplyPositionOffset( Data.AimPositionOffset, aimLerp );

			// Sprinting
			//rotationOffsetTarget *= Rotation.From( Data.SprintAngleOffset * sprintLerp );
			//ApplyPositionOffset( Data.SprintPositionOffset, sprintLerp );

			// Sprinting Camera Rotation
			Camera.Rotation *= Rotation.From(
				new Angles(
					MathF.Abs( MathF.Sin( cycle ) * 2.0f ),
					MathF.Cos( cycle ),
					0
				) * sprintLerp * 0.35f );

			// Apply the same offset as above for a nicer sprint bob
			float sprintBob = MathF.Pow( MathF.Sin( cycle ) * 0.5f + 0.5f, 2.0f );
			float sprintBob2 = MathF.Pow( MathF.Cos( cycle ) * 0.5f + 0.5f, 3.0f );
			//rotationOffsetTarget *= Rotation.From( Data.SprintAngleOffset * sprintLerp * sprintBob * 0.2f );
			//ApplyPositionOffset( -Data.SprintPositionOffset * sprintBob2 * 0.3f, sprintLerp );


			// Sliding
			var slideRotationOffset = Rotation.From( Angles.Zero.WithRoll( leftAmt ) * slideLerp * -15.0f );

			/*if ( !isAiming )
			{
				rotationOffsetTarget *= Rotation.From( Data.SlideAngleOffset * slideLerp );
				ApplyPositionOffset( Data.SlidePositionOffset, slideLerp );
			}
			else
				rotationOffsetTarget *= slideRotationOffset;*/

			Camera.Rotation *= slideRotationOffset;
			Camera.FieldOfView += 5f * slideLerp;
		}

		realRotationOffset = rotationOffsetTarget;
		realPositionOffset = positionOffsetTarget;

		Rotation *= realRotationOffset;
		Position += realPositionOffset;

		Camera.FieldOfView -= 10f * aimLerp;
		Camera.Main.SetViewModelCamera( 85f, 1, 2048 );
	}
}
