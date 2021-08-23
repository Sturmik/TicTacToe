using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class Globals
{
    #region Constants

    // Field size
    public const int MIN_FIELD_SIZE = 3;
    public const int MAX_FIELD_SIZE = 30;

    #endregion

    #region Tags

    public const string MARK_CELL_TAG = "MarkCell";

    #endregion
}

public class GameManager : MonoBehaviour
{
    [Header("Field control")]
    // Field control prefab
    [SerializeField] private GameObject _fieldControl;
    // Win row quantity
    [SerializeField] private int _winRowQuant;
    // Field size (number of cells _fieldSize x _fieldSize)
    [Range(Globals.MIN_FIELD_SIZE, Globals.MAX_FIELD_SIZE)]
    [SerializeField] private int _fieldSize;
    // MarkField size
    [SerializeField] private float _markFieldSizeOnScreen;

    [Header("Inputs prefabs")]
    // Input check prefabs
    [SerializeField] private GameObject _inputUser;
    [SerializeField] private GameObject _inputAI;

    // Start is called before the first frame update
    void Start()
    {
        _fieldControl = SpawnManager.GetInstance().SpawnObject(SpawnManager.PoolType.FieldControl, _fieldControl);
        FieldControlManager fieldControlScript = _fieldControl.GetComponent<FieldControlManager>();
        fieldControlScript.CreateField(MarkType.Circle, _winRowQuant, _fieldSize, _markFieldSizeOnScreen, -4);
        fieldControlScript.SetInputs(SpawnManager.GetInstance().SpawnObject(SpawnManager.PoolType.InputUser, _inputUser),
            SpawnManager.GetInstance().SpawnObject(SpawnManager.PoolType.InputUser, _inputUser));

        GameObject fieldControl2 = SpawnManager.GetInstance().SpawnObject(SpawnManager.PoolType.FieldControl, _fieldControl);
        FieldControlManager fieldControlScript2 = fieldControl2.GetComponent<FieldControlManager>();
        fieldControlScript2.CreateField(MarkType.Circle, _winRowQuant, _fieldSize, _markFieldSizeOnScreen, 4);
        fieldControlScript2.SetInputs(SpawnManager.GetInstance().SpawnObject(SpawnManager.PoolType.InputUser, _inputUser),
            SpawnManager.GetInstance().SpawnObject(SpawnManager.PoolType.InputUser, _inputUser));
    }
}
