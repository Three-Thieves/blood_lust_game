using Sandbox;

namespace BloodLust.Player;

public partial class PlayerCamera
{
	public virtual void BuildInput( BloodPawn player )
	{
		//
	}

	public virtual void Update( BloodPawn player )
	{
		Camera.Position = player.EyePosition;
		Camera.Rotation = player.EyeRotation;
		Camera.FieldOfView = Game.Preferences.FieldOfView;
		Camera.FirstPersonViewer = player;
		Camera.ZNear = 0.5f;

		UpdatePostProcess();
	}
}
