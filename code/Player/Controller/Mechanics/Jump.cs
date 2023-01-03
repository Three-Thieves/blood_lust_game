using Sandbox;

namespace BloodLust.Player.Mechanics;

/// <summary>
/// The jump mechanic for players.
/// </summary>
public partial class JumpMechanic : PlayerControllerMechanic
{
	public override int SortOrder => 25;

	private float Cooldown => 0.5f;
	private float Gravity => 700f;
	
	protected override bool ShouldStart()
	{
		if ( !Input.Pressed( InputButton.Jump ) ) return false;
		if ( !Controller.GroundEntity.IsValid() ) return false;

		return true;
	}

	protected override void OnStart()
	{
		Simulate();
	}

	protected override void OnStop()
	{
		TimeUntilCanStart = Cooldown;
	}

	protected override void Simulate()
	{
		if ( !Controller.GroundEntity.IsValid() )
		{
			return;
		}

		if ( Controller.GroundEntity.IsValid() )
		{
			float flGroundFactor = 1.0f;
			float flMul = 325f;
			float startz = Velocity.z;

			Velocity = Velocity.WithZ( startz + flMul * flGroundFactor );
			Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;

			Controller.GetMechanic<WalkMechanic>()
				.ClearGroundEntity();
		}
	}
}
