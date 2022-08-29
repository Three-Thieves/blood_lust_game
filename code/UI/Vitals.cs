﻿using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
public class Vitals : Panel
{
	public Panel HealthBar;
	public Panel bar;
	public Panel Border;

	public Vitals()
	{
		StyleSheet.Load( "UI/Styles/vitalbar.scss" );

		HealthBar = Add.Panel("HealthBar");
		bar = HealthBar.Add.Panel( "bar" );
		Border = Add.Panel( "border" );
	}

	public override void Tick()
	{
		var player = Local.Pawn as BLPawn;
		if ( player == null ) return;

		bar.SetClass( "human", player.BLCurTeam == BLPawn.BLTeams.Human || player.BLCurTeam == BLPawn.BLTeams.Hunter );
		bar.SetClass( "vampire", player.BLCurTeam == BLPawn.BLTeams.Vampire );

		Border.SetClass( "human", player.BLCurTeam == BLPawn.BLTeams.Human || player.BLCurTeam == BLPawn.BLTeams.Hunter );
		Border.SetClass( "vampire", player.BLCurTeam == BLPawn.BLTeams.Vampire );

		bar.Style.Height = Length.Percent( player.Health.CeilToInt() );
	}
}
