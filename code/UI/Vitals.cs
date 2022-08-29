using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
public class Vitals : Panel
{
	public Panel HealthBar;

	public Vitals()
	{
		StyleSheet.Load( "UI/Styles/vitalbar.scss" );

		HealthBar = Add.Panel("bar");
	}

	public override void Tick()
	{
		var player = Local.Pawn as BLPawn;
		if ( player == null ) return;

		HealthBar.SetClass( "human", player.BLCurTeam == BLPawn.BLTeams.Human || player.BLCurTeam == BLPawn.BLTeams.Hunter );
		HealthBar.SetClass( "vampire", player.BLCurTeam == BLPawn.BLTeams.Vampire );

		HealthBar.Style.Height = Length.Percent( player.Health.CeilToInt() );

	}
}
