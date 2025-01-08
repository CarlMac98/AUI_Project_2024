using UnityEngine;

#pragma warning disable 0649

namespace ProcGenMusic
{
    public class DemoLightObject : MonoBehaviour
    {
        public Transform Transform => mTransform;

        public void Initialize( Color color, DemoParameters demoParameters )
        {
            mTransform = transform;
            mDemoParameters = demoParameters;
            mColor = color;
            mMaterial = GetComponent<MeshRenderer>().material;
            mLight.intensity = 0f;
            mLight.color = mColor;
            mIsInitialized = true;
        }

        public virtual void Select()
        {
            mLight.intensity = mDemoParameters.mBounceLightIntensity;
            mEmissiveIntensity = mDemoParameters.mBounceLightEmissiveIntensity;
        }

        protected DemoParameters mDemoParameters;

        [SerializeField] protected Light mLight;
        private Material mMaterial;

        private float mEmissiveIntensity;
        private Color mColor;
        private bool mIsInitialized;
        private Transform mTransform;
        private static readonly int ColorID = Shader.PropertyToID( "_BaseColor" );
        private static readonly int InteriorColorID = Shader.PropertyToID( "_EmissionColor" );

        private void Update()
        {
            if ( mIsInitialized )
            {
                DoUpdate();
            }
        }

        protected virtual void DoUpdate()
        {
            if ( mLight.intensity > mDemoParameters.mMinLightIntensity )
            {
                mLight.intensity -= mDemoParameters.mLightDecreaseMultiplier * Time.deltaTime;
            }
            else
            {
                mLight.intensity = mDemoParameters.mMinLightIntensity;
            }

            if ( mEmissiveIntensity > mDemoParameters.mMinEmissiveLightIntensity )
            {
                Color.RGBToHSV( mColor, out var h, out var s, out var v );
                var saturation = mColor.Equals( Color.white ) ? 0f : .8f;
                mColor = Color.HSVToRGB( h, saturation, v );

                mEmissiveIntensity -= mDemoParameters.mEmmissionDecreaseMultiplier * Time.deltaTime;
                mMaterial.SetColor( ColorID, mColor * Mathf.LinearToGammaSpace( mEmissiveIntensity ) );
                mMaterial.SetColor( InteriorColorID, mColor * Mathf.LinearToGammaSpace( mEmissiveIntensity ) );
            }
            else
            {
                mEmissiveIntensity = mDemoParameters.mMinEmissiveLightIntensity;
                mMaterial.SetColor( ColorID, mColor * Mathf.LinearToGammaSpace( mDemoParameters.mMinColorIntensity ) );
                mMaterial.SetColor( InteriorColorID, mColor * Mathf.LinearToGammaSpace( mEmissiveIntensity ) );
            }
        }
    }
}