sampler tex : register(s0);
sampler cover : register(s1);
float uProgress;

float4 PixelShaderFunction(float2 coords : TEXCOORD0) : COLOR0
{
    // 图上的任何白点都替换成cover上的对应点
    float4 texColor = tex2D(tex, coords);
    float3 one = float3(1, 1, 1);
    if (length(texColor.rgb - one) < 0.001)
        return coords.x > uProgress ? float4(0, 0, 0, 0) : tex2D(cover, coords);
    else
        return texColor;
}

technique Technique1
{
    pass Cover
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}