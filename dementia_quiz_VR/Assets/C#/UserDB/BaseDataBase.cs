using System.Collections.Generic;
using UnityEngine;

namespace App.BaseSystem.DataStores.ScriptableObjects
{
    /// <summary>
    /// �f�[�^�x�[�X�̊��
    /// </summary>
    public abstract class BaseDataBase<T> : ScriptableObject where T : BaseData
    {
        public List<T> ItemList => itemList;

        [SerializeField]
        private List<T> itemList = new List<T>(); // T�^�f�[�^�x�[�X
    }
}
