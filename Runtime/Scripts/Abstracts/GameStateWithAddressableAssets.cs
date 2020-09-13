using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
namespace GameStateMachineCore
{
    public abstract class GameStateWithAddressableAssets : GameState
    {
        private readonly GameStatePrefabReferences prefabReferences;

        protected virtual List<AssetReference> ExtraAssets { get; } = new List<AssetReference>();

        Dictionary<AssetReference, AsyncOperationHandle<GameObject>> _asyncOperationHandle = new Dictionary<AssetReference, AsyncOperationHandle<GameObject>>();

        protected List<ScriptableObject> scriptableObjects;
        protected List<GameObject> gameObjects;

        int initializationSteps;
        int currentInstantiateStep;

        int soTotalSteps;
        int soCurrentSteps;
        protected GameObject _root;
        protected GameStateProxy proxy;
        public static Action<float> OnInstantiationProgress;

        public GameStateWithAddressableAssets()
        {
            prefabReferences = GameStatePrefabsManager.GetPrefabReferences(this);
            gameObjects = new List<GameObject>();
            scriptableObjects = new List<ScriptableObject>();
        }

        public abstract void OnEnter();
        public abstract void OnExit();

        /// <summary>
        /// Dont override
        /// </summary>
        public override void Enter()
        {
            InstantiatePrefabs();
            AssignGameStateReferences(this);
        }

        public override void Exit()
        {
            base.Exit();
            RemoveGameStateReferences(this);
            DestroyAndReleasePrefabs();
            OnExit();
        }

        /// <summary>
        /// Destruye los prefabs del modo y libera la referencia del asset para salvar memoria
        /// </summary>
        private void DestroyAndReleasePrefabs()
        {
            for (int i = gameObjects.Count - 1; i >= 0; i--)
                Addressables.ReleaseInstance(gameObjects[i]);

            for (int i = 0; i < scriptableObjects.Count; i++)
                Addressables.Release(scriptableObjects[i]);

            UnityEngine.Object.Destroy(_root);
        }

        internal void Release(SelfReleaseAddresable selfReleaseAddresable)
        {
            GameObject go = selfReleaseAddresable.gameObject;
            int index = gameObjects.IndexOf(go);
            if (index != -1)
            {
                Debug.Log($"SelfRelease of {go.name} from {this} Successfull ");
                gameObjects.RemoveAt(index);
                Addressables.ReleaseInstance(go);
            }
            else
                Debug.LogError($"Objeto no pertenece a {this}");
        }


        /// <summary>
        /// Instancia los prefabs del modo utilizando el sistema de Addresable assets para tener una mejor performance
        /// </summary>
        protected void InstantiatePrefabs()
        {
            _root = new GameObject();
            proxy = _root.AddComponent<GameStateProxy>();
            proxy.Initialize(this);
            _root.name = this.ToString();

            List<AssetReference> assetReferences = new List<AssetReference>(prefabReferences.GetGameObjectReferences());
            Debug.Log($"<color=green> Instantiating {assetReferences.Count} from {this.GetType().Name} </color>");
            assetReferences.AddRange(ExtraAssets);
            Debug.Log($"Instantiating {ExtraAssets.Count} InheritedAssets from {this.GetType().Name}");

            for (int i = 0; i < ExtraAssets.Count; i++)
                Debug.Log($"{ExtraAssets[i]}");

            initializationSteps = assetReferences.Count * 2;

            if (assetReferences.Count == 0)
                InstantiationDone();

            Debug.Log($"assetReferences.Count:{assetReferences.Count}");

            for (int i = 0; i < assetReferences.Count; i++)
            {
                AssetReference itemReference = assetReferences[i];
                //CreateObjectAsync(itemReference);
                Debug.Log($"OnInstantiationUpdate {(float)currentInstantiateStep / initializationSteps}");
                currentInstantiateStep++;
                OnInstantiationProgress?.Invoke((float)currentInstantiateStep / initializationSteps);
#if UNITY_EDITOR
                Debug.Log($"<color=blue> InstantiateAsync Started:</color> {itemReference.editorAsset.name}");
#endif
                Addressables.InstantiateAsync(itemReference, _root.transform).Completed += AsyncInstantiationComplete;
            }
        }

        private void AsyncInstantiationComplete(AsyncOperationHandle<GameObject> obj)
        {

            Debug.Log($"<color=green> AsyncInstantiationComplete </color> {obj.Result.name}");

            currentInstantiateStep++;
            Debug.Log($"OnInstantiationUpdate <color=white> { ((float)currentInstantiateStep / initializationSteps) * 100 }% </color>");
            OnInstantiationProgress?.Invoke((float)currentInstantiateStep / initializationSteps);

            gameObjects.Add(obj.Result);
            obj.Result.AddComponent<SelfReleaseAddresable>().Initialize(this);

            if (currentInstantiateStep == initializationSteps)
                InstantiationDone();
        }

        private void InstantiationDone()
        {
            Debug.Log("<color=green> InstantiationDone </color>");
            OnInstantiationProgress?.Invoke(1);
            LoadScriptableObjects();
        }

        private void LoadScriptableObjects()
        {
#if UNITY_EDITOR
            Debug.Log($"<color=green> LoadScriptableObjects Started </color>");
#endif
            List<AssetReferenceScriptableObject> assetReferences = new List<AssetReferenceScriptableObject>(prefabReferences.GetScriptableObjectReferences());
            soCurrentSteps = 0;
            soTotalSteps = assetReferences.Count;

            if (assetReferences.Count == 0)
            {
#if UNITY_EDITOR
                Debug.Log($"<color=green> LoadScriptableObjects DONE </color>");
#endif
                InitializationDone();
                return;
            }

            foreach (AssetReferenceScriptableObject item in assetReferences)
            {
                item.LoadAssetAsync().Completed += ScriptableObjectLoaded;
            }

#if UNITY_EDITOR
            Debug.Log($"<color=green> LoadScriptableObjects DONE </color>");
#endif
            //Addressables.LoadAssetsAsync<ScriptableObject>(assetReferences, ScriptableObjectLoaded).Completed += LoadAssetsAsyncComplete;
        }

        private void ScriptableObjectLoaded(AsyncOperationHandle<ScriptableObject> obj)
        {
            if (obj.Status == AsyncOperationStatus.Succeeded)
            {
#if UNITY_EDITOR
                Debug.Log($"<color=green> ScriptableObjectLoaded:  </color> {obj.Result.name} ");
#endif
                ScriptableObjectLoaded(obj.Result);
            }
            else
                Debug.LogError($"Error cargando {obj.DebugName}");
        }

        private void ScriptableObjectLoaded(ScriptableObject obj)
        {
            soCurrentSteps++;
            Debug.Log($"OnScripableObjectsLoadUpdate {(float)soCurrentSteps / soTotalSteps}");
            Debug.Log($"Loaded {obj.ToString()}");
            OnInstantiationProgress?.Invoke((float)soCurrentSteps / soTotalSteps);
            scriptableObjects.Add(obj);

            if (soCurrentSteps == soTotalSteps)
                InitializationDone();
        }


        private void OnLoadResourceLocationsDone(AsyncOperationHandle<IList<IResourceLocation>> obj)
        {
            Debug.Log("OnLoadResourceLocationsDone");
            IList<IResourceLocation> locations = obj.Result;
            Addressables.LoadAssetsAsync<ScriptableObject>(locations, ScriptableObjectLoaded).Completed += LoadAssetsAsyncComplete;
            Debug.Log("obj.Result.Count:" + obj.Result.Count);
        }

        private void AssignGameStateReferences<T>(T myself) where T : GameState
        {
            foreach (GameObject item in gameObjects)
            {
                IUseGameState useGameState = item.GetComponent<IUseGameState<T>>();
                if (useGameState != null)
                    useGameState.GamePlayState = myself;
            }
            foreach (ScriptableObject item in scriptableObjects)
            {
                IUseGameState useGameState = item as IUseGameState<T>;
                if (useGameState != null)
                    useGameState.GamePlayState = myself;
            }
        }


        private void RemoveGameStateReferences<T>(T myself) where T : GameState
        {
            foreach (GameObject item in gameObjects)
            {
                IUseGameState useGameState = item.GetComponent<IUseGameState<T>>();
                if (useGameState != null)
                    useGameState.GamePlayState = null;
            }
            foreach (ScriptableObject item in scriptableObjects)
            {
                IUseGameState useGameState = item as IUseGameState<T>;
                if (useGameState != null)
                    useGameState.GamePlayState = null;
            }
        }




        private void LoadAssetsAsyncComplete(AsyncOperationHandle<IList<ScriptableObject>> obj)
        {
            scriptableObjects = new List<ScriptableObject>(obj.Result);
            InitializationDone();
        }

        private void InitializationDone()
        {
            Debug.Log($"InitializationDone");
            OnEnter();
        }

        protected T FindComponentIntLocalPrefab<T>(SearchType searchType = SearchType.Simple) where T : Component
        {
            Debug.Log($"{this} => FindLocalPrefab => {typeof(T).ToString()}");

            GameObject go;
            switch (searchType)
            {
                case SearchType.Simple:
                default:
                    go = gameObjects.Find(x => x.gameObject.GetComponent<T>() != null);
                    break;
                case SearchType.Parent:
                    go = gameObjects.Find(x => x.gameObject.GetComponentInParent<T>() != null);
                    break;
                case SearchType.Children:
                    go = gameObjects.Find(x => x.gameObject.GetComponentInChildren<T>() != null);
                    break;
            }

            if (go == null)
            {
                Debug.LogError($"No se encontro ningun prefab con el componente {typeof(T).ToString()} en {this}");
                return null;
            }
            else return go.gameObject.GetComponent<T>();
        }

        protected enum SearchType { Simple, Parent, Children }
        protected void AssignPrefabTo<T>(out T targetObject, SearchType searchType = SearchType.Simple) where T : Component
        {
            targetObject = FindComponentIntLocalPrefab<T>(searchType);
        }

        protected T FindLocalScriptableObject<T>() where T : ScriptableObject
        {
            Type t = typeof(T);
            Debug.Log($"{this} => FindLocalPrefab => {typeof(T).ToString()}");
            return scriptableObjects.Find(x => (x.GetType() == t)) as T;
        }

        protected void AssignScriptableObjectTo<T>(out T targetObject) where T : ScriptableObject
        {
            targetObject = FindLocalScriptableObject<T>();
        }
    }


    public interface IUseGameState
    {
        GameState GamePlayState { get; set; }
    }

    public interface IUseGameState<T> : IUseGameState where T : GameState
    {
        new T GamePlayState { get; set; }
    }
}