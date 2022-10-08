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

		AddChild<BLChat>();

		AddChild<Scoreboard<ScoreboardEntry>>();
		AddChild<Vitals>();
		AddChild<Crosshair>();
		AddChild<RoundRoleTeller>();
		AddChild<PlayerInfo>();

		AddChild<DeathNotice>();
		AddChild<BackpackBar>();
		//AddChild<ChatBox>();
		AddChild<VoiceSpeaker>();
		AddChild<VoiceList>();

		Current = this;
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
