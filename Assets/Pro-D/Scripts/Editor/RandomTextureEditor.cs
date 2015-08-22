using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ProD
{
	[CustomEditor(typeof(RandomTexture))]
	public class RandomTextureEditor : Editor
	{
		public void OnEnable()
		{
			RandomTexture myTarget = target as RandomTexture;
			if (myTarget == null) return;
		}

		public override void OnInspectorGUI()
		{
			bool hasChanged = false;
			RandomTexture myTarget = target as RandomTexture;
			if (myTarget == null) return;

			if (myTarget.textures == null) myTarget.textures = new List<Texture>();
			if (myTarget.weights == null) myTarget.weights = new List<int>();

			int newSize = EditorGUILayout.IntField("Size", myTarget.textures.Count);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Textures");
			EditorGUILayout.LabelField("Weights");
			EditorGUILayout.EndHorizontal();
			if (newSize != myTarget.weights.Count || newSize != myTarget.textures.Count)
			{
				myTarget.weights.Resize(newSize);
				myTarget.textures.Resize(newSize);

				hasChanged = true;
			}

			//foreach (RandomSprite.WeightAndSprite weightedSprite in myTarget.texturesWithWeight)
			for (int i = 0; i < myTarget.weights.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();

				Texture texture = EditorGUILayout.ObjectField(myTarget.textures[i], typeof(Texture), true) as Texture;
				int weight = EditorGUILayout.IntField(myTarget.weights[i]);

				if (texture != myTarget.textures[i] || weight != myTarget.weights[i])
				{
					hasChanged = true;
				}

				myTarget.textures[i] = texture;
				myTarget.weights[i] = weight;

				EditorGUILayout.EndHorizontal();
			}

			if (hasChanged)
			{
				EditorUtility.SetDirty(myTarget);
			}
		}
	}
}
