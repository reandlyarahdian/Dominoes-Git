using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SC_GameLogic : MonoBehaviour
{
    private Dictionary<string, GameObject> gameObjects;
    public SC_GlobalEnums.CurTurn curTurn;
    private SC_GlobalEnums.GameMode gameMode;
    private List<int> deck;
    private List<int> deckAI;
    private List<int> deckAI2;
    private List<int> deckAI3;
    public bool placingTile;
    public string tileToPlace;
    private int pickedHandIndex;

    private float timer;
    private float waitTime;

    public Sprite[] tileSprites;


    static SC_GameLogic instance;

    public static SC_GameLogic Instance
    {
        get
        {
            if (instance == null)
                instance = GameObject.Find("SC_GameLogic").GetComponent<SC_GameLogic>();

            return instance;
        }
    }

    private void OnEnable()
    {
        SC_Tile.OnSlotClicked += OnSlotClicked;
    }

    private void OnDisable()
    {
        SC_Tile.OnSlotClicked -= OnSlotClicked;
    }

    // Event for a click on a tile in the hand slots
    private void OnSlotClicked(string _index, int handIndex)
    {
        if(curTurn != SC_GlobalEnums.CurTurn.Yours)
        {
            return;
        }

        _index = _index.Substring(_index.Length - 2);
        if(_index[0] == '_')
          _index = _index.Substring(1);
      
        tileToPlace = _index;
        placingTile = true;
        pickedHandIndex = handIndex;

        gameObjects["SP_Board"].GetComponent<SC_Board>().PlaceTile(tileToPlace);
    }

    void Awake()
    {
        Init();
    }

    private void FixedUpdate()
    {
        SC_GlobalEnums.CurTurn temp = curTurn;
        if (gameMode == SC_GlobalEnums.GameMode.SinglePlayer && (temp != SC_GlobalEnums.CurTurn.Yours))
        {
            if (timer >= waitTime)
            {

                MakeAIMove(temp);
                timer = 0f;
                curTurn = NextTurns(temp);
                if (gameObjects["Txt_Status"] != null)
                    gameObjects["Txt_Status"].GetComponent<Text>().text = curTurn.ToString() + " turn";
            }
            else
            {
                timer += Time.deltaTime;
            }
        }
    }

    private void Init()
    {
        timer = 0f;
        waitTime = 4f;

        gameObjects = new Dictionary<string, GameObject>();
        GameObject[] _objects = GameObject.FindGameObjectsWithTag("GameObject");
        foreach (GameObject g in _objects)
            gameObjects.Add(g.name, g);

        gameObjects["Screen_GameOver"].SetActive(false);

    }

    // Set all data for a new game
    public void InitGame(SC_GlobalEnums.GameMode _gameMode=SC_GlobalEnums.GameMode.SinglePlayer)
    {
        gameMode = _gameMode;
        deck = Enumerable.Range(0, 28).ToList();
        placingTile = false;

        for (int i = 0; i < 7; i++) // Set the hand slots to disabled
        {
            gameObjects["Btn_HandSlot0" + i.ToString()].GetComponent<SC_Tile>().ChangeSlotState(SC_GlobalEnums.SlotState.Empty, null);
        }

        if(gameMode == SC_GlobalEnums.GameMode.SinglePlayer)
        {
            if (UnityEngine.Random.Range(0, 4) == 0)
            {
                curTurn = SC_GlobalEnums.CurTurn.Yours;
                if (gameObjects["Txt_Status"] != null)
                    gameObjects["Txt_Status"].GetComponent<Text>().text = curTurn.ToString() + " turn";
            }
            else if(UnityEngine.Random.Range(0, 4) == 1)
            {
                curTurn = SC_GlobalEnums.CurTurn.Opponent1;
                if (gameObjects["Txt_Status"] != null)
                    gameObjects["Txt_Status"].GetComponent<Text>().text = curTurn.ToString()+ " turn";
            }
            else if (UnityEngine.Random.Range(0, 4) == 2)
            {
                curTurn = SC_GlobalEnums.CurTurn.Opponent2;
                if (gameObjects["Txt_Status"] != null)
                    gameObjects["Txt_Status"].GetComponent<Text>().text = curTurn.ToString() + " turn";
            }
            else if (UnityEngine.Random.Range(0, 4) == 3)
            {
                curTurn = SC_GlobalEnums.CurTurn.Opponent3;
                if (gameObjects["Txt_Status"] != null)
                    gameObjects["Txt_Status"].GetComponent<Text>().text = curTurn.ToString() + " turn";
            }

            for (int i = 0; i < 7; i++)
                gameObjects["Btn_HandSlot0" + i.ToString()].GetComponent<SC_Tile>().ChangeSlotState(SC_GlobalEnums.SlotState.Occupied, tileSprites[GetTileFromDeck()]);

            deckAI = new List<int>();
            for (int i = 0; i < 7; i++)
                deckAI.Add(GetTileFromDeck() + 1);
            deckAI2 = new List<int>();
            for (int i = 0; i < 7; i++)
                deckAI2.Add(GetTileFromDeck() + 1);
            deckAI3 = new List<int>();
            for (int i = 0; i < 7; i++)
                deckAI3.Add(GetTileFromDeck() + 1);
        }
        SC_Board.Instance.ResetBoard();
    }

    // After placing the tile on board, remove it from the hand, also check win condition
    public void RemoveFromHand()
    {
        // Change hand slot state and sprite
        gameObjects["Btn_HandSlot0" + pickedHandIndex.ToString()].GetComponent<SC_Tile>().ChangeSlotState(SC_GlobalEnums.SlotState.Empty, null);
        SC_GlobalEnums.CurTurn A;
        A = NextTurns(curTurn);
        curTurn = A;
        if (gameObjects["Txt_Status"] != null)
            gameObjects["Txt_Status"].GetComponent<Text>().text = curTurn.ToString() + " turn";

        // Check win condition
        if (IsGameOver(SC_GlobalEnums.CurTurn.Yours) == true)
        {
            curTurn = SC_GlobalEnums.CurTurn.GameOver;

            if (gameObjects["Txt_GameOverStatus"] != null)
                gameObjects["Txt_GameOverStatus"].GetComponent<Text>().text = "You won";

            gameObjects["Screen_GameOver"].SetActive(true);

            /* Inform opponent of your won */
            Dictionary<string, object> _toSer = new Dictionary<string, object>();
            _toSer.Add("Action", "GameOver");
            _toSer.Add("Value", "win");
        }
    }

    // Draws a card from the deck
    public void DrawCard()
    {
        gameObjects["SP_Board"].GetComponent<SC_Board>().closePlacingButtons();

        if(deck.Count == 0)
        {
            curTurn = SC_GlobalEnums.CurTurn.GameOver;
            if (gameObjects["Txt_GameOverStatus"] != null)
                gameObjects["Txt_GameOverStatus"].GetComponent<Text>().text = "You lost";
            gameObjects["Screen_GameOver"].SetActive(true);

            /* Inform opponent of your lose */
            Dictionary<string, object> _toSer = new Dictionary<string, object>();
            _toSer.Add("Action", "GameOver");
            _toSer.Add("Value", "lose");

            return;
        }

        if(curTurn != SC_GlobalEnums.CurTurn.Yours)
        {
            return;
        }

        int tileFromDeck = GetTileFromDeck();
        for (int i = 0; i < 7; i++)
        {
            if (gameObjects["Btn_HandSlot0" + i.ToString()].GetComponent<SC_Tile>().state == SC_GlobalEnums.SlotState.Empty)
            {
                gameObjects["Btn_HandSlot0" + i.ToString()].GetComponent<SC_Tile>().ChangeSlotState(SC_GlobalEnums.SlotState.Occupied, tileSprites[tileFromDeck]);
                curTurn = SC_GlobalEnums.CurTurn.Opponent1;
                if (gameObjects["Txt_Status"] != null)
                    gameObjects["Txt_Status"].GetComponent<Text>().text = curTurn.ToString() + " turn";
                break;
            }
        }

        gameObjects["Txt_TilesLeft"].GetComponent<Text>().text = "Tiles Left: " + deck.Count;
    }

    // For single player mode
    private void MakeAIMove(SC_GlobalEnums.CurTurn turn)
    {
        List<int> dect = decks(turn);

        if (gameObjects["SP_Board"] != null)
        {
            int indexPlaced = gameObjects["SP_Board"].GetComponent<SC_Board>().PlaceForAI(dect);
            if (indexPlaced == -1)
            {
                dect.Add(GetTileFromDeck() + 1);
                gameObjects["Txt_TilesLeft"].GetComponent<Text>().text = "Tiles Left: " + deck.Count;
            }
            else
                dect.RemoveAt(indexPlaced);
        }

        if (IsGameOver(turn) == true)
        {
            curTurn = SC_GlobalEnums.CurTurn.GameOver;

            if (gameObjects["Txt_GameOverStatus"] != null)
                gameObjects["Txt_GameOverStatus"].GetComponent<Text>().text = "Opponent won";

            gameObjects["Screen_GameOver"].SetActive(true);
        }
    }

    public void SkipTurn()
    {
        if (curTurn != SC_GlobalEnums.CurTurn.Yours)
            return;
        curTurn = SC_GlobalEnums.CurTurn.Opponent1;
        if (gameObjects["Txt_Status"] != null)
            gameObjects["Txt_Status"].GetComponent<Text>().text = curTurn.ToString() + " turn";
    }

    public void RestartGame()
    {
        gameObjects["Screen_GameOver"].SetActive(false);
        InitGame(gameMode);
    }

    // Draws a random tile from the deck
    public int GetTileFromDeck()
    {
        int deckIndex = UnityEngine.Random.Range(0, deck.Count);
        int tileIndex = deck[deckIndex];
        if (tileIndex < 0)
            IsGameOver(curTurn);
        deck.RemoveAt(deckIndex);
        return tileIndex;
    }

    // Check if one of the players is out of tiles
    private bool IsGameOver(SC_GlobalEnums.CurTurn turns)     
    {
        List<int> dect = decks(turns);

        if(gameMode == SC_GlobalEnums.GameMode.SinglePlayer && deckAI.Count == 0)
        {
            return true;
        }

        for (int i = 0; i < 7; i++)
        {
            if (gameObjects["Btn_HandSlot0" + i.ToString()].GetComponent<SC_Tile>().state != SC_GlobalEnums.SlotState.Empty)
                return false;
        }
        return true;
    }

    public SC_GlobalEnums.CurTurn NextTurns (SC_GlobalEnums.CurTurn turns)
    {
       if(turns == SC_GlobalEnums.CurTurn.Opponent1)
        {
            return SC_GlobalEnums.CurTurn.Opponent2;
        }
        else if(turns == SC_GlobalEnums.CurTurn.Opponent2)
        {
            return SC_GlobalEnums.CurTurn.Opponent3;
        }
        else if(turns == SC_GlobalEnums.CurTurn.Opponent3)
        {
            return SC_GlobalEnums.CurTurn.Yours;
        }
        else if (turns == SC_GlobalEnums.CurTurn.Yours)
        {
            return SC_GlobalEnums.CurTurn.Opponent1;
        }
        else
        {
            return SC_GlobalEnums.CurTurn.GameOver;
        }
    }

    public List<int> decks (SC_GlobalEnums.CurTurn turns)
    {
        List<int> dect = new List<int>();
        switch (turns)
        {
            case SC_GlobalEnums.CurTurn.Opponent1:
                dect = deckAI;
                break;
            case SC_GlobalEnums.CurTurn.Opponent2:
                dect = deckAI2;
                break;
            case SC_GlobalEnums.CurTurn.Opponent3:
                dect = deckAI3;
                break;
        }
        return dect;
    }
}
