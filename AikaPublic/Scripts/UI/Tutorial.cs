using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public List<GameObject> m_Panels; // the first panel of this tutorial dialogue sequence
    int m_PanelIndex;
    bool m_FinishedTutorial;

    // Start is called before the first frame update
    void Start()
    {
        m_PanelIndex = 0;
        m_Panels[m_PanelIndex].SetActive(true);
        m_FinishedTutorial = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!m_FinishedTutorial)
            {
                m_Panels[m_PanelIndex].SetActive(false);

                ++m_PanelIndex;
                //Case when there are still more panels to display
                if (m_PanelIndex < m_Panels.Count)
                {
                    m_Panels[m_PanelIndex].SetActive(true);
                }
                else
                {
                    m_FinishedTutorial = true;
                }
            }
        }
    }
}
