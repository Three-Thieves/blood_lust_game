using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
public partial class BLRagdoll : ModelEntity
{
	public BLPawn CorpseOwner;
	public BLPawn.BLTeams CorpseTeam = BLPawn.BLTeams.Unknown;
}
