﻿using UnityEditor;

using UnityEngine;

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using Net.FabreJean.UnityEditor;

namespace Net.FabreJean.PlayMaker.Ecosystem
{

/* RawData
{
    "name": "Vector3ToVector2.cs",
    "type": "Action",
    "pretty name": "Vector3 To Vector2",
    "path": "Assets/PlayMaker Custom Actions/Vector2/Vector3ToVector2.cs",
    "category": "Vector2",
    "unity_version": "3",
    "beta": "false",
    "repository": {
        "id": 17312600,
        "name": "PlayMakerCustomActions_U3",
        "full_name": "jeanfabre/PlayMakerCustomActions_U3",
        "owner": {
            "login": "jeanfabre",
            "id": 1140265,
            "gravatar_id": ""
        }
    }
}
*/
	
	public class Item {

		#region Enums
		public enum ItemTypes {Action,Template,Sample,Package};
		public enum AsynchContentStatus {Pending,Downloading,Available,Unavailable};
		#endregion
		#region Define


		// action screenshots is generated by PlayMaker in one folder, so we'll stick to this, it's enough to have to do the skin dance between dark and light
		// 0-> repository FullPath
		// 1-> skin
		// 2-> filename
		static string __ActionScreenShotUrlFormat__ = "https://github.com/{0}/raw/master/PlayMaker/Screenshots/{1}/{2}";

		// other items will have their doc as a relative path to the package itself, which will be easier to deal with I suspect since it's not in the Assets folder itself anyway.
		// maybe the meta data should be able to overide this to share docs? maybe...
		// but taking screenshots of inspector is tricky...
		// 0 -> Repository FullPath
		// 1-> root Path
		// 2-> skin
		// 3-> filename
		static string __PackageScreenShotUrlFormat__ = "https://github.com/{0}/raw/master/{1}/Documentation/Screenshots/{2}/{3}";

		#endregion Define

		#region Cache 

		static Dictionary<string,Texture> DocumentationImage_Cache = new Dictionary<string, Texture>();

		#endregion Cache

		#region Interface

		public Item(Hashtable _rawData)
		{
			RawData = _rawData;

			// process properties guaranteed to be used
			switch ((string)_rawData["type"])
			{
			case "Action":
				_Type = ItemTypes.Action;
				break;
			case "Template":
				_Type = ItemTypes.Template;
				break;
			case "Sample":
				_Type = ItemTypes.Sample;
				break;
			case "Package":
				_Type = ItemTypes.Package;
				break;
			} 
			_Name =	(string)RawData["name"];
			_PrettyName = (string)RawData["pretty name"];
			_Path = (string)RawData["path"];

			_FolderPath = _Path.Substring(0,_Path.LastIndexOf('/'));

			Hashtable rep = (Hashtable)RawData["repository"];
			_RepositoryFullNamePath = (string)rep["full_name"];
			//...
		}

		public override string ToString()
		{
			return "Item";
		}


		public Texture DocumentationImage
		{
			get{ 

				if (DocumentationImage_Cache.ContainsKey(DocumentationImageUrl))
				{
					return DocumentationImage_Cache[DocumentationImageUrl]; 
				}else
				{
					EditorCoroutine.start(LoadDocumentationImage());
				}

				return null;
			}
		}
		#endregion Interface


		#region Public Properties
		public Hashtable RawData;

		ItemTypes _Type;
		public ItemTypes Type
		{
			get{
				return _Type;
			}
		}

		string _Name;
		public string Name
		{
			get{
				return _Name;
			}
		}
		string _PrettyName;
		public string PrettyName
		{
			get{
				return _PrettyName;
			}
		}


		string _FolderPath;
		public string FolderPath
		{
			get{
				return _FolderPath;
			}
		}

		string _Path;
		public string Path
		{
			get{
				return _Path;
			}
		}

		string _RepositoryFullNamePath;
		public string RepositoryFullNamePath
		{
			get{
				return _RepositoryFullNamePath;
			}
		}


		public AsynchContentStatus DocumentationImageStatus = AsynchContentStatus.Pending;

		#endregion

		#region Private Properties

		string DocumentationImageUrl;

		#endregion

		public delegate void ProcessLoadDocumentation();
		public void LoadDocumentation()
		{
			if (EcosystemBrowser.IsDebugOn) Debug.Log("LoadDocumentation for <"+Name+"> status:"+DocumentationImageStatus);

			if (DocumentationImageStatus == AsynchContentStatus.Pending)
			{
				EditorCoroutine.start(LoadDocumentationImage());
			}
			//EditorCoroutine.start(GetDocumentationDescription());
		}

		IEnumerator LoadDocumentationImage()
		{

			if (string.IsNullOrEmpty(DocumentationImageUrl))
			{
				string screenshotFileName;
				string skin = EditorGUIUtility.isProSkin?"Dark":"Light";

				if (_Type == ItemTypes.Action)
				{
					screenshotFileName = _Name.Replace(".cs",".png");
					DocumentationImageUrl = string.Format(__ActionScreenShotUrlFormat__,_RepositoryFullNamePath,skin,screenshotFileName);
				}else{
					screenshotFileName = _Name.Replace("."+_Type.ToString().ToLower()+".txt",".png");
					DocumentationImageUrl = string.Format(__PackageScreenShotUrlFormat__,_RepositoryFullNamePath,_FolderPath,skin,screenshotFileName);
				}
				DocumentationImageUrl = DocumentationImageUrl.Replace(" ","%20");

//				Debug.Log(DocumentationImageUrl);
			}
			 
			if (EcosystemBrowser.IsDebugOn) Debug.Log("LoadDocumentation for <"+Name+"> url:"+DocumentationImageUrl);

			DocumentationImageStatus = AsynchContentStatus.Downloading;
			WWW _www = new WWW(DocumentationImageUrl);
			while (!_www.isDone) yield return null;
			
			if (string.IsNullOrEmpty(_www.error))
			{
				DocumentationImageStatus = AsynchContentStatus.Available;
				Texture2D _t2d= new Texture2D(2,2);
				_www.LoadImageIntoTexture(_t2d);
				DocumentationImage_Cache[DocumentationImageUrl] = _t2d as Texture;
			}else{
				Debug.LogError("LoadDocumentation error for "+Name+" : "+_www.error);
				DocumentationImageStatus = AsynchContentStatus.Unavailable;
			}

			
			yield break;
		}

		/// <summary>
		/// Loads the meta data.
		/// </summary>
		/// <returns>The meta data.</returns>
		IEnumerator LoadMetaData()
		{
			yield break;
		}
	}

}
