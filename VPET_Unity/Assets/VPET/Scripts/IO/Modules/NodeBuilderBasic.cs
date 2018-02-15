﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using UnityEngine;

namespace vpet
{

	public enum LightTypeKatana { disk, directional, sphere, rect }


	public class NodeBuilderBasic
	{
		public static GameObject BuildNode(ref SceneNode node, Transform parent, GameObject obj)
		{
	        if ( node.GetType() == typeof(SceneNodeGeo) )
	        {
                SceneNodeGeo nodeGeo = (SceneNodeGeo)Convert.ChangeType( node, typeof(SceneNodeGeo) );
	            return CreateObject( nodeGeo, parent );
	        }
	        else if ( node.GetType() == typeof(SceneNodeLight) )
	        {
                SceneNodeLight nodeLight = (SceneNodeLight)Convert.ChangeType( node, typeof(SceneNodeLight) );
                GameObject _obj = CreateLight(nodeLight, parent);
				SceneLoader.SelectableLights.Add(_obj);						  
				return _obj;
			}
	        else if ( node.GetType() == typeof(SceneNodeCam) )
	        {
                SceneNodeCam nodeCam = (SceneNodeCam)Convert.ChangeType( node, typeof(SceneNodeCam) );
				// make the camera editable
                nodeCam.editable = true;
	            return CreateCamera( nodeCam, parent );			
			}
            else if ( node.GetType() == typeof(SceneNode))
            {
	            return CreateNode( node, parent );
	        }

			return null;

		}


	
	    //!
	    //! function create the object from mesh data
	    //! @param  scnObjKtn   object which holds the data
	    //!
	    public static GameObject CreateNode( SceneNode node, Transform parentTransform )
	    {
	        // Tranform
	        Vector3 pos = new Vector3( node.position[0], node.position[1], node.position[2] );
	        //print( "Position: " + pos );
	        Quaternion rot = new Quaternion( node.rotation[0], node.rotation[1], node.rotation[2], node.rotation[3] );
	        // Vector3 euler = rot.eulerAngles;
	        //print( "Euler: " + euler );
	        //rot = new Quaternion();
	        //Vector3 axis = new Vector3(0, 180, 0);
	        //rot.eulerAngles = euler+axis;
	        Vector3 scl = new Vector3( node.scale[0], node.scale[1], node.scale[2] );
	        //print( "Scale: " + scl );
	
	
	        // set up object basics
	        GameObject objMain = new GameObject();
	        objMain.name = Encoding.ASCII.GetString( node.name);
	
	        //place object
	        objMain.transform.parent = parentTransform; // GameObject.Find( "Scene" ).transform;
	        objMain.transform.localPosition =  pos; // new Vector3( 0, 0, 0 );
	        objMain.transform.localRotation =   rot; //  Quaternion.identity;
	        objMain.transform.localScale =    scl; // new Vector3( 1, 1, 1 );
	        objMain.layer = 0;
	
	        return objMain;
	    }


	    //!
	    //! function create the object from mesh data
	    //! @param  scnObjKtn   object which holds the data
	    //!
	    public static GameObject CreateObject( SceneNodeGeo nodeGeo, Transform parentTransform )
	    {
	
	        // Material
	        Material mat = new Material( Shader.Find( "Standard" ) );
	        //available parameters in this physically based shader:
	        // _Color                   diffuse color (color including alpha)
	        // _MainTex                 diffuse texture (2D texture)
	        // _Cutoff                  alpha cutoff
	        // _Glossiness              smoothness of surface
	        // _Metallic                matallic look of the material
	        // _MetallicGlossMap        metallic texture (2D texture)
	        // _BumpScale               scale of the bump map (float)
	        // _BumpMap                 bumpmap (2D texture)
	        // _Parallax                scale of height map
	        // _ParallaxMap             height map (2D texture)
	        // _OcclusionStrength       scale of occlusion
	        // _OcclusionMap            occlusionMap (2D texture)
	        // _EmissionColor           color of emission (color without alpha)
	        // _EmissionMap             emission strength map (2D texture)
	        // _DetailMask              detail mask (2D texture)
	        // _DetailAlbedoMap         detail diffuse texture (2D texture)
	        // _DetailNormalMapScale    scale of detail normal map (float)
	        // _DetailAlbedoMap         detail normal map (2D texture)
	        // _UVSec                   UV Set for secondary textures (float)
	        // _Mode                    rendering mode (float) 0 -> Opaque , 1 -> Cutout , 2 -> Transparent
	        // _SrcBlend                source blend mode (enum is UnityEngine.Rendering.BlendMode)
	        // _DstBlend                destination blend mode (enum is UnityEngine.Rendering.BlendMode)
	        // test texture
	        // WWW www = new WWW("file://F:/XML3D_Examples/tex/casual08a.jpg");
	        // Texture2D texture = www.texture;
	        // meshRenderer.material.SetTexture("_MainTex",texture);
	
	        // Material Properties
	        mat.color = new Color( nodeGeo.color[0], nodeGeo.color[1], nodeGeo.color[2] );
	        mat.SetFloat( "_Glossiness", nodeGeo.roughness );
	
	        // Texture
	        if (nodeGeo.textureId > -1 && nodeGeo.textureId < SceneLoader.SceneTextureList.Count)
	        {
                Texture2D texRef = SceneLoader.SceneTextureList[nodeGeo.textureId];

                mat.SetTexture("_MainTex", texRef);

                // set materials render mode to fate to senable alpha blending
                if (Textures.hasAlpha(texRef))
                {
                    mat.SetFloat("_Mode", 2);
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = 3000;
                }
            }

            // Tranform / convert handiness
            Vector3 pos = new Vector3( nodeGeo.position[0], nodeGeo.position[1], nodeGeo.position[2] );
	        //print( "Position: " + pos );
	        // Rotation / convert handiness
	        Quaternion rot = new Quaternion( nodeGeo.rotation[0], nodeGeo.rotation[1], nodeGeo.rotation[2], nodeGeo.rotation[3] );
	        //print("rot: " + rot.ToString());
	

	
	        // Scale
	        Vector3 scl = new Vector3( nodeGeo.scale[0], nodeGeo.scale[1], nodeGeo.scale[2] );
	        //print( "Scale: " + scl );
	
	
	        // set up object basics
	        GameObject objMain = new GameObject();
	        objMain.name = Encoding.ASCII.GetString(nodeGeo.name);
	
	        // Add Material
	        MeshRenderer meshRenderer = objMain.AddComponent<MeshRenderer>();
	        meshRenderer.material = mat;
	
	
	
	        // Add Mesh
	        if ( nodeGeo.geoId > -1 && nodeGeo.geoId < SceneLoader.SceneMeshList.Count )
	        {
	            Mesh[] meshes = SceneLoader.SceneMeshList[nodeGeo.geoId];
	            objMain.AddComponent<MeshFilter>();
	            objMain.GetComponent<MeshFilter>().mesh  = meshes[0];
	            for( int i=1; i<meshes.Length; i++ )
	            {
	                GameObject subObj = new GameObject( objMain.name+"_part"+i.ToString() );
	                subObj.AddComponent<MeshFilter>();
	                subObj.GetComponent<MeshFilter>().mesh = meshes[i];
	                MeshRenderer subMeshRenderer = subObj.AddComponent<MeshRenderer>();
	                subMeshRenderer.material = mat;
	                subObj.transform.parent = objMain.transform;
	            }
	
	        }
	
	        //place object
	        objMain.transform.parent = parentTransform; // GameObject.Find( "Scene" ).transform;
	        objMain.transform.localPosition =  pos; // new Vector3( 0, 0, 0 );
	        objMain.transform.localRotation =   rot; //  Quaternion.identity;
	        objMain.transform.localScale =    scl; // new Vector3( 1, 1, 1 );
	        //objMain.layer = 0;
	

	        return objMain;
	
	    }
	

	
	    //!
	    //! function create the object from mesh data
	    //! @param  node   object which holds the data
	    //! @param  parentTransform   parent object
	    //!
	    public static GameObject CreateLight( SceneNodeLight nodeLight, Transform parentTransform )
	    {	
	
	        // Tranform
	        Vector3 pos = new Vector3( nodeLight.position[0], nodeLight.position[1], nodeLight.position[2] );
	        Quaternion rot = new Quaternion( nodeLight.rotation[0], nodeLight.rotation[1], nodeLight.rotation[2], nodeLight.rotation[3] );
	        Vector3 scl = new Vector3( nodeLight.scale[0], nodeLight.scale[1], nodeLight.scale[2] );
	
	        // set up object basics
	        GameObject objMain = new GameObject();
	        objMain.name = Encoding.ASCII.GetString( nodeLight.name);
	
	        //place object
			objMain.transform.SetParent(parentTransform, false );
	        objMain.transform.localPosition = pos; 
	        objMain.transform.localRotation = rot;
	        objMain.transform.localScale = scl; 
	
	        // Add light prefab
	        GameObject lightUber = Resources.Load<GameObject>("VPET/Prefabs/UberLight");
	        GameObject _lightUberInstance = GameObject.Instantiate(lightUber);
	        _lightUberInstance.name = lightUber.name;
	
	        Light lightComponent = _lightUberInstance.GetComponent<Light>();
	        lightComponent.type = nodeLight.lightType;
	        lightComponent.color = new Color(nodeLight.color[0], nodeLight.color[1], nodeLight.color[2]);            
            lightComponent.intensity = nodeLight.intensity * VPETSettings.Instance.lightIntensityFactor;
            lightComponent.spotAngle = Mathf.Min(150, nodeLight.angle);
            lightComponent.range = nodeLight.range;

            Debug.Log("Create Light: " + nodeLight.name + " of type: " + ((LightTypeKatana)(nodeLight.lightType)).ToString() + " Intensity: " + nodeLight.intensity + " Pos: " + pos  );

            // Add light specific settings
            if (nodeLight.lightType == LightType.Directional)
	        {
	        }
	        else if (nodeLight.lightType == LightType.Spot)
	        {
	        }
	        else if (nodeLight.lightType == LightType.Area)
	        {
                // TODO: use are lights when supported in unity
                lightComponent.type = LightType.Spot;
                lightComponent.spotAngle = 120;
	        }
	        else
	        {
	        }
	
	
	        // parent 
	        _lightUberInstance.transform.SetParent(objMain.transform, false);
	
	        // add scene object for interactivity at the light quad
			SceneObject sco = objMain.AddComponent<SceneObject>();
			sco.exposure = nodeLight.exposure;

            // Rotate 180 around y-axis because lights and cameras have additional eye space coordinate system
            // objMain.transform.Rotate(new Vector3(0, 180f, 0), Space.Self);

            // TODO: what for ??
            objMain.layer = 0;


            return objMain;
	
	    }
	
	    //!
	    //! function create the object from mesh data
	    //! @param  node   object which holds the data
	    //! @param  parentTransform   parent object
	    //!
	    public static GameObject CreateCamera(SceneNodeCam nodeCam, Transform parentTransform )
	    {
	        // Tranform
	        Vector3 pos = new Vector3( nodeCam.position[0], nodeCam.position[1], nodeCam.position[2] );
	        Quaternion rot = new Quaternion( nodeCam.rotation[0], nodeCam.rotation[1], nodeCam.rotation[2], nodeCam.rotation[3] );
	        Vector3 scl = new Vector3( nodeCam.scale[0], nodeCam.scale[1], nodeCam.scale[2] );
	
	        // set up object basics
	        GameObject objMain = new GameObject();
	        objMain.name =Encoding.ASCII.GetString(nodeCam.name);
	
	        // add camera data script and set values
	        CameraObject camScript = objMain.AddComponent<CameraObject>();
	        camScript.fov = nodeCam.fov;
	        camScript.near = nodeCam.near;
	        camScript.far = nodeCam.far;
	
	        // place camera
	        objMain.transform.parent = parentTransform; 
	        objMain.transform.localPosition =  pos; 
	        objMain.transform.localRotation =   rot; 
	        objMain.transform.localScale =    scl; 
	        
            // Rotate 180 around y-axis because lights and cameras have additional eye space coordinate system
	        // objMain.transform.Rotate(new Vector3(0, 180f, 0), Space.Self);
	
	        // TODO: what for ??
	        objMain.layer = 0;






	        // add to list for later access as camera location
	        SceneLoader.SceneCameraList.Add( objMain );






            // add camera dummy mesh
            GameObject cameraObject = Resources.Load<GameObject>("VPET/Prefabs/cameraObject");
            GameObject cameraInstance = GameObject.Instantiate(cameraObject);
            cameraInstance.name = cameraObject.name;
            cameraInstance.transform.SetParent(objMain.transform, false);
            cameraInstance.transform.localScale = new Vector3(1, 1, 1) * VPETSettings.Instance.sceneScale * 2f;
            cameraInstance.transform.localPosition = new Vector3(0, 0, -.5f * VPETSettings.Instance.sceneScale);

            return objMain;
	    }
	





	}
	
}