using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

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
        public Vector3 Position
        {
            get => position;
            set => position = value;
        }
        [SerializeField]
        private Vector3 position;
        public Vector3 PastPosition
        {
            get => pastpos;
            set => pastpos = value;
        }
        [SerializeField]
        private Vector3 pastpos;


        public float rotY
        {
            get => rotationY;
            set => rotationY = value;
        }
        [SerializeField]
        private float rotationY;
        public int Distance
        {
            get => distance;
            set => distance = value;
        }
        [SerializeField]
        private int distance;

    }
}