using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 * CURRENTLY ACHIEVING FUNCTIONALITY WITH GAMESTATEMANAGER
 */ 
namespace AikaGame.GameData
{
    public class GameStateData : MonoBehaviour
    {
        public static int highestLevelCompleted = 0; // only one int is needed since players cannot skip levels. If the player could skip levels, a separate bool would be needed for each level
        public static bool reloadedLevel = false;
    }
}