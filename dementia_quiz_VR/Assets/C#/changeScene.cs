using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class changeScene : MonoBehaviour
{
    public void change_button()
    {
        SceneManager.LoadScene("New_WalkScene");
    }

}
