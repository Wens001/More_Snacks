#ifndef STFUNCTIONS_INCLUDED
#define STFUNCTIONS_INCLUDED

float clamp01 (float value) {
	return clamp(value, 0, 1);
}

float rev (float value) {
	return 1.0 - value;
}

float rev01 (float value) {
	return clamp01(rev(value));
}

float pos (float value) {
	return value > 0 ? 1 : 0;
}

float posz (float value) {
	return value >= 0 ? 1 : 0;
}

float neg (float value) {
	return value < 0 ? 1 : 0;
}

float negz (float value) {
	return value <= 0 ? 1 : 0;
}

float lerp01 (float from, float to, float value) {
	return clamp01(lerp(from, to, value));
}

float invLerp (float from, float to, float value, float equal = 0.) {
	float val = (value - from) / (to - from);
	return from == to ? equal : val;
}

float invLerp01 (float from, float to, float value, float equal = 0.) {
	float val = invLerp(from, to, value, equal);
	return from == to ? val : clamp01(val);
}

float wght_invLerp (float from, float to, float value, bool invert = false) {
	float val = (value - from) / (to - from);

	float wgtMin = !invert ? 0 : 1;
	float wgtMax = !invert ? 1 : 0;
	float wgt = value < from ? wgtMin : wgtMax;

	float res = value == from ? 0.5 : wgt;
	return from == to ? res : val;
}

float smoothstep (float from, float to, float value, float equal) {
	float val = smoothstep(from, to, value);
	return from == to ? equal : val;
}

float wght_smoothstep (float from, float to, float value, bool invert = false) {
	float val = smoothstep(from, to, value);

	float wgtMin = !invert ? 0 : 1;
	float wgtMax = !invert ? 1 : 0;
	float wgt = value < from ? wgtMin : wgtMax;

	float res = value == from ? 0.5 : wgt;
	return from == to ? res : val;
}

float smoothlerp (float from, float to, float value) {
	float val = -(2.0 / ((value + 0.34) * 4.7)) + 1.3;
	return lerp01(from, to, val);
}

float colmagnmin (float3 color) {
	float m1 = min(color.r, color.g);
	return min(m1, color.b);
}

float colmagnmax (float3 color) {
	float m1 = max(color.r, color.g);
	return max(m1, color.b);
}

float colspacemax (float3 color) {
	return rev(colmagnmin(color));
}

float colspacemin (float3 color) {
	return rev(colmagnmax(color));
}

float4 ColorBlend (float4 tcol, float4 dcol, float blendf)
{
	float4 res = tcol;
	res.r = lerp(tcol.r, dcol.r, blendf);
	res.g = lerp(tcol.g, dcol.g, blendf);
	res.b = lerp(tcol.b, dcol.b, blendf);
	res.a = lerp(tcol.a, dcol.a, blendf);

	return res;
}

fixed4 texNoTileTech1(sampler2D tex, sampler2D _NoiseTex, float2 uv ,float _BlendRatio) {
	float2 iuv = floor(uv);
	float2 fuv = frac(uv);

	// Generate per-tile transformation
#if defined (USE_HASH)
	float4 ofa = hash4(iuv + float2(0, 0));
	float4 ofb = hash4(iuv + float2(1, 0));
	float4 ofc = hash4(iuv + float2(0, 1));
	float4 ofd = hash4(iuv + float2(1, 1));
#else
	float4 ofa = tex2D(_NoiseTex, (iuv + float2(0.5, 0.5)) / 256.0);
	float4 ofb = tex2D(_NoiseTex, (iuv + float2(1.5, 0.5)) / 256.0);
	float4 ofc = tex2D(_NoiseTex, (iuv + float2(0.5, 1.5)) / 256.0);
	float4 ofd = tex2D(_NoiseTex, (iuv + float2(1.5, 1.5)) / 256.0);
#endif

	// Compute the correct derivatives
	float2 dx = ddx(uv);
	float2 dy = ddy(uv);

	// Mirror per-tile uvs
	ofa.zw = sign(ofa.zw - 0.5);
	ofb.zw = sign(ofb.zw - 0.5);
	ofc.zw = sign(ofc.zw - 0.5);
	ofd.zw = sign(ofd.zw - 0.5);

	float2 uva = uv * ofa.zw + ofa.xy, dxa = dx * ofa.zw, dya = dy * ofa.zw;
	float2 uvb = uv * ofb.zw + ofb.xy, dxb = dx * ofb.zw, dyb = dy * ofb.zw;
	float2 uvc = uv * ofc.zw + ofc.xy, dxc = dx * ofc.zw, dyc = dy * ofc.zw;
	float2 uvd = uv * ofd.zw + ofd.xy, dxd = dx * ofd.zw, dyd = dy * ofd.zw;

	// Fetch and blend
	float2 b = smoothstep(_BlendRatio, 1.0 - _BlendRatio, fuv);

	return lerp(lerp(tex2D(tex, uva, dxa, dya), tex2D(tex, uvb, dxb, dyb), b.x),
		lerp(tex2D(tex, uvc, dxc, dyc), tex2D(tex, uvd, dxd, dyd), b.x), b.y);
}

fixed4 texNoTileTech2(sampler2D tex,sampler2D _NoiseTex, float2 uv,float _BlendRatio) {
	float2 iuv = floor(uv);
	float2 fuv = frac(uv);

	// Compute the correct derivatives for mipmapping
	float2 dx = ddx(uv);
	float2 dy = ddy(uv);

	// Voronoi contribution
	float4 va = 0.0;
	float wt = 0.0;
	float blur = -(_BlendRatio + 0.5) * 30.0;
	for (int j = -1; j <= 1; j++) {
		for (int i = -1; i <= 1; i++) {
			float2 g = float2((float)i, (float)j);
#if defined (USE_HASH)
			float4 o = hash4(iuv + g);
#else
			float4 o = tex2D(_NoiseTex, (iuv + g + float2(0.5, 0.5)) / 256.0);
#endif
			// Compute the blending weight proportional to a gaussian fallof
			float2 r = g - fuv + o.xy;
			float d = dot(r, r);
			float w = exp(blur * d);
			float4 c = tex2D(tex, uv + o.zw, dx, dy);
			va += w * c;
			wt += w;
		}
	}
	return va / wt;
}

float4 GreyColor(float4 _srcColor) {
	float grey = dot(_srcColor.rgb, float3(0.22, 0.707, 0.071));
	return float4(grey, grey, grey, _srcColor.a);
}

#endif
