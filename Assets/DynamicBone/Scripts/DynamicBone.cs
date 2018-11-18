using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Dynamic Bone/Dynamic Bone")]
public class DynamicBone : MonoBehaviour
{
    //The root of the transform hierarchy to apply physics
    public Transform root = null;
    //internal physics simulation rate (in fps)
    public float updateRate = 60.0f;
    //how much the bones are slowed down
    [Range(0, 1)]
    public float damping = 0.1f;
    public AnimationCurve dampingDistribution = null;
    //how much force is applied to return bones to original orientation
    [Range(0, 1)]
    public float elasticity = 0.1f;
    public AnimationCurve elasticityDistribution = null;
    //how much the original orientation is preserved
    [Range(0, 1)]
    public float stiffness = 0.1f;
    public AnimationCurve stiffnessDistribution = null;
    //how much the character's positional change is ignored in simulation
    [Range(0, 1)]
    public float inertia = 0;
    public AnimationCurve inertiaDistribution = null;
    //radius of the sphere collider
    public float radius = 0;
    public AnimationCurve radiusDistribution = null;
    //if nonzero generates an extra bone
    public float endLength = 0;
    //if nonzero generates an extra bone
    public Vector3 endOffset = Vector3.zero;
    //the force to apply to bones in world space Partial force apply to character's initial pose is cancelled out.
    public Vector3 gravity = Vector3.zero;
    //force to apply in world space
    public Vector3 force = Vector3.zero;
    //colliders to interact with the bones - no penetration can occur
    public List<DynamicBoneCollider> colliders = null;
    //bones to exclude from physics simulation
    public List<Transform> exclusions = null;
    //constrain bones to only move on certain planes
    public enum FreezeAxis
    {
        None, X, Y, Z
    }
    public FreezeAxis freezeAxis = FreezeAxis.None;
    //disable physics simulation if the character is distancetoobject away from the reference object
    public bool distantDisable = false;
    public Transform referenceObject = null;
    public float distanceToObject = 20;

    Vector3 localGravity = Vector3.zero;
    Vector3 objectMove = Vector3.zero;
    Vector3 objectPreviousPosition = Vector3.zero;
    float boneTotalLength = 0;
    float objectScale = 1.0f;
    float time = 0;
    float weight = 1.0f;
    bool distantDisabled = false;

    class Particle
    {
        public Transform transform = null;
        public int parentIndex = -1;
        public float damping = 0;
        public float elasticity = 0;
        public float stiffness = 0;
        public float inertia = 0;
        public float radius = 0;
        public float boneLength = 0;

        public Vector3 position = Vector3.zero;
        public Vector3 previousPosition = Vector3.zero;
        public Vector3 endOffset = Vector3.zero;
        public Vector3 initLocalPosition = Vector3.zero;
        public Quaternion initLocalRotation = Quaternion.identity;
    }

    List<Particle> particles = new List<Particle>();

    void Start()
    {
        SetupParticles();
    }

    void Update()
    {
        if (weight > 0 && !(distantDisable && distantDisabled))
            InitTransforms();
    }

    void LateUpdate()
    {
        if (distantDisable)
            CheckDistance();

        if (weight > 0 && !(distantDisable && distantDisabled))
            UpdateDynamicBones(Time.deltaTime);
    }

    void CheckDistance()
    {
        Transform rt = referenceObject;
        if (rt == null && Camera.main != null)
            rt = Camera.main.transform;
        if (rt != null)
        {
            float d = (rt.position - transform.position).sqrMagnitude;
            bool disable = d > distanceToObject * distanceToObject;
            if (disable != distantDisabled)
            {
                if (!disable)
                    ResetParticlesPosition();
                distantDisabled = disable;
            }
        }
    }

    //when enabled in hierarchy
    void OnEnable()
    {
        ResetParticlesPosition();
    }
    //when disabled in hierarchy
    void OnDisable()
    {
        InitTransforms();
    }

    void OnValidate()
    {
        updateRate = Mathf.Max(updateRate, 0);
        damping = Mathf.Clamp01(damping);
        elasticity = Mathf.Clamp01(elasticity);
        stiffness = Mathf.Clamp01(stiffness);
        inertia = Mathf.Clamp01(inertia);
        radius = Mathf.Max(radius, 0);

        if (Application.isEditor && Application.isPlaying)
        {
            InitTransforms();
            SetupParticles();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!enabled || root == null)
            return;

        if (Application.isEditor && !Application.isPlaying && transform.hasChanged)
        {
            InitTransforms();
            SetupParticles();
        }

        Gizmos.color = Color.white;
        for (int i = 0; i < particles.Count; ++i)
        {
            Particle p = particles[i];
            if (p.parentIndex >= 0)
            {
                Particle p0 = particles[p.parentIndex];
                Gizmos.DrawLine(p.position, p0.position);
            }
            if (p.radius > 0)
                Gizmos.DrawWireSphere(p.position, p.radius * objectScale);
        }
    }

    public void SetWeight(float w)
    {
        if (weight != w)
        {
            if (w == 0)
                InitTransforms();
            else if (weight == 0)
                ResetParticlesPosition();
            weight = w;
        }
    }

    public float GetWeight()
    {
        return weight;
    }

    void UpdateDynamicBones(float t)
    {
        if (root == null)
            return;

        objectScale = Mathf.Abs(transform.lossyScale.x);
        objectMove = transform.position - objectPreviousPosition;
        objectPreviousPosition = transform.position;

        int loop = 1;
        if (updateRate > 0)
        {
            float dt = 1.0f / updateRate;
            time += t;
            loop = 0;

            while (time >= dt)
            {
                time -= dt;
                if (++loop >= 3)
                {
                    time = 0;
                    break;
                }
            }
        }

        if (loop > 0)
        {
            for (int i = 0; i < loop; ++i)
            {
                UpdateParticles1();
                UpdateParticles2();
                objectMove = Vector3.zero;
            }
        }
        else
        {
            SkipUpdateParticles();
        }

        ApplyParticlesToTransforms();
    }

    void SetupParticles()
    {
        particles.Clear();
        if (root == null)
            return;

        localGravity = root.InverseTransformDirection(gravity);
        objectScale = transform.lossyScale.x;
        objectPreviousPosition = transform.position;
        objectMove = Vector3.zero;
        boneTotalLength = 0;
        AppendParticles(root, -1, 0);

        for (int i = 0; i < particles.Count; ++i)
        {
            Particle p = particles[i];
            p.damping = damping;
            p.elasticity = elasticity;
            p.stiffness = stiffness;
            p.inertia = inertia;
            p.radius = radius;

            if (boneTotalLength > 0)
            {
                float a = p.boneLength / boneTotalLength;
                if (dampingDistribution != null && dampingDistribution.keys.Length > 0)
                    p.damping *= dampingDistribution.Evaluate(a);
                if (elasticityDistribution != null && elasticityDistribution.keys.Length > 0)
                    p.elasticity *= elasticityDistribution.Evaluate(a);
                if (stiffnessDistribution != null && stiffnessDistribution.keys.Length > 0)
                    p.stiffness *= stiffnessDistribution.Evaluate(a);
                if (inertiaDistribution != null && inertiaDistribution.keys.Length > 0)
                    p.inertia *= inertiaDistribution.Evaluate(a);
                if (radiusDistribution != null && radiusDistribution.keys.Length > 0)
                    p.radius *= radiusDistribution.Evaluate(a);
            }

            p.damping = Mathf.Clamp01(p.damping);
            p.elasticity = Mathf.Clamp01(p.elasticity);
            p.stiffness = Mathf.Clamp01(p.stiffness);
            p.inertia = Mathf.Clamp01(p.inertia);
            p.radius = Mathf.Max(p.radius, 0);
        }
    }

    void AppendParticles(Transform b, int parentIndex, float boneLength)
    {
        Particle p = new Particle();
        p.transform = b;
        p.parentIndex = parentIndex;
        if (b != null)
        {
            p.position = p.previousPosition = b.position;
            p.initLocalPosition = b.localPosition;
            p.initLocalRotation = b.localRotation;
        }
        else 	// end bone
        {
            Transform pb = particles[parentIndex].transform;
            if (endLength > 0)
            {
                Transform ppb = pb.parent;
                if (ppb != null)
                    p.endOffset = pb.InverseTransformPoint((pb.position * 2 - ppb.position)) * endLength;
                else
                    p.endOffset = new Vector3(endLength, 0, 0);
            }
            else
            {
                p.endOffset = pb.InverseTransformPoint(transform.TransformDirection(endOffset) + pb.position);
            }
            p.position = p.previousPosition = pb.TransformPoint(p.endOffset);
        }

        if (parentIndex >= 0)
        {
            boneLength += (particles[parentIndex].transform.position - p.position).magnitude;
            p.boneLength = boneLength;
            boneTotalLength = Mathf.Max(boneTotalLength, boneLength);
        }

        int index = particles.Count;
        particles.Add(p);

        if (b != null)
        {
            for (int i = 0; i < b.childCount; ++i)
            {
                bool exclude = false;
                if (exclusions != null)
                {
                    for (int j = 0; j < exclusions.Count; ++j)
                    {
                        Transform e = exclusions[j];
                        if (e == b.GetChild(i))
                        {
                            exclude = true;
                            break;
                        }
                    }
                }
                if (!exclude)
                    AppendParticles(b.GetChild(i), index, boneLength);
            }

            if (b.childCount == 0 && (endLength > 0 || endOffset != Vector3.zero))
                AppendParticles(null, index, boneLength);
        }
    }

    //Restores particles to their initial transformations
    void InitTransforms()
    {
        for (int i = 0; i < particles.Count; ++i)
        {
            Particle p = particles[i];
            if (p.transform != null)
            {
                p.transform.localPosition = p.initLocalPosition;
                p.transform.localRotation = p.initLocalRotation;
            }
        }
    }

    void ResetParticlesPosition()
    {
        for (int i = 0; i < particles.Count; ++i)
        {
            Particle p = particles[i];
            if (p.transform != null)
            {
                p.position = p.previousPosition = p.transform.position;
            }
            else	// end bone
            {
                Transform pb = particles[p.parentIndex].transform;
                p.position = p.previousPosition = pb.TransformPoint(p.endOffset);
            }
        }
        objectPreviousPosition = transform.position;
    }

    void UpdateParticles1()
    {
        Vector3 force = gravity;
        Vector3 fdir = gravity.normalized;
        Vector3 rf = root.TransformDirection(localGravity);
        Vector3 pf = fdir * Mathf.Max(Vector3.Dot(rf, fdir), 0);	// project current gravity to rest gravity
        force -= pf;	// remove projected gravity
        force = (force + this.force) * objectScale;

        for (int i = 0; i < particles.Count; ++i)
        {
            Particle p = particles[i];
            if (p.parentIndex >= 0)
            {
                // verlet integration
                Vector3 v = p.position - p.previousPosition;
                Vector3 rmove = objectMove * p.inertia;
                p.previousPosition = p.position + rmove;
                p.position += v * (1 - p.damping) + force + rmove;
            }
            else
            {
                p.previousPosition = p.position;
                p.position = p.transform.position;
            }
        }
    }

    void UpdateParticles2()
    {
        Plane movePlane = new Plane();

        for (int i = 1; i < particles.Count; ++i)
        {
            Particle p = particles[i];
            Particle p0 = particles[p.parentIndex];

            float restLen;
            if (p.transform != null)
                restLen = (p0.transform.position - p.transform.position).magnitude;
            else
                restLen = p0.transform.localToWorldMatrix.MultiplyVector(p.endOffset).magnitude;

            // keep shape
            float stiffness = Mathf.Lerp(1.0f, p.stiffness, weight);
            if (stiffness > 0 || p.elasticity > 0)
            {
                Matrix4x4 m0 = p0.transform.localToWorldMatrix;
                m0.SetColumn(3, p0.position);
                Vector3 restPos;
                if (p.transform != null)
                    restPos = m0.MultiplyPoint3x4(p.transform.localPosition);
                else
                    restPos = m0.MultiplyPoint3x4(p.endOffset);

                Vector3 d = restPos - p.position;
                p.position += d * p.elasticity;

                if (stiffness > 0)
                {
                    d = restPos - p.position;
                    float len = d.magnitude;
                    float maxlen = restLen * (1 - stiffness) * 2;
                    if (len > maxlen)
                        p.position += d * ((len - maxlen) / len);
                }
            }

            // collide
            if (colliders != null)
            {
                float particleRadius = p.radius * objectScale;
                for (int j = 0; j < colliders.Count; ++j)
                {
                    DynamicBoneCollider c = colliders[j];
                    if (c != null && c.enabled)
                        c.Collide(ref p.position, particleRadius);
                }
            }

            // freeze axis, project to plane 
            if (freezeAxis != FreezeAxis.None)
            {
                switch (freezeAxis)
                {
                    case FreezeAxis.X:
                        movePlane.SetNormalAndPosition(p0.transform.right, p0.position);
                        break;
                    case FreezeAxis.Y:
                        movePlane.SetNormalAndPosition(p0.transform.up, p0.position);
                        break;
                    case FreezeAxis.Z:
                        movePlane.SetNormalAndPosition(p0.transform.forward, p0.position);
                        break;
                }
                p.position -= movePlane.normal * movePlane.GetDistanceToPoint(p.position);
            }

            // keep length
            Vector3 dd = p0.position - p.position;
            float leng = dd.magnitude;
            if (leng > 0)
                p.position += dd * ((leng - restLen) / leng);
        }
    }

    // only update stiffness and keep bone length
    void SkipUpdateParticles()
    {
        for (int i = 0; i < particles.Count; ++i)
        {
            Particle p = particles[i];
            if (p.parentIndex >= 0)
            {
                p.previousPosition += objectMove;
                p.position += objectMove;

                Particle p0 = particles[p.parentIndex];

                float restLen;
                if (p.transform != null)
                    restLen = (p0.transform.position - p.transform.position).magnitude;
                else
                    restLen = p0.transform.localToWorldMatrix.MultiplyVector(p.endOffset).magnitude;

                // keep shape
                float stiffness = Mathf.Lerp(1.0f, p.stiffness, weight);
                if (stiffness > 0)
                {
                    Matrix4x4 m0 = p0.transform.localToWorldMatrix;
                    m0.SetColumn(3, p0.position);
                    Vector3 restPos;
                    if (p.transform != null)
                        restPos = m0.MultiplyPoint3x4(p.transform.localPosition);
                    else
                        restPos = m0.MultiplyPoint3x4(p.endOffset);

                    Vector3 d = restPos - p.position;
                    float len = d.magnitude;
                    float maxlen = restLen * (1 - stiffness) * 2;
                    if (len > maxlen)
                        p.position += d * ((len - maxlen) / len);
                }

                // keep length
                Vector3 dd = p0.position - p.position;
                float leng = dd.magnitude;
                if (leng > 0)
                    p.position += dd * ((leng - restLen) / leng);
            }
            else
            {
                p.previousPosition = p.position;
                p.position = p.transform.position;
            }
        }
    }

    void ApplyParticlesToTransforms()
    {
        for (int i = 1; i < particles.Count; ++i)
        {
            Particle p = particles[i];
            Particle p0 = particles[p.parentIndex];

            if (p0.transform.childCount <= 1)		// do not modify bone orientation if has more then one child
            {
                Vector3 v;
                if (p.transform != null)
                    v = p.transform.localPosition;
                else
                    v = p.endOffset;
                Quaternion rot = Quaternion.FromToRotation(p0.transform.TransformDirection(v), p.position - p0.position);
                p0.transform.rotation = rot * p0.transform.rotation;
            }

            if (p.transform != null)
                p.transform.position = p.position;
        }
    }
}
