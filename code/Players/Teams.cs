﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class BLPawn : Player
{
	public enum BLTeams
	{
		Unknown,
		Spectator,
		Human,
		Hunter,
		Vampire
	}

	public enum StatusEnum
	{
		Dead,
		Alive,
	}

	[Net]
	public BLTeams CurTeam { get; protected set; } = BLTeams.Spectator;
	[Net]
	public BLTeams LastTeam { get; set; } = BLTeams.Unknown;

	public void UpdatePlayerTeam(BLTeams updateTeam)
	{
		CurTeam = updateTeam;
	}
}

