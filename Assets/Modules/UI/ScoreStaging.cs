using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NaughtyDoggy.UI
{
    public class ScoreStaging : MonoBehaviour
    {
        public TextMeshProUGUI scoreText;
        public float AnimationTime;
        void Start()
        {
            transform.localScale = Vector2.zero;
        }

        public void OpenStage(float score)
        {
            scoreText.text = score.ToString();
            transform.LeanScale(Vector2.one, AnimationTime).setEaseInExpo();
        }

        public void OpenStage(string content)
        {
            scoreText.text = content;
            transform.LeanScale(Vector2.one, AnimationTime).setEaseInExpo();
        }

        public void CloseStage()
        {
            transform.LeanScale(Vector2.zero, AnimationTime).setEaseOutExpo();
        }
    }
        
}

