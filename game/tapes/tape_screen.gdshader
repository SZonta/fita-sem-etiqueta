// Picture of the test TV while a tape plays: the tape's feed (a 320x240 viewport) with the defects
// of a worn rental tape, as chosen by the owner (option "C", delivery 6):
// - the last lines torn sideways (head switching), mostly hidden by the tube's overscan;
// - at the cut from the film to a home recording, the picture rolls and a tracking band passes, ~0.6 s;
// - the home recording is a copy: softer, colour smeared to the right, a little line jitter, gain grain;
// - rare white dropouts.
// Then blank tape (snow) and the VCR's blue screen. The game's VHS filter still goes over everything.
shader_type spatial;
render_mode unshaded;

// Read as stored (sRGB): every effect below works on sRGB values, like the test page the owner chose
// from, and the result is turned to linear only at the end. Noise added to linear values swamps the dark.
uniform sampler2D picture : filter_linear, repeat_disable;
// 0 film, 1 home recording, 2 snow (blank tape), 3 blue screen.
uniform int segment = 0;
// Seconds since the home recording began (negative or large when not around the cut).
uniform float since_splice = 100.0;
// Video frame number (30 per second), for the noise.
uniform float frame = 0.0;
uniform float time_s = 0.0;

// Share of the picture hidden behind the bezel on each side (CRT overscan).
const float OVERSCAN = 0.03;
const float LINES = 240.0;

float h1(float n) { return fract(sin(n * 127.1) * 43758.5453); }
float h2(vec2 p) { return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453); }
vec3 to_yiq(vec3 c) { return vec3(dot(c, vec3(0.299, 0.587, 0.114)), dot(c, vec3(0.596, -0.274, -0.322)), dot(c, vec3(0.211, -0.523, 0.312))); }
vec3 to_rgb(vec3 y) { return vec3(y.x + 0.956 * y.y + 0.621 * y.z, y.x - 0.272 * y.y - 0.647 * y.z, y.x - 1.106 * y.y + 1.703 * y.z); }

void fragment() {
	vec2 uv = mix(vec2(OVERSCAN), vec2(1.0 - OVERSCAN), UV);
	vec3 c;
	if (segment == 3) {
		c = vec3(0.07, 0.16, 0.62);
	} else {
		float line = floor(uv.y * LINES);
		float home = segment == 1 ? 1.0 : 0.0;
		vec2 q = uv;

		// The cut: the picture rolls and tears for 0.65 s.
		float splice = smoothstep(0.65, 0.0, since_splice) * step(0.0, since_splice);
		q.y = fract(q.y + splice * (1.0 - since_splice / 0.65) * 0.9);

		// Line jitter on the copy.
		q.x += 0.5 * home * (h1(line + frame * 7.0) - 0.5) * 0.006;
		q.x += 0.5 * home * sin(uv.y * 30.0 + time_s * 3.0) * 0.002;

		// Head switching: the last lines torn sideways.
		float hs = smoothstep(0.955, 1.0, uv.y);
		q.x += hs * (0.03 + 0.03 * h1(line + frame));

		// Tracking band, only at the cut.
		float tear = splice * smoothstep(0.08, 0.0, abs(uv.y - fract(0.3 + since_splice * 1.7)));
		q.x += tear * (h1(line + frame * 3.0) - 0.3) * 0.08;

		// The copy: softer, with the colour smeared to the right.
		vec3 p = texture(picture, q).rgb;
		vec3 pl = texture(picture, q - vec2(0.004 * home, 0.0)).rgb;
		vec3 pr = texture(picture, q + vec2(0.004 * home, 0.0)).rgb;
		vec3 y = to_yiq(mix(p, (p + pl + pr) / 3.0, home));
		vec3 chroma = to_yiq(texture(picture, q - vec2(0.012 * home, 0.0)).rgb);
		y.yz = mix(y.yz, chroma.yz, home * 0.8) * (1.0 - 0.15 * home);
		c = to_rgb(y);

		// Grain, one speck per picture element (320x240), not per screen pixel: high gain at night on
		// the home recording, barely any on the film.
		c += (h2(floor(q * vec2(320.0, LINES)) + frame) - 0.5) * (home > 0.5 ? 0.08 : 0.015);

		if (segment == 2) {
			c = vec3(h2(floor(uv * vec2(320.0, LINES)) + frame * 1.7)) * 0.8;
		}

		c = mix(c, vec3(h2(vec2(floor(uv.x * 320.0), line) + frame)), tear * 0.7 + hs * 0.5);

		// Dropouts: short white streaks, now and then.
		float slot = floor(frame / 2.0);
		float dl = floor(h1(slot * 3.1) * LINES);
		float dx = h1(slot * 5.7);
		float hit = step(1.0 - 0.3 * 0.35, h1(slot * 9.3)) * step(abs(line - dl), 0.5) * step(abs(uv.x - dx), 0.04 + 0.05 * h1(slot));
		c = mix(c, vec3(1.0), hit * 0.9);
	}

	// The tube darkens a little towards its corners.
	vec2 edge = UV - 0.5;
	c *= 1.0 - 0.35 * dot(edge, edge);
	ALBEDO = pow(clamp(c, 0.0, 1.0), vec3(2.2));
}
