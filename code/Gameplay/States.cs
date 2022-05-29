using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class BLGame : Game
{
	public static GameStates CurrentState => (Current as BLGame)?.GameState ?? GameStates.Idle;

	public int MaxRounds { get; private set; } = 8;

	[Net]
	public int CurRound { get; protected set; } = 1;

	[Net]
	public RealTimeUntil StateTimer { get; set; } = 0f;

	[Net]
	public GameStates GameState { get; set; } = GameStates.Idle;

	[Net]
	public string NextMap { get; set; } = "facepunch.construct";

	private async Task WaitStateTimer()
	{
		while ( StateTimer > 0 || GameState == GameStates.Idle )
		{
			await Task.DelayRealtimeSeconds( 1.0f );
		}

		// extra second for fun
		await Task.DelayRealtimeSeconds( 1.0f );
	}

	private async Task BLGameLoopAsync()
	{
		await WaitStateTimer();

		GameState = GameStates.Start;
		StateTimer = 10.0f;
		Log.Info( "Updated state to Start" );
		await WaitStateTimer();

		Log.Info( "Updated state to Active" );
		GameState = GameStates.Active;
		StateTimer = 5 * 60;
		await WaitStateTimer();

		Log.Info( "Updated state to Post" );
		GameState = GameStates.Post;
		StateTimer = 10.0f;
		CurRound++;

		if ( CurRound >= MaxRounds )
		{
			Log.Info( "ROUNDS EXCEED MAX, starting map vote" );
			GameState = GameStates.MapVote;

			var mapVote = new MapVoteEntity();
			mapVote.VoteTimeLeft = 10.0f;
			StateTimer = mapVote.VoteTimeLeft;
			await WaitStateTimer();

			Global.ChangeLevel( mapVote.WinningMap );
		}
	}
	
	/*private async Task GameLoopAsync()
	{
		GameState = GameStates.Idle;
		StateTimer = 10;
		await WaitStateTimer();

		GameState = GameStates.Active;
		StateTimer = 5 * 60;
		await WaitStateTimer();

		GameState = GameStates.Post;
		StateTimer = 10.0f;
		await WaitStateTimer();

		GameState = GameStates.MapVote;

		var mapVote = new MapVoteEntity();
		mapVote.VoteTimeLeft = 10.0f;
		StateTimer = mapVote.VoteTimeLeft;
		await WaitStateTimer();

		Global.ChangeLevel( mapVote.WinningMap );
	}*/

	public enum GameStates
	{
		Idle,
		Start,
		Active,
		Post,
		MapVote
	}
}

