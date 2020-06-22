using System;
using System.Collections.Generic;
using Microsoft.Win32;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Utility;

namespace UI
{
    public class Scoreboard : MonoBehaviourPunCallbacks, IPunObservable, IPunInstantiateMagicCallback
    {
        [Serializable]
        private class CellChangeDescription
        {
            public object newValue;
        }

        [Serializable]
        private class RowChangeDescription
        {
            public bool? locked;
            public Color? newColor;
        }

        private class RowComparer : Comparer<ScoreboardRow>
        {
            private string _sortingColumnName;
            private int _reverse = 1;

            public RowComparer(string sortingColumnName, bool reverse = false)
            {
                _sortingColumnName = sortingColumnName;
                if (reverse)
                    _reverse = -1;
            }

            public override int Compare(ScoreboardRow x, ScoreboardRow y)
            {
                var cellX = x.GetCell(_sortingColumnName);
                var cellY = y.GetCell(_sortingColumnName);

                if (cellX == null && cellY == null)
                    return 0;

                if (cellX == null)
                    return -1 * _reverse;

                if (cellY == null)
                    return 1 * _reverse;

                if (cellX.IsFloat && cellY.IsFloat)
                    return cellX.FloatValue.CompareTo(cellY.FloatValue);

                if (!cellX.IsFloat && !cellY.IsFloat)
                    return cellX.StringValue.CompareTo(cellY.StringValue);

                throw new ArgumentException("Cells are not of same type");
            }
        }

        public ScoreboardRow rowPrefab;
        public ScoreboardCell cellPrefab;
        public Transform rowParent;
        public ScoreboardTitle title;
        public RectTransform defaultPosition;
        public RectTransform expandedPosition;
        public bool reverseRank;
        public bool built;
        public bool positionLocked;

        private Dictionary<ScoreboardRow, int> _lockedRowsAndIndices = new Dictionary<ScoreboardRow, int>();
        private Dictionary<int, ScoreboardRow> _rowByActorNumber = new Dictionary<int, ScoreboardRow>();

        private Dictionary<CellLocation, CellChangeDescription> _cellChanges =
            new Dictionary<CellLocation, CellChangeDescription>();

        private Dictionary<int, RowChangeDescription> _rowChanges = new Dictionary<int, RowChangeDescription>();
        private List<ScoreboardColumn> _columns = new List<ScoreboardColumn>();
        private List<ScoreboardRow> _rows = new List<ScoreboardRow>();
        private string _sortingColumnName;
        private string _rankColumnName;

        private const KeyCode ExpandKey = KeyCode.Tab;

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

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            // info.photonView.InstantiationData = {columns, sortingColumnName}
            var data = NetworkUtility.Deserialize<object[]>((byte[]) info.photonView.InstantiationData[0]);
            _columns = (List<ScoreboardColumn>) data[0];
            _sortingColumnName = (string) data[1];
            InitPosition();
            BuildFromColumns();
        }

        private void InitPosition()
        {
            var t = GetComponent<RectTransform>();
            t.SetParent(GameObject.FindWithTag("Canvas").transform);
            t.SetSiblingIndex(t.parent.childCount - 2); // put behind windows
            
            MetaUtility.SyncRectTransforms(defaultPosition, t);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                var message = new object[] {_cellChanges, _rowChanges};
                var serializedMessage = NetworkUtility.Serialize(message);
                stream.SendNext(stream);
                _cellChanges = new Dictionary<CellLocation, CellChangeDescription>();
                _rowChanges = new Dictionary<int, RowChangeDescription>();
            }
            else
            {
                var messageEncoded = (byte[]) stream.ReceiveNext();
                var message = NetworkUtility.Deserialize<object[]>(messageEncoded);
                _cellChanges = (Dictionary<CellLocation, CellChangeDescription>) message[0];
                _rowChanges = (Dictionary<int, RowChangeDescription>) message[1];
                ImplementChanges();
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            RemoveRowByActorNumber(otherPlayer.ActorNumber);
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            AddRow(newPlayer.ActorNumber);
        }

        private void SortRows(string sortingColumnName, bool reverse = false)
        {
            _rows.Sort(new RowComparer(sortingColumnName, reverse));

            for (var i = 0; i < _rows.Count; i++) // physically sort them in scene
                _rows[i].transform.SetSiblingIndex(i);

            foreach (var pair in _lockedRowsAndIndices) // put locked rows back
                pair.Key.transform.SetSiblingIndex(pair.Value);

            if (string.IsNullOrEmpty(_rankColumnName))
                return;

            for (var i = 0; i < _rows.Count; i++)
            {
                var cell = _rows[i].GetCell(_rankColumnName);
                if (cell.IsFloat)
                    cell.FloatValue = i + 1;
                else
                    cell.StringValue = (i + 1).ToString();
            }
        }

        private void SortRows()
        {
            SortRows(_sortingColumnName, reverseRank);
        }

        private void SetUpTitle()
        {
            title.BuildFromColumns(_columns);
        }

        public void LockRowAtIndex(int index)
        {
            var row = _rows[index];
            row.locked = true;
            _lockedRowsAndIndices.Add(row, index);
            AddRowChange(row.actorNumber, true);
        }

        public void LockRow(ScoreboardRow row)
        {
            LockRowAtIndex(_rows.IndexOf(row));
        }

        public void LockRowByActorNumber(int actorNumber)
        {
            LockRow(_rowByActorNumber[actorNumber]);
        }

        public void BuildFromColumns()
        {
            SetUpTitle();
            foreach (var pair in PhotonNetwork.CurrentRoom.Players)
            {
                var actorNumber = pair.Key;
                AddRow(actorNumber);
            }

            built = true;
            print("Scoreboard built");
        }

        public void AddRow(int actorNumber)
        {
            var rowObject = Instantiate(rowPrefab.gameObject, rowParent);

            var rowComponent = rowObject.GetComponent<ScoreboardRow>();
            rowComponent.actorNumber = actorNumber;
            rowComponent.BuildFromColumns(_columns);

            _rowByActorNumber.Add(actorNumber, rowComponent);
            _rows.Add(rowComponent);

            SortRows();
        }

        public void RemoveRowByActorNumber(int actorNumber)
        {
            var row = _rowByActorNumber[actorNumber];
            RemoveRow(row);
        }

        public void RemoveRowAtIndex(int index)
        {
            var row = _rows[index];
            RemoveRow(row);
        }

        public void TrySetExpand(bool expand)
        {
            if (positionLocked)
                return;
            
            var rectTransform = GetComponent<RectTransform>();
            MetaUtility.SyncRectTransforms(
                expand ? expandedPosition : defaultPosition, 
                rectTransform);
        }

        public void RemoveRow(ScoreboardRow row)
        {
            Destroy(row.gameObject);
            _rows.Remove(row);
            _rowByActorNumber.Remove(row.actorNumber);
            _lockedRowsAndIndices.Remove(row);
        }

        public void AddToCell(ScoreboardCell cell, float addition)
        {
            if (!cell.IsFloat)
                throw new ArgumentException("Cell is not numeric");

            cell.FloatValue += addition;
            AddCellChange(cell.CellLoc, cell.Value);
        }

        public void AddToCellAtIndex(int index, string columnName, float addition)
        {
            AddToCell(_rows[index].GetCell(columnName), addition);
        }

        public void AddToCellByActorNumber(int actorNumber, string columnName, float addition)
        {
            AddToCell(_rowByActorNumber[actorNumber].GetCell(columnName), addition);
        }

        public void SetCellByActorNumber(int actorNumber, string columnName, object value)
        {
            SetCell(_rowByActorNumber[actorNumber].GetCell(columnName), value);
        }

        public void SetCellAtIndex(int index, string columnName, object value)
        {
            SetCell(_rows[index].GetCell(columnName), value);
        }

        public void SetCell(ScoreboardCell cell, object value)
        {
            switch (value)
            {
                case float floatValue:

                    if (!cell.IsFloat)
                        throw new ArgumentException("Cannot assign float to string cell!");

                    cell.FloatValue = floatValue;
                    break;

                case string stringValue:

                    if (cell.IsFloat)
                        throw new ArgumentException("Cannot assign string to float cell!");

                    cell.StringValue = stringValue;
                    break;

                default:

                    throw new ArgumentException("Value must be either float or string!");
            }

            if (cell.Column.Name.Equals(_sortingColumnName))
                SortRows();

            AddCellChange(cell.CellLoc, value);
        }

        public void SetRowColor(ScoreboardRow row, Color color)
        {
            row.SetAllCellColors(color);
            AddRowChange(row.actorNumber, newColor: color);
        }

        public void SetRowColorAtIndex(int index, Color color)
        {
            SetRowColor(_rows[index], color);
        }

        public void SetRowColorByActorNumber(int actorNumber, Color color)
        {
            SetRowColor(_rowByActorNumber[actorNumber], color);
        }

        public void ResetRowColor(ScoreboardRow row)
        {
            row.ResetAllCellColors();
            AddRowChange(row.actorNumber, null, row.defaultColor);
        }

        public void ResetRowColorByActorNumber(int actorNumber)
        {
            ResetRowColor(_rowByActorNumber[actorNumber]);
        }

        public void ResetRowColorAtIndex(int index)
        {
            ResetRowColor(_rows[index]);
        }

        public void SetNewColumns(List<ScoreboardColumn> columns)
        {
            MetaUtility.DestroyAllChildren(rowParent);

            _columns = columns;
            BuildFromColumns();
        }

        public void AddColumn(ScoreboardColumn column, int index = -1)
        {
            foreach (var row in _rows)
                row.AddCell(column, index);
        }

        private void AddRowChange(int actorNumber, bool? locked = null, Color? newColor = null)
        {
            var description = new RowChangeDescription
            {
                locked = locked,
                newColor = newColor
            };
            
            if (!_rowChanges.ContainsKey(actorNumber))
                _rowChanges.Add(actorNumber, description);
            else
                _rowChanges[actorNumber] = description;
        }

        private void AddCellChange(CellLocation location, object newValue = null)
        {
            var description = new CellChangeDescription
            {
                newValue = newValue
            };

            if (!_cellChanges.ContainsKey(location))
                _cellChanges.Add(location, description);
            else
                _cellChanges[location] = description;
        }

        private void ImplementChanges()
        {
            foreach (var pair in _cellChanges)
            {
                var location = pair.Key;
                var description = pair.Value;
                
                if (description.newValue != null)
                    SetCellByActorNumber(location.ActorNumber, location.ColumnName, description.newValue);
            }
            
            foreach (var pair in _rowChanges)
            {
                var actorNumber = pair.Key;
                var description = pair.Value;
                
                if (description.locked == true)
                    LockRowByActorNumber(actorNumber);
                
                if (description.newColor != null)
                    SetRowColorByActorNumber(actorNumber, (Color) description.newColor);
                
                _cellChanges = new Dictionary<CellLocation, CellChangeDescription>();
                _rowChanges = new Dictionary<int, RowChangeDescription>();
            }
        }

        public void SetRankColumn(string columnName)
        {
            _rankColumnName = columnName;
            SortRows();
        }

        public void NameRows(string columnName)
        {
            foreach (var row in _rows)
            {
                var actorNumber = row.actorNumber;
                var nickName = PhotonNetwork.CurrentRoom.Players[actorNumber].NickName;
                row.GetCell(columnName).StringValue = nickName;
            }
        }

        public void Update()
        {
            KeyCheck();
        }

        private void KeyCheck()
        {
            if (Input.GetKeyDown(ExpandKey))
                TrySetExpand(true);
            else if (Input.GetKeyUp(ExpandKey))
                TrySetExpand(false);
        }
    }
}