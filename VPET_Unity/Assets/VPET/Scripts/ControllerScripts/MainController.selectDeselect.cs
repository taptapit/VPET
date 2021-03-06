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
//! MainController part handling selection and deselection of objects
//!
namespace vpet
{
    public partial class MainController : MonoBehaviour {

        //!
        //! deselect wrapper
        //!
        public void handleSelection()
        {
            if (currentSelection)
            {
                this.deselect();
                activeMode = Mode.idle;
            }
        }

        //!
        //! handles the case when a user points to any object on screen
        //! @param      sObject     Pointer to the object, selected by user
        //!
        public void handleSelection(Transform sObject)
        {
            Debug.Log("Raycast selection: " + sObject.name);
            Debug.Log("Current selection: " + currentSelection);

            if (currentSelection)
            {
                //user is pointing at WorldCollider
                if (sObject.tag == "WorldCollider") {
                    this.deselect();
                    activeMode = Mode.idle;
                    return;
                }

                //user is pointing at the MoveToFloorQuad
                if (sObject.name == "MoveToFloorQuad")
                {
                    //move current selection to floor
                    if (currentSelection.GetComponent<BoxCollider>())
                    {
                        currentSelection.position = new Vector3(currentSelection.position.x, currentSelection.position.y - currentSelection.GetComponent<BoxCollider>().bounds.min.y, currentSelection.position.z);
                    }
                    else if (currentSelection.GetComponent<KeyframeScript>())
                    {
                        currentSelection.position = new Vector3(currentSelection.position.x, currentSelection.position.y - currentSelection.GetComponent<Renderer>().bounds.min.y, currentSelection.position.z);
                        currentSelection.GetComponent<KeyframeScript>().updateKeyInCurve();
                    }
                    return;
                }

                //user is pointing at previous selected object
                //if (currentSelection == sObject)
                //{
                //}

                //user is pointing at another sceneObject
                if (sObject.GetComponent<SceneObject>())
                {
                    if (activeMode == Mode.addMode)
                    {
                        Debug.Log("TODO: Add mode!");
                    }
                    else
                    {
                        this.deselect();
                        this.select(sObject);
                    }
                }
                else //user is pointing at other object, not beeing sceneObject
                {                    
                    this.deselect();
                    activeMode = Mode.idle;
                }
            }
            else if (sObject.GetComponent<SceneObject>()) // no current selection
            {
                this.select(sObject);
            }
        }

		public void callSelect(Transform sObject)
		{
			select(sObject);
		}
        //!
        //! select an object
        //! @param      sObject     Pointer to the object, selected by user
        //!
        private void select(Transform sObject) 
        {
            //cache current selection
            currentSelection = sObject;
            currentSceneObject = sObject.GetComponent<SceneObject>();

            if ((currentSceneObject.locked && !(currentSceneObject is SceneObjectCamera)) || (ui.LayoutUI == layouts.SCOUT))
            {
                currentSelection = null;
                currentSceneObject = null;
                return;
            }

            //set selection
            currentSceneObject.selected = true;
            serverAdapter.SendObjectUpdate(currentSceneObject, ParameterType.LOCK);
            Debug.Log("Select " + currentSelection);

            if (currentSceneObject.GetType() == typeof(SceneObjectLight))
            {
                if (!(activeMode == Mode.translationMode || activeMode == Mode.objectLinkCamera || activeMode == Mode.rotationMode || activeMode == Mode.animationEditing || activeMode == Mode.lightSettingsMode))
                {
                    activeMode = Mode.lightMenuMode;
                }
            }
            else if (currentSceneObject.GetType() == typeof(SceneObjectCamera))
            {
                if (!(activeMode == Mode.translationMode || activeMode == Mode.objectLinkCamera || activeMode == Mode.rotationMode || activeMode == Mode.scaleMode || activeMode == Mode.animationEditing))
                {
                    if (!getCurrentSelection().GetComponent<SceneObject>().locked)
                        activeMode = Mode.cameraMenuMode;
                    else
                        activeMode = Mode.cameraLockedMode;
                }
            }
            else
            {
                if (!(activeMode == Mode.translationMode || activeMode == Mode.objectLinkCamera || activeMode == Mode.rotationMode || activeMode == Mode.scaleMode || activeMode == Mode.animationEditing))
                {
                    activeMode = Mode.objectMenuMode;
                }
            }
		}

        //!
        //! unselect an object or reset state
        //!
        public void deselect()
	    {
			if (!currentSelection)
				return;

            currentSelection.gameObject.GetComponent<SceneObject>().selected = false;
            camVisualizer.SetActive(false);
            ui.hideLightSettingsWidget();

            // make sure its not more locked
            serverAdapter.SendObjectUpdate(currentSceneObject, ParameterType.LOCK);
            Debug.Log("Deselect " + currentSelection);

            if ( activeMode == Mode.objectLinkCamera)
            {
                if (currentSceneObject.GetType() == typeof(SceneObjectLight))
                {
                    currentSelection.parent = oldParent;
                }
                else
                {
                    currentSceneObject.setKinematic(currentSceneObject.globalKinematic, false);
                    currentSelection.parent = oldParent;
                }
            }

            // mutex missing, lock objectSender thread first !! 
            currentSelection = null;
            currentSceneObject = null;
	    }
    }
}