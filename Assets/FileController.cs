using UnityEngine;

public class FileController : MonoBehaviour
{
    private static FileController _instance;

    public TextAsset[] StructureJson;
    public TextAsset[] ItemJson;

    public static FileController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.Find("FileController").GetComponent<FileController>();
            }

            return _instance;
        }
    }
}