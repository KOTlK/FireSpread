using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using static Context;
using static UIManager;
using static Systems;

public class Main : MonoBehaviour {
    public Transform      ParticlesParent;
    public EntityManager  EntityManager;
    public Canvas         UIParent;
    public TaskRunner     TaskRunner;
    public Vector2Int     Size = new Vector2Int(100, 100);
    public string         Localization = "eng";

    private void Awake() {
        Config.ParseVars();
        Locale.LoadLocalization(Localization);
        InitContext(EntityManager);
        Coroutines.InitCoroutines();
        TaskRunner = new TaskRunner();
        Events.Init();
        ResourceManager.Initialize();
        Particles.Initialize(ParticlesParent);
        SaveSystem.Init();

        UIManager.Init(UIParent, UIParent.transform);
        UIManager.RegisterDependencies(EntityManager);

        ResourceManager.LoadBundle("playables");
        ResourceManager.LoadBundle("particles");

        ComponentSystem<Burnable>.Make();
        ComponentSystem<Burned>.Make();
        ComponentSystem<Fire>.Make(UpdateFire);

        DontDestroyOnLoad(gameObject);

        var field = PlantField.Make(Size);
        Context.Field = field;
    }

    private void OnDestroy() {
        SaveSystem.Dispose();
        ResourceManager.Free();
        DestroyContext();
        SaveSystem.Dispose();
    }

    private void Start() {
        EntityManager.BakeEntities();
    }

    private void Update() {
        SingleFrameArena.Free();
        Clock.Update();
        UpdateUI(Clock.Delta);
        Coroutines.RunCoroutines();
        TaskRunner.RunTaskGroup(TaskGroupType.ExecuteAlways);
        EntityManager.Execute();
        ComponentSystem<Fire>.Update();
        UpdateLateUI(Clock.Delta);
    }

    [ConsoleCommand("quit")]
    public static void Quit() {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}
