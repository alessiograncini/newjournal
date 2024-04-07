using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject [] Panels;
    
    public void OpenPanel(int panel){
      for (int i = 0; i < Panels.Length; i++)
      {
        Panels[i].SetActive(false);
        Panels[panel].SetActive(true);
      }        
    }
}
