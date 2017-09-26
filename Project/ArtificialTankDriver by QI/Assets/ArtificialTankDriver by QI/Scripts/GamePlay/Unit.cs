using UnityEngine;

namespace ArtificialTankDriver_by_QI {
	
	public class Unit : MonoBehaviour {

		public float fullHealth = 100f;
		private float m_curHealth;

		public float health{
			set { m_curHealth = fullHealth * value; }
			get{ return m_curHealth / fullHealth; }}
		
		public virtual void Setup() {
			health = 1f;
			gameObject.SetActive(true);
		}

		public bool ApplyDamage(float point) {
			m_curHealth -= point;
			if (!(m_curHealth <= 0)) return false;
			Dead();
			return true;
		}

		public virtual void Dead() {
			gameObject.SetActive(false);
		}
		
	}
	
}