shader_type canvas_item;

uniform sampler2D mask_sampler;
uniform vec2 mask_offset = vec2(0,0);

void fragment() {
	vec4 mask_color = texture(mask_sampler, UV);
	COLOR.a = mask_color.g;
}