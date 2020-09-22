using Assets.Map;
using Assets.Models;
using Assets.Structures;
using Camera;
using UnityEngine;

namespace Assets.ServiceLocator
{
    public class Bootstrapper : MonoBehaviour
    {
        public void Awake()
        {
            ServiceLocator.Initiailze();
            ServiceLocator.Current.Register(FindObjectOfType<FileController>());
            ServiceLocator.Current.Register(FindObjectOfType<SpriteStore>());
            ServiceLocator.Current.Register(FindObjectOfType<MeshRendererFactory>());

            ServiceLocator.Current.Register(FindObjectOfType<CameraController>());

            ServiceLocator.Current.Register(new IdService());
            ServiceLocator.Current.Register(FindObjectOfType<FactionController>());
            ServiceLocator.Current.Register(FindObjectOfType<StructureController>());
            ServiceLocator.Current.Register(FindObjectOfType<ItemController>());
            ServiceLocator.Current.Register(FindObjectOfType<CreatureController>());
            ServiceLocator.Current.Register(FindObjectOfType<ZoneController>());

            ServiceLocator.Current.Register(FindObjectOfType<MapController>());

            ServiceLocator.Current.Register(FindObjectOfType<TimeManager>());
            ServiceLocator.Current.Register(FindObjectOfType<VisualEffectController>());

            ServiceLocator.Current.Register(FindObjectOfType<CursorController>());

            ServiceLocator.Current.InitializeServices();
        }
    }
}