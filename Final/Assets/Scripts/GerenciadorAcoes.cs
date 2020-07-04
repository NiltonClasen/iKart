using System.Collections;
using System.Collections.Generic;
using SWNetwork;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GerenciadorAcoes : MonoBehaviour
{
    public int voltas = 3;
    public int contagemRegre = 5;

    public GerenciadorGUI guiManager;

    public enum GameState { waiting, starting, started, finished };
    public GameState State { get => _state; private set => _state = value; }
    private GameState _state;

    int contagem_;
    int voltas_;

    RoomPropertyAgent roomPropertyAgent;
    const string PLAYER_PRESSED_ENTER = "PlayersPressedEnter";
    const string WIINER_ID = "WinnerId";

    private void Start()
    {
        guiManager.SetLapRecord(voltas_, voltas);

        roomPropertyAgent = GetComponent<RoomPropertyAgent>();
    }

    void OnTriggerEnter(Collider other)
    {
        GameObject go = other.gameObject;
        NetworkID networkID = go.GetComponent<NetworkID>();

        if (networkID.IsMine)
        {
            voltas_ = voltas_ + 1;
            guiManager.SetLapRecord(voltas_, voltas);

            if (voltas_ == voltas)
            {
                Debug.Log("Ganhou!!");
                string idGanhador = roomPropertyAgent.GetPropertyWithName(WIINER_ID).GetStringValue();
                Debug.Log("OnTriggerEnter winnerID " + idGanhador);

                if (string.IsNullOrEmpty(idGanhador) || idGanhador.Equals(NetworkClient.Instance.PlayerId))
                {
                    roomPropertyAgent.Modify(WIINER_ID, NetworkClient.Instance.PlayerId);
                    guiManager.SetMainText("Primeiro");
                }
                else
                {
                    guiManager.SetMainText("Segundo");
                }

                State = GameState.finished;
            }
        }
    }

    void Update()
    {
        if (State == GameState.waiting)
        {
            if (Input.GetKeyUp(KeyCode.Return))
            {
                Debug.Log("Começando a corrida...");
                State = GameState.starting;
	            // Modifica a propriedade de inicio de corrida e sincroniza entre os corredores
                int playerPressedEnter = roomPropertyAgent.GetPropertyWithName(PLAYER_PRESSED_ENTER).GetIntValue();
                roomPropertyAgent.Modify(PLAYER_PRESSED_ENTER, playerPressedEnter + 1);
            }
        }
        else if (State == GameState.finished)
        {
            if (Input.GetKeyUp(KeyCode.Return))
            {
                Debug.Log("Saindo...");
                Exit();
            }
        }
    }

    void Exit()
    {
        NetworkClient.Instance.DisconnectFromRoom();

        NetworkClient.Lobby.LeaveRoom((bool ok, SWLobbyError error) =>
        {
            if (!ok)
            {
                Debug.LogError(error);
            }

            Debug.Log("Saiu da sala");
            // Volta para a tela do lobby
            SceneManager.LoadScene(0);
        });
    }

    void Countdown()
    {
        if (State == GameState.starting)
        {
            Debug.Log(contagem_);
            if (contagem_ == 0)
            {
                guiManager.SetMainText("Já!");
                State = GameState.started;
                Debug.Log("Começou");
            }
            else
            {
                guiManager.SetMainText(contagem_.ToString());
                contagem_ = contagem_ - 1;
            }
        }
        else
        {
            guiManager.SetMainText("");
            CancelInvoke("Countdown");
        }
    }


    public void OnSpawnerReady(bool finishedSceneSetup)
    {
        if (!finishedSceneSetup)
        {

            if (NetworkClient.Instance.IsHost)
            {//caso seja o host, spawna o carro no lado esquerdo
                NetworkClient.Instance.LastSpawner.SpawnForPlayer(0, 1);
            }
            else
            {
                NetworkClient.Instance.LastSpawner.SpawnForPlayer(0, 0);
            }
            // O início do jogo está correto e avisa o SceneSpawner
            NetworkClient.Instance.LastSpawner.PlayerFinishedSceneSetup();
        }
    }
    // Evento de enter
    public void OnPlayersPressedEnterValueChanged()
    {
        int playerPressedEnter = roomPropertyAgent.GetPropertyWithName(PLAYER_PRESSED_ENTER).GetIntValue();
        // Verifica se todos os jogadores apertaram enter
        if(playerPressedEnter == 2)
        {
            // começa a contagem
            InvokeRepeating("Countdown", 0.0f, 1.0f);
            contagem_ = contagemRegre;
        }
    }

    public void OnPlayersPressedEnterValueReady()
    {
        int playerPressedEnter = roomPropertyAgent.GetPropertyWithName(PLAYER_PRESSED_ENTER).GetIntValue();
        // Verifica se todos os jogadores apertaram enter
        if (playerPressedEnter == 2)
        {
            // Se todos os jogadores apertaram enter, começa o jogo e vai para a contagem
            State = GameState.started;
            Debug.Log("Todos os jogadores apertaram enter");
        }
    }

    public void OnPlayersPressedEnterValueConflict(SWSyncConflict conflict, SWSyncedProperty property)
    {
        //tratamento para ver se os dois jogadores apertaram enter ao mesmo tempo
        int remotePlayerPressed = (int)conflict.remoteValue;
        // Adiciona 1 ao valor PlayerPressedEnter remoto para resolver o conflito.
        int resolvedPlayerPressed = remotePlayerPressed + 1;

        property.Resolve(resolvedPlayerPressed);
    }
    // Evento de ganhador
    public void OnWinnerIdValueChanged()
    {
        string idGanhador = roomPropertyAgent.GetPropertyWithName(WIINER_ID).GetStringValue();
        Debug.Log("OnWinnerIdValueChanged idGanhador " + idGanhador);
    }

    public void OnWinnerIdValueReady()
    {
        string idGanhador = roomPropertyAgent.GetPropertyWithName(WIINER_ID).GetStringValue();
        Debug.Log("OnWinnerIdValueReady idGanhador " + idGanhador);
    }
}