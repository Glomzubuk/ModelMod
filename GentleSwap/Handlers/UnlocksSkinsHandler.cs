using HarmonyLib;
using LLGUI;
using LLHandlers;
using LLScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace GentleSwap {
    public class UnlocksSkinsHandler : MonoBehaviour {
        public static UnlocksSkinsHandler instance;
        public ScreenUnlocksSkins screenUnlocksSkins;
        public LLButton selectedSkinButton;
        public int selectedSkinButtonIndex = -1;
        Vector3 selectedSkinButtonTargetPosition = new Vector3(-0.73f, 0.42f, -5);

        public static UnlocksSkinsHandler Initialize() {
            GameObject gameObject = new GameObject("UnlocksSkinsMenuHandler");
            instance = gameObject.AddComponent<UnlocksSkinsHandler>();
            return instance;
        }

        public void Update() {
            MoveScreenUnlocksSkinsVariantButtons();
            ScreenUnlocksSkinsScrollHandler();
        }


        public void UpdateSkinButtonDelegates() {
            ScreenUnlocksSkins screen = FindObjectOfType<ScreenUnlocksSkins>();
            foreach (var b in screen.btSkins) {
                var index = screen.btSkins.IndexOf(b);

                b.onClick = delegate (int playerNr) {
                    selectedSkinButton = b;
                    selectedSkinButtonIndex = index;
                    DNPFJHMAIBP.GKBNNFEAJGO(Msg.SEL_SKIN, playerNr, index);
                };

                b.onHover = delegate (int playerNr) {
                    if (Controller.all.GetButton(InputAction.MENU_UP) || Controller.all.GetButton(InputAction.MENU_DOWN) || Controller.all.GetStick().y != 0) {
                        selectedSkinButton = b;
                        selectedSkinButtonIndex = index;
                        DNPFJHMAIBP.GKBNNFEAJGO(Msg.SEL_SKIN, playerNr, index);
                    }
                    else DNPFJHMAIBP.GKBNNFEAJGO(Msg.HOVER_SKIN, playerNr, index);
                };
            }
        }

        public void MoveScreenUnlocksSkinsVariantButtons() {
            if (selectedSkinButton != null) {
                ScreenUnlocksSkins screen = FindObjectOfType<ScreenUnlocksSkins>();
                Transform t = selectedSkinButton.gameObject.transform.parent.gameObject.transform;
                if (t.position != selectedSkinButtonTargetPosition) {
                    var bTransform = t.transform;
                    var oldX = bTransform.position.x;
                    var oldY = bTransform.position.y;

                    selectedSkinButton.gameObject.transform.parent.gameObject.transform.position = Vector3.Lerp(selectedSkinButton.gameObject.transform.parent.gameObject.transform.position, selectedSkinButtonTargetPosition, 30f * Time.deltaTime);

                    var xDiff = oldX - bTransform.position.x;
                    var yDiff = oldY - bTransform.position.y;
                    foreach (var button in screen.btSkins) {
                        if (button != selectedSkinButton) {
                            Transform buttonTransform = button.gameObject.transform.parent.gameObject.transform;
                            buttonTransform.position = new Vector3(buttonTransform.position.x - xDiff, buttonTransform.position.y - yDiff, buttonTransform.position.z);
                        }
                    }
                }
                else selectedSkinButton = null;
            }
        }


        public void ScreenUnlocksSkinsScrollHandler() {
            if (selectedSkinButtonIndex != -1) {
                if (screenUnlocksSkins == null) screenUnlocksSkins = FindObjectOfType<ScreenUnlocksSkins>();
                else {
                    if ((Input.mouseScrollDelta.y > 0) && selectedSkinButtonIndex > 0) {
                        selectedSkinButtonIndex--;
                        screenUnlocksSkins.btSkins[selectedSkinButtonIndex].OnHover(0);
                        screenUnlocksSkins.btSkins[selectedSkinButtonIndex].OnClick(0);
                    }

                    if ((Input.mouseScrollDelta.y < 0) && selectedSkinButtonIndex + 1 < screenUnlocksSkins.btSkins.Count) {
                        selectedSkinButtonIndex++;
                        screenUnlocksSkins.btSkins[selectedSkinButtonIndex].OnHover(0);
                        screenUnlocksSkins.btSkins[selectedSkinButtonIndex].OnClick(0);
                    }
                }
            }
        }

        public static class ScreenUnlocksSkins_Patches {
            [HarmonyPatch(typeof(ScreenUnlocksSkins), "InitSkinButtons")]
            [HarmonyPostfix]

            static void InitSkinButtons_Patch(ScreenUnlocksSkins __instance) {
                instance.UpdateSkinButtonDelegates();
                instance.selectedSkinButton = __instance.btFirstButton;
                instance.selectedSkinButtonIndex = 0;
            }

            [HarmonyPatch(typeof(ScreenUnlocksSkins), "OnOpen")]
            [HarmonyPostfix]

            static void OnOpen_Patch() {
                Initialize();
                GentleSwap.controllerSimplifier = ControllerSimplifier.Initialize();
            }

            [HarmonyPatch(typeof(ScreenUnlocksSkins), "OnClose")]
            [HarmonyPostfix]

            static void OnClose_Patch() {
                instance.selectedSkinButtonIndex = -1;
                Destroy(GentleSwap.controllerSimplifier);
                Destroy(instance);
            }
        }

    }

    public class ControllerSimplifier : MonoBehaviour {
        private ScreenUnlocksSkins skinsScreen;
        private ScreenPlayers playersScreen;
        private CharacterModel skinsModel;
        public static ControllerSimplifier instance;
        private bool showGui = false;
        Transform camController;
        Transform fovController;

        public static ControllerSimplifier Initialize() {
            GameObject gameObject = new GameObject("ControllerSimplifier");
            instance = gameObject.AddComponent<ControllerSimplifier>();
            return instance;
        }

        void Awake() {
            GentleSwap.Log.LogDebug("Controller simplifier created");

        }

        void OnDestroy() {
            GentleSwap.Log.LogDebug("Controller simplifier destroyed");
        }

        void Update() {
            if (skinsScreen == null && playersScreen == null) {
                foreach (ScreenBase currentScreen in UIScreen.currentScreens) {
                    if (currentScreen != null) {
                        switch (currentScreen.screenType) {
                            case ScreenType.UNLOCKS_SKINS: skinsScreen = (ScreenUnlocksSkins)currentScreen; break;
                            case ScreenType.PLAYERS: playersScreen = (ScreenPlayers)currentScreen; break;
                        }
                    }
                }
            }

            if ((Controller.all.GetButton(InputAction.GRAB) && Controller.all.GetButton(InputAction.SWING) && Controller.all.GetButtonDown(InputAction.BUNT)) ||
                (Controller.all.GetButton(InputAction.GRAB) && Controller.all.GetButtonDown(InputAction.SWING) && Controller.all.GetButton(InputAction.BUNT)) ||
                (Controller.all.GetButtonDown(InputAction.GRAB) && Controller.all.GetButton(InputAction.SWING) && Controller.all.GetButton(InputAction.BUNT))) {
                showGui = !showGui;
            }
        }

        void FixedUpdate() {
            if (skinsModel != null) {
                if (skinsModel.camController != null) camController = skinsModel.camController.transform;
                if (skinsModel.FOVSlider != null) fovController = skinsModel.FOVSlider.transform;
            }
            else {
                if (skinsScreen != null && skinsScreen.previewModel != null) skinsModel = skinsScreen.previewModel;
                if (playersScreen != null) {
                    if (playersScreen.playerSelections[0] != null && playersScreen.playerSelections[0].characterModel != null) skinsModel = playersScreen.playerSelections[0].characterModel;
                }
            }

            if (camController != null && fovController != null) {
                var grabButton = Controller.all.GetButton(InputAction.GRAB);
                var swingButton = Controller.all.GetButton(InputAction.SWING);
                var buntButton = Controller.all.GetButton(InputAction.BUNT);


                if (grabButton && Input.GetKey(KeyCode.I))
                    camController.localPosition = new Vector3(camController.localPosition.x, camController.localPosition.y, camController.localPosition.z - 0.1f);
                if (grabButton && Input.GetKey(KeyCode.K))
                    camController.localPosition = new Vector3(camController.localPosition.x, camController.localPosition.y, camController.localPosition.z + 0.1f);
                if (grabButton && Input.GetKey(KeyCode.J))
                    camController.localPosition = new Vector3(camController.localPosition.x - 0.1f, camController.localPosition.y, camController.localPosition.z);
                if (grabButton && Input.GetKey(KeyCode.L))
                    camController.localPosition = new Vector3(camController.localPosition.x + 0.1f, camController.localPosition.y, camController.localPosition.z);
                if (grabButton && Input.GetKey(KeyCode.U))
                    camController.localPosition = new Vector3(camController.localPosition.x, camController.localPosition.y - 0.1f, camController.localPosition.z);
                if (grabButton && Input.GetKey(KeyCode.O))
                    camController.localPosition = new Vector3(camController.localPosition.x, camController.localPosition.y + 0.1f, camController.localPosition.z);

                if (swingButton && Input.GetKey(KeyCode.I))
                    camController.eulerAngles = new Vector3(camController.eulerAngles.x - 0.5f, camController.eulerAngles.y, camController.eulerAngles.z);
                if (swingButton && Input.GetKey(KeyCode.K))
                    camController.eulerAngles = new Vector3(camController.eulerAngles.x + 0.5f, camController.eulerAngles.y, camController.eulerAngles.z);
                if (swingButton && Input.GetKey(KeyCode.J))
                    camController.eulerAngles = new Vector3(camController.eulerAngles.x, camController.eulerAngles.y + 0.5f, camController.eulerAngles.z);
                if (swingButton && Input.GetKey(KeyCode.L))
                    camController.eulerAngles = new Vector3(camController.eulerAngles.x, camController.eulerAngles.y - 0.5f, camController.eulerAngles.z);


                if (buntButton && Input.GetKey(KeyCode.J)) {
                    fovController.localPosition = new Vector3(fovController.localPosition.x, fovController.localPosition.y, fovController.localPosition.z - 0.1f);
                    float fovFlipped = fovController.localPosition.z * -1f;
                    skinsModel.poseCam.cam.fieldOfView = fovFlipped * 0.5625f;
                    skinsModel.poseCam.FOV = fovFlipped * 0.5625f;
                }
                if (buntButton && Input.GetKey(KeyCode.L)) {
                    fovController.localPosition = new Vector3(fovController.localPosition.x, fovController.localPosition.y, fovController.localPosition.z + 0.1f);
                    float fovFlipped = fovController.localPosition.z * -1f;
                    skinsModel.poseCam.cam.fieldOfView = fovFlipped * 0.5625f;
                    skinsModel.poseCam.FOV = fovFlipped * 0.5625f;
                }
            }
        }

        void OnGUI() {
            GUILayout.BeginVertical("box");
            GUILayout.Label("Grab+Swing+Bunt to show controller gui");
            if (showGui) {
                if (camController != null) GUILayout.Label("Camera Controller position: " + camController.localPosition.ToString() + " (Grab Button + IJKL)");
                if (camController != null) GUILayout.Label("Camera Controller rotation: " + camController.eulerAngles.ToString() + " (Swing Button + IJKL)");
                if (fovController != null) GUILayout.Label("FOV Controller position: " + fovController.localPosition.ToString() + " (Bunt Button + IJKL)");
                GUILayout.Label("Set these values in your cameraController and fovController");
            }
            GUILayout.EndVertical();

        }
    }
}
