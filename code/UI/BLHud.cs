using Sandbox;
using Sandbox.UI;

public class BLHud : RootPanel
{
	public static BLHud Current;

	public Scoreboard Scoreboard { get; set; }

	public BLHud()
	{
		Current = this;

		StyleSheet.Load( "/UI/Styles/hud.scss" );
		SetTemplate( "/UI/HTML/hud.html" );

		AddChild<DamageIndicator>();
		AddChild<HitIndicator>();

		AddChild<InventoryBar>();
		AddChild<PickupFeed>();

		AddChild<ChatBox>();
		Scoreboard = AddChild<Scoreboard>();
		AddChild<VoiceList>();
	}

	public override void Tick()
	{
		base.Tick();

		//SetClass( "game-end", DeathmatchGame.CurrentState == DeathmatchGame.GameStates.GameEnd );
		//SetClass( "game-warmup", DeathmatchGame.CurrentState == DeathmatchGame.GameStates.Warmup );
	}

	protected override void UpdateScale( Rect screenSize )
	{
		base.UpdateScale( screenSize );
	}
}
