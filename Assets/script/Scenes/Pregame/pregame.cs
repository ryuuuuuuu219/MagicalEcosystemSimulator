using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class pregame : MonoBehaviour
{
    const string WorldSeedKey = "world_seed";

    public int seed = 0;
    public TextMeshProUGUI seedfield;

    void Awake()
    {
        EnsureEventSystem();
    }

    void Start()
    {
        if (PlayerPrefs.HasKey(WorldSeedKey))
        {
            seed = PlayerPrefs.GetInt(WorldSeedKey);
        }
        else
        {
            seed = Random.Range(0, 99999);
            PlayerPrefs.SetInt(WorldSeedKey, seed);
            PlayerPrefs.Save();
        }

        RefreshSeedLabel();
    }

    void Update()
    {
        RefreshSeedLabel();
    }

    void RefreshSeedLabel()
    {
        if (seedfield != null)
            seedfield.text = "seed:" + seed.ToString();
    }

    void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
            return;

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        DontDestroyOnLoad(eventSystemObject);
    }

    public void OnClickButtom()
    {
        PlayerPrefs.SetInt(WorldSeedKey, seed);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Ingame");
    }

    public void OnClickButtom2()
    {
        SceneManager.LoadScene("main");
    }
}
