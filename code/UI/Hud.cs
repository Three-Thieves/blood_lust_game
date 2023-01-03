using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace BloodLust.UI;

public partial class BloodHud : RootPanel
{
	public static BloodHud Current;

	public BloodHud()
	{
		Current?.Delete();
		Current = null;

		StyleSheet.Load( "UI/Styles/BloodHud.scss" );
		AddChild<BloodChat>();

		Current = this;
	}
}
