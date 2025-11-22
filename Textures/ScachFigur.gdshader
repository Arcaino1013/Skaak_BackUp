shader_type spatial;

uniform sampler2D mask : hint_default_black;
uniform sampler2D emission_Texture : source_color;
uniform vec3 emission_Color = vec3(1.0,0.0,1.0);
uniform float emission_Intensity = 0.2;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	
	ALBEDO = vec3(1.0,1.0,1.0);
	ROUGHNESS = 0.1;
	RIM = 1.0;
	//if(sin(UV.y * exp(3)*25.0) > 0.0) {EMISSION = emission_Color * emission_Intensity;}
	//EMISSION = ;
}

