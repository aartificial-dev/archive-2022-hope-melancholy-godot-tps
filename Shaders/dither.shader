shader_type canvas_item;

uniform vec2 resol = vec2(1280, 720);

uniform sampler2D DITHER_PATTERN;
uniform vec2 SCREEN_TEXEL_SIZE = vec2(320, 180);
uniform vec2 DITHER_TEXEL_SIZE = vec2(28, 4);
uniform float DITHER_COLORS = 64;

uniform bool applyChroma = true;
uniform float amountChroma = 0.1;
uniform bool applyVignette = true;
uniform float amountVignette = 0.1;

vec4 chromaticAbberation(sampler2D sTex, vec2 uv) {
	vec4 tex = texture(sTex, uv);
	vec4 col = tex;
	if (applyChroma) {
		float dis = distance(uv , vec2(0.5)) * amountChroma;
    
		col.r = texture(sTex, uv + (dis * amountChroma)).r;
		col.g = texture(sTex, uv).g;
		col.b = texture(sTex, uv - (dis * amountChroma)).b;
	} else {
		return tex;
	}
	return col;
}

vec4 vignette(vec2 uv, vec4 col) {
	if (applyVignette) {
		float vig = 1.0 - length(uv - vec2(0.5));
		vig = pow(abs(vig), amountVignette);
		return col * vec4(vig, vig, vig, 1.0);
	} else {
		return col;
	}
	
}

float channelError(float col, float colMin, float colMax) {
	float range = abs(colMin - colMax);
	float aRange = abs(col - colMin);
	return aRange / range;
}

float ditheredChannel(float error, vec2 ditherBlockUV, float ditherSteps) {
	error = floor(error * ditherSteps) / ditherSteps;
	vec2 ditherUV = vec2(error, 0);
	ditherUV.x += ditherBlockUV.x;
	ditherUV.y = ditherBlockUV.y;
	return texture(DITHER_PATTERN, ditherUV).x;
}

vec4 RGBtoYUV(vec4 rgba) {
	vec4 yuva;
	yuva.r = rgba.r * 0.2126 + 0.7152 * rgba.g + 0.0722 * rgba.b;
	yuva.g = (rgba.b - yuva.r) / 1.8556;
	yuva.b = (rgba.r - yuva.r) / 1.5748;
	yuva.a = rgba.a;

	// Adjust to work on GPU
	yuva.gb += 0.5;

	return yuva;
}

vec4 YUVtoRGB(vec4 yuva) {
    yuva.gb -= 0.5;
    return vec4(
        yuva.r * 1. + yuva.g * 0. + yuva.b * 1.5748,
        yuva.r * 1. + yuva.g * -0.187324 + yuva.b * -0.468124,
        yuva.r * 1. + yuva.g * 1.8556 + yuva.b * 0.,
        yuva.a);
}

void fragment() {
	// sample the texture and convert to YUV color space
	vec4 col = chromaticAbberation(TEXTURE, UV);
	col = vignette(UV, col);
//	vec4 col = texture(TEXTURE, UV);
	vec4 yuv = RGBtoYUV(col);

	// Clamp the YUV color to specified color depth (default: 32, 5 bits per channel, as per playstation hardware)
	vec4 col1 = floor(yuv * DITHER_COLORS) / DITHER_COLORS;
	vec4 col2 = ceil(yuv * DITHER_COLORS) / DITHER_COLORS;

	// Calculate dither texture UV based on the input texture
	float ditherSize = DITHER_TEXEL_SIZE.y;
	float ditherSteps = DITHER_TEXEL_SIZE.x / ditherSize;

	vec2 ditherBlockUV = UV;
	ditherBlockUV.x = mod(ditherBlockUV.x, (ditherSize / SCREEN_TEXEL_SIZE.x));
	ditherBlockUV.x /= (ditherSize / SCREEN_TEXEL_SIZE.x);
	ditherBlockUV.y = mod(ditherBlockUV.y, (ditherSize / SCREEN_TEXEL_SIZE.y));
	ditherBlockUV.y /= (ditherSize / SCREEN_TEXEL_SIZE.y);
	ditherBlockUV.x /= ditherSteps;

	// Dither each channel individually
	yuv.x = mix(col1.x, col2.x, ditheredChannel(channelError(yuv.x, col1.x, col2.x), ditherBlockUV, ditherSteps));
	yuv.y = mix(col1.y, col2.y, ditheredChannel(channelError(yuv.y, col1.y, col2.y), ditherBlockUV, ditherSteps));
	yuv.z = mix(col1.z, col2.z, ditheredChannel(channelError(yuv.z, col1.z, col2.z), ditherBlockUV, ditherSteps));

	COLOR = YUVtoRGB(yuv);
	
//	vec4 col = chromaticAbberation(TEXTURE, UV);
//	col = vignette(UV, col);
//	vec3 luminocity = vec3(0.299, 0.587, 0.114);
//	float luma = dot(col.rgb, luminocity);
//	COLOR = col * vec4(vec3(dither8x8(resol*UV, luma)), 1.0);
//	//COLOR = col;
//
}