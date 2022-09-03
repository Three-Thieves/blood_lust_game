// Created by Crowbar 0.71

$modelname "weapons\w_shot_nova.mdl"

$bodygroup "studio"
{
	studio "w_shot_nova_nobayo.smd"
}


$surfaceprop "weapon"

$contents "solid"

$illumposition 8.693 0 1.384

$cdmaterials "models\weapons\v_models\1897win\"

$attachment "muzzle_flash" "ValveBiped.flash" 0 0 0 rotate 0 0 0
$attachment "shell_eject" "ValveBiped.shelleject" 0 0 0 rotate 0 0 0

$cbox 0 0 0 0 0 0

$bbox -12.781 -1.329 -3.102 31.527 1.33 6.028

$hboxset "default"
$hbox 0 "ValveBiped.weapon_bone" -2.031 -6.821 -15.596 0.128 1.967 26.853001
$hbox 0 "ValveBiped.weapon_bone_Slide" -1.011 -1.16 -0.646 1.011 1.452 11.823

$definebone "ValveBiped" "" 0 0 0 0 0 89.999982 0 0 0 0 0 0
$definebone "ValveBiped.weapon_bone" "ValveBiped" 0.951702 3.811288 3.064471 0 0 0 0 0 0 0 0 0
$definebone "ValveBiped.flash" "ValveBiped.weapon_bone" -0.951498 0.464882 26.853096 -89.999982 -89.999982 0 0 0 0 0 0 0
$definebone "ValveBiped.weapon_bone_Slide" "ValveBiped.weapon_bone" -0.951702 -0.979782 8.416632 0 0 0 0 0 0 0 0 0
$definebone "ValveBiped.shelleject" "ValveBiped.weapon_bone" -0.951701 0.558536 5.695249 0 -179.999964 0 0 0 0 0 0 0


$sectionframes 30 100

$sequence "idle" {
	"w_shot_nova_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}

$collisionmodel "w_shot_nova_physics.smd"
{
	$mass 10
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "

}

$keyvalues
{
	prop_data 
	{
		"base"		"Metal.Medium"
	}
}
