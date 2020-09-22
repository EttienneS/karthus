using Assets.Helpers;
using Assets.Map;
using Assets.Structures;
using Camera;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.ServiceLocator
{
    public sealed class Loc
    {
        private readonly Dictionary<string, IGameService> _services = new Dictionary<string, IGameService>();

        private Loc()
        {
        }

        public static Loc Current { get; private set; }

        public static CreatureController GetCreatureController()
        {
            return Current.Get<CreatureController>();
        }

        public static ItemController GetItemController()
        {
            return Current.Get<ItemController>();
        }

        public static MapController GetMap()
        {
            return Current.Get<MapController>();
        }

        public static IdService GetIdService()
        {
            return Current.Get<IdService>();
        }

        public static SpriteStore GetSpriteStore()
        {
            return Current.Get<SpriteStore>();
        }

        public static FileController GetFileController()
        {
            return Current.Get<FileController>();
        }

        public static StructureController GetStructureController()
        {
            return Current.Get<StructureController>();
        }

        public static Game GetGameController()
        {
            return Current.Get<Game>();
        }

        public static VisualEffectController GetVisualEffectController()
        {
            return Current.Get<VisualEffectController>();
        }

        public static FactionController GetFactionController()
        {
            return Current.Get<FactionController>();
        }

        public static TimeManager GetTimeManager()
        {
            return Current.Get<TimeManager>();
        }

        public static CameraController GetCamera()
        {
            return Current.Get<CameraController>();
        }

        public static ZoneController GetZoneController()
        {
            return Current.Get<ZoneController>();
        }

        public static void Initiailze()
        {
            Current = new Loc();
        }

        public T Get<T>() where T : IGameService
        {
            string key = typeof(T).Name;
            if (!_services.ContainsKey(key))
            {
                Debug.LogError($"{key} not registered with {GetType().Name}");
                throw new InvalidOperationException();
            }

            return (T)_services[key];
        }

        public static void Reset()
        {
            Current = null;
        }

        public void InitializeServices()
        {
            foreach (var service in _services)
            {
                using (Instrumenter.Start(service.Key))
                {
                    service.Value.Initialize();
                }
            }
        }

        public void Register<T>(T service) where T : IGameService
        {
            string key = typeof(T).Name;
            if (_services.ContainsKey(key))
            {
                Debug.LogError($"Attempted to register service of type {key} which is already registered with the {GetType().Name}.");
                return;
            }

            _services.Add(key, service);
        }

        public void Unregister<T>() where T : IGameService
        {
            string key = typeof(T).Name;
            if (!_services.ContainsKey(key))
            {
                Debug.LogError($"Attempted to unregister service of type {key} which is not registered with the {GetType().Name}.");
                return;
            }

            _services.Remove(key);
        }
    }
}