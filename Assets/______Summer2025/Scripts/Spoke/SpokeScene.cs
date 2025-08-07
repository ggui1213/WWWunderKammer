using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Root scene object
[JsonObject]
public class SpokeScene
{
    [JsonProperty("version")]
    public string Version { get; set; }

    [JsonProperty("root")]
    public string Root { get; set; }

    [JsonProperty("metadata")]
    public Metadata Metadata { get; set; }

    [JsonProperty("entities")]
    public Dictionary<string, SpokeEntity> Entities { get; set; }
}

// Scene metadata
[JsonObject]
public class Metadata
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("previewCameraTransform")]
    public PreviewCameraTransform PreviewCameraTransform { get; set; }

    [JsonProperty("creatorAttribution")]
    public string CreatorAttribution { get; set; }

    [JsonProperty("allowRemixing")]
    public bool AllowRemixing { get; set; }

    [JsonProperty("allowPromotion")]
    public bool AllowPromotion { get; set; }
}

// Preview camera transform matrix
[JsonObject]
public class PreviewCameraTransform
{
    [JsonProperty("elements")]
    public float[] Elements { get; set; }
}

// Entity wrapper with name and components
[JsonObject]
public class SpokeEntity
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("components")]
    public List<SpokeComponent> Components { get; set; }
}

// Generic component container
[JsonObject]
public class SpokeComponent
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("props")]
    public JObject Props { get; set; }
}

#region Typed Props Classes

// 3D vector
[JsonObject]
public class Vec3
{
    [JsonProperty("x")]
    public float X { get; set; }
    [JsonProperty("y")]
    public float Y { get; set; }
    [JsonProperty("z")]
    public float Z { get; set; }
}

// 2D vector
[JsonObject]
public class Vec2
{
    [JsonProperty("x")]
    public float X { get; set; }
    [JsonProperty("y")]
    public float Y { get; set; }
}

[JsonObject]
public class TransformProps
{
    [JsonProperty("position")]
    public Vec3 Position { get; set; }

    [JsonProperty("rotation")]
    public Vec3 Rotation { get; set; }

    [JsonProperty("scale")]
    public Vec3 Scale { get; set; }
}

[JsonObject]
public class GltfModelProps
{
    [JsonProperty("src")]
    public string Src { get; set; }

    [JsonProperty("attribution")]
    public AttributionProps Attribution { get; set; }
}

[JsonObject]
public class AttributionProps
{
    [JsonProperty("title")]
    public string Title { get; set; }
}

[JsonObject]
public class AmbientLightProps
{
    [JsonProperty("color")]
    public string Color { get; set; }

    [JsonProperty("intensity")]
    public float Intensity { get; set; }
}

[JsonObject]
public class DirectionalLightProps
{
    [JsonProperty("castShadow")]
    public bool CastShadow { get; set; }

    [JsonProperty("color")]
    public string Color { get; set; }

    [JsonProperty("intensity")]
    public float Intensity { get; set; }

    [JsonProperty("shadowMapResolution")]
    public int[] ShadowMapResolution { get; set; }

    [JsonProperty("shadowBias")]
    public float ShadowBias { get; set; }

    [JsonProperty("shadowRadius")]
    public float ShadowRadius { get; set; }
}

[JsonObject]
public class PointLightProps
{
    [JsonProperty("castShadow")]
    public bool CastShadow { get; set; }

    [JsonProperty("color")]
    public string Color { get; set; }

    [JsonProperty("intensity")]
    public float Intensity { get; set; }

    [JsonProperty("range")]
    public float Range { get; set; }

    [JsonProperty("shadowMapResolution")]
    public int[] ShadowMapResolution { get; set; }

    [JsonProperty("shadowBias")]
    public float ShadowBias { get; set; }

    [JsonProperty("shadowRadius")]
    public float ShadowRadius { get; set; }
}

[JsonObject]
public class SpotLightProps
{
    [JsonProperty("castShadow")]
    public bool CastShadow { get; set; }

    [JsonProperty("color")]
    public string Color { get; set; }

    [JsonProperty("intensity")]
    public float Intensity { get; set; }

    [JsonProperty("range")]
    public float Range { get; set; }

    [JsonProperty("innerConeAngle")]
    public float InnerConeAngle { get; set; }

    [JsonProperty("outerConeAngle")]
    public float OuterConeAngle { get; set; }

    [JsonProperty("shadowMapResolution")]
    public int[] ShadowMapResolution { get; set; }

    [JsonProperty("shadowBias")]
    public float ShadowBias { get; set; }

    [JsonProperty("shadowRadius")]
    public float ShadowRadius { get; set; }
}

[JsonObject]
public class FogProps
{
    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("color")]
    public string Color { get; set; }

    [JsonProperty("near")]
    public float Near { get; set; }

    [JsonProperty("far")]
    public float Far { get; set; }

    [JsonProperty("density")]
    public float Density { get; set; }
}

[JsonObject]
public class BackgroundProps
{
    [JsonProperty("color")]
    public string Color { get; set; }
}

[JsonObject]
public class VideoProps
{
    [JsonProperty("src")]
    public string Src { get; set; }

    [JsonProperty("projection")]
    public string Projection { get; set; }

    [JsonProperty("loop")]
    public bool Loop { get; set; }

    [JsonProperty("autoPlay")]
    public bool AutoPlay { get; set; }

    [JsonProperty("controls")]
    public bool Controls { get; set; }
}

[JsonObject]
public class ImageProps
{
    [JsonProperty("src")]
    public string Src { get; set; }

    [JsonProperty("projection")]
    public string Projection { get; set; }

    [JsonProperty("controls")]
    public bool Controls { get; set; }

    [JsonProperty("alphaMode")]
    public string AlphaMode { get; set; }

    [JsonProperty("alphaCutoff")]
    public float AlphaCutoff { get; set; }
}

[JsonObject]
public class LinkProps
{
    [JsonProperty("href")]
    public string Href { get; set; }
}

[JsonObject]
public class AudioParamsProps
{
    [JsonProperty("audioType")]
    public string AudioType { get; set; }

    [JsonProperty("gain")]
    public float Gain { get; set; }

    [JsonProperty("distanceModel")]
    public string DistanceModel { get; set; }

    [JsonProperty("rolloffFactor")]
    public float RolloffFactor { get; set; }

    [JsonProperty("refDistance")]
    public float RefDistance { get; set; }

    [JsonProperty("maxDistance")]
    public float MaxDistance { get; set; }

    [JsonProperty("coneInnerAngle")]
    public float ConeInnerAngle { get; set; }

    [JsonProperty("coneOuterAngle")]
    public float ConeOuterAngle { get; set; }

    [JsonProperty("coneOuterGain")]
    public float ConeOuterGain { get; set; }

    [JsonProperty("overrideAudioSettings")]
    public bool OverrideAudioSettings { get; set; }
}

[JsonObject]
public class AudioSettingsProps
{
    [JsonProperty("overrideAudioSettings")]
    public bool OverrideAudioSettings { get; set; }

    [JsonProperty("avatarDistanceModel")]
    public string AvatarDistanceModel { get; set; }

    [JsonProperty("avatarRolloffFactor")]
    public float AvatarRolloffFactor { get; set; }

    [JsonProperty("avatarRefDistance")]
    public float AvatarRefDistance { get; set; }

    [JsonProperty("avatarMaxDistance")]
    public float AvatarMaxDistance { get; set; }

    [JsonProperty("mediaDistanceModel")]
    public string MediaDistanceModel { get; set; }

    [JsonProperty("mediaRolloffFactor")]
    public float MediaRolloffFactor { get; set; }

    [JsonProperty("mediaRefDistance")]
    public float MediaRefDistance { get; set; }

    [JsonProperty("mediaMaxDistance")]
    public float MediaMaxDistance { get; set; }

    [JsonProperty("mediaVolume")]
    public float MediaVolume { get; set; }

    [JsonProperty("mediaConeInnerAngle")]
    public float MediaConeInnerAngle { get; set; }

    [JsonProperty("mediaConeOuterAngle")]
    public float MediaConeOuterAngle { get; set; }

    [JsonProperty("mediaConeOuterGain")]
    public float MediaConeOuterGain { get; set; }
}

[JsonObject]
public class EditorSettingsProps
{
    [JsonProperty("enabled")]
    public bool Enabled { get; set; }

    [JsonProperty("enabledProperties")]
    public List<string> EnabledProperties { get; set; }

    [JsonProperty("modifiedProperties")]
    public List<string> ModifiedProperties { get; set; }
}

[JsonObject]
public class FloorPlanProps
{
    [JsonProperty("autoCellSize")]
    public bool AutoCellSize { get; set; }

    [JsonProperty("cellSize")]
    public float CellSize { get; set; }

    [JsonProperty("cellHeight")]
    public float CellHeight { get; set; }

    [JsonProperty("agentHeight")]
    public float AgentHeight { get; set; }

    [JsonProperty("agentRadius")]
    public float AgentRadius { get; set; }

    [JsonProperty("agentMaxClimb")]
    public float AgentMaxClimb { get; set; }

    [JsonProperty("agentMaxSlope")]
    public float AgentMaxSlope { get; set; }

    [JsonProperty("regionMinSize")]
    public int RegionMinSize { get; set; }

    [JsonProperty("maxTriangles")]
    public int MaxTriangles { get; set; }

    [JsonProperty("forceTrimesh")]
    public bool ForceTrimesh { get; set; }

    [JsonProperty("navMeshMode")]
    public string NavMeshMode { get; set; }

    [JsonProperty("navMeshSrc")]
    public string NavMeshSrc { get; set; }
}

[JsonObject]
public class ParticleEmitterProps
{
    [JsonProperty("src")]
    public string Src { get; set; }

    [JsonProperty("startColor")]
    public string StartColor { get; set; }

    [JsonProperty("middleColor")]
    public string MiddleColor { get; set; }

    [JsonProperty("endColor")]
    public string EndColor { get; set; }

    [JsonProperty("startOpacity")]
    public int StartOpacity { get; set; }

    [JsonProperty("middleOpacity")]
    public int MiddleOpacity { get; set; }

    [JsonProperty("endOpacity")]
    public int EndOpacity { get; set; }

    [JsonProperty("colorCurve")]
    public string ColorCurve { get; set; }

    [JsonProperty("sizeCurve")]
    public string SizeCurve { get; set; }

    [JsonProperty("startSize")]
    public int StartSize { get; set; }

    [JsonProperty("endSize")]
    public int EndSize { get; set; }

    [JsonProperty("sizeRandomness")]
    public int SizeRandomness { get; set; }

    [JsonProperty("ageRandomness")]
    public int AgeRandomness { get; set; }

    [JsonProperty("lifetime")]
    public int Lifetime { get; set; }

    [JsonProperty("lifetimeRandomness")]
    public int LifetimeRandomness { get; set; }

    [JsonProperty("particleCount")]
    public int ParticleCount { get; set; }

    [JsonProperty("startVelocity")]
    public Vec3 StartVelocity { get; set; }

    [JsonProperty("endVelocity")]
    public Vec3 EndVelocity { get; set; }

    [JsonProperty("velocityCurve")]
    public string VelocityCurve { get; set; }

    [JsonProperty("angularVelocity")]
    public int AngularVelocity { get; set; }
}

[JsonObject]
public class ShadowProps
{
    [JsonProperty("cast")]
    public bool Cast { get; set; }

    [JsonProperty("receive")]
    public bool Receive { get; set; }
}

[JsonObject]
public class SimpleWaterProps
{
    [JsonProperty("opacity")]
    public float Opacity { get; set; }

    [JsonProperty("color")]
    public string Color { get; set; }

    [JsonProperty("tideHeight")]
    public float TideHeight { get; set; }

    [JsonProperty("tideScale")]
    public Vec2 TideScale { get; set; }

    [JsonProperty("tideSpeed")]
    public Vec2 TideSpeed { get; set; }

    [JsonProperty("waveHeight")]
    public float WaveHeight { get; set; }

    [JsonProperty("waveScale")]
    public Vec2 WaveScale { get; set; }

    [JsonProperty("waveSpeed")]
    public Vec2 WaveSpeed { get; set; }

    [JsonProperty("ripplesSpeed")]
    public float RipplesSpeed { get; set; }

    [JsonProperty("ripplesScale")]
    public float RipplesScale { get; set; }
}

[JsonObject]
public class SkyboxProps
{
    [JsonProperty("turbidity")]
    public float Turbidity { get; set; }

    [JsonProperty("rayleigh")]
    public float Rayleigh { get; set; }

    [JsonProperty("luminance")]
    public float Luminance { get; set; }

    [JsonProperty("mieCoefficient")]
    public float MieCoefficient { get; set; }

    [JsonProperty("mieDirectionalG")]
    public float MieDirectionalG { get; set; }

    [JsonProperty("inclination")]
    public float Inclination { get; set; }

    [JsonProperty("azimuth")]
    public float Azimuth { get; set; }

    [JsonProperty("distance")]
    public float Distance { get; set; }
}

[JsonObject]
public class VisibleProps
{
    [JsonProperty("visible")]
    public bool Visible { get; set; }
}

#endregion
