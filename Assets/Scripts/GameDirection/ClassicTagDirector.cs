using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Main;
using Networking;
using Photon.Pun;
using Photon.Realtime;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace GameDirection
{
    public class ClassicTagDirector : GameDirector, IOnEventCallback
    {
        public override GameModeEnum GameMode => GameModeEnum.ClassicTag;

        public int currentItActorNumber = -1;
        public int lastItActorNumber = -1;

        public Color itTint = Color.red;
        public Color scoreboardEntryItColor;

        public Vector3 scoreboardOffset = new Vector3(0, 0, 0);

        public float tagCooldownTime = 10;

        public Scoreboard scoreboardPrefab;

        private float _lastTagTime;
        private PlayerConnection _playerConnection;

        private float _timeIntroStarted;
        private Text _messageText;

        private float _timeGameStarted;

        private float _gameLength;

        private UiClock _clock;

        private const float WaitingTime = 10f;
        
        private const float DistanceFromItPointHalvingDistance = 30f;
        
        // per second
        private const float MinNotItPoints = 0f;
        private const float MaxNotItPoints = 10f;
        private const float ItPointDetraction = 5f;

        private const float TagPoints = 300f;

        private Scoreboard _scoreboard;

        public override void OnEnable()
        {
            base.OnEnable();

            _lastTagTime = -tagCooldownTime;
            _playerConnection = GameObject.FindGameObjectWithTag("ConnectionObject")
                .GetComponent<PlayerConnection>();
            _timeIntroStarted = Time.time;
            _messageText = GameObject.FindWithTag("MessageText").GetComponent<Text>();
            _gameLength = (int) PhotonNetwork.CurrentRoom.CustomProperties["gameLength"];

            _clock = GameObject.FindWithTag("Clock").GetComponent<UiClock>();
            _clock.Seconds = _gameLength * 60;

            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.InstantiateSceneObject(
                    scoreboardPrefab.name, scoreboardOffset, Quaternion.identity);
            
            PhotonNetwork.AddCallbackTarget(this);
        }

        public override void OnDisable()
        {
            base.OnDisable();

            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {

                case (byte) PhotonEventCode.NewIt:
                    
                    SetNewIt(((int[]) photonEvent.CustomData)[0]);
                    if (PhotonNetwork.IsMasterClient)
                        _scoreboard.AddToScore(lastItActorNumber, TagPoints);
                    break;
                
                case (byte) PhotonEventCode.StartingGame:

                    gameState = GameState.Initializing;
                    GameStartSetup();
                    break;
                
                case (byte) PhotonEventCode.EndingGame:

                    gameState = GameState.Ended;
                    EndGameSetup();
                    break;

            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            _scoreboard.RemoveEntry(otherPlayer.ActorNumber);

            if (otherPlayer.ActorNumber == currentItActorNumber && PhotonNetwork.IsMasterClient)
                OnItLeftRoom();
        }

        private void OnItLeftRoom()
        {
            var actorNumbers = PhotonNetwork.CurrentRoom.Players.Keys;
            var i = (int) (Random.value * (actorNumbers.Count - 1));
            
            var j = 0;
            var newItActorNumber = 0;
            foreach (var key in actorNumbers)
            {
                if (j == i)
                    newItActorNumber = key;

                j++;
            }
            
            TryRaiseNewItEvent(newItActorNumber, true);
        }

        private void GameStartSetup()
        {
            SetRobotsKinematic(false);
            SetInitialIt();
            gameState = GameState.Started;
            _timeGameStarted = Time.time;
            _messageText.text = "";
            _clock.StartClock();
            RobotMain.OnTriggerEnterCallbacks += OnTriggerEnterTagCallback;
        }

        private void TryRaiseNewItEvent(int actorNumber, bool ignoreCooldown = false)
        {
            if ((Time.time - _lastTagTime < tagCooldownTime && !ignoreCooldown) || !FullyLoaded)
                return;

            _lastTagTime = Time.time;
            print("Raising It Event!");
            RaiseEventDefaultSettings(PhotonEventCode.NewIt, new [] {actorNumber});
        }

        protected override void OnFullyLoaded()
        {
            gameState = GameState.Initializing;
            SetRobotsKinematic(true);
        }

        private void SetInitialIt()
        {
            print("setting initial it");
            
            currentItActorNumber = PhotonNetwork.MasterClient.ActorNumber;
            lastItActorNumber = currentItActorNumber;

            CombineTintWithRobot(currentItActorNumber, itTint);
            
            try
            {
                _scoreboard = GameObject.FindWithTag("Scoreboard").GetComponent<Scoreboard>();
            }
            catch (NullReferenceException)
            {
                return; // scoreboard has not been instantiated yet
            }
            
            _scoreboard.SetEntryColor(currentItActorNumber, scoreboardEntryItColor);
        }

        public void Update()
        {
            switch (gameState)
            {
                case GameState.Initializing:
                    
                    PreGameUpdate();
                    break;
                
                case GameState.Started:
                    
                    InGameUpdate();
                    break;
                
                case GameState.Ended:

                    EndGameUpdate();
                    break;
            }
        }

        private void EndGameSetup()
        {
            SetRobotsKinematic(true);
            //var winningPlayerName = _scoreboard.GetNameOfRank(1);
            _scoreboard.SetExpand(true);
            _scoreboard.positionLocked = true;
            GameObject.FindWithTag("Hud").SetActive(false);
        }

        private void EndGameUpdate()
        {
        }

        private void InGameUpdate()
        {
            var timeRemaining = _clock.Seconds;
            var gameOver = timeRemaining < 0;

            if (gameOver)
            {
                _clock.Stop();
            }
            
            if (!PhotonNetwork.IsMasterClient)
                return;
            
            if (gameOver)
            {
                _clock.Stop();
                RaiseEndGameEvent();
                return;
            }

            if (!_scoreboard)
            {
                try
                {
                    _scoreboard = GameObject.FindWithTag("Scoreboard").GetComponent<Scoreboard>();
                }
                catch (NullReferenceException)
                {
                    return; // scoreboard has not been instantiated yet
                }
            }

            ScorePlayers();
        }

        private void ScorePlayers()
        {
            var itLocation = _playerConnection.robots[currentItActorNumber].transform.position;
            
            foreach (var pair in _playerConnection.robots)
            {
                var actorNumber = pair.Key;
                var robot = pair.Value;

                if (actorNumber == currentItActorNumber)
                {
                    _scoreboard.AddToScore(currentItActorNumber, -ItPointDetraction * Time.deltaTime);
                }
                else
                {
                    var distanceToIt = (robot.transform.position - itLocation).magnitude;
                    var pointsPerSecond = NotItPointsPerSecond(distanceToIt);
                    _scoreboard.AddToScore(actorNumber, pointsPerSecond * Time.deltaTime);
                }

            }
        }

        private static float NotItPointsPerSecond(float distanceToIt)
        {
            return (MaxNotItPoints - MinNotItPoints)
                   * Mathf.Pow(2, -(distanceToIt / DistanceFromItPointHalvingDistance))
                   + MinNotItPoints;
        }

        private void PreGameUpdate()
        {
            var starting = Time.time - _timeIntroStarted < WaitingTime;

            if (starting)
            {
                _messageText.text = "Starting in: " + ((int) (WaitingTime - (Time.time - _timeIntroStarted)) + 1) + "...";
                return;
            }

            if (!PhotonNetwork.IsMasterClient)
                return;
            
            RaiseStartGameEvent();
        }

        private void SetRobotsKinematic(bool isKinematic)
        {
            foreach (var rigidbody in _playerConnection.robotRigidbodies.Values)
                rigidbody.isKinematic = isKinematic;
        }

        private void SetNewIt(int actorNumber)
        {
            _lastTagTime = Time.time;
            
            lastItActorNumber = currentItActorNumber;
            currentItActorNumber = actorNumber;

            CombineTintWithRobot(currentItActorNumber, itTint);
            _scoreboard.SetEntryColor(currentItActorNumber, scoreboardEntryItColor);

            try
            {
                CombineTintWithRobot(lastItActorNumber, itTint, true); // reset to old color
                _scoreboard.ResetEntryColor(lastItActorNumber);
            }
            catch (KeyNotFoundException)
            {
            }
        }

        private void CombineTintWithRobot(int actorNumber, Color tint, bool undo = false)
        {
            var robot = _playerConnection.robots[actorNumber];

            robot.GetComponent<RobotMain>().CombineTint(tint, undo);
        }

        private static void OnTriggerEnterTagCallback(Collider other, RobotMain robotMain)
        {
            var collisionRoot = other.transform.root.gameObject;
            
            if (!collisionRoot.CompareTag("Robot"))
                return;
            
            var tagDirector = GameObject.FindWithTag("GameDirector").GetComponent<ClassicTagDirector>();
            if (robotMain.robotNetworkBridge.actorNumber == tagDirector.currentItActorNumber)
                    tagDirector.TryRaiseNewItEvent(collisionRoot.GetComponent<RobotNetworkBridge>().actorNumber);
        }
    }
}