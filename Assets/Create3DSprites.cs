using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Create3DSprites : MonoBehaviour
{
    public TakeScreenshot screenshotTaker;
    public MapBuilderController mbCntrler;
    public GameObject ObjectsHolder;
    Vector3 centerpos;
    Camera genCam;
    
    void Start()
    {
        centerpos = ObjectsHolder.transform.position;
        genCam = screenshotTaker.GetComponentInChildren<Camera>();

        StartCoroutine(CreateAllIcons());
    }

    IEnumerator CreateAllIcons(){
        foreach (var item in mbCntrler.spawnableObjects){
            // Check if this item has a screenshot already, probably needs more checks than a simple name.
            screenshotTaker.spritename = item.name;
            var filepath = "./TTData/Data/Assets/files/images/build_icons/";
            var itempath = filepath + "/" + item.name + ".png";
            if(File.Exists(itempath)){
                Debug.Log("found file: " + itempath);
                var imgData = File.ReadAllBytes(itempath);
                Texture2D img = new Texture2D(128,128);
                img.LoadImage(imgData);
                mbCntrler.objectIcons.Add(img);
                continue;
            }

            Debug.Log("no file : " + itempath);

            var itemInstance = Instantiate(item, centerpos, Quaternion.identity);
            // var bc = itemInstance.AddComponent<BoxCollider>();
            Bounds bounds = CalculateLocalBounds(itemInstance);
            // bc.center = bounds.center;
            // bc.size = bounds.size;

            // set the camera half an objects length away from the instantance
            screenshotTaker.transform.position = centerpos - new Vector3 (0,0,bounds.extents.z*2 + bounds.extents.y);

            // align the object to be in front of the camera.
            ObjectsHolder.transform.position = bounds.center;
            itemInstance.transform.parent = ObjectsHolder.transform;
            itemInstance.transform.position = Vector3.zero;
            ObjectsHolder.transform.position = centerpos;
            ObjectsHolder.transform.rotation = Quaternion.Euler(0,225,0);

            screenshotTaker.CreateIcon(filepath);

            ObjectsHolder.transform.rotation = Quaternion.identity;
            Destroy(itemInstance);

            yield return new WaitForEndOfFrame(); // so the engine has time to save the image an such.
            // Debug.Log("Found this many renderers in item: " + itemInstance.name + ", " + c);
        }
        mbCntrler.imagesReady = true;
        yield return null;
    }

    private Bounds CalculateLocalBounds(GameObject o)
    {
        Quaternion currentRotation = o.transform.rotation;
        o.transform.rotation = Quaternion.Euler(0f,0f,0f);

        Bounds bounds = new Bounds(o.transform.position, Vector3.zero);

        foreach(Renderer renderer in o.GetComponentsInChildren<Renderer>()){
            bounds.Encapsulate(renderer.bounds);
        }

        Vector3 localCenter = bounds.center - o.transform.position;
        bounds.center = localCenter;
        // Debug.Log("The local bounds of this model is " + bounds);

        o.transform.rotation = currentRotation;

        return bounds;
    }

}
