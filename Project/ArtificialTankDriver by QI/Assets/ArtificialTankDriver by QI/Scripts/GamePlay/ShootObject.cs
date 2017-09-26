using System;
using UnityEngine;

namespace ArtificialTankDriver_by_QI {
	public class ShootObject : MonoBehaviour {

		public LayerMask hitLayerMask;
		public float hit;
		public float hitRange;
		public float launchSpeed;
		public GameObject expEffect;

		private Action<float> m_scoreCallback;
		
		public void Setup(Action<float> scoreCallback) {
			m_scoreCallback = scoreCallback;
		}
		
		private void Start () {
			GetComponent<Rigidbody>().velocity = transform.forward * launchSpeed;
		}

		private void Update () {
			var cols = Physics.OverlapSphere(transform.position, hitRange, hitLayerMask);
			foreach (var col in cols) {
				var unit = col.GetComponent<Unit>();
				if (!unit) continue;
				var killed = unit.ApplyDamage(hit);
				m_scoreCallback(killed ? 5 : 1);
			}

			if (cols.Length <= 0) return;
			Destroy(Instantiate(expEffect,transform.position,Quaternion.identity),2f);
			Destroy(gameObject);
		}

		private void OnDrawGizmosSelected() {
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireSphere(transform.position,hitRange);
		}
	}
}

