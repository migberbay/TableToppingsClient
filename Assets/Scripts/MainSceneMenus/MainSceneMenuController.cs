using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;

public class MainSceneMenuController : MonoBehaviour
{
    public GameObject movableMenuPrefab;
    public Transform diceThrower;

    public GameObject[] dices;
    public Dictionary<string, int> diceEquivalence = new Dictionary<string, int>(){
        {"d6", 0}
    };

    public PlaySceneChatController chatController;

    public string currentDiceFormula;

    public bool diceRolling = false;

    public void Start(){
        currentDiceFormula = "1d6";
    }

    public void SpawnNewMenu(){
        var menuInstance = Instantiate(movableMenuPrefab);
        var uiDoc = menuInstance.GetComponent<UIDocument>();
        var root = uiDoc.rootVisualElement;
        var all_text_fields = root.Query<TextField>();
        
        all_text_fields.ForEach( f => {
            foreach(var i in f.Children()){
                i.style.paddingBottom = 0;
                i.style.paddingLeft = 0;
                i.style.paddingRight = 0;
                i.style.paddingTop = 0;
            }
        });
    }

    # region diceRolling
    async public void ThrowDice(){
        if(diceRolling){
            return;
        }
        diceRolling = true;

        var qtity = 0;
        int[] numDicesToThrow = new int[dices.Length];
        try{
            var aux = parseDiceFormula();
            numDicesToThrow = aux.Item1;
            qtity = aux.Item2;
        }catch (Exception e){
            Debug.LogError("there was an error when parsing your formula.");
            Debug.LogException(e);
            return;
        }

        // Create list of tasks
        var tasks = new List<Task>();

        for (int i = 0; i < dices.Length; i++)
        {  
            await Task.Delay(200);
            for (int j = 0; j < numDicesToThrow[i]; j++)
            {
                //add as many dices as you want
                tasks.Add(ThrowDie(i));
                await Task.Delay(200);
            }
        }

        await Task.WhenAll(tasks);
        List<int> results = new List<int>();
        int dicetotal = 0;

        foreach (var t in tasks){
            var result = ((Task<int>)t).Result;
            dicetotal += result;
        }

        Debug.Log("Dice throw result from formula [" + currentDiceFormula + "] was " + dicetotal.ToString() + "from dice + " + qtity.ToString()); 
        chatController.PostDiceRoll(dicetotal, currentDiceFormula);
        diceRolling = false;
    }

    private (int[], int) parseDiceFormula(){
        // var diceparts = currentDiceFormula.Split(new char[]{'+' , '-'});
        var diceparts = Regex.Split(currentDiceFormula, @"(?<=[+-])");

        int[] diceAmmounts = new int[dices.Length];
        int numvalues = 0;

        foreach(var i in diceparts){
            if(i.Contains("d")){ // if its a dice
                Debug.Log("Dice part found in formula = " + i);
                var ammount_and_type = i.Trim().Split('d');
                var ammount = int.Parse(ammount_and_type[0]);
                var dice = "d" + ammount_and_type [1];
                diceAmmounts[diceEquivalence[dice]] = ammount;
            }else{
                Debug.Log("Numeric part found in formula = "+i);
                numvalues += int.Parse(i.Trim());
            }
        }
        
        return (diceAmmounts, numvalues);
    }

    async private Task<int> ThrowDie(int diceIndex){
        var diceInstance = Instantiate(dices[diceIndex], diceThrower.position, Quaternion.Euler(new Vector3(UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(0f, 360f), UnityEngine.Random.Range(0f, 360f))));
        var rb = diceInstance.GetComponent<Rigidbody>();

        rb.AddForce(new Vector3(0f,0.5f,0.5f) * 25 , ForceMode.Impulse);

        await Task.Delay(1000); // we give it some time before we atart checking rotations

        Vector3 prev_rot_values = diceInstance.transform.rotation.eulerAngles;
        Vector3 new_rot_values = new Vector3();

        while (prev_rot_values != new_rot_values){
            prev_rot_values = diceInstance.transform.rotation.eulerAngles;
            await Task.Delay(330);
            new_rot_values = diceInstance.transform.rotation.eulerAngles;
            Debug.Log("rotation_prev = " + prev_rot_values.ToString() + " rotation_post= " + new_rot_values.ToString());
        }

        var faces = diceInstance.transform.Find("faces");
        Transform highestChild = this.gameObject.transform;
        bool init = false;
        foreach (Transform child in faces)
        {
            if(!init){
                highestChild = child;
                init = true;
            }else{
                if(highestChild.transform.position.y < child.transform.position.y){
                    highestChild = child;
                }
            }
        }

        var res = int.Parse(highestChild.gameObject.name);
        Destroy(diceInstance);
        return res;
    }

    #endregion
}
