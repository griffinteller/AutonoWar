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
    /*public class Scoreboard : MonoBehaviourPunCallbacks, IPunObservable, IPunInstantiateMagicCallback
    {
        public string RankingKey
        {
            get => _rankingKey;
            set
            {
                _rankingKey = value;
                RefreshOrder(_rankingKey);
            }
        }

        public const string RankKey = "Rank";

        public bool invertRank;
        public bool positionLocked;

        [SerializeField] private RectTransform defaultTransform;
        [SerializeField] private RectTransform expandedTransform;
        [SerializeField] private Vector2 offset = new Vector2(0, 0);
        [SerializeField] private ScoreboardEntry scoreboardEntryPrefab;
        
        private readonly Dictionary<int, ScoreboardEntry> _entriesByActorNumber =
            new Dictionary<int, ScoreboardEntry>();
        private readonly Dictionary<int, int> _lockedActorNumbersAndRanks = new Dictionary<int, int>();

        private List<int> _actorNumberOrder = new List<int>();
        private List<ScoreboardColumn> _columns = new List<ScoreboardColumn>
        {
            new ScoreboardColumn(RankKey),
            new ScoreboardColumn("Name", true),
            new ScoreboardColumn("Score")
        };
        private bool _changedSinceLastUpdate;
        private bool _expanded;
        private string _rankingKey;

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            // info = {columns, rankingKey}
            
            _columns = (List<ScoreboardColumn>) info.photonView.InstantiationData[0];
            _rankingKey = (string) info.photonView.InstantiationData[1];

            Init();
        }

        private Dictionary<string, Dictionary<int, string>> GetSerialization()
        {
            var result = new Dictionary<string, Dictionary<int, string>>();
            foreach (var column in _columns)
                if (column.isSerialized)
                    result.Add(column.name, GetColumnValues(column.name));

            return result;
        }

        private Dictionary<int, string> GetColumnValues(string columnName)
        {
            var result = new Dictionary<int, string>();
            foreach (var entryPair in _entriesByActorNumber)
            {
                var entry = entryPair.Value;
                var actorNumber = entryPair.Key;

                result.Add(actorNumber, entry[columnName]);
            }

            return result;
        }
        
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                if (!_changedSinceLastUpdate)
                    return;

                stream.SendNext(GetSerialization());
            }
            else
            {
                var columnNamesAndValues = (Dictionary<string, Dictionary<int, string>>) stream.ReceiveNext();
                foreach (var pair in columnNamesAndValues)
                    UpdateColumn(pair.Key, pair.Value);
                RefreshOrder(RankingKey);
            }
        }

        private void RefreshOrder(string key)
        {
            var resultArray = new int[_entriesByActorNumber.Count];

            foreach (var pair in _lockedActorNumbersAndRanks)
                resultArray[pair.Value - 1] = pair.Key;

            _actorNumberOrder.Sort(new EntryComparerByActorNumber(_entriesByActorNumber, key, invertRank));
            
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
                _entriesByActorNumber[actorNumber][RankKey] = (i + 1).ToString();
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
            _entriesByActorNumber[actorNumber].locked = true;
            if (newColor != null)
                SetEntryColor(actorNumber, (Color) newColor);
        }

        public void Update()
        {
            if (!positionLocked)
                SetExpand(Input.GetKey(KeyCode.Tab));
        }

        public int GetActorNumberOfRank(int rank)
        {
            return _actorNumberOrder[rank - 1];
        }

        public string GetNameOfRank(int rank)
        {
            return PhotonNetwork.CurrentRoom.Players[GetActorNumberOfRank(rank)].NickName;
        }

        private void UpdateColumn(string columnKey, Dictionary<int, string> newColumnValues)
        {
            var actorNumbersNotInDictArray = new int[newColumnValues.Count];
            newColumnValues.Keys.CopyTo(actorNumbersNotInDictArray, 0);
            var actorNumbersNotInDict = new HashSet<int>(actorNumbersNotInDictArray);

            foreach (var pair in newColumnValues)
            {
                _entriesByActorNumber[pair.Key][columnKey] = pair.Value;
                actorNumbersNotInDict.Remove(pair.Key);
            }

            foreach (var actorNumber in actorNumbersNotInDict)
            {
                RemoveEntry(actorNumber);
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void Init()
        {
            var t = GetComponent<RectTransform>();
            t.SetParent(GameObject.FindWithTag("Canvas").transform);
            t.SetSiblingIndex(t.parent.childCount - 2); // put behind windows
            t.anchoredPosition = offset;
            t.localScale = Vector3.one;

            foreach (var pair in PhotonNetwork.CurrentRoom.Players)
            {
                var actorNumber = pair.Key;

                var entry = Instantiate(scoreboardEntryPrefab, transform).GetComponent<ScoreboardEntry>();
                entry.SetColumns(_columns);
                entry["Name"] = pair.Value.NickName;
                
                _entriesByActorNumber.Add(actorNumber, entry);
                _actorNumberOrder.Add(actorNumber);
            }

            RefreshOrder(RankingKey);
            RefreshColumns();
        }

        private void RefreshColumns()
        {
            
        }

        public void RemoveEntry(int actorNumber)
        {
            _actorNumberOrder.Remove(actorNumber);

            Destroy(_entriesByActorNumber[actorNumber].gameObject);
            _entriesByActorNumber.Remove(actorNumber);

            RefreshOrder(RankingKey);
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

        public void TrySetCell(int actorNumber, string key, string value)
        {
            var entry = _entriesByActorNumber[actorNumber];
            if (entry.locked)
                return;
            
            entry[key] = value;
            _changedSinceLastUpdate = true;
            RefreshOrder(RankingKey);
        }

        public string GetCellValue(int actorNumber, string key)
        {
            return _entriesByActorNumber[actorNumber][key];
        }

        public void TryAddToCell(int actorNumber, string key, int value)
        {
            var originalValue = int.Parse(GetCellValue(actorNumber, key));
            var newValue = originalValue + value;
            TrySetCell(actorNumber, key, newValue.ToString());
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
            private readonly string key;

            public EntryComparerByActorNumber(Dictionary<int, ScoreboardEntry> entries, string key, bool reverse)
            {
                _entriesByActorNumber = entries;
                this.reverse = reverse;
                this.key = key;
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

                var xScore = float.Parse(x[key]);
                var yScore = float.Parse(y[key]);

                if (xScore < yScore)
                    return 1 * inversion;

                if (xScore == yScore)
                    return 0;

                return -1 * inversion;
            }
        }
    }*/
}