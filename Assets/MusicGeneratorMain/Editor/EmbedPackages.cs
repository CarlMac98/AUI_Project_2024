using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace ProcGenMusic
{
	static class EmbedPackages
	{
		static EmbedRequest AddressableRequest;
		static EmbedRequest TMPRequest;

		//[MenuItem("PMG/EmbedPackages")]
		static void Embed()
		{
			// Embed a package in the project
			AddressableRequest = Client.Embed("com.unity.addressables");
			EditorApplication.update += AddressableProgress;
		}

		static void AddressableProgress()
		{
			if (AddressableRequest.IsCompleted)
			{
				if (AddressableRequest.Status == StatusCode.Success)
					Debug.Log("Embedded: " + AddressableRequest.Result.packageId);
				else if (AddressableRequest.Status >= StatusCode.Failure)
					Debug.Log(AddressableRequest.Error.message);

				EmbedTMP();
				EditorApplication.update -= AddressableProgress;
			}
		}

		static void EmbedTMP()
		{
			TMPRequest = Client.Embed("com.unity.textmeshpro");
			EditorApplication.update += TMPProgress;
		}

		static void TMPProgress()
		{
			if (TMPRequest.IsCompleted)
			{
				if (TMPRequest.Status == StatusCode.Success)
					Debug.Log("Embedded: " + TMPRequest.Result.packageId);
				else if (TMPRequest.Status >= StatusCode.Failure)
					Debug.Log(TMPRequest.Error.message);

				EditorApplication.update -= TMPProgress;
			}
		}
	}
}