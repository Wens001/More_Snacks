using UnityEngine;

namespace EasyAnimation {

    [AddComponentMenu("EasyAnimation/缩放效果")]
    public class EasyAnimation_Enlarge : EasyAnimationTemplateMethod
    {
        public Vector3 startScale  = Vector3.one;
        public Vector3 targetScale = Vector3.zero;
        //private Vector3 differentScale;
        Vector3 rectSize = Vector3.one;

        protected override void Easy_Animation_Awake()
        {
            //rectSize = transform.localScale;
            rectSize = startScale;
            //differentScale = startScale - targetScale;
        }

        protected override void PrimitiveOperation_Start()
        {
            ead = new EaseAinmationDrive(1, 0, 1, easetype);
        }

        protected override bool PrimitiveOperation_UpDate(float time)
        {
            //transform.localScale = rectSize * ead.getProgress(time) ;
            transform.localScale = startScale + (targetScale - startScale) * ead.getProgress(time);
            return true;
        }

        public override void Rese()
        {
            transform.localScale = initScale;
        }
    }
}
