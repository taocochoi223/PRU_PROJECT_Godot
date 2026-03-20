shader_type canvas_item;

uniform vec4 shadow_color : source_color = vec4(0.0, 0.0, 0.0, 0.5);
uniform vec2 offset = vec2(5.0, 5.0);
uniform float blur : hint_range(0.0, 10.0) = 0.0;

void fragment() {
    vec4 col = texture(TEXTURE, UV);
    vec4 shadow = texture(TEXTURE, UV - offset * TEXTURE_PIXEL_SIZE);
    shadow.rgb = shadow_color.rgb;
    shadow.a *= shadow_color.a;
    
    // Mix shadow and texture, then multiply by vertex color (modulate/fade)
    COLOR = mix(shadow, col, col.a) * COLOR;
}
