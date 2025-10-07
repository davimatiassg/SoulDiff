shader_type canvas_item;

uniform float time_scale : hint_range(0.1, 5.0) = 1.5;
uniform vec4 anger_color : source_color = vec4(1.0, 0.2, 0.1, 1.0);
uniform float aura_intensity : hint_range(0.0, 1.0) = 0.6;
uniform float color_pulse_strength : hint_range(0.0, 1.0) = 0.4;


uniform vec4 outline_color : source_color = vec4(0.0);
void fragment() {

	vec4 src = texture(TEXTURE, UV);
    if (src.a > 0.0) {
        COLOR = src;
    

        // Animação base
        float t = TIME * time_scale;


        // Aura flamejante ao redor (borda animada)
        float dist = length(UV - vec2(0.5));
        float aura = smoothstep(0.45, 0.55 + 0.05 * sin(t * 3.0 + UV.y * 10.0), dist);
        aura = (1.0 - aura) * aura_intensity;

        // Pulso de cor (vermelho oscilante)
        float pulse = (sin(t * 5.0) * 0.5 + 0.5) * color_pulse_strength;
        vec4 pulse_color = mix(src, anger_color, pulse);

        // Combina tudo
        COLOR = mix(pulse_color, anger_color, aura);
        COLOR.a *= src.a;

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