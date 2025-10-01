shader_type canvas_item;

uniform vec4 outline_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);
uniform int thickness = 1; 

void fragment() {
	 vec4 src = texture(TEXTURE, UV);
    if (src.a > 0.0) {
        COLOR = src;
    } else {

        vec4 n1 = texture(TEXTURE, UV + vec2(TEXTURE_PIXEL_SIZE.x, 0.0));
        vec4 n2 = texture(TEXTURE, UV - vec2(TEXTURE_PIXEL_SIZE.x, 0.0));
        vec4 n3 = texture(TEXTURE, UV + vec2(0.0, TEXTURE_PIXEL_SIZE.y));
        vec4 n4 = texture(TEXTURE, UV - vec2(0.0, TEXTURE_PIXEL_SIZE.y));

        if (n1.a > 0.0 || n2.a > 0.0 ||
            n3.a > 0.0 || n4.a > 0.0) {
            COLOR = outline_color;
        } else {
            COLOR = vec4(0.0);
        }
    }
	
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
