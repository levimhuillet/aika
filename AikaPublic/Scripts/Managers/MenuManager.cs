using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using AikaGame.Tutorials;
using AikaGame.Aikas;

namespace AikaGame.Managers
{
    public class MenuManager : MonoBehaviour
    {
        public EventSystem eventSystem; // for rendering Canvases
        public Canvas levelMenu; // Canvas containing the In-Level Menu
        public Canvas metaMenu; // Canvas containing the Meta World Menu
        public Canvas paradoxMenu; // Canvas containing options when a paradox occurs
        public Canvas crushedMenu; // Canvas containing options when an Aika is crushed
        public Canvas vanishedMenu;
        public Canvas pausedMenu;

        public static int numMenusOpen;

        private void Awake()
        {
            numMenusOpen = 0;

        }

        private void Start()
        {
            Replicate.Paradox.AddListener(ToggleParadoxMenu);
            Aika.Crushed.AddListener(ToggleCrushedMenu);
            ReplicateKey.Vanish.AddListener(ToggleVanishedMenu);
        }

        private void Update()
        {
            if (Input.GetKeyDown("p"))
            {
                TogglePausedMenu();
            }
        }

        // ---------- Menus ----------

        // Level Menu

        public void ToggleLevelMenu()
        {
            if (levelMenu.gameObject.activeSelf == true)
            {
                CloseLevelMenu();
            }
            else
            {
                if (numMenusOpen == 0)
                {
                    DisplayLevelMenu();
                }
            }
        }

        /*
         * Displays the in-level menu
         */
        void DisplayLevelMenu()
        {
            LevelManager.Pause.Invoke();
            levelMenu.gameObject.SetActive(true);
            eventSystem.gameObject.SetActive(true);
            ++numMenusOpen;
        }

        /*
         * Closes the in-level menu
         */
        void CloseLevelMenu()
        {
            levelMenu.gameObject.SetActive(false);
            eventSystem.gameObject.SetActive(false);
            --numMenusOpen;
            if (numMenusOpen <= 0)
            {
                // pause level
                LevelManager.Unpause.Invoke();
            }
        }

        // Meta Menu

        public void ToggleMetaMenu()
        {
            if (metaMenu.gameObject.activeSelf == true)
            {
                CloseMetaMenu();
            }
            else
            {
                if (numMenusOpen == 0)
                {
                    DisplayMetaMenu();
                }
            }
        }

        /*
         * Displays the menu for the Meta World
         */
        void DisplayMetaMenu()
        {
            LevelManager.Pause.Invoke();
            metaMenu.gameObject.SetActive(true);
            eventSystem.gameObject.SetActive(true);
            ++numMenusOpen;
        }

        /*
         * Closes the menu for the Meta World 
         */
        void CloseMetaMenu()
        {
            metaMenu.gameObject.SetActive(false);
            eventSystem.gameObject.SetActive(false);
            --numMenusOpen;
            if (numMenusOpen <= 0)
            {
                // pause level
                LevelManager.Unpause.Invoke();
            }
        }

        // Paradox Menu

        public void ToggleParadoxMenu()
        {
            if (paradoxMenu.gameObject.activeSelf == true)
            {
                CloseParadoxMenu();
            }
            else
            {
                DisplayParadoxMenu();
            }
        }

        /*
         * Displays the menu when a paradox occurs
         */
        public void DisplayParadoxMenu()
        {
            LevelManager.Pause.Invoke();
            paradoxMenu.gameObject.SetActive(true);
            eventSystem.gameObject.SetActive(true);
            ++numMenusOpen;
        }

        /*
         * Closes the paradox menu
         */
        public void CloseParadoxMenu()
        {
            paradoxMenu.gameObject.SetActive(false);
            eventSystem.gameObject.SetActive(false);
            --numMenusOpen;
            if (numMenusOpen <= 0)
            {
                // pause level
                LevelManager.Unpause.Invoke();
            }
        }

        // Crushed Menu

        public void ToggleCrushedMenu()
        {
            if (crushedMenu.gameObject.activeSelf == true)
            {
                CloseCrushedMenu();
            }
            else
            {
                DisplayCrushedMenu();
            }
        }

        /*
         * Displays the menu when an aika is crushed
         */
        public void DisplayCrushedMenu()
        {
            LevelManager.Pause.Invoke();
            crushedMenu.gameObject.SetActive(true);
            eventSystem.gameObject.SetActive(true);
            ++numMenusOpen;
        }

        /*
         * Closes the crushed menu
         */
        public void CloseCrushedMenu()
        {
            crushedMenu.gameObject.SetActive(false);
            eventSystem.gameObject.SetActive(false);
            --numMenusOpen;
            if (numMenusOpen <= 0)
            {
                // pause level
                LevelManager.Unpause.Invoke();
            }
        }

        // Vanish Menu

        public void ToggleVanishedMenu()
        {
            if (vanishedMenu.gameObject.activeSelf == true)
            {
                CloseVanishedMenu();
            }
            else
            {
                DisplayVanishedMenu();
            }
        }

        /*
         * Displays the menu when an aika is crushed
         */
        public void DisplayVanishedMenu()
        {
            LevelManager.Pause.Invoke();
            vanishedMenu.gameObject.SetActive(true);
            eventSystem.gameObject.SetActive(true);
            ++numMenusOpen;
        }

        /*
         * Closes the crushed menu
         */
        public void CloseVanishedMenu()
        {
            vanishedMenu.gameObject.SetActive(false);
            eventSystem.gameObject.SetActive(false);
            --numMenusOpen;
            if (numMenusOpen <= 0)
            {
                // pause level
                LevelManager.Unpause.Invoke();
            }
        }

        // Pause Menu

        public void TogglePausedMenu()
        {
            if (pausedMenu.gameObject.activeSelf == true)
            {
                ClosePausedMenu();
            }
            else
            {
                DisplayPausedMenu();
            }
        }

        /*
         * Displays the menu when an aika is crushed
         */
        public void DisplayPausedMenu()
        {
            LevelManager.Pause.Invoke();
            pausedMenu.gameObject.SetActive(true);
            eventSystem.gameObject.SetActive(true);
            ++numMenusOpen;
        }

        /*
         * Closes the crushed menu
         */
        public void ClosePausedMenu()
        {
            pausedMenu.gameObject.SetActive(false);
            eventSystem.gameObject.SetActive(false);
            --numMenusOpen;
            if (numMenusOpen <= 0)
            {
                // pause level
                LevelManager.Unpause.Invoke();
            }
        }
    }
}
