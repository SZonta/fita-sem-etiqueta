shader_type canvas_item;

// Full-screen VHS look, drawn over the finished image: the picture of an old
// camera, with nothing that flashes or rolls across the screen.
// Everything scales with "intensity": at 0 the image passes through untouched.

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_linear;
uniform float intensity : hint_range(0.0, 1.0) = 0.7;

// A VHS picture has about 480 visible lines and its grain changes 30 times per second.
const float LINES = 480.0;
const float NOISE_FPS = 30.0;

// Pseudo-random number from 0 to 1 for a given cell.
float hash(vec2 cell) {
	return fract(sin(dot(cell, vec2(12.9898, 78.233))) * 43758.5453);
}

// Soft noise: random values at whole coordinates, blended smoothly in between.
float soft_noise(vec2 p) {
	vec2 cell = floor(p);
	vec2 blend = fract(p);
	blend = blend * blend * (3.0 - 2.0 * blend);
	return mix(
		mix(hash(cell), hash(cell + vec2(1.0, 0.0)), blend.x),
		mix(hash(cell + vec2(0.0, 1.0)), hash(cell + vec2(1.0, 1.0)), blend.x),
		blend.y);
}

// YIQ splits a color into brightness (y) and color (i, q), the way tape stores it.
vec3 rgb_to_yiq(vec3 rgb) {
	return vec3(
		dot(rgb, vec3(0.299, 0.587, 0.114)),
		dot(rgb, vec3(0.596, -0.274, -0.322)),
		dot(rgb, vec3(0.211, -0.523, 0.312)));
}

vec3 yiq_to_rgb(vec3 yiq) {
	return vec3(
		dot(yiq, vec3(1.0, 0.956, 0.621)),
		dot(yiq, vec3(1.0, -0.272, -0.647)),
		dot(yiq, vec3(1.0, -1.106, 1.703)));
}

float luma_at(vec2 uv) {
	return rgb_to_yiq(texture(screen_texture, uv).rgb).x;
}

vec2 chroma_at(vec2 uv) {
	return rgb_to_yiq(texture(screen_texture, uv).rgb).yz;
}

void fragment() {
	// Wraps around so the numbers fed to hash() stay small and precise.
	float noise_frame = mod(floor(TIME * NOISE_FPS), 100.0);
	float scanline = floor(SCREEN_UV.y * LINES);
	vec2 uv = SCREEN_UV;

	// The picture sways very slowly, and its last lines sit slightly to the side
	// (where the tape heads switch). Both are steady: nothing jumps.
	uv.x += sin(SCREEN_UV.y * 4.0 + TIME * 1.7) * 0.0005 * intensity;
	uv.x += smoothstep(0.985, 1.0, SCREEN_UV.y) * 0.02 * intensity;

	// Brightness: slightly blurred sideways, then given the hard halo around
	// edges that a VCR "sharpness" circuit adds.
	vec2 luma_step = vec2(intensity / 500.0, 0.0);
	float blurred = (luma_at(uv - luma_step) + luma_at(uv) + luma_at(uv + luma_step)) / 3.0;
	float wide = (luma_at(uv - 3.0 * luma_step) + luma_at(uv + 3.0 * luma_step)) * 0.5;
	float luma = blurred + (blurred - wide) * 0.6 * intensity;

	// Color: very blurry, arriving late (so it smears to the right, hence -5..2)
	// and leaking down from the lines above.
	float chroma_step = intensity / 150.0;
	vec2 line_above = vec2(0.0, 2.0 * intensity / LINES);
	vec2 chroma = vec2(0.0);
	for (int i = -5; i <= 2; i++) {
		vec2 tap_uv = uv + vec2(float(i) * chroma_step, 0.0);
		chroma += chroma_at(tap_uv) + chroma_at(tap_uv - line_above);
	}
	chroma /= 16.0;

	// Grain: brightness noise in short streaks, color noise in longer ones.
	float streak_shift = hash(vec2(scanline, noise_frame)) * 60.0;
	luma += (soft_noise(vec2(uv.x * 210.0 + streak_shift, scanline * 0.5 + noise_frame * 3.1)) - 0.5) * 0.065 * intensity;
	vec2 chroma_noise = vec2(
		soft_noise(vec2(uv.x * 35.0 + noise_frame * 1.7, scanline * 0.4 + noise_frame)),
		soft_noise(vec2(uv.x * 35.0 - noise_frame * 2.3, scanline * 0.4 + noise_frame + 40.0)));
	chroma += (chroma_noise - 0.5) * 0.06 * intensity;

	// Tape cannot hold pure black or pure white, and it oversaturates color.
	luma = mix(luma, luma * 0.82 + 0.07, intensity);
	chroma *= mix(1.0, 1.15, intensity);

	COLOR = vec4(yiq_to_rgb(vec3(luma, chroma)), 1.0);
}
