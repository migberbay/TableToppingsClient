using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TT_UIController : MonoBehaviour
{
    public Button dragButton, minimizeButton, closeButton;
    public UnityEngine.UIElements.VisualElement content, root, main_actor_menu;
    float mouse_delta_x = 0, mouse_delta_y = 0;
    bool isMinimized;

    int originalHeight = 0;

    // Start is called before the first frame update
    void Start(){
        root = GetComponent<UIDocument>().rootVisualElement;
        dragButton  = root.Q<Button>("drag_button");
        minimizeButton = root.Q<Button>("minimize_button");
        closeButton = root.Q<Button>("close_button");
        content = root.Q<VisualElement>("content");
        main_actor_menu = root.Q<VisualElement>("main_actor_menu");

        minimizeButton.clicked += MinimizeMenu;
    }

    // Update is called once per frame
    void Update(){
        if(dragButton.HasMouseCapture()){
            root.style.top = Input.mousePosition.x;
            root.style.left = Input.mousePosition.y;
        }
    }

    private void MinimizeMenu(){
        if(!isMinimized){
            content.visible = false;
            isMinimized = true;

            originalHeight= (int) main_actor_menu.layout.height;

            main_actor_menu.style.height = 40;
        }else{
            content.visible = true;
            isMinimized = false;
            main_actor_menu.style.height = originalHeight;
        }
    }
}
