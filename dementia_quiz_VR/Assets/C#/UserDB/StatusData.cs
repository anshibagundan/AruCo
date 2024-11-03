using UnityEngine;
using System.Collections.Generic;

namespace App.BaseSystem.DataStores.ScriptableObjects.Status
{
    [CreateAssetMenu(menuName = "ScriptableObject/Data/Status")]
    public class StatusData : BaseData
    {
        public List<int> QuizDiff
        {
            get => quizDiff;
            set => quizDiff = value;
        }
        [SerializeField]
        private List<int> quizDiff = new List<int>();

        public float ActDiff
        {
            get => actDiff;
            set => actDiff = value;
        }
        [SerializeField]
        private float actDiff;

        public int SerialNum
        {
            get => serialNum;
            set => serialNum = value;
        }
        [SerializeField]
        private int serialNum;

        public string CurrentName
        {
            get => currentName;
            set => currentName = value;
        }
        [SerializeField]
        private string currentName;

        public string UUID
        {
            get => uuid;
            set => uuid = value;
        }
        [SerializeField]
        private string uuid;

        public List<float> LR
        {
            get => lr;
            set => lr = value;
        }
        [SerializeField]
        private List<float> lr = new List<float>();
    }
}