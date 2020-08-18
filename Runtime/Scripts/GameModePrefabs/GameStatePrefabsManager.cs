using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using UnityEngine;
using UnityEditor;
using System;
namespace GameStateMachineCore
{
    [CreateAssetMenu(fileName = "_GameStatePrefabsManager")]
    public class GameStatePrefabsManager : ScriptableObject
    {
        public List<GameStatePrefabReferences> gameStatePrefabs = new List<GameStatePrefabReferences>();

        private const string InstancePath = "GameStatePrefabs/_GameStatePrefabsManager";

        private static GameStatePrefabsManager instance;
        public static GameStatePrefabsManager Instance
        {
            get
            {
                if (instance == null)
                    InitializeInstance();
                return instance;
            }
        }

        private void Awake()
        {
#if UNITY_EDITOR
            GetAllSubclasses();
#endif
        }

        private static void InitializeInstance()
        {
#if UNITY_EDITOR
            string folderPath = Application.dataPath + "/Resources/" + "GameStatePrefabs/";
            Debug.Log("folderPath: " + folderPath);
            if (!Directory.Exists(folderPath))
            {
                Debug.Log("Creating folder at: " + folderPath);
                Directory.CreateDirectory(folderPath);
            }
#endif
            instance = Resources.Load<GameStatePrefabsManager>(InstancePath);
#if UNITY_EDITOR
            if (instance == null)
                instance = MakeScriptableObject.CreateAsset<GameStatePrefabsManager>("Assets/Resources/GameStatePrefabs/_GameStatePrefabsManager.asset");
            Instance.GetAllSubclasses();
#endif
        }

#if UNITY_EDITOR
        [MenuItem("GameStateMachine/UpdateGameStates", priority = 0)]
        public static void UpdateGameStates()
        {
            Instance.GetAllSubclasses();
            Instance.RemoveDuplicates();
        }

        private void RemoveDuplicates()
        {
            foreach (GameStatePrefabReferences item in gameStatePrefabs)
                item.RemoveDuplicatedAssets();
        }
#endif

#if UNITY_EDITOR
        public void GetAllSubclasses()
        {
            IEnumerable<System.Type> subclassTypes = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                                     from type in assembly.GetTypes()
                                                     where (type.IsSubclassOf(typeof(GameStateWithAddressableAssets)) && !type.IsAbstract)
                                                     select type;

            RemoveEmpty();

            //Search Or Create   
            foreach (System.Type item in subclassTypes)
            {
                string prefabsName = $"{item.Name}_Prefabs";
                if (gameStatePrefabs.Find(x => x != null && x.name == prefabsName) == null)
                {
                    string currentPath = $"Assets/Resources/GameStatePrefabs/{prefabsName}.asset";
                    string localPath = $"GameStatePrefabs/{prefabsName}";
                    GameStatePrefabReferences gameStatePrefab = Resources.Load<GameStatePrefabReferences>(localPath);

                    if (gameStatePrefab == null)
                    {
                        Debug.Log($"Created: {currentPath}");
                        gameStatePrefab = MakeScriptableObject.CreateAsset<GameStatePrefabReferences>(currentPath);
                    }
                    gameStatePrefabs.Add(gameStatePrefab);
                    EditorUtility.SetDirty(this);
                }
            }

            //Assign Parent
            foreach (System.Type item in subclassTypes)
            {
                string prefabsName = $"{item.Name}_Prefabs";

                GameStatePrefabReferences gameStatePrefabReferences = gameStatePrefabs.Find(x => x != null && x.name == prefabsName);
                Type parentType = item.BaseType;

                //   Debug.Log($"Parent type of {item.Name} => {parentType.Name}");
                if (parentType.Name != typeof(GameStateWithAddressableAssets).Name)
                {
                    string parentName = $"{parentType.Name}_Prefabs";
                    gameStatePrefabReferences.SetParent(gameStatePrefabs.Find(x => x != null && x.name == parentName));
                }
                EditorUtility.SetDirty(gameStatePrefabReferences);
            }


            AssetDatabase.SaveAssets();
        }

        private void RemoveEmpty()
        {
            for (int i = gameStatePrefabs.Count - 1; i >= 0; i--)
            {
                if (gameStatePrefabs[i] == null)
                    gameStatePrefabs.RemoveAt(i);
            }
        }
#endif

        internal static GameStatePrefabReferences GetPrefabReferences(GameStateWithAddressableAssets gameState)
        {
            Type originalType = gameState.GetType();
            string prefabsName = $"{originalType.Name}_Prefabs";
            return Instance.gameStatePrefabs.Find(x => x.name == prefabsName);
        }

#if UNITY_EDITOR
        [MenuItem("GameStateMachine/Manager", priority = 0)]
        public static void SelectMe()
        {
            Selection.SetActiveObjectWithContext(Instance, Instance);
        }
#endif

    }

#if UNITY_EDITOR
    public class MakeScriptableObject
    {
        public static T CreateAsset<T>(string path) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, $"{path}");
            AssetDatabase.SaveAssets();
            return asset;
        }
    }
#endif
}