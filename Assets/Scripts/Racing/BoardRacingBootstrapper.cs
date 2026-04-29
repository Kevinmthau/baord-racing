using UnityEngine;

namespace BoardRacing
{
    public static class BoardRacingBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Object.FindAnyObjectByType<RaceGameController>() != null)
            {
                return;
            }

            Application.targetFrameRate = 60;

            var gameObject = new GameObject("Board Racing Game");
            gameObject.AddComponent<RaceGameController>();
        }
    }
}
