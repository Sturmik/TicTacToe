using System;
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
    // There must be two interface elements. First is cross choice, second is circle choice
    [SerializeField] private GameObject[] _gameEditInterfaceMarkChoice;
    // Game edit game mode choice
    // There must be two interface elements. First is human vs computer, second is computer vs computer
    [SerializeField] private GameObject[] _gameEditInterfaceGameModeChoice;
    // Game edit field size
    [SerializeField] private Slider _gameEditInterfaceFieldSizeSlider;
    [SerializeField] private Text _gameEditInterfaceFieldSizeText;
    // Game edit field win row quant
    [SerializeField] private Slider _gameEditInterfaceWinRowQuantSlider;
    [SerializeField] private Text _gameEditInterfaceWinRowQuantText;
    // Game area interface
    [SerializeField] private GameObject _gameAreaInterface;
    // Game are recent turn
    // There must be two interface elements. First is cross turn, second is circle turn
    [SerializeField] private GameObject[] _gameAreaRecentTurn;
    // Game area cross player
    // There must be two interface elements. First is human, second is computer
    [SerializeField] private GameObject[] _gameAreaCrossPlayer;
    [SerializeField] private Text _gameAreaCrossPlayerWinCounterText;
    // Game area circle player
    // There must be two interface elements. First is human, second is computer
    [SerializeField] private GameObject[] _gameAreaCirclePlayer;
    [SerializeField] private Text _gameAreaCirclePlayerWinCounterText;
    // Game are draw counter
    [SerializeField] private Text _gameAreaDrawCounterText;
    // Button for next round start
    [SerializeField] private GameObject _nextRoundButton;

    [Header("Managers")]
    // Spawn manager control
    [SerializeField] private SpawnManager _spawnManagerScript;
    // Field control manager
    [SerializeField] private FieldControlManager _fieldControlScript;

    [Header("Field control prefab")]
    // Chosen mark
    private MarkType _chosenMarkForPlayerOne;
    // Game mode (false = human vs computer, true = human vs human)
    private bool _gameMode_HvsC_HvsH;
    // Win row quantity
    private int _winRowQuant;
    // Field size (number of cells _fieldSize x _fieldSize)
    private int _fieldSize;
    // Win counters
    private int _crossWinCounter;
    private int _circleWinCounter;
    private int _drawCounter;
    // Field mark is changing the right of the first turn to the circle and then to the cross
    private MarkType _turnCycle;

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
        // Hide button
        _nextRoundButton.SetActive(false);
        // Set default parameters
        SetPlayerMarkType(0);
        SetGameMode(0);
        SetFieldSize(Globals.MIN_FIELD_SIZE);
        SetWinRowQuant(Globals.MIN_FIELD_SIZE);
        // Activa main menu and demo field
        _mainMenuInterface.SetActive(true);
        // Set input back, disable field and detach input
        _fieldControlScript.SetInputs(_spawnManagerScript.SpawnObject(SpawnManager.PoolType.InputAI, _inputAI),
             _spawnManagerScript.SpawnObject(SpawnManager.PoolType.InputAI, _inputAI));
        MainMenuDemoField();
    }

    // Main menu demo field, where AI will figth with each other 
    // It is made only for showcase, can be called again by pushing title button
    public void MainMenuDemoField()
    {
        // Randomizing the field size
        int fieldSize = UnityEngine.Random.Range(Globals.MIN_FIELD_SIZE + 1, Globals.MAX_FIELD_SIZE);
        _fieldControlScript.CreateField(_spawnManagerScript, MarkType.Cross, fieldSize, fieldSize, 5, 7, -1);
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
        // Hide button
        _nextRoundButton.SetActive(false);
        // Detach previous inputs
        _fieldControlScript.DisableField(true);
        // Create field
        _fieldControlScript.CreateField(_spawnManagerScript, MarkType.Cross, _winRowQuant, _fieldSize);
        // Getting recent turn
        _turnCycle = _fieldControlScript.TurnState;
        // Subscribe to field updates
        _fieldControlScript.FieldIsMarked -= UpdateGameArea;
        _fieldControlScript.FieldIsMarked += UpdateGameArea;
        // Update counters
        ResetCounters();
        // First, clear previous players sprites
        for (int i = 0; i < _gameAreaCirclePlayer.Length; i++)
        {
            _gameAreaCirclePlayer[i].SetActive(false);
        }
        for (int i = 0; i < _gameAreaCrossPlayer.Length; i++)
        {
            _gameAreaCrossPlayer[i].SetActive(false);
        }
        // Setup inputs
        if (_gameMode_HvsC_HvsH == false)
        {
            // Human vs computer
            if (_chosenMarkForPlayerOne == MarkType.Cross)
            {
                _fieldControlScript.SetInputs(_spawnManagerScript.SpawnObject(SpawnManager.PoolType.InputUser, _inputUser),
                    _spawnManagerScript.SpawnObject(SpawnManager.PoolType.InputAI, _inputAI));
                // Activating according sprites
                _gameAreaCrossPlayer[0].SetActive(true);
                _gameAreaCirclePlayer[1].SetActive(true);
            }
            else
            {
                _fieldControlScript.SetInputs(_spawnManagerScript.SpawnObject(SpawnManager.PoolType.InputAI, _inputAI),
                    _spawnManagerScript.SpawnObject(SpawnManager.PoolType.InputUser, _inputUser));
                // Activating according sprites
                _gameAreaCrossPlayer[1].SetActive(true);
                _gameAreaCirclePlayer[0].SetActive(true);
            }
        }
        else
        {
            // Human vs human
            _fieldControlScript.SetInputs(_spawnManagerScript.SpawnObject(SpawnManager.PoolType.InputUser, _inputUser),
                  _spawnManagerScript.SpawnObject(SpawnManager.PoolType.InputUser, _inputUser));
            // Activating according sprites
            _gameAreaCrossPlayer[0].SetActive(true);
            _gameAreaCirclePlayer[0].SetActive(true);
        }
        UpdateGameArea();
    }

    #endregion

    #region GameArea

    // Resets all counters
    public void ResetCounters()
    {
        _crossWinCounter = _circleWinCounter = _drawCounter = 0;
        _gameAreaCrossPlayerWinCounterText.text
            = _gameAreaCirclePlayerWinCounterText.text
            = _gameAreaDrawCounterText.text = _crossWinCounter.ToString();
    }

    // Updates counters
    public void UpdateCounters()
    {
        _gameAreaCrossPlayerWinCounterText.text = _crossWinCounter.ToString();
        _gameAreaCirclePlayerWinCounterText.text = _circleWinCounter.ToString();
        _gameAreaDrawCounterText.text = _drawCounter.ToString();
    }

    // Updates game area data
    public void UpdateGameArea()
    {
        // If we finished game
        if (_fieldControlScript.IsGameOverConditionReached == true)
        {
            // Check who has won
            switch (_fieldControlScript.TurnState)
            {
                // Cross win
                case MarkType.Cross:
                    _crossWinCounter++;
                    break;
                // Circle win
                case MarkType.Circle:
                    _circleWinCounter++;
                    break;
                // Draw
                case MarkType.Empty:
                    _drawCounter++;
                    break;
            }
            // Turn on button for next round
            _nextRoundButton.SetActive(true);
            // Update score
            UpdateCounters();
        }
        else
        {
            // Update turn state
            switch (_fieldControlScript.TurnState)
            {
                // Cross win
                case MarkType.Cross:
                    _gameAreaRecentTurn[0].SetActive(true);
                    _gameAreaRecentTurn[1].SetActive(false);
                    break;
                // Circle win
                case MarkType.Circle:
                    _gameAreaRecentTurn[0].SetActive(false);
                    _gameAreaRecentTurn[1].SetActive(true);
                    break;
            }
        }
    }
   
    // Starts next round
    public void NextRound()
    {
        // Hide button
        _nextRoundButton.SetActive(false);
        // Change turn cycle
        _turnCycle = _turnCycle == MarkType.Cross ? MarkType.Circle : MarkType.Cross;
        // Create the field
        _fieldControlScript.CreateField(_spawnManagerScript, _turnCycle, _winRowQuant, _fieldSize);
    }

    #endregion

    #endregion

    #endregion
}
