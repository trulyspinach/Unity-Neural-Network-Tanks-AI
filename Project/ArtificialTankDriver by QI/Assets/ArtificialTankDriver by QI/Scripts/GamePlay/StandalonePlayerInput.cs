using UnityEngine;

namespace ArtificialTankDriver_by_QI {
	public class StandalonePlayerInput : MonoBehaviour {

		public Tank target;

		private void Update() {
			var h = Input.GetAxis("Horizontal1");
			var v = Input.GetAxis("Vertical1");
			target.SetMove(v);
			target.SetRotate(h);
			if(Input.GetKey(KeyCode.Space)) target.Shoot();
		}
	}
}

