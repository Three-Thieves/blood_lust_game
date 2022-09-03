using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

public partial class BLRagdoll : ModelEntity
{
	public BLPawn CorpseOwner;
	[Net] public BLPawn.BLTeams CorpseTeam { get; set; } = BLPawn.BLTeams.Unknown;
	public BLPawn.StatusEnum CorpseStatus = BLPawn.StatusEnum.Dead;
	[Net] public BLPawn.IdentityEnum CorpseIdent { get; set; }
	[Net] public string CorpseName { get; set; }
	public float BloodAmount = 50.0f;
	[Net] public bool IsStaked { get; set; } = false;
}
