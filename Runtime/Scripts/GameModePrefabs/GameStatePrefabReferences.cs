using System.Collections; 
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace GameStateMachineCore
{
    [CreateAssetMenu]
    public class GameStatePrefabReferences : ScriptableObject
    {
        [SerializeField] private GameStatePrefabReferences Parent;
        [SerializeField] private List<AssetReferenceGameObject> instantiableReferences = new List<AssetReferenceGameObject>();
        [SerializeField] private List<AssetReferenceScriptableObject> scriptableObjectsReferences = new List<AssetReferenceScriptableObject>();

        public void SetParent(GameStatePrefabReferences nParent)
        {
            Parent = nParent;
        }

        public List<AssetReferenceGameObject> GetGameObjectReferences()
        {
            List<AssetReferenceGameObject> rList = new List<AssetReferenceGameObject>(instantiableReferences);
            if (Parent != null)
                rList.AddRange(Parent.GetGameObjectReferences());
            return rList;
        }

        public List<AssetReferenceScriptableObject> GetScriptableObjectReferences()
        {
            List<AssetReferenceScriptableObject> rList = new List<AssetReferenceScriptableObject>(scriptableObjectsReferences);
            if (Parent != null)
                rList.AddRange(Parent.GetScriptableObjectReferences());
            return rList;
        }

#if UNITY_EDITOR
        private void Save()
        {
            Debug.Log($"Saving {this}");
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public void RemoveDuplicatedAssets()
        {
            if (Parent != null)
            {
                RemoveDuplicateReferences(ref instantiableReferences, Parent.GetGameObjectReferences());
                RemoveDuplicateReferences(ref scriptableObjectsReferences, Parent.GetScriptableObjectReferences());
            }
        }

        private void RemoveDuplicateReferences<T>(ref List<T> myAssets, List<T> parentAssets) where T : AssetReference
        {
            for (int i = myAssets.Count - 1; i >= 0; i--)
            {
                Object currentAsset = myAssets[i].Asset;
                if (parentAssets.Find(x => x.Asset == currentAsset) != null)
                {
                    Debug.LogWarning(
                        $"Duplicated Asset in ${this.name}. Asset:{myAssets[i].Asset} already existed in {Parent.name} " +
                        $"Asset:{myAssets[i].Asset} was removed from ${this.name}");
                    myAssets.RemoveAt(i);
                }
            }
        }
#endif
    }

    [System.Serializable]
    public class AssetReferenceScriptableObject : AssetReferenceT<ScriptableObject>
    {
        public AssetReferenceScriptableObject(string guid) : base(guid)
        {
        }
    }


}