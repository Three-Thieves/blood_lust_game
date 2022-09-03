using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class BLHud : RootPanel
{
	public static BLHud Current;

	public BLHud()
	{
		if( Current != null )
		{
			Current.Delete();
			Current = null;
		}

		StyleSheet.Load( "/UI/Styles/blhud.scss" );
		SetTemplate( "/UI/HTML/hud.html" );

		Current = this;

		AddChild<DamageIndicator>();

		AddChild<ChatBox>();

		AddChild<Scoreboard<ScoreboardEntry>>();
		AddChild<Vitals>();
		AddChild<Crosshair>();
		AddChild<BackpackBar>();
		AddChild<RoundRoleTeller>();
		AddChild<PlayerInfo>();
	}

	public override void Tick()
	{
		base.Tick();
	}

	protected override void UpdateScale( Rect screenSize )
	{
		base.UpdateScale( screenSize );
	}
}
