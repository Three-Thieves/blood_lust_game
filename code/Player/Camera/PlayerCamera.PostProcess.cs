using Sandbox;

namespace BloodLust.Player;

public partial class PlayerCamera
{
	protected void UpdatePostProcess()
	{
		var postProcess = Camera.Main.FindOrCreateHook<Sandbox.Effects.ScreenEffects>();
		postProcess.MotionBlur.Scale = 0f;
		postProcess.Saturation = 1f;

		postProcess.FilmGrain.Response = 1f;
		postProcess.FilmGrain.Intensity = 0.01f;
	}
}
