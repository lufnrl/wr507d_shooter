// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; // Nécessaire pour détecter les mains

public class ArrowSideSwitcher : MonoBehaviour
{
    public string rightHandTag = "RightHand";
    public Transform arrow; // la flèche visible sur la corde avant le tir (contient le spawnPoint)

    public void UpdateArrowSide(SelectEnterEventArgs args)
    {
        // Debug.Log("Objet qui attrape : " + args.interactorObject.transform.name + " | Tag : " + args.interactorObject.transform.tag);

        // On identifie qui a attrapé l'arc (le "Interactor")
        Transform handTransform = args.interactorObject.transform; // args.interactorObject est la main. On regarde son Transform.
        
        // On vérifie le tag sur l'objet main OU sur son parent (le Controller)
        bool isRightHand = handTransform.CompareTag(rightHandTag) || 
                (handTransform.parent != null && handTransform.parent.CompareTag(rightHandTag));


        if (arrow != null)
        {
            Vector3 pos = arrow.localPosition;

            // On définit le facteur : 1 pour Droitier, -1 pour Gaucher
            float sideFactor = isRightHand ? 1f : -1f;

            // On prend la valeur absolue et on applique le signe
            pos.x = Mathf.Abs(pos.x) * sideFactor;

            arrow.localPosition = pos;

            // Debug.Log(isRightHand ? "Mode Gaucher : Flèche à Droite (+)" : "Mode Droitier : Flèche à Gauche (-)");
        }
    }
    
}

