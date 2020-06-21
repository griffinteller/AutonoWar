using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace UI
{
    /*[RequireComponent(typeof(HorizontalLayoutGroup))]
    public class ScoreboardEntry : MonoBehaviour
    {
        
        public Color defaultColor;
        public int actorNumber;
        public GameObject cellPrefab;
        public bool locked;

        private Dictionary<string, Text> _textFromColumnName = new Dictionary<string, Text>();
        private List<ScoreboardColumn> _columns = new List<ScoreboardColumn>();

        public void SetColor(Color color)
        {
            var images = MetaUtility.GetComponentsInProperChildren<Image>(gameObject);
            foreach (var image in images)
                image.color = color;
        }

        public void SetColumns(List<ScoreboardColumn> columns)
        {
            MetaUtility.DestroyAllChildren(gameObject);
            _textFromColumnName = new Dictionary<string, Text>();
            _columns = new List<ScoreboardColumn>();

            foreach (var column in columns)
                AddColumn(column);
        }

        public void AddColumn(ScoreboardColumn column, int index = -1)
        {
            if (index == -1)
                index = _columns.Count;
            
            var cell = Instantiate(cellPrefab, transform);
            cell.transform.SetSiblingIndex(index);
            
            var cellText = cell.GetComponentInChildren<Text>();
            var cellImage = cell.GetComponentInChildren<Image>();
            var cellLayoutElement = cell.GetComponent<LayoutElement>();

            cell.name = column.name;
            cellText.text = column.name;
            cellImage.color = defaultColor;
            cellLayoutElement.minWidth = column.defaultWidth;
            if (column.expand)
                cellLayoutElement.preferredWidth = 9999;
            
            _textFromColumnName.Add(column.name, cellText);
            _columns.Insert(index, column);
        }
        
        public void ResetColor()
        {
            SetColor(defaultColor);
        }

        public string this[string cellName]
        {
            get => _textFromColumnName[cellName].text;
            set => _textFromColumnName[cellName].text = value;
        }

        /*private Image[] _backgrounds;

        private float _score;

        public Color defaultColor;

        [SerializeField] private Text nameText;
        [SerializeField] private Text rankText;
        [SerializeField] private Text scoreText;
        public int ActorNumber { get; set; }

        public string Name
        {
            set => nameText.text = value;
        }

        public float Score
        {
            get => _score;
            set
            {
                _score = value;
                scoreText.text = ((int) value).ToString();
            }
        }

        public int Rank
        {
            set => rankText.text = value.ToString();
        }

        public void OnEnable()
        {
            _backgrounds = MetaUtility.GetComponentsInProperChildren<Image>(gameObject);
            SetColor(defaultColor);
        }

        public void SetColor(Color color)
        {
            foreach (var image in _backgrounds)
                image.color = color;
        }

        public void ResetColor()
        {
            SetColor(defaultColor);
        }

        public void SetColumns(List<ScoreboardColumn> columns)
        {
            foreach (Transform child in transform)
            {
                var childLayoutElement = child.GetComponent<LayoutElement>();
                childLayoutElement.minWidth = columns
            }
        }
    }*/
}