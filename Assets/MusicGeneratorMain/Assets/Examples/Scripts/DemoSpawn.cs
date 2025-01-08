using UnityEngine;

#pragma warning disable 0649
namespace ProcGenMusic
{
	public class DemoSpawn : DemoLightObject
	{
		[SerializeField] private Rigidbody mRigidBody;
		private bool mDidSelect;

		public override void Select()
		{
			base.Select();
			mDidSelect = true;
		}

		protected override void DoUpdate()
		{
			base.DoUpdate();
			if ( mDidSelect )
			{
				mRigidBody.AddForce( Vector3.up * mDemoParameters.mBounceForce, ForceMode.Impulse );
				mDidSelect = false;
			}
		}
	}
}
