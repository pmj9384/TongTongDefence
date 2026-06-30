using UnityEngine;

namespace UnityCommunity.UnitySingleton
{
    /// <summary>
    /// The basic MonoBehaviour singleton implementation, this singleton is destroyed after scene changes, use <see cref="PersistentMonoSingleton{T}"/> if you want a persistent and global singleton instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour, ISingleton where T : MonoSingleton<T>
    {
        private static T instance;
        private SingletonInitializationStatus initializationStatus = SingletonInitializationStatus.None;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
#if UNITY_6000
                    instance = FindAnyObjectByType<T>();
#else
                    instance = FindObjectOfType<T>();
#endif
                    if (instance == null)
                    {
                        GameObject obj = new GameObject();
                        obj.name = typeof(T).Name;
                        instance = obj.AddComponent<T>();
                        instance.OnMonoSingletonCreated();
                    }
                }
                return instance;
            }
        }

        public virtual bool IsInitialized => this.initializationStatus == SingletonInitializationStatus.Initialized;

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                InitializeSingleton();
            }
            else
            {
                if (Application.isPlaying)
                    Destroy(gameObject);
                else
                    DestroyImmediate(gameObject);
            }
        }

        protected virtual void OnMonoSingletonCreated() { }
        protected virtual void OnInitializing() { }
        protected virtual void OnInitialized() { }

        public virtual void InitializeSingleton()
        {
            if (this.initializationStatus != SingletonInitializationStatus.None)
                return;

            this.initializationStatus = SingletonInitializationStatus.Initializing;
            OnInitializing();
            this.initializationStatus = SingletonInitializationStatus.Initialized;
            OnInitialized();
        }

        public virtual void ClearSingleton() { }

        public static void CreateInstance()
        {
            DestroyInstance();
            instance = Instance;
        }

        public static void DestroyInstance()
        {
            if (instance == null)
                return;

            instance.ClearSingleton();
            instance = default(T);
        }
    }
}
