using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
public class HealthHud : Panel
{
	public Image HealthBar;

	public HealthHud()
	{
		HealthBar = Add.Image( "ui/healthbar_human.png", "healthhud");
	}

	public override void Tick()
	{
		var player = Local.Pawn as BLPawn;
		if ( player == null ) return;

		HealthBar.Style.Height = Length.Percent( player.Health );

	}
}
