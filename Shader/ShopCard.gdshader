shader_type canvas_item;

const bool onStart = true;
uniform bool hide;
varying float a; 

void fragment() {
	if(onStart) 
	{
		a = COLOR.a;
	}
	if(hide) 
	{
		COLOR = vec4(COLOR.rgb,0);
	} else 
	{
		COLOR = vec4(COLOR.rgb,a);
	}
}
