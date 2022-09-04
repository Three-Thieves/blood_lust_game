using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class BLPawn
{
	[Net, Predicted]
	public bool FlashlightEnabled { get; private set; } = false;

	[Net, Local, Predicted]
	private TimeSince TimeSinceLightToggled { get; set; }

	SpotLightEntity wFlash;
	SpotLightEntity vmFlash;

	public void SimulateFlashlight()
	{
		if ( wFlash.IsValid() )
		{
			var transform = GetAttachment( "eyes" ) ?? default;
			transform.Position = EyePosition + EyeRotation.Forward * 10;
			wFlash.Transform = transform;
		}

		if ( Health <= 0 )
			return;

		if ( TimeSinceLightToggled > 0.25f && Input.Pressed( InputButton.Flashlight ) )
		{
			FlashlightEnabled = !FlashlightEnabled;

			PlaySound( "flashlight_toggle" );

			if ( wFlash.IsValid() )
				wFlash.Enabled = FlashlightEnabled;

			TimeSinceLightToggled = 0;
		}
	}

	protected void CreatePlayerFlashlight()
	{
		wFlash = CreateFlashlight();
		wFlash.EnableHideInFirstPerson = true;
		FlashlightEnabled = false;
		
		vmFlash = CreateFlashlight();
		vmFlash.EnableViewmodelRendering = true;
		vmFlash.Enabled = FlashlightEnabled;
	}

	protected void FixViewFlashlight()
	{
		vmFlash = CreateFlashlight();
		vmFlash.EnableViewmodelRendering = true;
		vmFlash.Enabled = FlashlightEnabled;
	}

	protected void DeleteFlashlight()
	{
		wFlash?.Delete();
		wFlash = null;

		vmFlash?.Delete();
		vmFlash = null;
	}

	[Event.Frame]
	private void FrameUpdate()
	{
		if ( FlashlightEnabled && !vmFlash.IsValid() )
			FixViewFlashlight();

		if ( !vmFlash.IsValid() )
			return;

		vmFlash.Enabled = FlashlightEnabled & IsFirstPersonMode;

		if ( !vmFlash.Enabled )
			return;

		var eyeTransform = new Transform( EyePosition, EyeRotation );
		vmFlash.Transform = eyeTransform;
	}

	private SpotLightEntity CreateFlashlight()
	{
		return new SpotLightEntity
		{
			Enabled = false,
			DynamicShadows = true,
			Range = 512,
			Falloff = 1.0f,
			LinearAttenuation = 0.0f,
			QuadraticAttenuation = 1.0f,
			Brightness = 1,
			Color = Color.White,
			InnerConeAngle = 20,
			OuterConeAngle = 40,
			FogStrength = 1.0f,
			Owner = this,
			Parent = this,
			LightCookie = Texture.Load( "materials/effects/lightcookie.vtex" )
		};
	}
}

