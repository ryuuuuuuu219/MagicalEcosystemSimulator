using UnityEngine;
using UnityEngine.SceneManagement;

public class menu : MonoBehaviour
{
    const string WorldSeedKey = "world_seed";

    public void OnClickButtom()
    {
        int seed = Random.Range(0, 99999);
        PlayerPrefs.SetInt(WorldSeedKey, seed);
        PlayerPrefs.Save();
        SceneManager.LoadScene("pregame");
    }
}
