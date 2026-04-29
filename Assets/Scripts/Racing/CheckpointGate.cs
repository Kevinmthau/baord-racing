using UnityEngine;

namespace BoardRacing
{
    public sealed class CheckpointGate : MonoBehaviour
    {
        [SerializeField] private int checkpointIndex;

        public void Configure(int index)
        {
            checkpointIndex = index;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var tracker = other.GetComponent<RaceProgressTracker>();
            if (tracker == null)
            {
                tracker = other.GetComponentInParent<RaceProgressTracker>();
            }

            tracker?.ProcessCheckpoint(checkpointIndex);
        }
    }
}
