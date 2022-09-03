using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class BLGame
{
	StandardPostProcess postProcess;

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		postProcess.Sharpen.Enabled = true;
		postProcess.Sharpen.Strength = 0.5f;

		postProcess.FilmGrain.Enabled = true;
		postProcess.FilmGrain.Intensity = 0.2f;
		postProcess.FilmGrain.Response = 1;

		postProcess.Vignette.Enabled = true;
		postProcess.Vignette.Intensity = 1.0f;
		postProcess.Vignette.Roundness = 1.5f;
		postProcess.Vignette.Smoothness = 0.5f;
		postProcess.Vignette.Color = Color.Black;

		postProcess.Saturate.Enabled = true;
		postProcess.Saturate.Amount = 1;

		postProcess.Blur.Enabled = false;

		if ( Local.Pawn is BLPawn player )
		{
			var timeSinceDamage = player.TimeSinceDamage.Relative;

			var damageUi = timeSinceDamage.LerpInverse( 0.25f, 0.0f, true ) * 0.3f;
			if ( damageUi > 0 )
			{
				postProcess.Saturate.Amount -= damageUi;
				postProcess.Vignette.Color = Color.Lerp( postProcess.Vignette.Color, Color.Red, damageUi );
				postProcess.Vignette.Intensity += damageUi;
				postProcess.Vignette.Smoothness += damageUi;
				postProcess.Vignette.Roundness += damageUi;

				postProcess.Blur.Enabled = true;
				postProcess.Blur.Strength = damageUi * 0.5f;
			}


			var healthDelta = player.Health.LerpInverse( 0, 100.0f, true );

			healthDelta = MathF.Pow( healthDelta, 0.5f );

			postProcess.Vignette.Color = Color.Lerp( postProcess.Vignette.Color, Color.Red, 1 - healthDelta );
			postProcess.Vignette.Intensity += (1 - healthDelta) * 0.5f;
			postProcess.Vignette.Smoothness += (1 - healthDelta);
			postProcess.Vignette.Roundness += (1 - healthDelta) * 0.5f;
			postProcess.Saturate.Amount *= healthDelta;
			postProcess.FilmGrain.Intensity += (1 - healthDelta) * 0.5f;

		}


		if ( CurrentState == GameStates.Idle )
		{
			postProcess.FilmGrain.Intensity = 0.4f;
			postProcess.FilmGrain.Response = 0.5f;

			postProcess.Saturate.Amount = 0;
		}
	}
}
