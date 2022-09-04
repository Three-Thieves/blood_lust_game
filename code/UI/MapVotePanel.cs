﻿
using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[UseTemplate]
class MapVotePanel : Panel
{
	public string TitleText { get; set; } = "Map Vote";
	public string SubtitleText { get; set; } = "Select next map";
	public string TimeText { get; set; } = "00:23";

	public Panel Body { get; set; }

	public List<MapIcon> MapIcons = new();

	public MapVotePanel()
	{
		StyleSheet.Load( "UI/Styles/MapVotePanel.scss" );
		SetTemplate( "UI/HTML/MapVotePanel.html" );
		_ = PopulateMaps();
	}

	public async Task PopulateMaps()
	{
		var query = new Package.Query
		{
			Type = Package.Type.Map,
			Order = Package.Order.User,
			Take = 16,
		};

		query.Tags.Add( "game:thieves.bloodlust" );

		var packages = await query.RunAsync( default );

		foreach ( var package in packages )
		{
			AddMap( package.FullIdent );
		}
	}

	private MapIcon AddMap( string fullIdent )
	{
		var icon = MapIcons.FirstOrDefault( x => x.Ident == fullIdent );

		if ( icon != null )
			return icon;

		icon = new MapIcon( fullIdent );
		icon.AddEventListener( "onmousedown", () => MapVoteEntity.SetVote( fullIdent ) );
		Body.AddChild( icon );

		MapIcons.Add( icon );
		return icon;
	}

	public override void Tick()
	{
		base.Tick();
	}

	internal void UpdateFromVotes( IDictionary<Client, string> votes )
	{
		foreach ( var icon in MapIcons )
			icon.VoteCount = "0";

		foreach ( var group in votes.GroupBy( x => x.Value ).OrderByDescending( x => x.Count() ) )
		{
			var icon = AddMap( group.Key );
			icon.VoteCount = group.Count().ToString( "n0" );
		}
	}
}

