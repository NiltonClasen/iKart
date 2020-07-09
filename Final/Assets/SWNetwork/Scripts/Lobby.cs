using UnityEngine;
using SWNetwork;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Lobby : MonoBehaviour
{
    //Botão para registrar e sync com o SocketWeaver
    public Button registrar;

    public Button jogar;

    public InputField nome;

    void Start()
    {
        //Evento da sala pronta
        NetworkClient.Lobby.OnRoomReadyEvent += SalaPronta;

        //Conectou ao lobby
        NetworkClient.Lobby.OnLobbyConnectedEvent += LobbyConectado;

        //Seta o botão de registrar como ativo
        registrar.gameObject.SetActive(true);
        jogar.gameObject.SetActive(false);
    }

    void onDestroy()
    {
        NetworkClient.Lobby.OnRoomReadyEvent -= SalaPronta;
        NetworkClient.Lobby.OnLobbyConnectedEvent -= LobbyConectado;
    }

    void SalaPronta(SWRoomReadyEventData eventData)
    {
        ConectarSala();
    }

    void LobbyConectado()
    {
        RegistrarJogador();
    }

    public void Register()
    {
        string NomeJogador = nome.text;

        if (NomeJogador != null && NomeJogador.Length > 0)
        {
            //Realiza o checkin do jogador
            NetworkClient.Instance.CheckIn(NomeJogador, (bool ok, string erro) =>
            {
                if (!ok)
                    Debug.LogError("check-in com sucesso: " + erro);
            });
        }
        else
        {
            //Gera um nome aleatorio pelo SocketWeaver
            NetworkClient.Instance.CheckIn((bool ok, string error) =>
            {
                if (!ok)
                    Debug.LogError("Deu erro no check-in: " + error);
            });
        }
    }

    //Clicou no botão Jogar
    public void Jogar()
    {
        //Coloca os jogadores dentro da sala através de um Handle
        NetworkClient.Lobby.JoinOrCreateRoom(true, 2, 60, EntraCriaSala);
    }

    void RegistrarJogador()
    {
        NetworkClient.Lobby.Register((successful, reply, error) =>
        {
            if (successful)
            {
                Debug.Log("Registrado " + reply);

                if (reply.started)
                {
                    //O jogador já está na sala e pode iniciar a sala
                    ConectarSala();
                }
                else
                {
                    //Habilita o botao jogar
                    jogar.gameObject.SetActive(true);
                    registrar.gameObject.SetActive(false);
                }
            }
        });
    }

    void EntraCriaSala(bool successful, SWJoinRoomReply reply, SWLobbyError error)
    {
        if (successful)
        {
            //Pode iniciar a sala
            if (reply.started)
            {
                ConectarSala();
            }
            else if (NetworkClient.Lobby.IsOwner)
            {
                StartRoom(); //Jogador pode Entrar a sala no socketWeaver
            }
        }
    }

    void StartRoom()
    {
        NetworkClient.Lobby.StartRoom((okay, error) =>
        {
            if (okay)
            {
                //SocketWeaver interpreta e permite a criacao 
                Debug.Log("Deu boa");
            }
            else
            {
                Debug.Log("Deu erro para entrar na sala " + error);
            }
        });
    }

    void ConectarSala()
    {
        NetworkClient.Instance.ConnectToRoom(HandleConectarSala);
    }

    void HandleConectarSala(bool connected)
    {
        if (connected)
        {
            //Conseguiu entrar na sala e irá carregar a parte do jogo
            SceneManager.LoadScene(1);
        }
        else
        {
            Debug.Log("Deu erro ao tentar entrar no jogo");
        }
    }
}