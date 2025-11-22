shader_type canvas_item;

//uniform vec4 line_color = vec4(1);
uniform float line_thickness : hint_range(0, 10) = 2.0;
uniform int rarity: hint_range(0, 6, 1);

const vec4 line_colors[6] = vec4[](
	vec4(1.0, 1.0, 1.0, 1.0), // White
	vec4(0.0, 1.0, 0.0, 1.0), // Green
    vec4(0.0, 0.0, 1.0, 1.0), // Blue
	vec4(0.627, 0.125, 0.941, 1.0), // Purple
    vec4(0.917, 0.737, 0.145, 1.0) , //Gold
	vec4(1.0, 0.0, 0.0, 1.0) // Red
);

void fragment() {
	vec2 size = TEXTURE_PIXEL_SIZE * line_thickness;
	if(rarity != 0) 
	{
	vec4 line_color = line_colors[rarity -1];
	float outline = texture(TEXTURE, UV + vec2(-size.x, 0)).a;
	outline += texture(TEXTURE, UV + vec2(0, size.y)).a;
	
	outline += texture(TEXTURE, UV + vec2(size.x, 0)).a;
	
	outline += texture(TEXTURE, UV + vec2(0, -size.y)).a;
	
	outline = min(outline, 1.0);
	
	vec4 color = texture(TEXTURE, UV);
	COLOR = mix(color, line_color, outline - color.a);
	}
}