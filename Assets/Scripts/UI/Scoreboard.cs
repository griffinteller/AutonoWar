using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Photon.Pun;
using UnityEngine;

namespace UI
{
    
    public class Scoreboard : MonoBehaviour, IPunObservable
    {
        private readonly struct EntryComparerByActorNumber : IComparer<int>
        {
            private readonly Dictionary<int, ScoreboardEntry> _entriesByActorNumber;
            
            public EntryComparerByActorNumber(Dictionary<int, ScoreboardEntry> entries)
            {
                _entriesByActorNumber = entries;
            }
            
            public int Compare(int xNum, int yNum)
            {
                var x = _entriesByActorNumber[xNum];
                var y = _entriesByActorNumber[yNum];
                if (x == null || y == null)
                {
                    if (x != null)
                        return -1;

                    if (y != null)
                        return 1;

                    return 0;
                }

                var xScore = (int) x.Score;
                var yScore = (int) y.Score;
                
                if (xScore < yScore)
                    return 1;
            
                if (xScore == yScore)
                    return 0;

                return -1;
            }
        }
        
        private readonly Dictionary<int, ScoreboardEntry> _entriesByActorNumber = 
            new Dictionary<int, ScoreboardEntry>();

        private readonly List<int> _actorNumberOrder = new List<int>();
        private bool _changedSinceLastUpdate;

        [SerializeField] private ScoreboardEntry scoreboardEntryPrefab;

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
            foreach (var pair in scoreDict)
                _entriesByActorNumber[pair.Key].Score = pair.Value;
        }

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
        
        
        [SerializeField] private Vector2 offset = new Vector2(0, 0);

        public void OnEnable()
        {
            var t = GetComponent<RectTransform>();
            t.SetParent(GameObject.FindWithTag("Canvas").transform);
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

        public void ChangeScore(int actorNumber, float newScore)
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

        private void RefreshOrder()
        {
            _actorNumberOrder.Sort(new EntryComparerByActorNumber(_entriesByActorNumber));

            for (var i = 0; i < _actorNumberOrder.Count; i++)
            {
                var actorNumber = _actorNumberOrder[i];
                var entry = _entriesByActorNumber[actorNumber];
                
                entry.transform.SetSiblingIndex(i + 1);
                entry.Rank = i + 1;
            }
        }

        public void SetEntryColor(int actorNumber, Color color)
        {
            _entriesByActorNumber[actorNumber].SetColor(color);
        }

        public void ResetEntryColor(int actorNumber)
        {
            _entriesByActorNumber[actorNumber].ResetColor();
        }
    }
}