using System;
using System.IO;

public static class GenomeSerializer
{
    public const string HerbivorePrefix = "HG:";
    const int CurrentVersion = 2;

    public static byte[] Serialize(HerbivoreGenome g)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        bw.Write(CurrentVersion);
        bw.Write(g.forwardForce);
        bw.Write(g.turnForce);
        bw.Write(g.visionAngle);
        bw.Write(g.visionturnAngle);
        bw.Write(g.visionDistance);
        bw.Write(g.metabolismRate);
        bw.Write(g.eatspeed);
        bw.Write(g.threatWeight);
        bw.Write(g.threatDetectDistance);
        bw.Write(g.memorytime);
        bw.Write(g.runAwayDistance);
        bw.Write(g.contactEscapeDistance);
        bw.Write(g.evasionAngle);
        bw.Write(g.evasionDuration);
        bw.Write(g.evasionCooldown);
        bw.Write(g.evasionDistance);
        bw.Write(g.predictIntercept);
        bw.Write(g.zigzagFrequency);
        bw.Write(g.zigzagAmplitude);
        bw.Write(g.foodWeight);
        bw.Write(g.predatorWeight);
        bw.Write(g.corpseWeight);
        bw.Write(g.fearThreshold);
        bw.Write(g.escapeThreshold);
        bw.Write(g.curiosity);

        WriteWaves(bw, g.visionWaves);
        WriteWaves(bw, g.wanderWaves);

        bw.Flush();
        return ms.ToArray();
    }

    public static string EncodeGenome(HerbivoreGenome g)
    {
        return HerbivorePrefix + Convert.ToBase64String(Serialize(g));
    }

    public static HerbivoreGenome DecodeGenome(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("DNA code is empty.");

        string raw = text.Trim();
        if (raw.StartsWith(HerbivorePrefix, StringComparison.Ordinal))
            raw = raw.Substring(HerbivorePrefix.Length);

        byte[] bytes = Convert.FromBase64String(raw);
        return Deserialize(bytes);
    }

    static HerbivoreGenome Deserialize(byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        using var br = new BinaryReader(ms);

        int version = br.ReadInt32();
        if (version != 1 && version != CurrentVersion)
            throw new InvalidDataException($"Unsupported genome version: {version}");

        HerbivoreGenome g = new HerbivoreGenome
        {
            forwardForce = br.ReadSingle(),
            turnForce = br.ReadSingle(),
            visionAngle = br.ReadSingle(),
            visionturnAngle = br.ReadSingle(),
            visionDistance = br.ReadSingle(),
            metabolismRate = br.ReadSingle(),
            eatspeed = br.ReadSingle(),
            threatWeight = br.ReadSingle(),
            threatDetectDistance = br.ReadSingle(),
            memorytime = br.ReadSingle(),
            runAwayDistance = br.ReadSingle(),
            contactEscapeDistance = br.ReadSingle(),
            evasionAngle = br.ReadSingle(),
            evasionDuration = br.ReadSingle(),
            evasionCooldown = br.ReadSingle(),
            evasionDistance = br.ReadSingle(),
            predictIntercept = br.ReadBoolean(),
            zigzagFrequency = br.ReadSingle(),
            zigzagAmplitude = br.ReadSingle(),
            foodWeight = 1f,
            predatorWeight = 3f,
            corpseWeight = 1.2f,
            fearThreshold = 2f,
            escapeThreshold = 4f,
            curiosity = 0.3f
        };

        if (version >= 2)
        {
            g.foodWeight = br.ReadSingle();
            g.predatorWeight = br.ReadSingle();
            g.corpseWeight = br.ReadSingle();
            g.fearThreshold = br.ReadSingle();
            g.escapeThreshold = br.ReadSingle();
            g.curiosity = br.ReadSingle();
        }

        g.visionWaves = ReadWaves(br);
        g.wanderWaves = ReadWaves(br);

        return g;
    }

    static void WriteWaves(BinaryWriter bw, WaveGene[] waves)
    {
        int count = waves != null ? waves.Length : 0;
        bw.Write(count);
        for (int i = 0; i < count; i++)
        {
            bw.Write(waves[i].frequency);
            bw.Write(waves[i].amplitude);
            bw.Write(waves[i].phase);
        }
    }

    static WaveGene[] ReadWaves(BinaryReader br)
    {
        int count = br.ReadInt32();
        if (count < 0 || count > 1024)
            throw new InvalidDataException("Invalid wave count in DNA code.");

        WaveGene[] waves = new WaveGene[count];
        for (int i = 0; i < count; i++)
        {
            waves[i] = new WaveGene
            {
                frequency = br.ReadSingle(),
                amplitude = br.ReadSingle(),
                phase = br.ReadSingle()
            };
        }
        return waves;
    }
}
