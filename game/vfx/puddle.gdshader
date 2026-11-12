shader_type spatial;

// A puddle on the floor: dark, glossy water with rings that spread from where
// something stepped in. The rings are fed by Puddle.cs.

const int MAX_RIPPLES = 8;

// xy: where on the puddle, in metres from its centre.
// z: progress, from 0 (just made) to 1 (gone).
uniform vec3 ripples[MAX_RIPPLES];

// Average distance from the centre to the water's edge, in metres.
uniform float radius = 0.8;

uniform vec3 water_color : source_color = vec3(0.04, 0.07, 0.09);
uniform vec3 crest_color : source_color = vec3(0.4, 0.45, 0.5);

// Metres a ring travels before it is gone.
const float REACH = 1.2;
// Metres between one crest and the next.
const float WAVELENGTH = 0.1;
// How far from its leading ring a ripple still shows, in metres. Gives 2 or 3 rings.
const float PACKET_WIDTH = 0.12;
// Height of a fresh crest, in metres. Only tilts the surface; the mesh stays flat.
const float AMPLITUDE = 0.004;

varying vec2 plane_pos;

void vertex() {
	plane_pos = VERTEX.xz;
}

void fragment() {
	// Uneven outline, so the puddle does not read as a perfect disc.
	float angle = atan(plane_pos.y, plane_pos.x);
	float edge = radius * (1.0 + 0.1 * sin(3.0 * angle + 1.0) + 0.06 * sin(5.0 * angle + 4.0));
	float inside = 1.0 - smoothstep(edge - 0.1, edge, length(plane_pos));

	float height = 0.0;
	vec2 slope = vec2(0.0);

	for (int i = 0; i < MAX_RIPPLES; i++) {
		float progress = ripples[i].z;
		if (progress >= 1.0) {
			continue;
		}

		vec2 offset = plane_pos - ripples[i].xy;
		float dist = length(offset);

		// A short train of waves that rides outwards and fades as it goes.
		float from_front = dist - progress * REACH;
		float packet = exp(-(from_front * from_front) / (PACKET_WIDTH * PACKET_WIDTH));
		float fade = (1.0 - progress) * (1.0 - progress);
		float phase = from_front * TAU / WAVELENGTH;

		height += fade * packet * cos(phase);

		// Derivative of the line above along the distance: how steep the water is here.
		float steepness = fade * packet * (
				-sin(phase) * TAU / WAVELENGTH
				- cos(phase) * 2.0 * from_front / (PACKET_WIDTH * PACKET_WIDTH));
		slope += steepness * offset / max(dist, 0.001);
	}

	// The rings tilt the surface, which moves the reflection of the lamp.
	vec3 tilted = normalize(vec3(-slope.x * AMPLITUDE, 1.0, -slope.y * AMPLITUDE));
	NORMAL = normalize(mat3(VIEW_MATRIX) * mat3(MODEL_MATRIX) * tilted);

	// Crests also catch a little light, so the rings show away from the lamp too.
	ALBEDO = water_color + crest_color * clamp(height, 0.0, 1.0);
	ROUGHNESS = 0.15;
	SPECULAR = 0.6;
	ALPHA = inside * 0.6;
}
