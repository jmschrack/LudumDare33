/* 
* This code has been designed and developed by Gray Lake Studios.
* You may only use this code if you’ve acquired the appropriate license.
* To acquire such licenses you may visit www.graylakestudios.com and/or Unity Asset Store
* For all inquiries you may contact contact@graylakestudios.com
* Copyright © 2012 Gray Lake Studios
*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProD
{
	[RequireComponent(typeof(Renderer))]
	public class RandomTexture : MonoBehaviour
	{

		public List<Texture> textures;
		public List<int> weights;

		// Use this for initialization
		void Start()
		{
			if (textures == null || textures.Count == 0 ||
				   weights == null || weights.Count == 0 ||
				   textures.Count != weights.Count)
			{
				return;
			}

			int sum = 0;
			foreach (int weight in weights)
			{
				sum += (weight > 0) ? weight : 0;
			}

			int rand = Random.Range(0, sum + 1);

			sum = 0;
			int i = -1;
			do
			{
				i++;
				if (i >= weights.Count) return;
				sum += (weights[i] > 0) ? weights[i] : 0;
			} while (sum < rand || weights[i] <= 0);

			GetComponent<Renderer>().material.SetTexture("_MainTex", textures[i]);
		}
	}
}
