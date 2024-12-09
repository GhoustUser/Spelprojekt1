using System;
using System.Collections;
using UnityEngine;

namespace Default
{
    public static class Default
    {
        public static float TILE_SIZE = 1.0f;
        public static int PLAYER_MAX_HEALTH = 5;
        public static int FRAME_RATE = 60;

        // Function that executes selection action x times per second.
        public static IEnumerator ExecuteRepeatedly(Action action, int timesPerSecond)
        {
            float interval = 1f / timesPerSecond;
            while (true)
            {
                action.Invoke();
                yield return new WaitForSeconds(interval);
            }
        }
    }
}
