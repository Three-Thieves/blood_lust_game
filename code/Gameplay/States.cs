using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class BLGame : Game
{
	public static BLGame GameCurrent => Current as BLGame;

	public static GameStates CurrentState => Instance?.GameState ?? GameStates.Idle;

	public int MaxRounds { get; private set; } = 8;

	[Net]
	public int CurRound { get; protected set; } = 1;

	[Net]
	public RealTimeUntil StateTimer { get; set; } = 0f;

	[Net]
	public GameStates GameState { get; set; } = GameStates.Waiting;

	[Net]
	public string NextMap { get; set; } = "facepunch.construct";

	public enum GameStates
	{
		Waiting,
		Idle,
		Start,
		Active,
		Post,
		MapVote
	}
}

