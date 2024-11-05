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

        public int ActDiff
        {
            get => actDiff;
            set => actDiff = value;
        }
        [SerializeField]
        private int actDiff;

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
        public string uuid;

        public List<bool> LR
        {
            get => lr;
            set => lr = value;
        }
        [SerializeField]
        private List<bool> lr = new List<bool>();
    }
}