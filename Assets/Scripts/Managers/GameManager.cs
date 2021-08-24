using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Globals
{
    #region Constants

    // Field size
    public const int MIN_FIELD_SIZE = 3;
    public const int MAX_FIELD_SIZE = 10;

    #endregion

    #region Tags

    public const string MARK_CELL_TAG = "MarkCell";

    #endregion
}

public class GameManager : MonoBehaviour
{
    #region Variables

    [Header("Interface prefabs")]
    // Main menu interface
    [SerializeField] private GameObject _mainMenuInterface;
    // Game edit interface
    [SerializeField] private GameObject _gameEditInterface;
    // Game edit mark choice
    [SerializeField] private GameObject[] _gameEditInterfaceMarkChoice;
    // Game edit game mode choice
    [SerializeField] private GameObject[] _gameEditInterfaceGameModeChoice;
    // Game edit field size
    [SerializeField] private Slider _gameEditInterfaceFieldSizeSlider;
    [SerializeField] private Text _gameEditInterfaceFieldSizeText;
    // Game edit field win row quant
    [SerializeField] private Slider _gameEditInterfaceWinRowQuantSlider;
    [SerializeField] private Text _gameEditInterfaceWinRowQuantText;
    // Game area interface
    [SerializeField] private GameObject _gameAreaInterface;

    [Header("Field control prefab")]
    // Field control prefab
    [SerializeField] private GameObject _fieldControl;
    // Chosen mark
    private MarkType _chosenMarkForPlayerOne;
    // Game mode (false = human vs computer, true = human vs human)
    private bool _gameMode_HvsC_HvsH;
    // Win row quantity
    private int _winRowQuant;
    // Field size (number of cells _fieldSize x _fieldSize)
    private int _fieldSize;
    // Field control script
    private FieldControlManager _fieldControlScript;

    [Header("Inputs prefabs")]
    // Human input
    [SerializeField] private GameObject _inputUser;
    // AI input
    [SerializeField] private GameObject _inputAI;

    #endregion

    #region Unity

    // Start is called before the first frame update
    void Start()
    {
        // Initialize field only one time, after we can reuse it as any times as we want
        _fieldControl = SpawnManager.GetInstance().SpawnObject(SpawnManager.PoolType.FieldControl, _fieldControl);
        // Getting access to the script
        _fieldControlScript = _fieldControl.GetComponent<FieldControlManager>();
        // First we activate main menu and disable all others
        BackToMainMenu();
        // Spawn demo field
        MainMenuDemoField();
    }

    #endregion

    #region Methods

    #region InterfaceInteraction

    #region MainMenu

    // Gets you back to the main menu
    public void BackToMainMenu()
    {
        // Disable all other interfaces
        _gameEditInterface.SetActive(false);
        _gameAreaInterface.SetActive(false);
        // Set default parameters
        SetPlayerMarkType(0);
        SetGameMode(0);
        SetFieldSize(Globals.MIN_FIELD_SIZE);
        SetWinRowQuant(Globals.MIN_FIELD_SIZE);
        // Activa main menu and demo field
        _mainMenuInterface.SetActive(true);
        // Set input back, disable field and detach input
        _fieldControlScript.SetInputs(SpawnManager.GetInstance().SpawnObject(SpawnManager.PoolType.InputAI, _inputAI),
             SpawnManager.GetInstance().SpawnObject(SpawnManager.PoolType.InputAI, _inputAI));
        MainMenuDemoField();
    }

    // Main menu demo field, where AI will figth with each other 
    // It is made only for showcase, can be called again by pushing title button
    public void MainMenuDemoField()
    {
        // Randomizing the field size
        int fieldSize = UnityEngine.Random.Range(Globals.MIN_FIELD_SIZE + 1, Globals.MAX_FIELD_SIZE);
        _fieldControlScript.CreateField(MarkType.Cross, fieldSize, fieldSize, 5, 7, -1);
    }    

    // Play button method
    public void PlayButton()
    {
        // Deactivate main menu
        _mainMenuInterface.SetActive(false);
        // Disable field
        _fieldControlScript.DisableField();
        // Activate game edit
        _gameEditInterface.SetActive(true);
    }

    // Exit button method
    public void ExitButton()
    {
        // Quit the game
        Application.Quit();
    }

    #endregion

    #region GameMode

    // Sets player mark type
    public void SetPlayerMarkType(int markType)
    {
        for (int i = 0; i < _gameEditInterfaceMarkChoice.Length; i++)
        {
            _gameEditInterfaceMarkChoice[i].SetActive(false);
        }
        _gameEditInterfaceMarkChoice[markType].SetActive(true);
        _chosenMarkForPlayerOne = (MarkType)(markType + 1);
    }

    // Sets player game mode
    public void SetGameMode(int gameMode)
    {
        for (int i = 0; i < _gameEditInterfaceGameModeChoice.Length; i++)
        {
            _gameEditInterfaceGameModeChoice[i].SetActive(false);
        }
        _gameEditInterfaceGameModeChoice[gameMode].SetActive(true);
        _gameMode_HvsC_HvsH = Convert.ToBoolean(gameMode);
    }

    // Sets field size
    public void SetFieldSize(float fieldSize)
    {
        // Set field size 
        _gameEditInterfaceFieldSizeText.text = fieldSize.ToString();
        _gameEditInterfaceFieldSizeSlider.value = fieldSize;
        // Updates win row quant, if it exceeds field size
        if (_gameEditInterfaceWinRowQuantSlider.value > fieldSize)
        {
            SetWinRowQuant(fieldSize);
        }
    }

    // Sets win row quant
    public void SetWinRowQuant(float winRowQuant)
    {
        // Set win row
        _gameEditInterfaceWinRowQuantText.text = winRowQuant.ToString();
        _gameEditInterfaceWinRowQuantSlider.value = winRowQuant;
        // If win row quant doesn't exceed field size, change it
        if (_gameEditInterfaceWinRowQuantSlider.value >= _gameEditInterfaceFieldSizeSlider.value)
        {
            // Update slider and text
            _gameEditInterfaceWinRowQuantSlider.value = _gameEditInterfaceFieldSizeSlider.value;
            _gameEditInterfaceWinRowQuantText.text = _gameEditInterfaceFieldSizeSlider.value.ToString();
        }
    }

    // Start game button
    public void StartGame()
    {
        _fieldSize = (int)_gameEditInterfaceFieldSizeSlider.value;
        _winRowQuant = (int)_gameEditInterfaceWinRowQuantSlider.value;
        // Deactivate edit interface
        _gameEditInterface.SetActive(false);
        // Activate game area
        _gameAreaInterface.SetActive(true);
        // Detach previous inputs
        _fieldControlScript.DisableField(true);
        // Create field
        _fieldControlScript.CreateField(MarkType.Cross, _winRowQuant, _fieldSize);
        // Setup inputs
        if (_gameMode_HvsC_HvsH == false)
        {
            // Human vs computer
            if (_chosenMarkForPlayerOne == MarkType.Cross)
            {
                _fieldControlScript.SetInputs(SpawnManager.GetInstance().SpawnObject(SpawnManager.PoolType.InputUser, _inputUser),
                    SpawnManager.GetInstance().SpawnObject(SpawnManager.PoolType.InputAI, _inputAI));
            }
            else
            {
                _fieldControlScript.SetInputs(SpawnManager.GetInstance().SpawnObject(SpawnManager.PoolType.InputAI, _inputAI),
                    SpawnManager.GetInstance().SpawnObject(SpawnManager.PoolType.InputUser, _inputUser));
            }
        }
        else
        {
            // Human vs human
            _fieldControlScript.SetInputs(SpawnManager.GetInstance().SpawnObject(SpawnManager.PoolType.InputUser, _inputUser),
                  SpawnManager.GetInstance().SpawnObject(SpawnManager.PoolType.InputUser, _inputUser));
        }
    }

    #endregion

    #region GameArea



    #endregion

    #endregion

    #endregion
}
