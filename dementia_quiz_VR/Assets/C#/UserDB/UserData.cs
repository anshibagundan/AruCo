using UnityEngine;
using System.Collections.Generic;

namespace App.BaseSystem.DataStores.ScriptableObjects.Status
{
    [CreateAssetMenu(menuName = "ScriptableObject/Data/User")]
    public class UserData : BaseData
    {
        public List<string> UserName
        {
            get => userName;
            set => UserName = value;
        }
        [SerializeField]
        private List<string> userName = new List<string>();
    }
}