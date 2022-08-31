using Sandbox.UI;

public class Crosshair : Panel
{
	int fireCounter;

	public Crosshair()
	{
		StyleSheet.Load( "/UI/Styles/Crosshair.scss" );
		var p = Add.Panel();
	}

	public override void Tick()
	{
		base.Tick();

		SetClass( "fire", fireCounter > 0 );

		if ( fireCounter > 0 )
			fireCounter--;
	}

	[PanelEvent]
	public void FireEvent()
	{
		fireCounter += 2;
	}
}
