using ExitGames.Client.Photon;
using Networking;
using Photon.Pun;
using Photon.Realtime;

namespace GameDirection
{
    public abstract class DefaultCycleGameDirector : GameDirector, IOnEventCallback
    {
        public GameState gameState;

        protected virtual void PreGameSetup()
        {
            var gameInitializer = gameObject.AddComponent<GameInitializer>();
            gameInitializer.gameDirector = this;
            gameInitializer.enabled = true;
        }

        protected abstract void GameStartSetup();
        protected abstract void GameEndSetup();

        protected virtual void PreGameUpdate()
        {
        }
        protected abstract void InGameUpdate();
        protected abstract void GameEndUpdate();
        
        public void RaiseStartGameEvent()
        {
            RaiseEventDefaultSettings(PhotonEventCode.StartingGame);
            gameState = GameState.WaitingForTransition;
        }

        public void RaiseEndGameEvent()
        {
            RaiseEventDefaultSettings(PhotonEventCode.EndingGame);
            gameState = GameState.WaitingForTransition;
        }

        protected override void OnFullyLoaded()
        {
            gameState = GameState.Initializing;
            PreGameSetup();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            
            PhotonNetwork.AddCallbackTarget(this);
        }

        public override void OnDisable()
        {
            base.OnDisable();
            
            PhotonNetwork.RemoveCallbackTarget(this);
        }
        
        public virtual void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case (byte) PhotonEventCode.StartingGame:

                    gameState = GameState.Started;
                    GameStartSetup();
                    break;

                case (byte) PhotonEventCode.EndingGame:

                    gameState = GameState.Ended;
                    GameEndSetup();
                    break;
            }
        }

        public void Update()
        {
            if (!FullyLoaded)
                return;
            
            switch (gameState)
            {
                case GameState.Initializing:

                    PreGameUpdate();
                    break;

                case GameState.Started:

                    InGameUpdate();
                    break;

                case GameState.Ended:

                    GameEndUpdate();
                    break;
            }
        }
    }
}