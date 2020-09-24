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
            Loc.Initiailze();
            Loc.Current.Register(FindObjectOfType<FileController>());
            Loc.Current.Register(FindObjectOfType<SpriteStore>());
            Loc.Current.Register(FindObjectOfType<MeshRendererFactory>());

            Loc.Current.Register(new IdService());
            Loc.Current.Register(FindObjectOfType<StructureController>());
            Loc.Current.Register(FindObjectOfType<ItemController>());
            Loc.Current.Register(FindObjectOfType<CreatureController>());
            Loc.Current.Register(FindObjectOfType<ZoneController>());
            Loc.Current.Register(FindObjectOfType<TimeManager>());

            Loc.Current.Register(FindObjectOfType<FactionController>());

            Loc.Current.Register(FindObjectOfType<MapController>());

            Loc.Current.Register(FindObjectOfType<VisualEffectController>());

            Loc.Current.Register(FindObjectOfType<Game>());

            Loc.Current.Register(FindObjectOfType<CameraController>());
            Loc.Current.Register(FindObjectOfType<CursorController>());

            Loc.Current.InitializeServices();
        }
    }
}