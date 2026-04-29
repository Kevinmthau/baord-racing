using UnityEngine;

namespace BoardRacing
{
    public sealed class RaceTrack
    {
        private readonly Vector2[] samples;
        private readonly float[] cumulativeDistances;
        private readonly float[] checkpointDistances;

        public RaceTrack(Vector2[] centerlineSamples, float trackWidth, float[] checkpointFractions)
        {
            samples = centerlineSamples;
            TrackWidth = trackWidth;
            cumulativeDistances = BuildCumulativeDistances(samples);
            TotalLength = cumulativeDistances[^1];

            checkpointDistances = new float[checkpointFractions.Length];
            for (var i = 0; i < checkpointFractions.Length; i++)
            {
                checkpointDistances[i] = Mathf.Repeat(checkpointFractions[i], 1f) * TotalLength;
            }
        }

        public int SampleCount => samples.Length;
        public int CheckpointCount => checkpointDistances.Length;
        public float TrackWidth { get; }
        public float TotalLength { get; }

        public TrackPose GetSamplePose(int index)
        {
            var previous = samples[(index - 1 + samples.Length) % samples.Length];
            var current = samples[index];
            var next = samples[(index + 1) % samples.Length];
            return TrackPose.FromPositionAndTangent(current, (next - previous).normalized);
        }

        public float GetSampleDistance(int index)
        {
            return cumulativeDistances[Mathf.Clamp(index, 0, cumulativeDistances.Length - 1)];
        }

        public float GetCheckpointDistance(int index)
        {
            return checkpointDistances[Mathf.Clamp(index, 0, checkpointDistances.Length - 1)];
        }

        public TrackPose GetPoseAtDistance(float distance)
        {
            var wrappedDistance = Mathf.Repeat(distance, TotalLength);

            for (var i = 0; i < samples.Length; i++)
            {
                var start = cumulativeDistances[i];
                var end = cumulativeDistances[i + 1];
                if (wrappedDistance > end)
                {
                    continue;
                }

                var a = samples[i];
                var b = samples[(i + 1) % samples.Length];
                var t = Mathf.InverseLerp(start, end, wrappedDistance);
                return TrackPose.FromPositionAndTangent(Vector2.Lerp(a, b, t), (b - a).normalized);
            }

            return TrackPose.FromPositionAndTangent(samples[0], (samples[1] - samples[0]).normalized);
        }

        public float GetNearestDistance(Vector2 position)
        {
            var closestDistance = 0f;
            var closestSqrMagnitude = float.MaxValue;

            for (var i = 0; i < samples.Length; i++)
            {
                var a = samples[i];
                var b = samples[(i + 1) % samples.Length];
                var segment = b - a;
                var segmentLengthSqr = segment.sqrMagnitude;
                if (segmentLengthSqr < 0.0001f)
                {
                    continue;
                }

                var t = Mathf.Clamp01(Vector2.Dot(position - a, segment) / segmentLengthSqr);
                var projected = a + segment * t;
                var sqrMagnitude = (position - projected).sqrMagnitude;
                if (sqrMagnitude >= closestSqrMagnitude)
                {
                    continue;
                }

                closestSqrMagnitude = sqrMagnitude;
                closestDistance = cumulativeDistances[i] + Mathf.Sqrt(segmentLengthSqr) * t;
            }

            return Mathf.Repeat(closestDistance, TotalLength);
        }

        private static float[] BuildCumulativeDistances(Vector2[] points)
        {
            var distances = new float[points.Length + 1];
            for (var i = 0; i < points.Length; i++)
            {
                distances[i + 1] = distances[i] + Vector2.Distance(points[i], points[(i + 1) % points.Length]);
            }

            return distances;
        }
    }

    public readonly struct TrackPose
    {
        public TrackPose(Vector2 position, Vector2 tangent, Vector2 normal)
        {
            Position = position;
            Tangent = tangent;
            Normal = normal;
            Rotation = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg - 90f;
            NormalRotation = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;
        }

        public Vector2 Position { get; }
        public Vector2 Tangent { get; }
        public Vector2 Normal { get; }
        public float Rotation { get; }
        public float NormalRotation { get; }

        public static TrackPose FromPositionAndTangent(Vector2 position, Vector2 tangent)
        {
            var safeTangent = tangent.sqrMagnitude > 0.0001f ? tangent.normalized : Vector2.up;
            var normal = new Vector2(-safeTangent.y, safeTangent.x);
            return new TrackPose(position, safeTangent, normal);
        }
    }
}
