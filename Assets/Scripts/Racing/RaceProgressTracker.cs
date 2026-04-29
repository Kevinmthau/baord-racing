using System;
using UnityEngine;

namespace BoardRacing
{
    public sealed class RaceProgressTracker : MonoBehaviour
    {
        private RaceTrack track;
        private int totalLaps;
        private int checkpointCount;
        private int nextCheckpointIndex;
        private int lastCheckpointIndex;
        private int checkpointsPassedThisLap;

        public event Action<RaceProgressTracker> FinishedRace;

        public int CompletedLaps { get; private set; }
        public int CurrentPosition { get; set; } = 1;
        public bool IsFinished { get; private set; }
        public string RacerName { get; private set; }
        public Color RacerColor { get; private set; }

        public void Initialize(RaceTrack raceTrack, int laps)
        {
            track = raceTrack;
            totalLaps = laps;
            checkpointCount = raceTrack.CheckpointCount;
            RacerName = GetComponent<ArcadeCarController>().PlayerIndex == 0 ? "RED" : "BLUE";
            RacerColor = GetComponent<ArcadeCarController>().PlayerIndex == 0 ? Color.red : new Color(0.05f, 0.32f, 1f);
            ResetProgress();
        }

        public void ResetProgress()
        {
            CompletedLaps = 0;
            nextCheckpointIndex = 1;
            lastCheckpointIndex = 0;
            checkpointsPassedThisLap = 0;
            IsFinished = false;
            CurrentPosition = 1;
        }

        public void ProcessCheckpoint(int checkpointIndex)
        {
            if (IsFinished || checkpointIndex != nextCheckpointIndex)
            {
                return;
            }

            if (checkpointIndex == 0)
            {
                CompletedLaps++;
                lastCheckpointIndex = 0;
                checkpointsPassedThisLap = 0;
                nextCheckpointIndex = 1;

                if (CompletedLaps >= totalLaps)
                {
                    IsFinished = true;
                    FinishedRace?.Invoke(this);
                }

                return;
            }

            lastCheckpointIndex = checkpointIndex;
            checkpointsPassedThisLap = checkpointIndex;
            nextCheckpointIndex = checkpointIndex + 1;
            if (nextCheckpointIndex >= checkpointCount)
            {
                nextCheckpointIndex = 0;
            }
        }

        public float GetRaceProgressScore()
        {
            if (IsFinished)
            {
                return totalLaps * checkpointCount + 100f;
            }

            var segmentStart = track.GetCheckpointDistance(lastCheckpointIndex);
            var segmentEnd = track.GetCheckpointDistance(nextCheckpointIndex);
            if (segmentEnd <= segmentStart)
            {
                segmentEnd += track.TotalLength;
            }

            var distance = track.GetNearestDistance(transform.position);
            if (distance < segmentStart - track.TotalLength * 0.5f)
            {
                distance += track.TotalLength;
            }

            var segmentFraction = Mathf.Clamp01(Mathf.InverseLerp(segmentStart, segmentEnd, distance));
            return CompletedLaps * checkpointCount + checkpointsPassedThisLap + segmentFraction;
        }
    }
}
