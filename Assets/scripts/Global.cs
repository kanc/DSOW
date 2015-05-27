using UnityEngine;
using System.Collections;

namespace GlobalData
{
    public enum InteractuableEvents
    {
        GetSecurityCard,
        GetReceptionNote,
        GetFernandoClue,
        GetITKey,
        GetITNote,
        ShowEliasFaceInScreen,
        MoveZombie,
        PlayIndepenceSound,
        PlayFxSound,
        PlaceZombieInWC,
        OpenDoor,
        CloseDoor,
    };

    public enum ZombieState
    {
        Rising,
        Moving,
        Attack,
        Death
    };

    public static class Constants
    {        
        public static string    ENEMY_TAG = "Enemy";
        public static int       INTERACTUABLE_COLLIDER_LAYER = 8;        
        public static int       GROUND_COLLIDER_LAYER = 9;
        public static int       ZOMBIE_COLLIDER_LAYER = 10;
        public static string    ZOMBIE_SPEED_PARAM = "Speed";
        public static string    ZOMBIE_ALIVE_PARAM = "Alive";
        public static string    ZOMBIE_HIT_PARAM = "Hit";
        public static string    ZOMBIE_ATTACK_PARAM = "Attacking";
        public static string    ZOMBIE_DEATH_PARAM = "Death";
        public static string    ZOMBIE_WALKTYPE_PARAM = "WalkType";

    }

    public static class CameraUtil
    {
        public static bool IsLookingAtInteract(GameObject target, out RaycastHit hit)
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);            
            int layerMask = 1 << GlobalData.Constants.INTERACTUABLE_COLLIDER_LAYER;

            if (Physics.Raycast(ray, out hit, 1000, layerMask))
            {                
                return (hit.collider.gameObject.GetInstanceID() == target.GetInstanceID()) ? true : false;
            }
            else
            {
                return false;
            }
        }
    }

    public static class GameEventsCall
    {
        public static void TriggerEvent(InteractuableEvents eventId)
        {
            GameEvents game = null; 
            game = (GameEvents)GameObject.FindObjectOfType(typeof(GameEvents));

            if (game == null)
            {
                GameObject gameEventObj = new GameObject("Game Events Obj");
                game = gameEventObj.AddComponent<GameEvents>();
            }

            switch (eventId)
            {
                case InteractuableEvents.GetSecurityCard:       game.GetSecurityCard();     break;
                case InteractuableEvents.GetReceptionNote:      game.GetReceptionNote();    break;
                case InteractuableEvents.GetFernandoClue:       game.GetFernandoClue();     break;
                case InteractuableEvents.GetITKey:              game.GetITKey();            break;
                case InteractuableEvents.GetITNote:             game.GetITNote();           break;
                case InteractuableEvents.ShowEliasFaceInScreen: game.ShowEliasZombieFace(); break;
                case InteractuableEvents.MoveZombie:            game.MoveZombie();          break;
                case InteractuableEvents.PlaceZombieInWC:       game.PlaceZombieInWC(); break;
                case InteractuableEvents.PlayIndepenceSound:    game.PlayIndepencia(); break;
                case InteractuableEvents.OpenDoor:              game.OpenDoorSound(); break;
                case InteractuableEvents.CloseDoor:             game.CloseDoorSound(); break;

            }
        }
    }


}