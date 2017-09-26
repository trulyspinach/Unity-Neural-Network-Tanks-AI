using UnityEngine;
using UnityEngine.UI;

namespace ArtificialTankDriver_by_QI {
	public class UIController : MonoBehaviour {

		public AudioSource music;
		public Text label;
		public Image bar;
		public WorldController controller;

		private void Update () {
			label.text = $"Epoch : {controller.epoch}";
			bar.fillAmount = (float)controller.currentStepsInEpoch / controller.totalStepsPerEpoch;
		}

		public void SetMusic(bool v) {
			music.volume = v ? 1f : 0f;
		}
	}
}

