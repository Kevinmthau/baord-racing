using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BoardRacing
{
    public sealed class RaceGameController : MonoBehaviour
    {
        private const int TotalLaps = 3;

        private readonly List<RaceProgressTracker> racers = new();

        private RaceTrack track;
        private RaceHud hud;
        private ArcadeCarController redCar;
        private ArcadeCarController blueCar;
        private bool raceFinished;
        private RaceProgressTracker winner;

        private void Start()
        {
            Physics2D.gravity = Vector2.zero;

            SetupCamera();
            track = TrackBuilder.Build();
            CreateCars();
            hud = RaceHud.Create(transform);
            ResetRace();
        }

        private void Update()
        {
            if (raceFinished && WasRestartPressed())
            {
                ResetRace();
            }

            UpdateRacePositions();
            hud.UpdateRace(racers, raceFinished, winner);
        }

        private void SetupCamera()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                cameraObject.AddComponent<AudioListener>();
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.transform.SetPositionAndRotation(new Vector3(0f, 0f, -10f), Quaternion.identity);
            camera.orthographic = true;
            camera.orthographicSize = 11.25f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.07f, 0.14f, 0.11f);
        }

        private void CreateCars()
        {
            var spawn = track.GetPoseAtDistance(1.35f);
            var sideOffset = spawn.Normal * GetLaneOffset();

            redCar = CarFactory.Create("Player 1 Red Car", 0, Color.red, spawn.Position + sideOffset, spawn.Rotation);
            blueCar = CarFactory.Create("Player 2 Blue Car", 1, new Color(0.05f, 0.32f, 1f), spawn.Position - sideOffset, spawn.Rotation);

            racers.Clear();
            racers.Add(redCar.GetComponent<RaceProgressTracker>());
            racers.Add(blueCar.GetComponent<RaceProgressTracker>());

            foreach (var racer in racers)
            {
                racer.Initialize(track, TotalLaps);
                racer.FinishedRace += HandleRacerFinished;
            }
        }

        public void ResetRace()
        {
            raceFinished = false;
            winner = null;

            var spawn = track.GetPoseAtDistance(1.35f);
            var sideOffset = spawn.Normal * GetLaneOffset();
            redCar.ResetCar(spawn.Position + sideOffset, spawn.Rotation);
            blueCar.ResetCar(spawn.Position - sideOffset, spawn.Rotation);

            foreach (var racer in racers)
            {
                racer.ResetProgress();
            }

            redCar.SetInputLocked(false);
            blueCar.SetInputLocked(false);
            hud.SetRestartVisible(false);
        }

        private float GetLaneOffset()
        {
            return track.TrackWidth * 0.18f;
        }

        private void HandleRacerFinished(RaceProgressTracker tracker)
        {
            if (raceFinished)
            {
                return;
            }

            raceFinished = true;
            winner = tracker;
            redCar.SetInputLocked(true);
            blueCar.SetInputLocked(true);
            hud.SetRestartVisible(true);
        }

        private void UpdateRacePositions()
        {
            var ordered = racers
                .OrderByDescending(racer => racer.GetRaceProgressScore())
                .ToList();

            for (var index = 0; index < ordered.Count; index++)
            {
                ordered[index].CurrentPosition = index + 1;
            }
        }

        private static bool WasRestartPressed()
        {
            foreach (var gamepad in Gamepad.all)
            {
                if (gamepad.startButton.wasPressedThisFrame || gamepad.buttonSouth.wasPressedThisFrame)
                {
                    return true;
                }
            }

            var keyboard = Keyboard.current;
            return keyboard != null &&
                   (keyboard.rKey.wasPressedThisFrame ||
                    keyboard.enterKey.wasPressedThisFrame ||
                    keyboard.spaceKey.wasPressedThisFrame);
        }
    }
}
