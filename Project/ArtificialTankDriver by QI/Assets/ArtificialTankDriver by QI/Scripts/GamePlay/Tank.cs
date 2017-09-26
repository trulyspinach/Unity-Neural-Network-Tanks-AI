using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArtificialTankDriver_by_QI {
	
	public class Tank : Unit {

		public LayerMask enemyMask;
		public float maxSpeed;
		public float maxTorque;

		public float shootCooldown;
		public Transform shootPoint;
		public GameObject bullet;

		public float score;
		
		public Rigidbody rigidbody {  get; private set; }
		private Collider m_collider;
		
		public override void Setup() {
			weaponReady = true;
			score = 0;
			base.Setup();
		}
		
		private void Start() {
			rigidbody = GetComponent<Rigidbody>();
			m_collider = GetComponent<Collider>();
		}

		public void SetMove(float dir) {
			rigidbody.velocity = transform.forward * maxSpeed * dir;
		}
		
		public void SetRotate(float dir) {
			rigidbody.angularVelocity = transform.up * maxTorque * dir;
		}

		public void Score(float v) {
			score += v;
		}
		
		public bool weaponReady { get; private set; } = true;

		public void Shoot() {
			if (!weaponReady || !gameObject.activeSelf) return;
			weaponReady = false;
			Instantiate(bullet, shootPoint.position, shootPoint.rotation).GetComponent<ShootObject>().Setup(Score);

			StartCoroutine(CooldownWeapon());
		}
		
		private IEnumerator CooldownWeapon() {
			yield return new WaitForSeconds(shootCooldown);
			weaponReady = true;
		}

		public Transform ClosestEnemy(float viewRange) {
			var cols = new List<Collider>(Physics.OverlapSphere(transform.position, viewRange, enemyMask));
			cols.Remove(m_collider);
			var firstOrDefault = cols.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).FirstOrDefault();
			return firstOrDefault != null ? firstOrDefault.transform : null;
		}

		public override void Dead() {
			score -= 20;
			score = Mathf.Max(0, score);
			base.Dead();
		}
	}
	
}


