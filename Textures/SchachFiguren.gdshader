shader_type spatial;

uniform bool isPlayer;

uniform sampler2D emission_texure;
const vec3 emission_colors[6] = vec3[6]
(
	vec3(1.0,1.0,1.0), //Rarity 0 Common White
	vec3(0.0,1.0,0.0), //Rarity 1 Uncommon Green
	vec3(0.0,0.0,1.0), //Rarity 2 Rare Blue
	vec3(1.0,0.0,1.0), //Rarity 3 Epic Purple
	vec3(1.0,(20.0/255.0),0.0), //Rarity 4 Legendary Orange
	vec3(1.0,0.0,0.0)  //Rarity 5 Blasphemy Red
);
uniform int rarity : hint_range(0,5,1);
uniform int emission_intensity : hint_range(4, 20, 2); //Recommended 10
uniform bool hasEmission;

vec3 clamp_color(vec3 color) 
{
	return step(0.5,color) * color;
}

void fragment() {
	ALBEDO = ALBEDO.rgb * float(isPlayer);
	
	EMISSION = clamp_color(texture(emission_texure, UV).rgb) * emission_colors[rarity].rgb * float(emission_intensity) * float(hasEmission);
}
