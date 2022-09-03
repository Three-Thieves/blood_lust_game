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

		postProcess.Saturate.Enabled = true;
		postProcess.Saturate.Amount = 1;

		postProcess.DepthOfField.Enabled = false;

		if ( Local.Pawn is BLPawn player )
		{
			var timeSinceDamage = player.TimeSinceDamage.Relative;

			var damageUi = timeSinceDamage.LerpInverse( 0.25f, 0.0f, true ) * 0.3f;
			
			if ( damageUi > 0 )
			{
				postProcess.Saturate.Amount -= damageUi;

				postProcess.Blur.Enabled = true;
				postProcess.Blur.Strength = damageUi * 0.5f;
			}

			postProcess.Blur.Enabled = player.Health <= 0 && CurrentState == GameStates.Active;

			var healthDelta = player.Health.LerpInverse( 0, 100.0f, true );

			healthDelta = MathF.Pow( healthDelta, 0.5f );

			postProcess.Saturate.Amount = healthDelta;

		}


		if ( CurrentState == GameStates.Idle )
		{
			postProcess.FilmGrain.Intensity = 0.4f;
			postProcess.FilmGrain.Response = 0.5f;

			postProcess.Saturate.Amount = 0;
		}
	}
}
