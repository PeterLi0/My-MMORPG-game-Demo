// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Shader created with Shader Forge v1.24 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.24;sub:START;pass:START;ps:flbk:Particles/Alpha Blended,iptp:0,cusa:False,bamd:0,lico:0,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,rfrpo:True,rfrpn:Refraction,coma:15,ufog:False,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:3138,x:32719,y:32712,varname:node_3138,prsc:2|emission-5523-OUT,alpha-1490-G;n:type:ShaderForge.SFN_Tex2d,id:1730,x:32046,y:32654,ptovrint:False,ptlb:MainTex,ptin:_MainTex,varname:node_1730,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-5892-OUT;n:type:ShaderForge.SFN_Panner,id:1975,x:31709,y:32212,varname:node_1975,prsc:2,spu:0.1,spv:1|UVIN-4652-UVOUT,DIST-8953-OUT;n:type:ShaderForge.SFN_TexCoord,id:4652,x:30738,y:32203,varname:node_4652,prsc:2,uv:0;n:type:ShaderForge.SFN_Panner,id:3653,x:31382,y:32774,varname:node_3653,prsc:2,spu:1,spv:0|UVIN-4652-UVOUT,DIST-2389-OUT;n:type:ShaderForge.SFN_Tex2d,id:1490,x:32046,y:32891,ptovrint:False,ptlb:Alpha(G),ptin:_AlphaG,varname:_node_89_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False|UVIN-9218-OUT;n:type:ShaderForge.SFN_Slider,id:2721,x:30808,y:32825,ptovrint:False,ptlb:AlphaSpeed(X),ptin:_AlphaSpeedX,varname:node_2721,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:2;n:type:ShaderForge.SFN_Time,id:4091,x:30850,y:32634,varname:node_4091,prsc:2;n:type:ShaderForge.SFN_Multiply,id:2389,x:31162,y:32774,varname:node_2389,prsc:2|A-4091-T,B-2721-OUT;n:type:ShaderForge.SFN_Slider,id:5502,x:31142,y:32243,ptovrint:False,ptlb:MainTexSpeed(Y),ptin:_MainTexSpeedY,varname:_node_2721_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:2;n:type:ShaderForge.SFN_Time,id:8207,x:31170,y:32045,varname:node_8207,prsc:2;n:type:ShaderForge.SFN_Multiply,id:8953,x:31499,y:32126,varname:node_8953,prsc:2|A-8207-T,B-5502-OUT;n:type:ShaderForge.SFN_Panner,id:9757,x:31709,y:32392,varname:node_9757,prsc:2,spu:1,spv:0.1|UVIN-4652-UVOUT,DIST-5399-OUT;n:type:ShaderForge.SFN_Multiply,id:5399,x:31466,y:32392,varname:node_5399,prsc:2|A-2893-T,B-8555-OUT;n:type:ShaderForge.SFN_Time,id:2893,x:31163,y:32330,varname:node_2893,prsc:2;n:type:ShaderForge.SFN_Slider,id:8555,x:31109,y:32509,ptovrint:False,ptlb:MainTexSpeed(X),ptin:_MainTexSpeedX,varname:_MainTexSpeedY,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:2;n:type:ShaderForge.SFN_Add,id:5892,x:31860,y:32349,varname:node_5892,prsc:2|A-1975-UVOUT,B-9757-UVOUT;n:type:ShaderForge.SFN_Panner,id:6105,x:31382,y:33054,varname:node_6105,prsc:2,spu:0,spv:1|UVIN-4652-UVOUT,DIST-9568-OUT;n:type:ShaderForge.SFN_Multiply,id:9568,x:31139,y:33054,varname:node_9568,prsc:2|A-606-T,B-8377-OUT;n:type:ShaderForge.SFN_Time,id:606,x:30827,y:32914,varname:node_606,prsc:2;n:type:ShaderForge.SFN_Slider,id:8377,x:30785,y:33105,ptovrint:False,ptlb:AlphaSpeed(Y),ptin:_AlphaSpeedY,varname:_AlphaSpeed_copy,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0,max:2;n:type:ShaderForge.SFN_Add,id:9218,x:31699,y:32888,varname:node_9218,prsc:2|A-3653-UVOUT,B-6105-UVOUT;n:type:ShaderForge.SFN_Color,id:3742,x:32070,y:32007,ptovrint:False,ptlb:Color,ptin:_Color,varname:node_3742,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Slider,id:8542,x:31981,y:32224,ptovrint:False,ptlb:Colorpower,ptin:_Colorpower,varname:node_8542,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:2,max:2;n:type:ShaderForge.SFN_Multiply,id:5523,x:32415,y:32553,varname:node_5523,prsc:2|A-2823-OUT,B-1730-RGB;n:type:ShaderForge.SFN_Multiply,id:2823,x:32320,y:32225,varname:node_2823,prsc:2|A-3742-RGB,B-8542-OUT;proporder:3742-8542-1730-8555-5502-1490-2721-8377;pass:END;sub:END;*/

Shader "HeroShader/Effect/UVmove" {
    Properties {
        _Color ("Color", Color) = (0.5,0.5,0.5,1)
        _Colorpower ("Colorpower", Range(0, 2)) = 2
        _MainTex ("MainTex", 2D) = "white" {}
        _MainTexSpeedX ("MainTexSpeed(X)", Range(0, 2)) = 0
        _MainTexSpeedY ("MainTexSpeed(Y)", Range(0, 2)) = 0
        _AlphaG ("Alpha(G)", 2D) = "white" {}
        _AlphaSpeedX ("AlphaSpeed(X)", Range(0, 2)) = 0
        _AlphaSpeedY ("AlphaSpeed(Y)", Range(0, 2)) = 0
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            #pragma exclude_renderers xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _TimeEditor;
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _AlphaG; uniform float4 _AlphaG_ST;
            uniform float _AlphaSpeedX;
            uniform float _MainTexSpeedY;
            uniform float _MainTexSpeedX;
            uniform float _AlphaSpeedY;
            uniform float4 _Color;
            uniform float _Colorpower;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex );
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
/////// Vectors:
////// Lighting:
////// Emissive:
                float4 node_8207 = _Time + _TimeEditor;
                float4 node_2893 = _Time + _TimeEditor;
                float2 node_5892 = ((i.uv0+(node_8207.g*_MainTexSpeedY)*float2(0.1,1))+(i.uv0+(node_2893.g*_MainTexSpeedX)*float2(1,0.1)));
                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(node_5892, _MainTex));
                float3 emissive = ((_Color.rgb*_Colorpower)*_MainTex_var.rgb);
                float3 finalColor = emissive;
                float4 node_4091 = _Time + _TimeEditor;
                float4 node_606 = _Time + _TimeEditor;
                float2 node_9218 = ((i.uv0+(node_4091.g*_AlphaSpeedX)*float2(1,0))+(i.uv0+(node_606.g*_AlphaSpeedY)*float2(0,1)));
                float4 _AlphaG_var = tex2D(_AlphaG,TRANSFORM_TEX(node_9218, _AlphaG));
                return fixed4(finalColor,_AlphaG_var.g);
            }
            ENDCG
        }
    }
    FallBack "Particles/Alpha Blended"
    CustomEditor "ShaderForgeMaterialInspector"
}
