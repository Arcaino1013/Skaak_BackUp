shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
uniform float radius : hint_range(1,10,1);

vec2 CalculateOffset(vec2 uv,vec2 pixel_size, vec2 dir) 
{
	return vec2(clamp(uv.x + (pixel_size.x * radius * dir.x),float(0),float(1)),clamp(uv.y + (pixel_size.y * radius * dir.y),float(0),float(1)));
}

vec3 FillColors(vec2 uv) 
{
	return texture(screen_texture,uv).rgb;
}

void fragment() {
	        int _Kernel[9] = {1,2,1,
                      		  2,4,2,
                      		  1,2,1};
        vec3 colors[9];
        vec3 color = vec3(0.0);//SAMPLE_TEXTURE2D_X(_MainTex, s_linear_clamp_sampler, input.texcoord).xyz;
		for(int i = 0; i < 9; i++) 
		{
			vec2 offsetDir;
			switch(i%3) 
			{
				case 0:
					offsetDir.x = -1.0;
					break;
				case 1:
					offsetDir.x = 0.0;
					break;
				case 2:
					offsetDir.x = 1.0;
					break;
			}
			if(i <= 6) 
			{
				offsetDir.y = -1.0;
			} else if(i <= 3) 
			{
				offsetDir.y = 0.0;
			} else {offsetDir.y = 1.0;}
			
			colors[i] = FillColors(CalculateOffset(UV,SCREEN_PIXEL_SIZE,offsetDir));	
		}  
        for (int i = 0; i < 9; i++)
        {
        color = color + (colors[i]) * float(_Kernel[i]);
        }
        color = clamp(color / float(16),float(0),float(1));
        COLOR = vec4(color, 1.0);
}