using Fleck;
using UnityEngine;
using System.Collections.Generic;
using PimDeWitte.UnityMainThreadDispatcher;
using System;
using Brawlley;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using System.Collections;

public class WebSocketServerManager : MonoBehaviour
{
    private WebSocketServer _server;
    private List<IWebSocketConnection> _clients = new List<IWebSocketConnection>();
    public Dictionary<string, PlayerController> connectedPlayers = new Dictionary<string, PlayerController>();

    private Dictionary<string, Vector2> activeInputs = new Dictionary<string, Vector2>();
    
    [Header("Input Settings")]
    public float inputCooldown = 0.02f; 

    private Dictionary<string, Dictionary<byte, float>> lastMoveTimes = new Dictionary<string, Dictionary<byte, float>>();

    
    void Start()
    {
        Debug.Log("Started WebSocket on ws://0.0.0.0:8080");
        _server = new WebSocketServer("ws://0.0.0.0:8080");
        _server.ListenerSocket.NoDelay = true;
        _server.RestartAfterListenError = true;
        _server.Start(socket =>
        {
            socket.OnOpen = () => {
                Debug.Log($"Client connected: {socket.ConnectionInfo.Id}");
                _clients.Add(socket);
                activeInputs[socket.ConnectionInfo.Id.ToString()] = Vector2.zero;
            };

            socket.OnClose = () => {
                Debug.Log($"Client disconnected: {socket.ConnectionInfo.Id}");
                _clients.Remove(socket);
                connectedPlayers.Remove(socket.ConnectionInfo.Id.ToString());
                activeInputs.Remove(socket.ConnectionInfo.Id.ToString());
            };

            socket.OnBinary = message => {
                UnityMainThreadDispatcher.Instance().Enqueue(() => HandleMessage(socket.ConnectionInfo.Id.ToString(), message));
            };
        });
    }

    void HandleMessage(string clientId, byte[] message)
    {
        Debug.Log("Message is: " + message.ToHexString());

        if (message.Length != 9) return;

        byte msgCode = message[0];
        double timestamp = BitConverter.ToDouble(message, 1);

        var client = _clients.Find(c => c.ConnectionInfo.Id.ToString() == clientId);
        if (client != null && client.IsAvailable)
        {
            client.Send(timestamp.ToString());
        }

        
        if (!connectedPlayers.ContainsKey(clientId))
        {
            PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
            if (players.Length > connectedPlayers.Count)
            {
                connectedPlayers[clientId] = players[connectedPlayers.Count];
                Debug.Log($"Assigned {clientId} to {players[connectedPlayers.Count - 1].gameObject.name}");
            }
            else
            {
                Debug.LogWarning("No available player slots for new connection.");
                return;
            }
        }

        PlayerController player = connectedPlayers[clientId];
        Vector2 direction = activeInputs.ContainsKey(clientId) ? activeInputs[clientId] : Vector2.zero;

        if (message.Length == 0)
            return;

        bool isMoveCommand = msgCode >= 0x01 && msgCode <= 0x04;
        bool isStopCommand = msgCode >= 0x05 && msgCode <= 0x08;

        if (isMoveCommand)
        {
            if (!lastMoveTimes.ContainsKey(clientId))
                lastMoveTimes[clientId] = new Dictionary<byte, float>();
            lastMoveTimes[clientId][msgCode] = Time.time;

            StartCoroutine(ResetDirectionAfterCooldown(clientId, msgCode));
        }
        else if (isStopCommand)
        {
            byte correspondingMove = (byte)(msgCode - 4);
            if (lastMoveTimes.ContainsKey(clientId) && lastMoveTimes[clientId].ContainsKey(correspondingMove))
            {
                float timeSinceMove = Time.time - lastMoveTimes[clientId][correspondingMove];
                if (timeSinceMove < inputCooldown)
                {
                    return;
                }
            }
        }

        switch (msgCode)
        {
            case 0x01: // MoveUp
                direction.y = 1;
                break;
            case 0x02: // MoveDown
                direction.y = -1;
                break;
            case 0x03: // MoveLeft
                direction.x = -1;
                break;
            case 0x04: // MoveRight
                direction.x = 1;
                break;
            case 0x05: // StopMoveUp
                if (direction.y == 1) direction.y = 0;
                break;
            case 0x06: // StopMoveDown
                if (direction.y == -1) direction.y = 0;
                break;
            case 0x07: // StopMoveLeft
                if (direction.x == -1) direction.x = 0;
                break;
            case 0x08: // StopMoveRight
                if (direction.x == 1) direction.x = 0;
                break;
            case 0x09: // Jump
                player.GetComponent<PlayerJump>().OnRemoteJump();
                break;
            case 0x0A: // Dash
                player.GetComponent<PlayerDash>().OnRemoteDash();
                break;
            case 0x0B: // Parry
                player.GetComponent<PlayerParry>().OnRemoteParry();
                break;
            case 0x0C: // SpellStart
                player.OnAiming(new InputAction.CallbackContext());
                break;
            case 0x0D: // SpellRelease
                player.OnStopAiming(new InputAction.CallbackContext());
                player.GetComponent<PlayerSpell>().OnRemoteAttack();
                break;
            case 0x0E: // Melee
                player.GetComponent<PlayerMelee>().OnAttack(new InputAction.CallbackContext());
                break;
            case 0x0F: // MoveUpLeft
                direction.x = -1;
                direction.y = 1;
                break;
            case 0x10: // MoveUpRight
                direction.x = 1;
                direction.y = 1;
                break;
            case 0x11: // MoveDownLeft
                direction.x = -1;
                direction.y = -1;
                break;
            case 0x12: // MoveDownRight
                direction.x = 1;
                direction.y = -1;
                break;
            case 0x13: //StopMovement
                direction.x = 0;
                direction.y = 0;
                break;
                
        }


        activeInputs[clientId] = direction;
        player.SetDirection(direction);
    }

    IEnumerator ResetDirectionAfterCooldown(string clientId, byte moveCommand)
    {
        yield return new WaitForSeconds(inputCooldown);

        if (!activeInputs.ContainsKey(clientId)) yield break;

        Vector2 currentDirection = activeInputs[clientId];
        bool shouldReset = false;

        switch (moveCommand)
        {
            case 0x01: // MoveUp
                if (currentDirection.y == 1 && (!lastMoveTimes[clientId].ContainsKey(moveCommand) || (Time.time - lastMoveTimes[clientId][moveCommand] >= inputCooldown)))
                {
                    currentDirection.y = 0;
                    shouldReset = true;
                }
                break;
            case 0x02: // MoveDown
                if (currentDirection.y == -1 && (!lastMoveTimes[clientId].ContainsKey(moveCommand) || (Time.time - lastMoveTimes[clientId][moveCommand] >= inputCooldown)))
                {
                    currentDirection.y = 0;
                    shouldReset = true;
                }
                break;
            case 0x03: // MoveLeft
                if (currentDirection.x == -1 && (!lastMoveTimes[clientId].ContainsKey(moveCommand) || (Time.time - lastMoveTimes[clientId][moveCommand] >= inputCooldown)))
                {
                    currentDirection.x = 0;
                    shouldReset = true;
                }
                break;
            case 0x04: // MoveRight
                if (currentDirection.x == 1 && (!lastMoveTimes[clientId].ContainsKey(moveCommand) || (Time.time - lastMoveTimes[clientId][moveCommand] >= inputCooldown)))
                {
                    currentDirection.x = 0;
                    shouldReset = true;
                }
                break;
        }

        if (shouldReset)
        {
            activeInputs[clientId] = currentDirection;
            if (connectedPlayers.TryGetValue(clientId, out PlayerController player))
            {
                player.SetDirection(currentDirection);
            }
        }
    }

    void OnDestroy()
    {
        _server?.Dispose();
    }
}
