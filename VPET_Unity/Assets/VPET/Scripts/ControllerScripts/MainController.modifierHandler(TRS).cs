/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2018 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project 
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
-----------------------------------------------------------------------------
*/
﻿using UnityEngine;
using System.Collections;

//!
//! MainController part handling interactions with the modifiers
//!
namespace vpet
{
	public partial class MainController : MonoBehaviour {
	
	    //!
	    //! ignore dragging
	    //!
	    private bool ignoreDrag = false;
        private Vector3 initPosition = Vector3.zero;
        private Vector3 initScale = Vector3.zero;
        private Quaternion initRotation = Quaternion.identity;
        private Quaternion inverseInitRotation = Quaternion.identity;
        private LineRenderer lineRenderer = null;

        //!
        //! prepare scene & selected pbject for modification
        //! lock the axis for modifications according to the selected modifier
        //! @param      modifier        link to currently active modifier
        //!
        public Modifier.Types handleModifier(Transform modifier){
            // Debug.Log("Hit modifier " + modifier.name + "!");

            Modifier modifierComponent = modifier.parent.GetComponent<Modifier>();
            modifierComponent.isUsed();
            if (ui.LayoutUI != layouts.ANIMATION) //  (activeMode != Mode.animationEditing)
            {
                currentSceneObject.transform.GetComponent<Rigidbody>().isKinematic = true;
            }

            initScale = currentSelection.localScale;
            initRotation = currentSelection.rotation;
            inverseInitRotation = Quaternion.Inverse(currentSelection.rotation);

            if (!lineRenderer)
                lineRenderer = trsGroup.GetComponent<LineRenderer>();
            lineRenderer.enabled = true;

            if (modifierComponent.type == Modifier.Types.TRANS)
            {
                isTranslating = true;
	            //translation modifier
	            modifier.GetComponent<Renderer>().material.color = new Color(1.0f, 185.0f / 255.0f, 55.0f / 255.0f, 1.0f);

                Vector3 planeVec = Vector3.zero;
                axisLocker = Vector3.one;

                // HACK for orthographic view
                if ( Camera.main.orthographic )
				{
                    // same orientation as camera 
                    if (modifier.name == "X-Axis")
                    {
                        planeVec = currentSelection.forward;
                        axisLocker = Vector3.right;
                    }
                    else if (modifier.name == "Y-Axis")
                    {
                        planeVec = currentSelection.forward;
                        axisLocker = Vector3.up;
                    }
                    else if (modifier.name == "Z-Axis")
                    {
                        planeVec = currentSelection.right;
                        axisLocker = Vector3.forward;
                    }
                    else if (modifier.name == "XYAxis")
                    {
                        planeVec = currentSelection.forward;
                    }
                    else if (modifier.name == "YZAxis")
                    {
                        planeVec = currentSelection.right;
                    }
                    else if (modifier.name == "XZAxis")
                    {
                        planeVec = currentSelection.up;
                    }
                    else if (modifier.name == "MoveToFloorQuad"){
						currentSceneObject.moveToFloor();
						translateModifier.transform.position = currentSelection.position;
						translateModifier.transform.GetChild(9).position = new Vector3(currentSelection.position.x, 0.001f, currentSelection.position.z);
						ignoreDrag = true;
					}
				}
				else
				{
		            if (modifier.name == "X-Axis"){
                        planeVec = currentSelection.forward;
                        axisLocker = Vector3.right;
		            }
		            else if (modifier.name == "Y-Axis"){
                        planeVec = currentSelection.forward;
                        axisLocker = Vector3.up;
		            }
		            else if (modifier.name == "Z-Axis"){
                        planeVec = currentSelection.right;
                        axisLocker = Vector3.forward;
		            }
		            else if (modifier.name == "XYAxis"){
                        planeVec = currentSelection.forward;
		            }
		            else if (modifier.name == "YZAxis"){
                        planeVec = currentSelection.right;
		            }
		            else if (modifier.name == "XZAxis"){
                        planeVec = currentSelection.up;
		            }
		            else if (modifier.name == "MoveToFloorQuad"){
		                currentSceneObject.moveToFloor();
		                translateModifier.transform.position = currentSelection.position;
		                translateModifier.transform.GetChild(9).position = new Vector3(currentSelection.position.x, 0.001f, currentSelection.position.z);
		                ignoreDrag = true;
		            }
				}
                //helperPlane = new Plane(planeVec, currentSelection.position);
                helperPlane = new Plane(planeVec, currentSelection.GetComponent<Collider>().bounds.center);
            }
            else if (modifierComponent.type == Modifier.Types.ROT) {
                isRotating = true;
	            //rotation modifier
	            rotationModifier.GetComponent<Modifier>().makeTransparent();
                modifier.GetComponent<Renderer>().material.color = new Color(1.0f, 185.0f / 255.0f, 55.0f / 255.0f, 1.0f);

                helperPlane = new Plane(Camera.main.transform.forward, currentSelection.position);
                Vector3 camToObj = currentSelection.position - Camera.main.transform.position;

                if (modifier.name == "xRotationModifier"){
                    float cfr = Vector3.Dot(camToObj, currentSelection.right);
                    axisLocker = (cfr > 0f) ? Vector3.right : -Vector3.right;
                }
                else if (modifier.name == "yRotationModifier"){
                    float cfu = Vector3.Dot(camToObj, currentSelection.up);
                    axisLocker = (cfu > 0f) ? Vector3.up : -Vector3.up;
                }
                else if (modifier.name == "zRotationModifier"){
                    float cff = Vector3.Dot(camToObj, currentSelection.forward);
                    axisLocker = (cff > 0f) ? Vector3.forward : -Vector3.forward;
                }
            }
            else if (modifierComponent.type == Modifier.Types.SCALE) { 
                isScaling = true;
	            //scale modifier
	            modifier.GetComponent<ModifierComponent>().setColor(new Color(1.0f, 185.0f / 255.0f, 55.0f / 255.0f, 1.0f));

                Vector3 planeVec;

                if (modifier.name == "xScale"){
                    planeVec = currentSelection.forward;
                    axisLocker = Vector3.right;
                }
	            else if (modifier.name == "yScale"){
                    planeVec = currentSelection.forward;
                    axisLocker = Vector3.up;
                }
	            else if (modifier.name == "zScale"){
                    planeVec = currentSelection.right;
                    axisLocker = -Vector3.forward;
                }
                else
                {
                    planeVec = Camera.main.transform.forward;
                    axisLocker = Vector3.one; 
                }
                helperPlane = new Plane(planeVec, currentSelection.position);
	        }
            return modifierComponent.type;
        }

        //!
        //! reset modifiers and push changes to server if neccessary
        //!
        public void resetModifiers()
        {
            // propagate value
            UpdateRangeSliderValue();

	        //reset transparency
	        translateModifier.GetComponent<Modifier>().resetColors();
	        rotationModifier.GetComponent<Modifier>().resetColors();
	        scaleModifier.GetComponent<Modifier>().resetColors();
	        ignoreDrag = false;
	        if (currentSelection && AnimationData.Data.getAnimationClips(currentSelection.gameObject) == null && ui.LayoutUI != layouts.ANIMATION)
                currentSceneObject.setKinematic(currentSceneObject.globalKinematic);

            // desable line renderer
            if (lineRenderer)
                lineRenderer.enabled = false;

            // reset object movement variables
            isTranslating = false;
            isRotating = false;
            isScaling = false;
        }

        //!
        //! hide all modifiers
        //!
        public void hideModifiers()
	    {
	        translateModifier.GetComponent<Modifier>().setVisible(false);
	        rotationModifier.GetComponent<Modifier>().setVisible(false);
	        scaleModifier.GetComponent<Modifier>().setVisible(false);
	    }

        private Vector3 getModifierScale()
        {
            if (Camera.main.orthographic)
            {
                return Vector3.one * Camera.main.orthographicSize / 4f;
            }
            else
            {
                if (arMode)
                {
                    //return Vector3.one * (Vector3.Distance(Camera.main.transform.position, currentSelection.GetComponent<Collider>().bounds.center) / 50) * (Camera.main.fieldOfView / 8) * (Screen.dpi / 300);
                    return Vector3.one
                        * (Vector3.Distance(Camera.main.transform.position, currentSelection.GetComponent<Collider>().bounds.center)
                        * (2.0f * Mathf.Tan(0.5f * (Mathf.Deg2Rad * Camera.main.fieldOfView)))
                        * Screen.dpi / 2000);
                }
                else
                {
                    //return Vector3.one * (Vector3.Distance(Camera.main.transform.position, currentSelection.GetComponent<Collider>().bounds.center) / 30) * (Camera.main.fieldOfView / 8) * (Screen.dpi / 300);
                    return Vector3.one
                            * (Vector3.Distance(Camera.main.transform.position, currentSelection.GetComponent<Collider>().bounds.center)
                            * (2.0f * Mathf.Tan(0.5f * (Mathf.Deg2Rad * Camera.main.fieldOfView)))
                            * Screen.dpi / 2000);
                }
            }
        }


    }
}