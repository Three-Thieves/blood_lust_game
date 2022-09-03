using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class BLGame
{
	public bool CanAdjustLighting()
	{
		if ( All.OfType<EnvironmentLightEntity>().Count() <= 0 )
			return false;

		if ( All.OfType<Sky>().Count() <= 0 )
			return false;

		return true;
	}

	public void AdjustMapEnvironment()
	{
		if ( !CanAdjustLighting() )
			return;

		All.OfType<Sky>().ToList().ForEach( x => x.Delete() );

		All.OfType<EnvironmentLightEntity>().ToList().ForEach( x =>
		{
			x.Color = Color.Gray;
			x.SkyColor = Color.Gray;
			x.Brightness = 1.0f;
			x.SkyIntensity = 1.0f;
		} );

		Sky skybox = new Sky();
		skybox.Skyname = "materials/sky/dark_sky.vmat";
		skybox.TintColor = Color.Gray;

		if (IsClient)
			skybox.ClientSpawn();

	}
}

