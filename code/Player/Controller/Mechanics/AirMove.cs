using Sandbox;

namespace BloodLust.Player.Mechanics;

/// <summary>
/// AirMove decides how the player moves while in the air. Drives effects such as gravity, wind, etc.
/// </summary>
public partial class AirMoveMechanic : PlayerControllerMechanic
{
	public float Gravity => 800.0f;
	public float AirControl => 30.0f;
	public float AirAcceleration => 15.0f;

	protected override void Simulate()
	{
		var ctrl = Controller;
		ctrl.Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
		ctrl.Velocity += new Vector3( 0, 0, ctrl.BaseVelocity.z ) * Time.Delta;
		ctrl.BaseVelocity = ctrl.BaseVelocity.WithZ( 0 );

		var velocityAtStart = ctrl.Velocity;
		var groundedAtStart = GroundEntity.IsValid() && !Controller.IsTouchingLadder;

		if ( groundedAtStart ) 
			return;

		var wishVel = ctrl.GetWishVelocity( true );
		var wishdir = wishVel.Normal;
		var wishspeed = wishVel.Length;

		ctrl.Accelerate( wishdir, wishspeed, AirControl, AirAcceleration );
		ctrl.Velocity += ctrl.BaseVelocity;
		ctrl.Move();
		ctrl.Velocity -= ctrl.BaseVelocity;
		ctrl.Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

		// if ( ctrl.GroundEntity != null && !groundedAtStart )
		//	DoFallDamage();

		// if ( ctrl.GroundEntity == null && groundedAtStart )
		//	new FallCameraModifier( -150, 1.5f );
	}

	protected override bool ShouldStart()
	{
		return true;
	}
}
