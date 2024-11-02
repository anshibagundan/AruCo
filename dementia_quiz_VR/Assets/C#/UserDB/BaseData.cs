using UnityEngine;

namespace App.BaseSystem.DataStores.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject‚ÅŠÇ—‚³‚ê‚éƒf[ƒ^‚ÌŠî”Õ
    /// </summary>
    public abstract class BaseData : ScriptableObject
    {
        public string Name
        {
            get => name;
            set => name = value;
        }
        [SerializeField]
        private new string name;

        public int Id => id;
        [SerializeField]
        private int id;
    }
}
