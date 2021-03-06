using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using NaughtyDoggy.Helper;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

namespace NaughtyDoggy.UI
{
    public class GameManager : SingletonHelper<GameManager>
    {
        private float _playerTotalScore;

        public TextMeshProUGUI _totalScoreText;
        public float ScoreShowTime;
        public float StagingInterval;

        public ScoreStaging _scoreStage;
        private Queue<float> _scoreStagingQueue;
        private bool _stagingCR_running;
        private bool _gameRunning;
        private bool _countDownEnd;

        public bool CountDownEnd 
        { 
            set => _countDownEnd = value;
            get => _countDownEnd;
        }

        // Start is called before the first frame update
        void Start()
        {
            _stagingCR_running = false;
            _gameRunning = true;
            _countDownEnd = false;
            
            _scoreStagingQueue = new Queue<float>();
            UpdateScore(0.0f);
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(0);
        }

        public void EndGame()
        {
            Application.Quit();
        }

        void UpdateScore(float scoreToAdd)
        {
            _playerTotalScore += scoreToAdd;
            _totalScoreText.text = "Score:" + _playerTotalScore;
        }

        public void TriggerScoreStaging(float score)
        {
            if (_stagingCR_running)
            {
                _scoreStagingQueue.Enqueue(score);
            }
            else
            {
                _stagingCR_running = true;
                _scoreStagingQueue.Enqueue(score);
                StartCoroutine("AnimateScoreStaging");
            }
        }

        IEnumerator AnimateScoreStaging()
        {
            while (_scoreStagingQueue.Any())
            {
                _scoreStage.OpenStage(_scoreStagingQueue.Peek());
                UpdateScore(_scoreStagingQueue.Dequeue());
                yield return new WaitForSeconds(ScoreShowTime);
                _scoreStage.CloseStage();
                yield return new WaitForSeconds(StagingInterval);
            }
            _stagingCR_running = false;
        }

        public void EndTitleDisplay()
        {
            _totalScoreText.text = "Time Up!";
        }
    }

}

