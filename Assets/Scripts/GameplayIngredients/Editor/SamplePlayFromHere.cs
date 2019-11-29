using UnityEditor;
using UnityEngine;
using GameplayIngredients.Editor;
using GameplayIngredients;

static class SamplePlayFromHere
{
    [InitializeOnLoadMethod]
    static void SetupPlayFromHere()
    {
        PlayFromHere.OnPlayFromHere += PlayFromHere_OnPlayFromHere;
    }

    private static void PlayFromHere_OnPlayFromHere(Vector3 position, Vector3 forward)
    {
        // Get the FirstPersonCharacter prefab in Resources and instantiate it
        var prefab = (GameObject)Resources.Load("PlayFromHere-FirstPersonCharacter");
        var player = GameObject.Instantiate(prefab);
        player.name = "(Play from Here) " + prefab.name;

        // position the character correctly, so the current POV matches the player's height
        var controller = player.GetComponent<GameplayIngredients.Controllers.FirstPersonController>();
        player.transform.position = new Vector3(position.x, position.y - controller.PlayerHeight, position.z);

        // orient the player correctly
        var orient = forward;
        orient.Scale(new Vector3(1, 0, 1));
        player.transform.forward = orient;
        Messager.Send("PLAY_FROM_HERE");
    }
}
