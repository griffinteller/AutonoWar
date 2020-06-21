using System;
using System.Collections.Generic;
using Photon.Pun;
using UI;
using UnityEngine;
using Utility;

namespace GameDirection
{
    public abstract class DefaultGameDirectorScoreboard : DefaultCycleGameDirector
    {
        public Scoreboard scoreboardPrefab;
        
        protected Scoreboard Scoreboard;

        protected abstract List<ScoreboardColumn> DefaultScoreboardColumns { get; }
        protected abstract string DefaultSortingColumnName { get; }

        public virtual void Start()
        {
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.InstantiateSceneObject(
                    scoreboardPrefab.name, Vector3.zero, Quaternion.identity,
                    0,
                    new object[] {NetworkUtility.Serialize(
                        new object[] {DefaultScoreboardColumns, DefaultSortingColumnName})});
        }

        protected override void PreGameSetup()
        {
            base.PreGameSetup();
            CheckScoreBoardIsInstantiated();
        }

        protected abstract void SetupScoreboard();

        private bool CheckScoreBoardIsInstantiated()
        {
            if (Scoreboard)
                return true;

            Scoreboard = GameObject.FindWithTag("Scoreboard").GetComponent<Scoreboard>();

            if (Scoreboard && Scoreboard.built)
            {
                print("Setting up scoreboard");
                SetupScoreboard();
            }

            return Scoreboard;
        }
    }
}