using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TT_UIController : MonoBehaviour
{
    Button dragButton, minimizeButton, closeButton;
    UnityEngine.UIElements.VisualElement content, root, main_actor_menu;
    List<float> foldout_heights;
    List<Foldout> foldouts;
    float mouse_pos_prev_x, mouse_pos_prev_y = 0;
    bool isMinimized, calledLateStart = false;
    int originalHeight = 0;
    int physUpdates = 0;


    void LateStart(){ // Some UIElements get their layout information after the first frame has ocurred (since its updated on draw calls.)
        foldout_heights = new List<float>();
        foldouts = root.Query<Foldout>().ToList();
        foreach (var f in foldouts)
        {
            var newref = f.resolvedStyle.height;
            foldout_heights.Add(newref);
            forceFoldoutContract(f);
            f.value = (f.value = true ? false : true);
        }
    }

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
            main_actor_menu.style.top = main_actor_menu.resolvedStyle.top + (mouse_pos_prev_y - Input.mousePosition.y);
            main_actor_menu.style.left = main_actor_menu.resolvedStyle.left - (mouse_pos_prev_x - Input.mousePosition.x);
        }
        mouse_pos_prev_x = Input.mousePosition.x;
        mouse_pos_prev_y = Input.mousePosition.y;
    }

    private void FixedUpdate() {
        if(!calledLateStart && physUpdates == 1){
            LateStart();
            calledLateStart = true;
        }
        physUpdates ++;
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

    private void forceFoldoutContract(Foldout f){
        f.RegisterValueChangedCallback((e)=>{
            var fd = (e.target as Foldout);
            for (int i = 0; i < foldouts.Count; i++)
            {
                var fld = foldouts[i];
                if(fld.Equals(fd)){
                    Debug.Log("Original height is: " + foldout_heights[i].ToString() + " and current height is: " + fd.resolvedStyle.height.ToString());
                    var clicked_height = int.Parse(fd.resolvedStyle.height.ToString());
                    var original_clicked_height = (int)foldout_heights[i];
                    var flag = clicked_height < original_clicked_height;
                    Debug.Log("size now smaller than size original?: " + flag);

                    if(flag){ // decompressing
                        Debug.Log("Decompressing");
                        fd.style.height = original_clicked_height;
                    }else{ // Compressing
                        Debug.Log("Compressing");
                        fld.style.height = 40;
                    }
                }
            }
        });
    }
}
