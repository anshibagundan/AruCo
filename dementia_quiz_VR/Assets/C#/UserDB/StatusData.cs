using UnityEngine;
using System.Collections.Generic;

namespace App.BaseSystem.DataStores.ScriptableObjects.Status
{
    /// <summary>
    /// ステータスデータオブジェクトのデータ群 (対象: プレイヤー、エネミー、操作可能オブジェクトなど)
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObject/Data/Status")]
    public class StatusData : BaseData
    {
        [SerializeField]
        private List<int> quizDiff = new List<int>();
        public List<int> QuizDiff
        {
            get => quizDiff;
            set => quizDiff = value;
        }
        [SerializeField]
        private int actDiff;
        public int ActDiff
        {
            get => actDiff;
            set => actDiff = value;
        }
        [SerializeField]
        public string uuid;
        public string Uuid
        {
            get => uuid;
            set => uuid = value;
        }
        [SerializeField]
        private List<bool> lr = new List<bool>();
        public List<bool> LR
        {
            get => lr;
            set => lr = value;
        }
        [SerializeField]
        private string x;
        public string X
        {
            get => x;
            set => x = value;
        }
        [SerializeField]
        private string y;
        public string Y
        {
            get => y;
            set => y = value;
        }
    }
}