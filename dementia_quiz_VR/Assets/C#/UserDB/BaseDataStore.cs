using UnityEngine;

namespace App.BaseSystem.DataStores.ScriptableObjects
{
    /// <summary>
    /// �f�[�^�x�[�X���O������Q�Ƃł���悤�ɂ���
    /// </summary>
    public abstract class BaseDataStore<T, U> : MonoBehaviour where T : BaseDataBase<U> where U : BaseData
    {
        public T DataBase => dataBase;
        [SerializeField]
        protected T dataBase; // �G�f�B�^�[�Ńf�[�^�x�[�X���w��

        /// <summary>
        /// �������p���ăf�[�^�x�[�X���̃f�[�^���擾
        /// </summary>
        public U FindWithName(string name)
        {
            if (string.IsNullOrEmpty(name)) { return default; } // null���

            return dataBase.ItemList.Find(e => e.name == name);
        }

        /// <summary>
        /// id��p���ăf�[�^�x�[�X���̃f�[�^���擾
        /// </summary>
        public U FindWithId(int id)
        {
            return dataBase.ItemList.Find(e => e.Id == id);
        }
    }
}
