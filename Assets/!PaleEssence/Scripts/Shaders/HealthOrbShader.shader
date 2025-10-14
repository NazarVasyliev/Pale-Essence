Shader "Custom/HealthOrbShader"
{
    Properties
    {
        _MainTex ("Texture (Unused, but needed for UI)", 2D) = "white" {}
        _Health ("Health (0-1)", Range(0.0, 1.0)) = 1.0
        _HealthChange ("Health Change", Range(-1.0, 1.0)) = 0.0
        _HealthAnimSpeed ("Health Animation Speed", Range(0.1, 10.0)) = 2.0
        _SplashImpulse ("Splash Impulse Strength", Range(0.0, 2.0)) = 0.5
        _LiquidColor ("Liquid Highlight Color", Color) = (1,0,0,1)
        _LiquidBaseColor ("Liquid Base Color", Color) = (0.3,0,0,1)
        _BubbleColor ("Bubble Color", Color) = (1,0.5,0,1)
        _BackgroundColor ("Background Color", Color) = (0.1,0.1,0.1,0.8)

        _LiquidSpeedX ("Liquid Scroll Speed X", Float) = 0.05
        _LiquidSpeedY ("Liquid Scroll Speed Y", Float) = 0.03
        _DistortionSpeed ("Distortion Scroll Speed", Float) = 0.1
        _BubbleSpeedX ("Bubble Scroll Speed X", Float) = -0.1
        _BubbleSpeedY ("Bubble Scroll Speed Y", Float) = 0.15

        _LiquidVisibleMinY ("Liquid Visible Min Y", Range(0.0, 1.0)) = 0.0
        _LiquidVisibleMaxY ("Liquid Visible Max Y", Range(0.0, 1.0)) = 1.0
        _WaveSuppressionEdgeThreshold ("Wave Suppression Edge", Range(0.001, 0.5)) = 0.1 

        _NoiseScaleLiquid ("Liquid Noise Scale", Float) = 5.0
        _NoiseScaleDistortion ("Distortion Noise Scale", Float) = 7.0
        _NoiseScaleBubbles ("Bubble Noise Scale", Float) = 10.0

        _DistortionAmount ("Liquid Distortion Amount", Range(0.0, 0.2)) = 0.05
        _BubbleDensity ("Bubble Density/Sharpness", Range(1, 30)) = 15.0
        _EdgeSoftness("Edge Softness", Range(0.001, 0.1)) = 0.01
        _SplashAmplitude ("Splash Amplitude (on HP change)", Range(0, 1)) = 0

        _SurfaceWaveFrequency ("Surface Wave Frequency", Float) = 30.0
        _SurfaceWaveSpeed ("Surface Wave Speed", Float) = 4.0
        _SurfaceWaveHeight ("Surface Wave Base Height", Float) = 0.01
        _SurfaceWaveSplashBoost ("Wave Boost On Splash", Float) = 0.03
        _SurfaceNoiseStrength ("Noise Strength", Float) = 0.02

        _TimeSinceLastHPChange ("Time Since Last HP Change (Script)", Float) = 100.0 
        _InitialSplashMagnitude ("Initial Splash Magnitude (Script)", Range(0,1)) = 0.0
        _SplashOverallDuration ("Overall Splash Duration", Range(0.1, 5.0)) = 1.5      
        _SplashImpactDurationFactor ("Impact Phase Duration Factor", Range(0.1, 0.8)) = 0.3 
        _SplashDampingFactor ("Splash Damping Factor", Range(0.1, 10.0)) = 3.0  
        _SplashMaxHeight ("Max Splash Height", Range(0.0, 0.15)) = 0.04         
        _ImpactWaveFreq ("Impact Wave Frequency", Float) = 12.0                 
        _ImpactWaveSpeed ("Impact Wave Speed", Float) = 7.0                    
        _RippleWaveFreqFactor ("Ripple Wave Freq Factor", Float) = 1.5     
        _RippleWaveSpeedFactor ("Ripple Wave Speed Factor", Float) = 1.1   
        _SplashDistortionBoost("Splash Distortion Boost", Range(0.0, 5.0)) = 1.5 

        _SphereDistortionAmount ("Sphere Content Distortion", Range(0.0, 1.0)) = 0.3
        _SphereDistortionCurve ("Sphere Distortion Curve", Range(0.1, 3.0)) = 1.0

        _BubbleColor2 ("Bubble Color 2", Color) = (0.5,1.0,1.0,0.8)
        _BubbleSpeedX2 ("Bubble Scroll Speed X 2", Float) = 0.08
        _BubbleSpeedY2 ("Bubble Scroll Speed Y 2", Float) = -0.05
        _NoiseScaleBubbles2 ("Bubble Noise Scale 2", Float) = 15.0
        _BubbleDensity2 ("Bubble Density/Sharpness 2", Range(1, 30)) = 18.0

        _GlassHighlightColor ("Glass Highlight Color", Color) = (1,1,1,0.7)
        _GlassShininess ("Glass Shininess", Range(1, 256)) = 40
        _LightDirectionX ("Light Dir X (View Space)", Range(-1,1)) = 0.4
        _LightDirectionY ("Light Dir Y (View Space)", Range(-1,1)) = 0.6
        _LightDirectionZ ("Light Dir Z (View Space)", Range(-1,1)) = -0.7

        _EffectIntensity ("Effect Intensity", Range(0.0, 1.0)) = 0.0
        _EffectColor ("Effect Color (Flash/Tint)", Color) = (1,1,0.8, 0.3)
        _EffectSpeedBoost ("Effect Speed Boost Factor", Range(1.0, 5.0)) = 2.5

        _CrescentColor ("Crescent Highlight Color", Color) = (0.9,0.9,1.0,0.25)
        _CrescentShapeOffset ("Crescent Shape Offset XY", Vector) = (0.12, 0.08, 0, 0)
        _CrescentOuterRadius ("Crescent Outer Radius", Range(0.01, 0.5)) = 0.46
        _CrescentInnerRadiusScale ("Crescent Inner Radius Scale", Range(0.5, 1.99)) = 0.85
        _CrescentSoftness ("Crescent Edge Softness", Range(0.001, 0.2)) = 0.04
        
        _LiquidTintTop ("Liquid Tint Top Multiplier", Range(0.5, 2.0)) = 1.1
        _LiquidTintBottom ("Liquid Tint Bottom Multiplier", Range(0.2, 2.0)) = 0.7
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; float4 color : COLOR; };

            sampler2D _MainTex;
            float _Health;
            float _HealthChange;
            float _HealthAnimSpeed;
            float _SplashImpulse;
            float _WaveSuppressionEdgeThreshold;
            fixed4 _LiquidColor, _LiquidBaseColor, _BubbleColor, _BackgroundColor;
            float _LiquidSpeedX, _LiquidSpeedY, _DistortionSpeed, _BubbleSpeedX, _BubbleSpeedY;
            float _NoiseScaleLiquid, _NoiseScaleDistortion, _NoiseScaleBubbles;
            float _DistortionAmount, _BubbleDensity, _EdgeSoftness;
            float _SphereDistortionAmount, _SphereDistortionCurve;
            float _SurfaceWaveFrequency;
            float _SurfaceWaveSpeed;
            float _SurfaceWaveHeight;
            float _SurfaceWaveSplashBoost;
            float _SurfaceNoiseStrength;

            float _TimeSinceLastHPChange;
            float _InitialSplashMagnitude;
            float _SplashOverallDuration;
            float _SplashImpactDurationFactor;
            float _SplashDampingFactor;
            float _SplashMaxHeight;
            float _ImpactWaveFreq;
            float _ImpactWaveSpeed;
            float _RippleWaveFreqFactor;
            float _RippleWaveSpeedFactor;
            float _SplashDistortionBoost;


            float _LiquidVisibleMinY;
            float _LiquidVisibleMaxY;

            float _SplashAmplitude;
            fixed4 _BubbleColor2;
            float _BubbleSpeedX2, _BubbleSpeedY2, _NoiseScaleBubbles2, _BubbleDensity2;
            fixed4 _GlassHighlightColor; 
            float _GlassShininess;
            float _LightDirectionX, _LightDirectionY, _LightDirectionZ;
            float _EffectIntensity; 
            fixed4 _EffectColor; 
            float _EffectSpeedBoost;
            fixed4 _CrescentColor;
            float4 _CrescentShapeOffset;
            float _CrescentOuterRadius;
            float _CrescentInnerRadiusScale;
            float _CrescentSoftness;
            float _LiquidTintTop, _LiquidTintBottom;

            float2 random2(float2 p) { return frac(sin(float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)))) * 43758.5453); }
            
            float simplexNoise(float2 uv) { 
                float2 i = floor(uv); 
                float2 f = frac(uv); 
                f = f * f * (3.0 - 2.0 * f); 
                float val = lerp(
                    lerp(random2(i + float2(0.0, 0.0)).x, random2(i + float2(1.0, 0.0)).x, f.x), 
                    lerp(random2(i + float2(0.0, 1.0)).x, random2(i + float2(1.0, 1.0)).x, f.x), 
                    f.y
                ); 
                return val * 2.0 - 1.0; 
            }
            
            float fbm(float2 uv, int octaves, float persistence) { 
                float total = 0; 
                float frequency = 1; 
                float amplitude = 1; 
                float maxValue = 0; 
                for (int k_oct = 0; k_oct < octaves; k_oct++) { 
                    total += simplexNoise(uv * frequency) * amplitude; 
                    maxValue += amplitude; 
                    amplitude *= persistence; 
                    frequency *= 2; 
                } 
                return (total / maxValue + 1.0) * 0.5; 
            }
            
            float worleyNoise(float2 uv_cell, float density, float time_offset) { 
                uv_cell *= density; 
                float2 i_uv = floor(uv_cell); 
                float2 f_uv = frac(uv_cell); 
                float minDist = 1.0; 
                for (int y = -1; y <= 1; y++) { 
                    for (int x = -1; x <= 1; x++) { 
                        float2 neighbor = float2(float(x), float(y)); 
                        float2 cellRandomPoint = random2(i_uv + neighbor); 
                        cellRandomPoint = 0.5 + 0.5 * sin(time_offset + 6.2831 * cellRandomPoint); 
                        float dist = length(neighbor + cellRandomPoint - f_uv); 
                        minDist = min(minDist, dist); 
                    } 
                } 
                return minDist; 
            }

            v2f vert (appdata v) { 
                v2f o; 
                o.vertex = UnityObjectToClipPos(v.vertex); 
                o.uv = v.uv; 
                o.color = v.color; 
                return o; 
            }

            fixed4 frag (v2f i) : SV_Target
                {
                    float current_time = _Time.y;
                    float2 centeredUV = i.uv - 0.5;
                    float distToCenter = length(centeredUV);
                    float sphereMask = 1.0 - smoothstep(0.5 - _EdgeSoftness, 0.5, distToCenter);
                    if (sphereMask <= 0.0) { discard; }

                    float normDist = distToCenter * 2.0;
                    float bulgeFactor = sqrt(1.0 - saturate(normDist * normDist));
                    float distortionTerm = pow(1.0 - bulgeFactor, _SphereDistortionCurve);
                    float2 contentDistortionOffset = -centeredUV * distortionTerm * _SphereDistortionAmount;
                    float2 uv_content = i.uv + contentDistortionOffset;
    
                    
                    float healthChangeRate = _HealthAnimSpeed * unity_DeltaTime.x; 
                                                                               
                                                                               
                    float animatedHealth = _Health; 
                                                    
                                                    

                    
                    float remappedHealthLevel = lerp(_LiquidVisibleMinY, _LiquidVisibleMaxY, _Health);

                    
                    float liquidVisibleRange = _LiquidVisibleMaxY - _LiquidVisibleMinY;
                    if (liquidVisibleRange < 0.001) liquidVisibleRange = 0.001; 

                    
                    
                    
                    float spaceBelowNormalized = saturate((remappedHealthLevel - _LiquidVisibleMinY) / liquidVisibleRange);

                    
                    
                    float spaceAboveNormalized = saturate((_LiquidVisibleMaxY - remappedHealthLevel) / liquidVisibleRange); 

                    float suppressionNearMin = smoothstep(0.0, _WaveSuppressionEdgeThreshold, spaceBelowNormalized);
                    float suppressionNearMax = smoothstep(0.0, _WaveSuppressionEdgeThreshold, spaceAboveNormalized);

                    float wave_amplitude_global_factor = min(suppressionNearMin, suppressionNearMax);
                    float time_since_hp_change = current_time - _TimeSinceLastHPChange;
                    float splash_progress = saturate(time_since_hp_change / _SplashOverallDuration);

                    float current_splash_strength = _InitialSplashMagnitude * pow(saturate(1.0 - splash_progress), 2.0);
                    current_splash_strength *= exp(-time_since_hp_change * _SplashDampingFactor * 0.25); 

                    if (time_since_hp_change > _SplashOverallDuration || _InitialSplashMagnitude < 0.001) {
                        current_splash_strength = 0.0;
                    }

                    float main_impact_wave = 0;
                    float secondary_ripple_wave = 0;

                    if (current_splash_strength > 0.005) {
                        float impact_phase_duration = _SplashOverallDuration * _SplashImpactDurationFactor;
                        float impact_phase_progress = saturate(time_since_hp_change / impact_phase_duration);
                        float impact_profile = pow(saturate(1.0 - impact_phase_progress), 3.0);

                        if (impact_profile > 0.01) {
                            float dist_x_from_center = abs(uv_content.x - 0.5) * 2.0;

                            main_impact_wave = sin(dist_x_from_center * _ImpactWaveFreq - time_since_hp_change * _ImpactWaveSpeed)
                                             * _SplashMaxHeight * current_splash_strength * impact_profile;
                            main_impact_wave *= saturate(1.0 - dist_x_from_center * 1.5);
                        }

                        float ripple_phase_start_factor = _SplashImpactDurationFactor * 0.5; 
                        float ripple_duration_factor = 1.0 - ripple_phase_start_factor;
                        float ripple_local_time = saturate((time_since_hp_change - (_SplashOverallDuration * ripple_phase_start_factor)) / (_SplashOverallDuration * ripple_duration_factor));
        
                        float ripple_profile = smoothstep(0.0, 0.2, ripple_local_time) * smoothstep(1.0, 0.6, ripple_local_time);


                        if (ripple_profile > 0.01) {
                            secondary_ripple_wave = sin(uv_content.x * _SurfaceWaveFrequency * _RippleWaveFreqFactor + time_since_hp_change * _SurfaceWaveSpeed * _RippleWaveSpeedFactor)
                                                  * _SurfaceWaveHeight * 0.8 * current_splash_strength * ripple_profile;
                            secondary_ripple_wave += cos(uv_content.x * _SurfaceWaveFrequency * 0.65 * _RippleWaveFreqFactor - time_since_hp_change * _SurfaceWaveSpeed * 0.75 * _RippleWaveSpeedFactor)
                                                   * _SurfaceWaveHeight * 0.5 * current_splash_strength * ripple_profile;
                        }
                    }

                    float wobble = sin(current_time * 10.0 + i.uv.x * 20.0) * 0.005 * (1.0 - animatedHealth);
                    float healthChangeWobble = 0.0;
                    if (current_splash_strength > 0.01 && abs(_HealthChange) > 0.001) { 
                         healthChangeWobble = sin(current_time * 20.0 + i.uv.x * 30.0) * 0.01 * current_splash_strength * _SplashImpulse;
                    }
    

                    float fillLineY = remappedHealthLevel + wobble + healthChangeWobble;

                    float calm_wave_speed = _SurfaceWaveSpeed;
                    float calm_wave_y = sin(uv_content.x * _SurfaceWaveFrequency + current_time * calm_wave_speed) * _SurfaceWaveHeight;

                    float calm_noise_wave = fbm(uv_content * _NoiseScaleLiquid * 0.5 + current_time * 0.2, 3, 0.6) * _SurfaceNoiseStrength; 

                    float agitation_factor = current_splash_strength * _SurfaceWaveSplashBoost; 
                    calm_wave_y *= (1.0 + agitation_factor);
                    calm_noise_wave *= (1.0 + agitation_factor * 1.5); 

                    float surfaceWave = calm_wave_y + calm_noise_wave + main_impact_wave + secondary_ripple_wave;
    
                    float currentY_InOrb = uv_content.y;

                    fixed3 base_rgb;
                    float base_a;

                    if (currentY_InOrb > fillLineY + surfaceWave) {
                        base_rgb = _BackgroundColor.rgb;
                        base_a = _BackgroundColor.a;
                    } else {
                        float2 liquidScroll = current_time * float2(_LiquidSpeedX, _LiquidSpeedY);
                        float2 liquidDistortionScroll = current_time * _DistortionSpeed * 0.5;

                        float dynamicDistortionAmount = _DistortionAmount * (1.0 + current_splash_strength * _SplashDistortionBoost);

                        float2 distortionVec = float2(
                            simplexNoise((uv_content + liquidDistortionScroll) * _NoiseScaleDistortion),
                            simplexNoise((uv_content + liquidDistortionScroll + 20.0) * _NoiseScaleDistortion)
                        ) * dynamicDistortionAmount; 

                        float liquidNoise1 = fbm((uv_content + liquidScroll + distortionVec) * _NoiseScaleLiquid, 3, 0.5);
                        float liquidNoise2 = fbm((uv_content * 1.7 + liquidScroll * 0.7 + distortionVec) * _NoiseScaleLiquid * 0.7, 4, 0.45);
                        float combinedLiquidNoise = (liquidNoise1 + liquidNoise2) * 0.5;
                        combinedLiquidNoise = smoothstep(0.3, 0.7, combinedLiquidNoise);
        
                        fixed3 liquidColorCalculated = lerp(_LiquidBaseColor.rgb, _LiquidColor.rgb, combinedLiquidNoise);

                        float liquidSurfaceLevel = fillLineY + surfaceWave;
                        float liquidBottomLevel = 0.0;
                        float y_in_liquid_normalized = saturate((currentY_InOrb - liquidBottomLevel) / max(0.001, liquidSurfaceLevel - liquidBottomLevel));

                        liquidColorCalculated *= lerp(_LiquidTintBottom, _LiquidTintTop, y_in_liquid_normalized);


                        float bubbleBoost = saturate(-_HealthChange) * 5.0 + current_splash_strength * 2.0; 

                        float2 bubbleScroll1 = current_time * float2(_BubbleSpeedX, _BubbleSpeedY);
                        float bubbles1 = worleyNoise(uv_content + bubbleScroll1, _NoiseScaleBubbles, current_time * 0.5 + bubbleBoost);
                        bubbles1 = 1.0 - bubbles1;
                        bubbles1 = pow(saturate(bubbles1), _BubbleDensity);
                        fixed3 bubbleColorEffect1 = _BubbleColor.rgb * bubbles1 * (1.0-combinedLiquidNoise*0.5);

                        float2 bubbleScroll2 = current_time * float2(_BubbleSpeedX2, _BubbleSpeedY2);
                        float bubbles2 = worleyNoise(uv_content + bubbleScroll2, _NoiseScaleBubbles2, current_time * 0.3 + 5.0 + bubbleBoost * 0.5);
                        bubbles2 = 1.0 - bubbles2;
                        bubbles2 = pow(saturate(bubbles2), _BubbleDensity2);
                        fixed3 bubbleColorEffect2 = _BubbleColor2.rgb * bubbles2 * (1.0-combinedLiquidNoise*0.2);
                                
                        base_rgb = liquidColorCalculated + bubbleColorEffect1 + bubbleColorEffect2;
                        base_a = _LiquidColor.a;
                    }
    
                    if (_EffectIntensity > 0.001) {
                        base_rgb = lerp(base_rgb, _EffectColor.rgb, _EffectColor.a * _EffectIntensity);
                    }

                    base_rgb *= i.color.rgb; 
                    base_a *= i.color.a;

                    float3 glassNormal;
                    float glassNormalZ = sqrt(1.0 - saturate(normDist * normDist));
                    glassNormal = normalize(float3(centeredUV.x * 2.0, centeredUV.y * 2.0, glassNormalZ));

                    float3 viewDir = float3(0,0,1); 
                    fixed3 final_highlights = fixed3(0,0,0);

                    float3 lightDirMain = normalize(float3(_LightDirectionX, _LightDirectionY, _LightDirectionZ));
                    float3 halfwayDirMain = normalize(lightDirMain + viewDir);
                    float specAngleMain = saturate(dot(glassNormal, halfwayDirMain));
                    float specularMain = pow(specAngleMain, _GlassShininess);
                    final_highlights += _GlassHighlightColor.rgb * specularMain * _GlassHighlightColor.a;

                    float crescent_mask = 0.0;
                    float dist_from_orb_center = length(centeredUV);

                    float outer_edge_factor = smoothstep(
                        _CrescentOuterRadius + _CrescentSoftness,
                        _CrescentOuterRadius - _CrescentSoftness,
                        dist_from_orb_center
                    );
    
                    float inner_target_radius = _CrescentOuterRadius * _CrescentInnerRadiusScale;
                    float dist_to_cutout_center = length(centeredUV - _CrescentShapeOffset.xy);

                    float inner_edge_factor = smoothstep(
                        inner_target_radius + _CrescentSoftness,
                        inner_target_radius - _CrescentSoftness,
                        dist_to_cutout_center
                    );
    
                    crescent_mask = saturate(outer_edge_factor * (1.0 - inner_edge_factor));
                    final_highlights += _CrescentColor.rgb * crescent_mask * _CrescentColor.a;
    
                    if (_HealthChange < -0.01 && current_splash_strength > 0.1) { 
                         base_rgb += _EffectColor.rgb * saturate(-_HealthChange) * 0.5 * current_splash_strength;
                    }
    
                    fixed3 final_rgb = base_rgb + final_highlights;
                    float final_a = base_a * sphereMask;

                    return fixed4(final_rgb, final_a);
                }
            ENDCG
        }
    }
    Fallback "Transparent/VertexLit"
}