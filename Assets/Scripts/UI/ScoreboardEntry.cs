﻿using System;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace UI
{
    public class ScoreboardEntry : MonoBehaviour
    {
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

        private float _score;
        private Image[] _backgrounds;

        [SerializeField] private Text nameText;
        [SerializeField] private Text rankText;
        [SerializeField] private Text scoreText;
        
        public Color defaultColor;

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
    }
}