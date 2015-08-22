using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace ProD
{
	[CustomEditor(typeof(RandomSprite))]
	public class RandomSpriteEditor : Editor
	{
		public void OnEnable()
		{
			RandomSprite myTarget = target as RandomSprite;
			if (myTarget == null) return;
		}

		public override void OnInspectorGUI()
		{
			bool hasChanged = false;
			RandomSprite myTarget = target as RandomSprite;
			if (myTarget == null) return;

			if (myTarget.sprites == null) myTarget.sprites = new List<Sprite>();
			if (myTarget.weights == null) myTarget.weights = new List<int>();

			int newSize = EditorGUILayout.IntField("Size", myTarget.sprites.Count);

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Sprites");
			EditorGUILayout.LabelField("Weights");
			EditorGUILayout.EndHorizontal();
			if (newSize != myTarget.weights.Count || newSize != myTarget.sprites.Count)
			{
				myTarget.weights.Resize(newSize);
				myTarget.sprites.Resize(newSize);

				hasChanged = true;
			}

			//foreach (RandomSprite.WeightAndSprite weightedSprite in myTarget.spritesWithWeight)
			for (int i = 0; i < myTarget.weights.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();

				Sprite sprite = EditorGUILayout.ObjectField(myTarget.sprites[i], typeof(Sprite), true) as Sprite;
				int weight = EditorGUILayout.IntField(myTarget.weights[i]);

				if (sprite != myTarget.sprites[i] || weight != myTarget.weights[i])
				{
					hasChanged = true;
				}

				myTarget.sprites[i] = sprite;
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
