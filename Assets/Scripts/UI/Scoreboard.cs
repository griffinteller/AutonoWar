using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Photon.Pun;
using Photon.Realtime;
using Unity.VectorGraphics;
using UnityEngine;
using Utility;

namespace UI
{
    public class Scoreboard : MonoBehaviourPunCallbacks, IPunObservable
    {
        public bool invertRank;

        private List<int> _actorNumberOrder = new List<int>();

        private readonly Dictionary<int, ScoreboardEntry> _entriesByActorNumber =
            new Dictionary<int, ScoreboardEntry>();
        
        private readonly Dictionary<int, int> _lockedActorNumbersAndRanks = new Dictionary<int, int>();

        private bool _changedSinceLastUpdate;
        private bool _expanded;
        [SerializeField] private RectTransform defaultTransform;
        [SerializeField] private RectTransform expandedTransform;


        [SerializeField] private Vector2 offset = new Vector2(0, 0);

        public bool positionLocked;

        [SerializeField] private ScoreboardEntry scoreboardEntryPrefab;

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                if (!_changedSinceLastUpdate)
                    return;

                stream.SendNext(GetScoreDict());
            }
            else
            {
                var scoreDict = (Dictionary<int, int>) stream.ReceiveNext();
                UpdateScores(scoreDict);
                RefreshOrder();
            }
        }

        private void RefreshOrder()
        {
            var resultArray = new int[_entriesByActorNumber.Count];

            foreach (var pair in _lockedActorNumbersAndRanks)
                resultArray[pair.Value - 1] = pair.Key;

            _actorNumberOrder.Sort(new EntryComparerByActorNumber(_entriesByActorNumber, invertRank));
            
            var iResult = 0;
            var iActorNumberOrder = 0;
            while (iResult < resultArray.Length)
            {
                if (resultArray[iResult] != 0) // already in array, thus entry is locked;
                {
                    iResult++;
                    continue;
                }

                if (_lockedActorNumbersAndRanks.ContainsKey(_actorNumberOrder[iActorNumberOrder]))
                {
                    iActorNumberOrder++;
                    continue;
                }

                resultArray[iResult] = _actorNumberOrder[iActorNumberOrder];
                iResult++;
                iActorNumberOrder++;
            }

            _actorNumberOrder = new List<int>(resultArray);
            SyncEntryRanks();
            SyncSiblingIndices();
        }

        private void SyncEntryRanks()
        {
            for (var i = 0; i < _actorNumberOrder.Count; i++)
            {
                var actorNumber = _actorNumberOrder[i];
                _entriesByActorNumber[actorNumber].Rank = i + 1;
            }
        }

        private void SyncSiblingIndices()
        {
            for (var i = 0; i < _actorNumberOrder.Count; i++)
            {
                var actorNumber = _actorNumberOrder[i];
                _entriesByActorNumber[actorNumber].transform.SetSiblingIndex(i + 1); // i=0 is title bar
            }
        }

        public void LockEntry(int actorNumber, Color? newColor = null)
        {
            _lockedActorNumbersAndRanks.Add(actorNumber, _actorNumberOrder.IndexOf(actorNumber) + 1);
            if (newColor != null)
                SetEntryColor(actorNumber, (Color) newColor);
        }

        private Dictionary<int, int> GetScoreDict()
        {
            var result = new Dictionary<int, int>();
            foreach (var pair in _entriesByActorNumber)
            {
                var actorNumber = pair.Key;
                var score = pair.Value.Score;

                result.Add(actorNumber, (int) score);
            }

            return result;
        }

        private byte[] GetScoreDictSerialization()
        {
            var memoryStream = new MemoryStream();
            var binaryFormatter = new BinaryFormatter();

            binaryFormatter.Serialize(memoryStream, GetScoreDict());
            return memoryStream.ToArray();
        }

        public void Update()
        {
            if (!positionLocked)
                SetExpand(Input.GetKey(KeyCode.Tab));
        }

        private Dictionary<int, int> DeserializeScoreDict(byte[] buffer)
        {
            var binaryFormatter = new BinaryFormatter();
            var memoryStream = new MemoryStream();

            memoryStream.Write(buffer, 0, buffer.Length);

            return (Dictionary<int, int>) binaryFormatter.Deserialize(memoryStream);
        }

        public int GetActorNumberOfRank(int rank)
        {
            return _actorNumberOrder[rank - 1];
        }

        public string GetNameOfRank(int rank)
        {
            return PhotonNetwork.CurrentRoom.Players[GetActorNumberOfRank(rank)].NickName;
        }

        private void UpdateScores(Dictionary<int, int> scoreDict)
        {
            var actorNumbersNotInDictArray = new int[scoreDict.Count];
            scoreDict.Keys.CopyTo(actorNumbersNotInDictArray, 0);
            var actorNumbersNotInDict = new HashSet<int>(actorNumbersNotInDictArray);

            foreach (var pair in scoreDict)
            {
                _entriesByActorNumber[pair.Key].Score = pair.Value;
                actorNumbersNotInDict.Remove(pair.Key);
            }

            foreach (var actorNumber in actorNumbersNotInDict)
            {
                RemoveEntry(actorNumber);
            }
        }

        public void OnEnable()
        {
            base.OnEnable();
            PhotonNetwork.AddCallbackTarget(this);
            
            var t = GetComponent<RectTransform>();
            t.SetParent(GameObject.FindWithTag("Canvas").transform);
            t.SetSiblingIndex(t.parent.childCount - 2); // put behind windows
            t.anchoredPosition = offset;
            t.localScale = Vector3.one;

            foreach (var pair in PhotonNetwork.CurrentRoom.Players)
            {
                var actorNumber = pair.Key;

                var entry = Instantiate(scoreboardEntryPrefab, transform).GetComponent<ScoreboardEntry>();
                entry.Name = pair.Value.NickName;
                entry.Score = 0;
                entry.ActorNumber = actorNumber;

                _entriesByActorNumber.Add(actorNumber, entry);
                _actorNumberOrder.Add(actorNumber);
            }

            RefreshOrder();
        }

        public void RemoveEntry(int actorNumber)
        {
            _actorNumberOrder.Remove(actorNumber);

            Destroy(_entriesByActorNumber[actorNumber].gameObject);
            _entriesByActorNumber.Remove(actorNumber);

            RefreshOrder();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            RemoveEntry(otherPlayer.ActorNumber);
        }

        public void SetScore(int actorNumber, float newScore)
        {
            _entriesByActorNumber[actorNumber].Score = newScore;
            _changedSinceLastUpdate = true;
            RefreshOrder();
        }

        public void AddToScore(int actorNumber, float addition)
        {
            _entriesByActorNumber[actorNumber].Score += addition;
            _changedSinceLastUpdate = true;
            RefreshOrder();
        }

        public void SetEntryColor(int actorNumber, Color color)
        {
            _entriesByActorNumber[actorNumber].SetColor(color);
        }

        public void ResetEntryColor(int actorNumber)
        {
            _entriesByActorNumber[actorNumber].ResetColor();
        }

        public void SetExpand(bool expand)
        {
            var rectTransform = GetComponent<RectTransform>();

            if (expand && !_expanded)
            {
                MetaUtility.SyncRectTransforms(expandedTransform, rectTransform);
            }
            else if (!expand && _expanded)
            {
                MetaUtility.SyncRectTransforms(defaultTransform, rectTransform);
                rectTransform.anchoredPosition += offset;
            }

            _expanded = expand;
        }

        private readonly struct EntryComparerByActorNumber : IComparer<int>
        {
            private readonly Dictionary<int, ScoreboardEntry> _entriesByActorNumber;
            private readonly bool reverse;

            public EntryComparerByActorNumber(Dictionary<int, ScoreboardEntry> entries, bool reverse)
            {
                _entriesByActorNumber = entries;
                this.reverse = reverse;
            }

            public int Compare(int xNum, int yNum)
            {
                var x = _entriesByActorNumber[xNum];
                var y = _entriesByActorNumber[yNum];
                var inversion = 1;
                if (reverse)
                    inversion = -1;
                
                
                if (x == null || y == null)
                {
                    if (x != null)
                        return -1 * inversion;

                    if (y != null)
                        return 1 * inversion;

                    return 0;
                }

                var xScore = (int) x.Score;
                var yScore = (int) y.Score;

                if (xScore < yScore)
                    return 1 * inversion;

                if (xScore == yScore)
                    return 0;

                return -1 * inversion;
            }
        }
    }
}