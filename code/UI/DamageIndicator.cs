using Sandbox;
using Sandbox.UI;
using System;
using System.Threading.Tasks;
public partial class DamageIndicator : Panel
{
	public static DamageIndicator Current;

	public DamageIndicator()
	{
		Current = this;
	}

	public void OnHit()
	{
		var p = new HitPoint();
		p.Parent = this;
	}

	public class HitPoint : Panel
	{
		public HitPoint()
		{
			StyleSheet.Load( "/UI/Styles/damageindicator.scss" );
			_ = Lifetime();
		}

		async Task Lifetime()
		{
			await Task.Delay( 200 );
			AddClass( "dying" );
			await Task.Delay( 200 );
			Delete();
		}


	}
}


