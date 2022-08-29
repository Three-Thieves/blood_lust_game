using System;
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


	[Net, Change( nameof( ServerNotifyTeam ) )]
	public BLTeams BLCurTeam { get; protected set; } = BLTeams.Spectator;

	public void ServerNotifyTeam( BLTeams oldTeam, BLTeams newTeam )
	{
		Log.Info( $"{Client.Name} has changed from {oldTeam} to {newTeam}" );
	}

	public void UpdatePlayerTeam(BLTeams updateTeam)
	{
		BLCurTeam = updateTeam;
	}
}

