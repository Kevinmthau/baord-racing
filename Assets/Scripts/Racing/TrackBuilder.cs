using System.Collections.Generic;
using UnityEngine;

namespace BoardRacing
{
    public static class TrackBuilder
    {
        private const float TrackWidth = 4.2f;
        private const int SamplesPerCurve = 14;

        public static RaceTrack Build()
        {
            var root = new GameObject("Twisting Loop Track");
            var controlPoints = new[]
            {
                new Vector2(-12.5f, -4.4f),
                new Vector2(-8.6f, -8.0f),
                new Vector2(-2.2f, -6.8f),
                new Vector2(0.9f, -3.2f),
                new Vector2(6.8f, -5.8f),
                new Vector2(12.0f, -1.8f),
                new Vector2(9.6f, 3.4f),
                new Vector2(4.0f, 4.8f),
                new Vector2(1.2f, 8.0f),
                new Vector2(-5.2f, 6.1f),
                new Vector2(-10.7f, 7.1f),
                new Vector2(-13.4f, 1.6f)
            };

            var samples = BuildCatmullRomLoop(controlPoints);
            var track = new RaceTrack(samples, TrackWidth, BuildCheckpointFractions());

            CreateBackground(root.transform);
            CreateRoadMesh(root.transform, track);
            CreateBoundaries(root.transform, track);
            CreateStartFinish(root.transform, track);
            CreateCheckpointGates(root.transform, track);

            return track;
        }

        private static float[] BuildCheckpointFractions()
        {
            return new[] { 0f, 0.14f, 0.28f, 0.43f, 0.58f, 0.73f, 0.87f };
        }

        private static Vector2[] BuildCatmullRomLoop(IReadOnlyList<Vector2> points)
        {
            var samples = new List<Vector2>(points.Count * SamplesPerCurve);
            for (var i = 0; i < points.Count; i++)
            {
                var p0 = points[(i - 1 + points.Count) % points.Count];
                var p1 = points[i];
                var p2 = points[(i + 1) % points.Count];
                var p3 = points[(i + 2) % points.Count];

                for (var step = 0; step < SamplesPerCurve; step++)
                {
                    var t = step / (float)SamplesPerCurve;
                    samples.Add(CatmullRom(p0, p1, p2, p3, t));
                }
            }

            return samples.ToArray();
        }

        private static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            return 0.5f * ((2f * p1) +
                           (-p0 + p2) * t +
                           (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                           (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }

        private static void CreateBackground(Transform root)
        {
            var background = new GameObject("Table Field");
            background.transform.SetParent(root, false);
            background.transform.position = new Vector3(0f, 0f, 1f);

            var renderer = background.AddComponent<SpriteRenderer>();
            renderer.sprite = RacingVisuals.WhiteSprite;
            renderer.color = new Color(0.07f, 0.22f, 0.13f);
            renderer.sortingOrder = -50;
            background.transform.localScale = new Vector3(44f, 28f, 1f);
        }

        private static void CreateRoadMesh(Transform root, RaceTrack track)
        {
            var road = new GameObject("Road Surface");
            road.transform.SetParent(root, false);

            var mesh = new Mesh { name = "Procedural Road Mesh" };
            var vertices = new Vector3[track.SampleCount * 2];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[track.SampleCount * 6];

            for (var i = 0; i < track.SampleCount; i++)
            {
                var sample = track.GetSamplePose(i);
                var left = sample.Position + sample.Normal * (TrackWidth * 0.5f);
                var right = sample.Position - sample.Normal * (TrackWidth * 0.5f);
                vertices[i * 2] = left;
                vertices[i * 2 + 1] = right;
                uvs[i * 2] = new Vector2(0f, track.GetSampleDistance(i));
                uvs[i * 2 + 1] = new Vector2(1f, track.GetSampleDistance(i));

                var next = (i + 1) % track.SampleCount;
                var triangleIndex = i * 6;
                triangles[triangleIndex] = i * 2;
                triangles[triangleIndex + 1] = next * 2;
                triangles[triangleIndex + 2] = i * 2 + 1;
                triangles[triangleIndex + 3] = next * 2;
                triangles[triangleIndex + 4] = next * 2 + 1;
                triangles[triangleIndex + 5] = i * 2 + 1;
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();

            road.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = road.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = RacingVisuals.CreateMaterial(new Color(0.18f, 0.18f, 0.18f));
            renderer.sortingOrder = -10;
        }

        private static void CreateBoundaries(Transform root, RaceTrack track)
        {
            CreateBoundary(root, track, true, "Outer Boundary", new Color(0.95f, 0.95f, 0.9f));
            CreateBoundary(root, track, false, "Inner Boundary", new Color(0.95f, 0.95f, 0.9f));
            CreateCenterDashes(root, track);
        }

        private static void CreateBoundary(Transform root, RaceTrack track, bool leftSide, string name, Color color)
        {
            var points = new Vector2[track.SampleCount + 1];
            var linePositions = new Vector3[track.SampleCount];

            for (var i = 0; i < track.SampleCount; i++)
            {
                var sample = track.GetSamplePose(i);
                var offset = sample.Normal * (TrackWidth * 0.5f);
                var point = leftSide ? sample.Position + offset : sample.Position - offset;
                points[i] = point;
                linePositions[i] = point;
            }

            points[^1] = points[0];

            var boundary = new GameObject(name);
            boundary.transform.SetParent(root, false);
            var collider = boundary.AddComponent<EdgeCollider2D>();
            collider.points = points;
            collider.edgeRadius = 0.08f;
            collider.sharedMaterial = RacingVisuals.WallPhysicsMaterial;

            var line = boundary.AddComponent<LineRenderer>();
            line.positionCount = linePositions.Length;
            line.SetPositions(linePositions);
            line.loop = true;
            line.widthMultiplier = 0.22f;
            line.numCornerVertices = 5;
            line.numCapVertices = 5;
            line.material = RacingVisuals.CreateMaterial(color);
            line.sortingOrder = 8;
        }

        private static void CreateCenterDashes(Transform root, RaceTrack track)
        {
            var dashRoot = new GameObject("Center Dashes");
            dashRoot.transform.SetParent(root, false);
            var material = RacingVisuals.CreateMaterial(new Color(1f, 0.86f, 0.2f));

            for (var distance = 3f; distance < track.TotalLength; distance += 5.2f)
            {
                var pose = track.GetPoseAtDistance(distance);
                var dash = new GameObject("Dash");
                dash.transform.SetParent(dashRoot.transform, false);
                dash.transform.SetPositionAndRotation(pose.Position, Quaternion.Euler(0f, 0f, pose.Rotation));

                var line = dash.AddComponent<LineRenderer>();
                line.positionCount = 2;
                line.SetPosition(0, new Vector3(0f, -0.55f, 0f));
                line.SetPosition(1, new Vector3(0f, 0.55f, 0f));
                line.useWorldSpace = false;
                line.widthMultiplier = 0.12f;
                line.numCapVertices = 3;
                line.material = material;
                line.sortingOrder = 7;
            }
        }

        private static void CreateStartFinish(Transform root, RaceTrack track)
        {
            var pose = track.GetPoseAtDistance(0f);
            var finish = new GameObject("Start Finish Line");
            finish.transform.SetParent(root, false);
            finish.transform.SetPositionAndRotation(pose.Position, Quaternion.Euler(0f, 0f, pose.NormalRotation));

            for (var i = 0; i < 7; i++)
            {
                var stripe = new GameObject($"Finish Stripe {i + 1}");
                stripe.transform.SetParent(finish.transform, false);
                stripe.transform.localPosition = new Vector3(-TrackWidth * 0.5f + 0.35f + i * 0.58f, 0f, 0f);
                stripe.transform.localScale = new Vector3(0.32f, 0.85f, 1f);

                var renderer = stripe.AddComponent<SpriteRenderer>();
                renderer.sprite = RacingVisuals.WhiteSprite;
                renderer.color = i % 2 == 0 ? Color.white : Color.black;
                renderer.sortingOrder = 15;
            }
        }

        private static void CreateCheckpointGates(Transform root, RaceTrack track)
        {
            var gates = new GameObject("Checkpoint Gates");
            gates.transform.SetParent(root, false);

            for (var index = 0; index < track.CheckpointCount; index++)
            {
                var pose = track.GetPoseAtDistance(track.GetCheckpointDistance(index));
                var gate = new GameObject(index == 0 ? "Finish Checkpoint" : $"Checkpoint {index}");
                gate.transform.SetParent(gates.transform, false);
                gate.transform.SetPositionAndRotation(pose.Position, Quaternion.Euler(0f, 0f, pose.NormalRotation));

                var collider = gate.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
                collider.size = new Vector2(TrackWidth + 0.8f, 0.9f);

                var checkpoint = gate.AddComponent<CheckpointGate>();
                checkpoint.Configure(index);
            }
        }
    }
}
