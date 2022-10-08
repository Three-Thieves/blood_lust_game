using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.Effects;

public partial class BLGame
{
	ScreenEffects postProcess;

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		postProcess.Enabled = true;
		postProcess.Sharpen = 0.5f;
		
		postProcess.FilmGrain.Intensity = 0.2f;
		postProcess.FilmGrain.Response = 1;

		postProcess.Saturation = 1;

		if ( Local.Pawn is BLPawn player )
		{
			var timeSinceDamage = player.TimeSinceDamage.Relative;

			var damageUi = timeSinceDamage.LerpInverse( 0.25f, 0.0f, true ) * 0.3f;
			
			if ( damageUi > 0 )
			{
				postProcess.Saturation -= damageUi;

				postProcess.MotionBlur.Scale = damageUi * 0.5f;
			}

			var healthDelta = player.Health.LerpInverse( 0, 100.0f, true );

			healthDelta = MathF.Pow( healthDelta, 0.5f );

			postProcess.Saturation = healthDelta;

			if(player.Health <= 30.0f && player.Health > 0)
			{
				postProcess.FilmGrain.Intensity = 0.5f;
				postProcess.FilmGrain.Response = 0.85f;
			}

		}


		if ( CurrentState == GameStates.Idle )
		{
			postProcess.FilmGrain.Intensity = 0.4f;
			postProcess.FilmGrain.Response = 0.5f;

			postProcess.Saturation = 0;
		}
	}
}
